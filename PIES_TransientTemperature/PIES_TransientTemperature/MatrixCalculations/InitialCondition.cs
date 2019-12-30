using PIESTransientTemperature;
using PIESTransientTemperature.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.MatrixCalculations
{
    public static class InitialCondition
    {
        public static double[] CalculateBoundaryVector(Area Area)
        {
            var vector = new double[Area.NumberOfCollocationPoints];

            foreach (var segment in Area.Segments)
            {
                Parallel.ForEach(segment.CollocationPoints, (collPoint) =>
                {
                    var r = Area.Segments.Where(x => x.Index < segment.Index).Sum(x => x.CollocationPoints.Count) + collPoint.Index;

                    vector[r] = CalculateValue(Area, new RealPoint(collPoint.RealPosition.x, collPoint.RealPosition.y));
                });
            }
            return vector;
        }

        public static double CalculateValue(Area Area, RealPoint point)
        {
            double value = 0.0;

            var sign = 1.0;

            foreach (var surface in Area.Surfaces)
            {
                double denominator1 = InitialCondition.denominator1(Area.configurationData.GetDiffusionCoefficient(), Area.configurationData.iterationProcess.TimeStep);
                double denominator2 = InitialCondition.denominator2(Area.configurationData.GetDiffusionCoefficient(), Area.configurationData.iterationProcess.TimeStep);

                if (surface.subractSurface)
                {
                    sign = -1.0;
                }

                foreach (var surfaceIntegrationPoint in surface.InitialConditionSurfaceIntegrationPoints)
                {
                    if (Area.configurationData.arePropertiesTimeDependent())
                    {
                        denominator1 = InitialCondition.denominator1(Area.configurationData.GetDiffusionCoefficient(surfaceIntegrationPoint.TemperatureValue), Area.configurationData.iterationProcess.TimeStep);
                        denominator2 = InitialCondition.denominator2(Area.configurationData.GetDiffusionCoefficient(surfaceIntegrationPoint.TemperatureValue), Area.configurationData.iterationProcess.TimeStep);
                    }

                    value += sign * CalculateSiglePointFunctionValue(point, denominator1, denominator2, surfaceIntegrationPoint) * surfaceIntegrationPoint.TemperatureValue;
                }
            }
            return value;
        }

        public static double CalculateSiglePointFunctionValue(RealPoint point, double denominator1, double denominator2, PIESTransientTemperature.Integration.SurfaceIntegrationPoint surfaceIntegrationPoint)
        {
            double lambda1 = point.x - surfaceIntegrationPoint.RealPosition.x;
            double lambda2 = point.y - surfaceIntegrationPoint.RealPosition.y;

            double r2 = (double)(lambda1 * lambda1 + lambda2 * lambda2);

            double value = (double)(surfaceIntegrationPoint.QuadratureValue.v
                 * surfaceIntegrationPoint.QuadratureValue.w
                 * surfaceIntegrationPoint.Jacobian
                 * (1.0 / denominator1)
                 * Math.Exp(-r2 / denominator2));
            return value;
        }

        public static double denominator1(double diffusionCoefficient, double timeStep)
        {
            return (double)16.0 * diffusionCoefficient * Math.PI * timeStep;
        }

        public static double denominator2(double diffusionCoefficient, double timeStep)
        {
            return (double)4.0 * diffusionCoefficient * timeStep;
        }
    }
}
