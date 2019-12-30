using PIESTransientTemperature.Objects;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.ApproximationSeries
{
    public class CollocationPoint
    {
        public double ParametricPosition { get; set; }

        public RealPoint RealPosition { get; set; }

        public int Index { get; set; }

        public List<double> SurfaceIntegrationPointsInitialConditionConstantValues { get; set; }

        public double CollocationPointInitialConditionConstantValue { get; set; }

        public CollocationPoint(int index, int collocationPointsCount, CollocationPlacementType collocationPlacementType, IBoundaryShape shapeFunction, double parametricDistanceFromEdge)
        {
            this.Index = index;
            this.ParametricPosition = this.calculateParametricPosition(collocationPointsCount, collocationPlacementType, parametricDistanceFromEdge);
            this.RealPosition = this.calculateRealPosition(shapeFunction);
            this.SurfaceIntegrationPointsInitialConditionConstantValues = new List<double>();
        }

        private double calculateParametricPosition(int collocationPointsCount, CollocationPlacementType collocationPlacementType, double parametricDistanceFromEdge)
        {
            double parametricPosition = 0;
            if (collocationPlacementType == CollocationPlacementType.Czebyshew)
            {
                double sk = -Math.Cos(Math.PI * (2.0 * (Index + 1.0)) / (2.0 * (collocationPointsCount + 1.0)));
                parametricPosition = 0.5 * (sk + 1.0);
            }
            else if (collocationPlacementType == CollocationPlacementType.Equal)
            {
                parametricPosition = (Index + 1.0) * (1.0 / (collocationPointsCount + 1.0));
            }
            else if (collocationPlacementType == CollocationPlacementType.CloserOnEdges)
            {
                if (Index == 0)
                {
                    parametricPosition = parametricDistanceFromEdge;
                }
                else if (Index == collocationPointsCount - 1)
                {
                    parametricPosition = 1.0 - parametricDistanceFromEdge;
                }
                else
                {
                    double s = parametricDistanceFromEdge;
                    double d = 1.0 - parametricDistanceFromEdge;
                    parametricPosition = s + (Index * (d / (collocationPointsCount - 1.0)));
                }
            }

            return parametricPosition;
        }

        private RealPoint calculateRealPosition(IBoundaryShape shapeFunction)
        {
            return shapeFunction.CalculateRealPosition(this.ParametricPosition);
        }

    }
}
