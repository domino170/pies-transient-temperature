using PIES_TransientTemperature.Integration;
using PIESTransientTemperature;
using PIESTransientTemperature.Integration;
using PIESTransientTemperature.MatrixCalculations;
using PIESTransientTemperature.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.MatrixCalculations
{
    public static class HeatSource
    {
        public static double[] CalculateBoundaryVector(Area Area)
        {
            var vector = new double[Area.NumberOfCollocationPoints];
            var subAreaIntegrationHelper = new SubAreaIntegrationHelper(3, Area.Surfaces.Select(x => x.SurfaceShape).ToList());

            foreach (var segment in Area.Segments)
            {
                Parallel.ForEach(segment.CollocationPoints, (collPoint) =>
                {
                    var r = Area.Segments.Where(x => x.Index < segment.Index).Sum(x => x.CollocationPoints.Count) + collPoint.Index;

                    vector[r] = CalculateValue(Area, collPoint.RealPosition, subAreaIntegrationHelper, segment.Index);
                });
            }
            return vector;
        }

        public static double CalculateValue(Area Area, RealPoint point, SubAreaIntegrationHelper subAreaIntegrationHelper, int boundarySegmentIndex = 0)
        {
            double value = 0.0;

            foreach (var surface in Area.Surfaces)
            {
                double denominator1 = HeatSource.denominator1(Area.configurationData.GetDiffusionCoefficient(), Area.configurationData.iterationProcess.TimeStep);
                double denominator2 = HeatSource.denominator2(Area.configurationData.GetThermalConductivity());

                for (int subAreaIndex = 0; subAreaIndex < subAreaIntegrationHelper.NumberOfSubSurfaces; subAreaIndex++)
                {
                    foreach (var surfaceIntegrationPoint in surface.HeatSourceSurfaceIntegrationPoints)
                    {
                        if (Area.configurationData.arePropertiesTimeDependent())
                        {
                            denominator1 = HeatSource.denominator1(Area.configurationData.GetDiffusionCoefficient(surfaceIntegrationPoint.TemperatureValue), Area.configurationData.iterationProcess.TimeStep);
                            denominator2 = HeatSource.denominator2(Area.configurationData.GetThermalConductivity(surfaceIntegrationPoint.TemperatureValue));
                        }

                        var subAreaSurfaceIntegrationPoint = subAreaIntegrationHelper.TransformIntegrationPoint(surface.Index, subAreaIndex,boundarySegmentIndex, surfaceIntegrationPoint, point);

                        var parameters = new Dictionary<string, double>();
                        parameters.Add("x", subAreaSurfaceIntegrationPoint.RealPosition.x);
                        parameters.Add("y", subAreaSurfaceIntegrationPoint.RealPosition.y);
                        parameters.Add("t", Area.configurationData.iterationProcess.CurrentTime);
                        var heatSource = surface.heatSourceFunction(parameters);

                        value += CalculateSiglePointFunctionValue(point, denominator1, denominator2, subAreaSurfaceIntegrationPoint) * heatSource;
                    }
                }
            }

            return value;
        }

        public static double CalculateSiglePointFunctionValue(RealPoint point, double denominator1, double denominator2, SurfaceIntegrationPoint surfaceIntegrationPoint)
        {
            double lambda1 = point.x - surfaceIntegrationPoint.RealPosition.x;
            double lambda2 = point.y - surfaceIntegrationPoint.RealPosition.y;

            double r2 = (double)(lambda1 * lambda1 + lambda2 * lambda2);
            double a = (double)r2 / denominator1;

            double value = (double)(surfaceIntegrationPoint.QuadratureValue.v
                 * surfaceIntegrationPoint.QuadratureValue.w
                 * surfaceIntegrationPoint.Jacobian
                 * (FunctionEi.calculate(a) / denominator2));
            return value;
        }


        private static double denominator1(double diffusionCoefficient, double timeStep)
        {
            return (double)4.0 * diffusionCoefficient * timeStep;
        }

        private static double denominator2(double thermalConductivity)
        {
            return (double)0.5 * 8.0 * Math.PI * thermalConductivity;
        }
    }
}
