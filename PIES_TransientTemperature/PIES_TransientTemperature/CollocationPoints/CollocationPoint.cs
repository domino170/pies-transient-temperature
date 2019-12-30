using PIESTransientTemperature.CollocationPoints;
using PIESTransientTemperature.Helpers;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.CollocationPoints
{
    class CollocationPoint
    {
        public double ParametricPosition { get; set; }

        public Point RealPosition { get; set; }

        public int Index { get; set; }

        public CollocationPoint(int index, int collocationPointsCount, CollocationPlacementType collocationPlacementType, IShapeFunction shapeFunction)
        {
            this.Index = index;
            this.ParametricPosition = this.calculateParametricPosition(collocationPointsCount, collocationPlacementType);
            this.RealPosition = this.calculateRealPosition(shapeFunction);
        }

        private double calculateParametricPosition(int collocationPointsCount, CollocationPlacementType collocationPlacementType)
        {
            double parametricPrecision = 0;
            if (collocationPlacementType == CollocationPlacementType.Czebyshev)
            {
                double Sk = -Math.Cos(Math.PI * (2.0 * (Index + 1.0)) / (2.0 * (collocationPointsCount + 1.0)));
                parametricPrecision = 0.5 * (Sk + 1.0);
            }

            return parametricPrecision;
        }

        private Point calculateRealPosition(IShapeFunction shapeFunction)
        {
            var _shapeFunction = (BezieCurve)shapeFunction;

            return shapeFunction.CalculateRealPosition(this.ParametricPosition);
        }

    }
}
