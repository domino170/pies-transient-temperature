using PIES_TransientTemperature.BoundaryConditions;
using PIES_TransientTemperature.Integration;
using PIES_TransientTemperature.MatrixCalculations;
using PIES_TransientTemperature.Oputput;
using PIESTransientTemperature.ApproximationSeries;
using PIESTransientTemperature.BoundaryConditions;
using PIESTransientTemperature.Integration;
using PIESTransientTemperature.MatrixCalculations;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Objects
{
    public class Area
    {
        public int Index;
        public List<Segment> Segments;
        public List<Surface> Surfaces;
        public ConfigurationData configurationData;

        public int NumberOfCollocationPoints
        {
            get
            {
                return this.CalculateNumberOfCollocationPoints();
            }
        }

        public Area(int Index, ConfigurationData configurationData)
        {
            this.Index = Index;
            this.Segments = new List<Segment>();
            this.Surfaces = new List<Surface>();
            this.configurationData = configurationData;
        }

        public double[] GetKnownBoundaryVector()
        {
            var F = new double[this.NumberOfCollocationPoints];
            int j = 0;
            foreach (var segmentI in this.Segments)
            {
                foreach (var value in segmentI.KnownBoundaryCondition.BoundaryConditionVector)
                {
                    F[j++] = value;
                }
            }
            return F;
        }

        public void CalculateBoundaryTemperature()
        {
            if (configurationData.iterationProcess.CurrentIteration == 1)
            {
                foreach (var segment in this.Segments)
                {
                    if (segment.KnownBoundaryCondition.BoundaryConditionType == BoundaryConditionType.Temperature)
                    {
                        segment.TemperatureValue = this.GetTemperatureResultBoundary(segment.Index, segment.Lenght / 2.0);
                    }
                    else
                    {
                        segment.TemperatureValue = this.Surfaces[0].initialTemperatureValue;
                    }
                }
            }
            else
            {
                foreach (var segment in this.Segments)
                {
                    segment.TemperatureValue = this.GetTemperatureResultBoundary(2, segment.Lenght / 2.0);
                }
            }
        }

        public double GetTemperatureResultBoundary(int index, double resultPoint)
        {
            return this.Segments.ElementAt(index).TemperatureBoundaryCondition.CalculateValue(resultPoint);
        }

        public double GetHeatFluxResultBoundary(int index, double resultPoint)
        {
            return this.Segments.ElementAt(index).HeatFluxBoundaryCondition.CalculateValue(resultPoint);
        }

        private int CalculateNumberOfCollocationPoints()
        {
            int number = 0;
            foreach (var segment in this.Segments)
            {
                number += segment.CollocationPoints.Count();
            }
            return number;
        }

        public void calculateVariableBoundaryConditions()
        {
            foreach (var segment in this.Segments)
            {
                if (segment.KnownBoundaryCondition.boundaryConditionValueType == BoundaryConditionValueType.ConvectionRadiation)
                {
                    double boundaryTemperature = configurationData.iterationProcess.CurrentIteration == 1 ? this.Surfaces[0].initialTemperatureValue : segment.TemperatureBoundaryCondition.CalculateValue(0.5 * segment.Lenght);
                    segment.KnownBoundaryCondition.CalculateHeatFluxFromRobinBoundaryCondition(boundaryTemperature);
                }
                else if (segment.KnownBoundaryCondition.boundaryConditionValueType == BoundaryConditionValueType.TimeIntervalValue)
                {
                    segment.KnownBoundaryCondition.CalculateTimeIntervalValueBoundaryCondition(configurationData.iterationProcess.CurrentTime);
                }
                else if (segment.KnownBoundaryCondition.boundaryConditionValueType == BoundaryConditionValueType.Value)
                {
                    segment.KnownBoundaryCondition.CalculateValueBoundaryCondition(configurationData.iterationProcess.CurrentTime);
                }
            }
        }
    }
}
