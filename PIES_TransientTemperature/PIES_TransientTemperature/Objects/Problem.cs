using PIES_TransientTemperature.Integration;
using PIES_TransientTemperature.MatrixCalculations;
using PIES_TransientTemperature.Oputput;
using PIESTransientTemperature;
using PIESTransientTemperature.ApproximationSeries;
using PIESTransientTemperature.BoundaryConditions;
using PIESTransientTemperature.Integration;
using PIESTransientTemperature.MatrixCalculations;
using PIESTransientTemperature.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.Objects
{
    public class Problem
    {
        public List<Area> Areas { get; set; }
        public IterationProcess IterationProcess { get; set; }
        public List<BoundaryOutputPoint> boundaryOutputPoints;
        public List<RealPoint> domainOutputPoints;
        public List<double> timeOutputPoints;

        public Problem(List<Area> areas, IterationProcess iterationProcess)
        {
            this.Areas = areas;
            this.IterationProcess = iterationProcess;
            boundaryOutputPoints = new List<BoundaryOutputPoint>();
            timeOutputPoints = new List<double>();
            domainOutputPoints = new List<RealPoint>();
        }

        public double GetResultsArea(RealPoint point)
        {
            double result = 0.0;
            foreach (var area in this.Areas)
            {
                foreach (var segmentI in area.Segments)
                {
                    int i = 0;
                    foreach (var collPointI in segmentI.CollocationPoints)
                    {
                        var functionT = Function_T.CalculateAreaValue(point, segmentI, collPointI, area.configurationData);
                        var functionq = Function_q.CalculateAreaValue(point, segmentI, collPointI, area.configurationData);

                        result += -functionq * segmentI.TemperatureBoundaryCondition.BoundaryConditionVector[i] + functionT * segmentI.HeatFluxBoundaryCondition.BoundaryConditionVector[i];
                        i++;
                    }

                }
            }
            foreach (var area in this.Areas)
            {
                var initial = InitialCondition.CalculateValue(area, point);
                result += initial;

                if (area.configurationData.addHeatSource)
                {
                    var subAreaIntegrationHelper = new SubAreaIntegrationHelper(4, area.Surfaces.Select(x => x.SurfaceShape).ToList());
                    result += HeatSource.CalculateValue(area, point, subAreaIntegrationHelper);
                }
            }

            return result;
        }

        public void CalculateSurfaceNewTemperature()
        {
            var newSurfaceIntegrationPointsTemperature = new List<double>();
            foreach (var area in this.Areas)
            {
                foreach (var surface in area.Surfaces)
                {
                    Parallel.ForEach(surface.InitialConditionSurfaceIntegrationPoints, (surfaceIntegrationPoint) =>
                    {
                        if (area.configurationData.arePropertiesTimeDependent())
                        {
                            surfaceIntegrationPoint.NewTemperatureValue = this.GetResultsArea(surfaceIntegrationPoint.RealPosition);
                            //surfaceIntegrationPoint.diffusionCoefficientTimeValue = configurationData.GetDiffusionCoefficient(surfaceIntegrationPoint.NewTemperatureValue);
                        }
                        else
                        {
                            surfaceIntegrationPoint.NewTemperatureValue = this.GetTemperatureFromSurfaceIntegrationPointConstants(surfaceIntegrationPoint);
                        }
                    });
                }
            }
            foreach (var area in this.Areas)
            {
                foreach (var surface in area.Surfaces)
                {
                    foreach (var surfaceIntegrationPoint in surface.InitialConditionSurfaceIntegrationPoints)
                    {
                        surfaceIntegrationPoint.TemperatureValue = surfaceIntegrationPoint.NewTemperatureValue;
                    }
                }
            }
        }

        public void CalculateSurfaceIntegrationPointsConstants()
        {
            //double denominator1 = InitialCondition.denominator1(configurationData);
            //double denominator2 = InitialCondition.denominator2(configurationData);
            foreach (var area in this.Areas)
            {
                foreach (var surface in area.Surfaces)
                {
                    Parallel.ForEach(surface.InitialConditionSurfaceIntegrationPoints, (surfaceIntegrationPoint) =>
                    {
                        var sur = surfaceIntegrationPoint;
                        foreach (var area1 in this.Areas)
                        {
                            foreach (var segmentI in area1.Segments)
                            {
                                foreach (var collPointI in segmentI.CollocationPoints)
                                {
                                    surfaceIntegrationPoint.FunctionTConstantValue.Add(Function_T.CalculateAreaValue(surfaceIntegrationPoint.RealPosition, segmentI, collPointI, area1.configurationData));
                                    surfaceIntegrationPoint.FunctionqConstantValue.Add(Function_q.CalculateAreaValue(surfaceIntegrationPoint.RealPosition, segmentI, collPointI, area1.configurationData));
                                }
                            }
                        }

                        //To w przypadku przechowywania parametrów
                        //foreach (var innerSurface in this.Surfaces)
                        //{
                        //    foreach (var innerSurfaceIntegrationPoint in innerSurface.SurfaceIntegrationPoints)
                        //    {
                        //        surfaceIntegrationPoint.InitialConditionConstantValues.Add(InitialCondition.CalculateSiglePointFunctionValue(surfaceIntegrationPoint.RealPosition, denominator1, denominator2, innerSurfaceIntegrationPoint));
                        //    }
                        //}

                        if (area.configurationData.addHeatSource)
                        {
                            if (!area.configurationData.isHeatSourceTimeDependent)
                            {
                                var subAreaIntegrationHelper = new SubAreaIntegrationHelper(4, area.Surfaces.Select(x => x.SurfaceShape).ToList());
                                surfaceIntegrationPoint.HeatSourceConstantValue = HeatSource.CalculateValue(area, surfaceIntegrationPoint.RealPosition, subAreaIntegrationHelper);
                            }
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Get constant values
        /// </summary>
        public void CalculateCollocationPointsConstants()
        {
            foreach (var area in this.Areas)
            {
                double denominator1 = InitialCondition.denominator1(area.configurationData.GetDiffusionCoefficient(), area.configurationData.iterationProcess.TimeStep);
                double denominator2 = InitialCondition.denominator2(area.configurationData.GetDiffusionCoefficient(), area.configurationData.iterationProcess.TimeStep);

                foreach (var segment in area.Segments)
                {
                    Parallel.ForEach(segment.CollocationPoints, (collPoint) =>
                    {
                        foreach (var innerSurface in area.Surfaces)
                        {
                            foreach (var innerSurfaceIntegrationPoint in innerSurface.InitialConditionSurfaceIntegrationPoints)
                            {
                                if (area.configurationData.arePropertiesTimeDependent())
                                {
                                    denominator1 = InitialCondition.denominator1(area.configurationData.GetDiffusionCoefficient(innerSurfaceIntegrationPoint.TemperatureValue), area.configurationData.iterationProcess.TimeStep);
                                    denominator2 = InitialCondition.denominator2(area.configurationData.GetDiffusionCoefficient(innerSurfaceIntegrationPoint.TemperatureValue), area.configurationData.iterationProcess.TimeStep);
                                }

                                collPoint.SurfaceIntegrationPointsInitialConditionConstantValues.Add(InitialCondition.CalculateSiglePointFunctionValue(collPoint.RealPosition, denominator1, denominator2, innerSurfaceIntegrationPoint));
                            }
                        }
                    });
                }
            }
        }

        public double GetInitialConditionValueFromCollocationPointsConstants(CollocationPoint point)
        {
            double value = 0.0;
            var k = 0;
            foreach (var area in this.Areas)
            {
                foreach (var surface in area.Surfaces)
                {
                    foreach (var surfaceIntegrationPoint in surface.InitialConditionSurfaceIntegrationPoints)
                    {
                        value += point.SurfaceIntegrationPointsInitialConditionConstantValues[k] * surfaceIntegrationPoint.TemperatureValue;
                        k++;
                    }
                }
            }
            return value;
        }

        public double GetTemperatureFromSurfaceIntegrationPointConstants(SurfaceIntegrationPoint point)
        {
            double result = 0.0;

            int j = 0;
            foreach (var area in this.Areas)
            {
                foreach (var segment in area.Segments)
                {
                    foreach (var collPoint in segment.CollocationPoints)
                    {
                        result += -point.FunctionqConstantValue[j] * segment.TemperatureBoundaryCondition.BoundaryConditionVector[collPoint.Index] + point.FunctionTConstantValue[j] * segment.HeatFluxBoundaryCondition.BoundaryConditionVector[collPoint.Index];
                        j++;
                    }
                }
            }
            int k = 0;

            foreach (var area in this.Areas)
            {
                //To tradycyjnie
                result += InitialCondition.CalculateValue(area, point.RealPosition);

                //To w przypadku przechowywania parametrów
                //foreach (var surface in this.Surfaces)
                //{
                //    foreach (var surfaceIntegrationPoint in surface.SurfaceIntegrationPoints)
                //    {
                //        value += point.InitialConditionConstantValues[k] * surfaceIntegrationPoint.TemperatureValue;
                //        k++;
                //    }
                //}

                if (area.configurationData.addHeatSource)
                {
                    if (area.configurationData.isHeatSourceTimeDependent)
                    {
                        var subAreaIntegrationHelper = new SubAreaIntegrationHelper(4, area.Surfaces.Select(x => x.SurfaceShape).ToList());

                        result += HeatSource.CalculateValue(area, point.RealPosition, subAreaIntegrationHelper);
                    }
                    else
                    {
                        result += point.HeatSourceConstantValue;
                    }
                }
            }

            return result;
        }

        public string PrintOutputResults()
        {
            var stringBuilder = new StringBuilder();

            if (this.timeOutputPoints.Contains(this.IterationProcess.CurrentTime))
            {
                foreach (var boundaryPoint in this.boundaryOutputPoints)
                {
                    if (boundaryPoint.boundaryConditionType == BoundaryConditionType.Temperature)
                    {
                        stringBuilder.Append(this.Areas[boundaryPoint.areaIndex].GetTemperatureResultBoundary(boundaryPoint.segmentIndex, boundaryPoint.parametricPosition * this.Areas[boundaryPoint.areaIndex].Segments[boundaryPoint.segmentIndex].Lenght));
                    }
                    else if (boundaryPoint.boundaryConditionType == BoundaryConditionType.HeatFlux)
                    {
                        stringBuilder.Append(this.Areas[boundaryPoint.areaIndex].GetHeatFluxResultBoundary(boundaryPoint.segmentIndex, boundaryPoint.parametricPosition * this.Areas[boundaryPoint.areaIndex].Segments[boundaryPoint.segmentIndex].Lenght));
                    }

                    stringBuilder.Append(Environment.NewLine);
                }

                foreach (var domainPoint in this.domainOutputPoints)
                {
                    stringBuilder.Append(this.GetResultsArea(domainPoint) + Environment.NewLine);
                }

                // stringBuilder.Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }
    }
}
