using PIESTransientTemperature.Helpers;
using PIESTransientTemperature.Helpers;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.MatrixVector
{
    class MatrixH
    {
        public double[,] matrixValues;

        private Values values;
        private Segments segments;
        private ConfigurationData configurationData;

        public MatrixH(Values values, Segments segments, ConfigurationData configurationData)
        {
            this.values = values;
            this.segments = segments;
            this.configurationData = configurationData;
            matrixValues = new double[segments.NumberOfCollocationPoints, segments.NumberOfCollocationPoints];
        }

        public void Calculate()
        {
            double mianownik = (double)8.0 * Math.PI * configurationData.ThermalConductivity;

            double mianownik2 = (double)4.0 * configurationData.DiffusionCoefficient * (configurationData.TimeStep);

            foreach (Segment segmentR in segments)
            {
                foreach (Segment segmentI in segments)
                {
                    foreach (var collPointR in segmentR.CollocationPoints)
                    {
                        foreach (var collPointI in segmentI.CollocationPoints)
                        {
                            var r = segmentR.Index * segmentR.CollocationPoints.Count() + collPointR.Index;
                            var i = segmentI.Index * segmentI.CollocationPoints.Count() + collPointI.Index;

                            double returnValue = 0.0;

                            if (i == r)
                            {
                                ///TODO: Sprawdzić jak dla nieliniowych ma być
                                var shapeFunctionR = (BezieCurve)segmentR.ShapeFunction;
                                var shapeFunctionI = (BezieCurve)segmentI.ShapeFunction;
                                var destx = (1 - collPointR.ParametricPosition) * shapeFunctionR.a0.x + (collPointR.ParametricPosition) * shapeFunctionR.a1.x;
                                var desty = (1 - collPointR.ParametricPosition) * shapeFunctionR.a0.y + (collPointR.ParametricPosition) * shapeFunctionR.a1.y;
                                var dest = (double)Math.Sqrt((shapeFunctionI.a0.x - destx) * (shapeFunctionI.a0.x - destx) + (shapeFunctionI.a0.y - desty) * (shapeFunctionI.a0.y - desty));
                                returnValue = -0.5 * CzebyshevSeries.calculate(dest, collPointI.Index);
                            }

                            if (i != r)
                            {
                                foreach (var integrationPoint in segmentI.IntegrationPoints)
                                {
                                    double lambda1 = collPointR.RealPosition.x - integrationPoint.RealPosition.x;
                                    double lambda2 = collPointR.RealPosition.y - integrationPoint.RealPosition.y;
                                    double r2 = (double)(lambda1 * lambda1 + lambda2 * lambda2);
                                    double d = (double)(lambda1 * integrationPoint.NormalVector.Y + lambda2 * integrationPoint.NormalVector.X);
                                    double mianownik1 = (double)4.0 * Math.PI * r2;

                                    returnValue -= (double)(integrationPoint.QuadratureValue
                                                   * CzebyshevSeries.calculate(integrationPoint.ParametricPosition * segmentI.Lenght, collPointI.Index)
                                                   * integrationPoint.Jacobian
                                                   * (d / mianownik1)
                                                   * Math.Exp(-r2 / mianownik2));
                                }
                            }
                            else
                            {
                                for (int singularIndex1 = 0; singularIndex1 < segmentI.SingularIntegrationPointsCount; singularIndex1++)
                                {
                                    var singularIntegrationPoint = new IntegrationPoint(
                                        singularIndex1,
                                        segmentI.GaussQuadratureForSingularIntegral.xi[singularIndex1],
                                        0.0,
                                        collPointR.ParametricPosition,
                                        segmentI.GaussQuadratureForSingularIntegral.Ai[singularIndex1],
                                        segmentI.ShapeFunction);

                                    double lambda1 = collPointR.RealPosition.x - singularIntegrationPoint.RealPosition.x;
                                    double lambda2 = collPointR.RealPosition.y - singularIntegrationPoint.RealPosition.y;
                                    double r2 = (double)(lambda1 * lambda1 + lambda2 * lambda2);
                                    double d = (double)(lambda1 * singularIntegrationPoint.NormalVector.Y + lambda2 * singularIntegrationPoint.NormalVector.X);
                                    double mianownik1 = (double)4.0 * Math.PI * r2;

                                    returnValue -= (double)((collPointR.ParametricPosition) * singularIntegrationPoint.QuadratureValue
                                                    * CzebyshevSeries.calculate(singularIntegrationPoint.ParametricPosition * segmentI.Lenght, collPointI.Index)
                                                    * singularIntegrationPoint.Jacobian
                                                    * (d / mianownik1)
                                                    * Math.Exp(-r2 / mianownik2));
                                }

                                for (int singularIndex1 = 0; singularIndex1 < segmentI.SingularIntegrationPointsCount; singularIndex1++)
                                {
                                    var singularIntegrationPoint = new IntegrationPoint(
                                        singularIndex1,
                                        segmentI.GaussQuadratureForSingularIntegral.xi[singularIndex1],
                                        collPointR.ParametricPosition,
                                        1.0,
                                        segmentI.GaussQuadratureForSingularIntegral.Ai[singularIndex1],
                                        segmentI.ShapeFunction);

                                    double lambda1 = collPointR.RealPosition.x - singularIntegrationPoint.RealPosition.x;
                                    double lambda2 = collPointR.RealPosition.y - singularIntegrationPoint.RealPosition.y;
                                    double r2 = (double)(lambda1 * lambda1 + lambda2 * lambda2);
                                    double d = (double)(lambda1 * singularIntegrationPoint.NormalVector.Y + lambda2 * singularIntegrationPoint.NormalVector.X);
                                    double mianownik1 = (double)4.0 * Math.PI * r2;

                                    returnValue -= (double)((1.0 - collPointR.ParametricPosition) * singularIntegrationPoint.QuadratureValue
                                                    * CzebyshevSeries.calculate(singularIntegrationPoint.ParametricPosition * segmentI.Lenght, collPointI.Index)
                                                    * singularIntegrationPoint.Jacobian
                                                    * (d / mianownik1)
                                                    * Math.Exp(-r2 / mianownik2));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
