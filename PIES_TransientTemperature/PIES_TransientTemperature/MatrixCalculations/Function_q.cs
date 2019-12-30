using PIESTransientTemperature.ApproximationSeries;
using PIESTransientTemperature.Objects;
using PIESTransientTemperature.ShapeFunctions;
using PIESTransientTemperature.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.MatrixCalculations
{
    public static class Function_q
    {
        public static double[,] CalculateBoundaryMatrix(Area Area)
        {
            double[,] matrix = matrix = new double[Area.NumberOfCollocationPoints, Area.NumberOfCollocationPoints];
            double denominator2 = Function_q.denominator2(Area.configurationData.GetDiffusionCoefficient(0), Area.configurationData.iterationProcess.TimeStep);
            int r = 0, i = 0;

            //Parallel.ForEach(Area.Segments, (segmentR) =>
            foreach (var segmentR in Area.Segments)
            {
                foreach (var collPointR in segmentR.CollocationPoints)
                {
                    foreach (var segmentI in Area.Segments)
                    {
                        if (Area.configurationData.arePropertiesTimeDependent())
                        {
                            denominator2 = Function_q.denominator2(Area.configurationData.GetDiffusionCoefficient(segmentI.TemperatureValue), Area.configurationData.iterationProcess.TimeStep);
                        }

                        foreach (var collPointI in segmentI.CollocationPoints)
                        {
                            r = Area.Segments.Where(x => x.Index < segmentR.Index).Sum(x => x.CollocationPoints.Count) + collPointR.Index;
                            i = Area.Segments.Where(x => x.Index < segmentI.Index).Sum(x => x.CollocationPoints.Count) + collPointI.Index;

                            double value = 0.0;

                            if (segmentI.Index != segmentR.Index)
                            {
                                foreach (var integrationPoint in segmentI.BoundaryIntegrationPoints)
                                {
                                    value -= (double)(CalculateCore(denominator2, collPointR.RealPosition, integrationPoint, segmentI, collPointI));
                                }
                            }
                            else
                            {
                                if (segmentR.DistanceFromSingularPoint == 0.0)
                                {
                                    value -= SigularSeparate(denominator2, segmentR, collPointR, segmentI, collPointI);
                                }
                                else
                                {
                                    value -= SigularSeparateWithAnalitycal(denominator2, segmentR, collPointR, segmentI, collPointI);
                                }
                                //value = -SigularAnalitycalMajchrzak(denominator2, segmentR, collPointR, segmentI, collPointI);
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
            double denominator2 = Function_q.denominator2(configurationData.GetDiffusionCoefficient(segmentI.TemperatureValue), configurationData.iterationProcess.TimeStep);

            foreach (var integrationPoint in segmentI.SingularBoundaryIntegrationPointsForArea)
            {
                result += CalculateCore(denominator2, resultPoint, integrationPoint, segmentI, collPointI);
            }
            return result;
        }

        private static double CalculateCore(double denominator2, RealPoint point, BoundaryIntegrationPoint integrationPoint, Segment segment, CollocationPoint collPoint)
        {
            double lambda1 = point.x - integrationPoint.RealPosition.x;
            double lambda2 = point.y - integrationPoint.RealPosition.y;

            double r2 = (double)(lambda1 * lambda1 + lambda2 * lambda2);
            double d = (double)(lambda1 * integrationPoint.NormalVector.X + lambda2 * integrationPoint.NormalVector.Y);
            double denomiantor1 = (double)4.0 * Math.PI * r2;
            double core = (double)d / denomiantor1 * Math.Exp(-r2 / denominator2);

            return (double)(integrationPoint.QuadratureValue
                                * CzebyshewSeries.calculate(integrationPoint.ParametricPosition * segment.Lenght, collPoint.Index)
                                * integrationPoint.Jacobian
                                * core);
        }

        private static double SigularSeparate(double denominator2, Segment segmentR, CollocationPoint collPointR, Segment segmentI, CollocationPoint collPointI)
        {
            double value = 0.0;
            ///TODO: Sprawdzić jak dla nieliniowych ma być
            var destx = (1 - collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV0().x + (collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV1().x;
            var desty = (1 - collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV0().y + (collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV1().y;
            var dest = (double)Math.Sqrt((segmentI.ShapeFunction.GetV0().x - destx) * (segmentI.ShapeFunction.GetV0().x - destx) + (segmentI.ShapeFunction.GetV0().y - desty) * (segmentI.ShapeFunction.GetV0().y - desty));
            value = 0.5 * CzebyshewSeries.calculate(dest, collPointI.Index);

            for (int singularIndex1 = 0; singularIndex1 < segmentI.SingularBoundaryIntegrationPointsCount; singularIndex1++)
            {
                var singularIntegrationPoint = new BoundaryIntegrationPoint(
                    singularIndex1,
                    segmentI.GaussQuadratureForSingularIntegral.xi[singularIndex1],
                    0.0,
                    collPointR.ParametricPosition,
                    segmentI.GaussQuadratureForSingularIntegral.Ai[singularIndex1],
                    segmentI.ShapeFunction);

                value += (double)((collPointR.ParametricPosition) * CalculateCore(denominator2, collPointR.RealPosition, singularIntegrationPoint, segmentI, collPointI));
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

                value += (double)((1.0 - collPointR.ParametricPosition) * CalculateCore(denominator2, collPointR.RealPosition, singularIntegrationPoint, segmentI, collPointI));
            }
            return value;
        }

        private static double SigularAnalitycalMajchrzak(double denominator2, Segment segmentR, CollocationPoint collPointR, Segment segmentI, CollocationPoint collPointI)
        {
            var destx = (1 - collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV0().x + (collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV1().x;
            var desty = (1 - collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV0().y + (collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV1().y;
            var dest = (double)Math.Sqrt((segmentI.ShapeFunction.GetV0().x - destx) * (segmentI.ShapeFunction.GetV0().x - destx) + (segmentI.ShapeFunction.GetV0().y - desty) * (segmentI.ShapeFunction.GetV0().y - desty));
            return 0.5 * CzebyshewSeries.calculate(dest, collPointI.Index);
        }

        private static double SigularSeparateWithAnalitycal(double denominator2, Segment segmentR, CollocationPoint collPointR, Segment segmentI, CollocationPoint collPointI)
        {
            double value = 0.0;
            ///TODO: Sprawdzić jak dla nieliniowych ma być
            var destx = (1 - collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV0().x + (collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV1().x;
            var desty = (1 - collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV0().y + (collPointR.ParametricPosition) * segmentR.ShapeFunction.GetV1().y;
            var dest = (double)Math.Sqrt((segmentI.ShapeFunction.GetV0().x - destx) * (segmentI.ShapeFunction.GetV0().x - destx) + (segmentI.ShapeFunction.GetV0().y - desty) * (segmentI.ShapeFunction.GetV0().y - desty));
            value = 0.5 * CzebyshewSeries.calculate(dest, collPointI.Index);

            double distance = segmentR.DistanceFromSingularPoint;
            double a = collPointR.ParametricPosition - distance;
            double b = collPointR.ParametricPosition + distance;

            double I1 = 0.0;
            double I2 = 0.0;
            double s1 = collPointR.ParametricPosition;
            var boundaryShape = (BezieCurve)segmentR.ShapeFunction;

            for (int singularIndex1 = 0; singularIndex1 < segmentI.SingularBoundaryIntegrationPointsCount; singularIndex1++)
            {
                double w = 0.5 * (segmentI.GaussQuadratureForSingularIntegral.xi[singularIndex1] + 1.0);
                double parametera = (a - s1) * w + s1;
                double parameterb = (b - s1) * w + s1;
                double sa = (parametera - a);
                double sb = (parameterb - b);

                I1 -= (functionF(boundaryShape, sa, s1, denominator2, collPointR.Index)
                    * segmentI.GaussQuadratureForSingularIntegral.Ai[singularIndex1])
                    - (functionF(boundaryShape, s1, s1, denominator2, collPointR.Index)
                    * Math.Log(Math.Abs(a * boundaryShape.lenght - s1 * boundaryShape.lenght)));

                I2 += (functionF(boundaryShape, sb, s1, denominator2, collPointR.Index)
                   * segmentI.GaussQuadratureForSingularIntegral.Ai[singularIndex1])
                   + (functionF(boundaryShape, s1, s1, denominator2, collPointR.Index)
                   * Math.Log(Math.Abs(b * boundaryShape.lenght - s1 * boundaryShape.lenght)));
            }

            value += (1.0 / (2.0 * Math.PI)) * (I1 + I2);

            for (int singularIndex1 = 0; singularIndex1 < segmentI.SingularBoundaryIntegrationPointsCount; singularIndex1++)
            {
                var singularIntegrationPoint = new BoundaryIntegrationPoint(
                    singularIndex1,
                    segmentI.GaussQuadratureForSingularIntegral.xi[singularIndex1],
                    0.0,
                    a,
                    segmentI.GaussQuadratureForSingularIntegral.Ai[singularIndex1],
                    segmentI.ShapeFunction);

                value += (double)((collPointR.ParametricPosition) * CalculateCore(denominator2, collPointR.RealPosition, singularIntegrationPoint, segmentI, collPointI));
            }

            for (int singularIndex1 = 0; singularIndex1 < segmentI.SingularBoundaryIntegrationPointsCount; singularIndex1++)
            {
                var singularIntegrationPoint = new BoundaryIntegrationPoint(
                    singularIndex1,
                    segmentI.GaussQuadratureForSingularIntegral.xi[singularIndex1],
                    b,
                    1.0,
                    segmentI.GaussQuadratureForSingularIntegral.Ai[singularIndex1],
                    segmentI.ShapeFunction);

                value += (double)((1.0 - collPointR.ParametricPosition) * CalculateCore(denominator2, collPointR.RealPosition, singularIntegrationPoint, segmentI, collPointI));
            }
            return value;
        }

        private static double functionF(BezieCurve boundaryShape, double s, double s1, double denominator2, int collPointIndex)
        {
            var n = boundaryShape.CalculateNormalVector(s);
            double L = (boundaryShape.a1.x + (s1 + s) * boundaryShape.a2.x + (s1 * s1 + s * s1 + s * s) * boundaryShape.a3.x) * n.X +
                (boundaryShape.a1.y + (s1 + s) * boundaryShape.a2.y + (s1 * s1 + s * s1 + s * s) * boundaryShape.a3.y) * n.Y;
            double M = Math.Pow((boundaryShape.a1.x + (s1 + s) * boundaryShape.a2.x + (s1 * s1 + s * s1 + s * s) * boundaryShape.a3.x), 2) +
                Math.Pow((boundaryShape.a1.y + (s1 + s) * boundaryShape.a2.y + (s1 * s1 + s * s1 + s * s) * boundaryShape.a3.y), 2);
            double r2 = Math.Pow(s1 * boundaryShape.lenght - s * boundaryShape.lenght, 2) * M;
            double f = L / M
                * Math.Exp(-r2 / denominator2)
                * CzebyshewSeries.calculate(s * boundaryShape.lenght, collPointIndex);
            return f;
        }

        private static double denominator2(double diffusionCoefficient, double timeStep)
        {
            return (double)4.0 * diffusionCoefficient * timeStep;
        }
    }
}
