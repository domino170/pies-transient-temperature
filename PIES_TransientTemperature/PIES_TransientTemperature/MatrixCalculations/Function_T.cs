using PIESTransientTemperature.ApproximationSeries;
using PIESTransientTemperature.Objects;
using PIESTransientTemperature.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.MatrixCalculations
{
    public static class Function_T
    {
        public static double[,] CalculateBoundaryMatrix(Area Area)
        {
            double[,] matrix = new double[Area.NumberOfCollocationPoints, Area.NumberOfCollocationPoints];
            double denominator1 = Function_T.denominator1(Area.configurationData.GetDiffusionCoefficient(), Area.configurationData.iterationProcess.TimeStep);
            double denominator2 = Function_T.denominator2(Area.configurationData.GetThermalConductivity());

            int r = 0, i = 0;


            foreach (var segmentR in Area.Segments)
            // Parallel.ForEach(Area.Segments, (segmentR) =>
            {
                foreach (var collPointR in segmentR.CollocationPoints)
                {
                    foreach (var segmentI in Area.Segments)
                    {
                        foreach (var collPointI in segmentI.CollocationPoints)
                        {
                            r = Area.Segments.Where(x => x.Index < segmentR.Index).Sum(x => x.CollocationPoints.Count) + collPointR.Index;
                            i = Area.Segments.Where(x => x.Index < segmentI.Index).Sum(x => x.CollocationPoints.Count) + collPointI.Index;

                            double value = 0.0;

                            if (Area.configurationData.arePropertiesTimeDependent())
                            {
                                denominator1 = Function_T.denominator1(Area.configurationData.GetDiffusionCoefficient(segmentI.TemperatureValue), Area.configurationData.iterationProcess.TimeStep);
                                denominator2 = Function_T.denominator2(Area.configurationData.GetThermalConductivity(segmentI.TemperatureValue));
                            }

                            if (segmentI.Index != segmentR.Index)
                            {
                                foreach (var integrationPoint in segmentI.BoundaryIntegrationPoints)
                                {
                                    value -= (double)(CalculateCore(collPointR.RealPosition, integrationPoint, denominator1, denominator2, segmentI, collPointI));
                                }
                            }
                            else
                            {
                                value -= SingularSeparate(denominator1, denominator2, collPointR, segmentI, collPointI);

                              //  value -= SigularAnalitycalMajchrzak(denominator1, denominator2, segmentR, collPointR, segmentI, collPointI);
                            }
                            matrix[r, i] = value;
                        }
                    }
                }
            }//);
            return matrix;
        }

         public static double CalculateAreaValue(RealPoint resultPoint, Segment segmentI, CollocationPoint collPointI, ConfigurationData configurationData)
        {
            double result = 0.0;
            double denominator1 = Function_T.denominator1(configurationData.GetDiffusionCoefficient(), configurationData.iterationProcess.TimeStep);
            double denominator2 = Function_T.denominator2(configurationData.GetThermalConductivity());

            if (configurationData.arePropertiesTimeDependent())
            {
                denominator1 = Function_T.denominator1(configurationData.GetDiffusionCoefficient(segmentI.TemperatureValue), configurationData.iterationProcess.TimeStep);
                denominator2 = Function_T.denominator2(configurationData.GetThermalConductivity(segmentI.TemperatureValue));
            }

            foreach (var integrationPoint in segmentI.SingularBoundaryIntegrationPointsForArea)
            {
                result += CalculateCore(resultPoint, integrationPoint, denominator1, denominator2, segmentI, collPointI);
            }
            return result;
        }

        private static double CalculateCore(RealPoint point, BoundaryIntegrationPoint integrationPoint, double denominator1, double denominator2, Segment segment, CollocationPoint collPoint)
        {
            double lambda1 = point.x - integrationPoint.RealPosition.x;
            double lambda2 = point.y - integrationPoint.RealPosition.y;

            double r2 = (double)(lambda1 * lambda1 + lambda2 * lambda2);
            double a = (double)r2 / denominator1;

            return (double)(integrationPoint.QuadratureValue
                               * CzebyshewSeries.calculate(integrationPoint.ParametricPosition * segment.Lenght, collPoint.Index)
                               * integrationPoint.Jacobian
                               * (FunctionEi.calculate(a) / denominator2));
        }

        private static double SingularSeparate(double denominator1, double denominator2, CollocationPoint collPointR, Segment segmentI, CollocationPoint collPointI)
        {
            double value = 0.0;
            for (int singularIndex1 = 0; singularIndex1 < segmentI.SingularBoundaryIntegrationPointsCount; singularIndex1++)
            {
                var singularIntegrationPoint = new BoundaryIntegrationPoint(
                    singularIndex1,
                    segmentI.GaussQuadratureForSingularIntegral.xi[singularIndex1],
                    0.0,
                    collPointR.ParametricPosition,
                    segmentI.GaussQuadratureForSingularIntegral.Ai[singularIndex1],
                    segmentI.ShapeFunction);

                value += (double)((collPointR.ParametricPosition) * CalculateCore(collPointR.RealPosition, singularIntegrationPoint, denominator1, denominator2, segmentI, collPointI));
            }

            for (int singularIndex1 = 0; singularIndex1 < segmentI.SingularBoundaryIntegrationPointsCount; singularIndex1++)
            {
                var singularIntegrationPoint = new BoundaryIntegrationPoint(
                    singularIndex1,
                    segmentI.GaussQuadratureForSingularIntegral.xi[singularIndex1],
                    collPointR.ParametricPosition,
                    1.0,
                    segmentI.GaussQuadratureForSingularIntegral.Ai[singularIndex1],
                    segmentI.ShapeFunction);

                value += (double)((1.0 - collPointR.ParametricPosition) * CalculateCore(collPointR.RealPosition, singularIntegrationPoint, denominator1, denominator2, segmentI, collPointI));
            }
            return value;
        }

        private static double SigularAnalitycalMajchrzak(double denominator1, double denominator2, Segment segmentR, CollocationPoint collPointR, Segment segmentI, CollocationPoint collPointI)
        {
            var destx = (1 - collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV0().x + (collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV1().x;
            var desty = (1 - collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV0().y + (collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV1().y;
            var dest = (double)Math.Sqrt((segmentI.ShapeFunction.GetV0().x - destx) * (segmentI.ShapeFunction.GetV0().x - destx) + (segmentI.ShapeFunction.GetV0().y - desty) * (segmentI.ShapeFunction.GetV0().y - desty));
            var beta = Math.Pow(segmentI.Lenght, 2.0) / (4.0 * denominator1);
            double value = 2.0 / denominator2 * (2.0 - FunctionEi.gamma - Math.Log10(beta) + (beta * FunctionEi.w[0]) / 3.0 + (Math.Pow(beta, 2) * FunctionEi.w[1]) / 5.0
               + (Math.Pow(beta, 3) * FunctionEi.w[2]) / 7.0 + (Math.Pow(beta, 4) * FunctionEi.w[3]) / 9.0 + (Math.Pow(beta, 5) * FunctionEi.w[4]) / 11.0);
            value *= CzebyshewSeries.calculate(dest, collPointI.Index);
            return value;
        }

        private static double denominator1(double diffusionCoefficient, double timeStep)
        {
            return (double)4.0 * diffusionCoefficient * timeStep;
        }

        private static double denominator2(double thermalConductivity)
        {
            return (double)8.0 * Math.PI * thermalConductivity;
        }
    }
}
