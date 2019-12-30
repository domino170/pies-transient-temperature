using PIESTransientTemperature.Helpers;
using PIESTransientTemperature.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.MatrixVector
{
    class MatrixG 
    {
        public double[,] matrixValues;

        private Values values;
        private Segments segments;
        private ConfigurationData configurationData;

        public MatrixG(Values values, Segments segments, ConfigurationData configurationData)
        {
            this.values = values;
            this.segments = segments;
            this.configurationData = configurationData;
            matrixValues = new double[segments.NumberOfCollocationPoints, segments.NumberOfCollocationPoints];
        }

        public void Calculate()
        {
            double secondDenominator = (double)8.0 * Math.PI * configurationData.ThermalConductivity;

            double firstDenominator = (double)4.0 * configurationData.DiffusionCoefficient * (configurationData.TimeStep);

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

                            if (i != r)
                            {
                                foreach (var integrationPoint in segmentI.IntegrationPoints)
                                {
                                    double lambda1 = collPointR.RealPosition.x - integrationPoint.RealPosition.x;
                                    double lambda2 = collPointR.RealPosition.y - integrationPoint.RealPosition.y;
                                    double r2 = (double)(lambda1 * lambda1 + lambda2 * lambda2);
                                    double a = (double)r2 / firstDenominator;

                                    returnValue -= (double)(integrationPoint.QuadratureValue
                                                   * CzebyshevSeries.calculate(integrationPoint.ParametricPosition * segmentI.Lenght, collPointI.Index)
                                                   * integrationPoint.Jacobian
                                                   * (FunctionEi.calculate(a) / secondDenominator));
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
                                    double a = (double)r2 / firstDenominator;

                                    returnValue -= (double)((collPointR.ParametricPosition) * singularIntegrationPoint.QuadratureValue
                                                    * CzebyshevSeries.calculate(singularIntegrationPoint.ParametricPosition * segmentI.Lenght, collPointI.Index)
                                                    * singularIntegrationPoint.Jacobian
                                                    * (FunctionEi.calculate(a) / secondDenominator));
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
                                    double a = (double)r2 / firstDenominator;

                                    returnValue -= (double)((1.0 - collPointR.ParametricPosition) * singularIntegrationPoint.QuadratureValue
                                                    * CzebyshevSeries.calculate(singularIntegrationPoint.ParametricPosition * segmentI.Lenght, collPointI.Index)
                                                    * singularIntegrationPoint.Jacobian
                                                    * (FunctionEi.calculate(a) / secondDenominator));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
