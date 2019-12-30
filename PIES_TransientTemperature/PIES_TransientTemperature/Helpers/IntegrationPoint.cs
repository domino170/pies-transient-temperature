using PIESTransientTemperature.CollocationPoints;
using PIESTransientTemperature.Helpers;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Helpers
{
    class IntegrationPoint
    {
        public int Index { get; set; }

        public double ParametricPosition { get; set; }

        public double QuadratureValue { get; set; }

        public Point RealPosition { get; set; }

        public Vector NormalVector { get; set; }

        public double Jacobian { get; set; }

        public IntegrationPoint(int index, double quadraturePointPosition, double quadratureRangeBegining, double quadratureRangeEnd, double quadraturePointValue, IShapeFunction shapeFunction)
        {
            this.Index = index;
            this.ParametricPosition = this.calculateParametricPosition(quadraturePointPosition, quadratureRangeBegining, quadratureRangeEnd);
            this.QuadratureValue = quadraturePointValue;
            this.RealPosition = this.calculateRealPosition(shapeFunction);
            this.NormalVector = this.calculateNormalVector(shapeFunction);
            this.Jacobian = this.calculateJacobian(shapeFunction);
        }

        private double calculateParametricPosition(double quadraturePointPosition, double quadratureRangeBegining, double quadratureRangeEnd)
        {
            double parametricPrecision = 0.5 * (1 - quadraturePointPosition) * quadratureRangeBegining + 0.5 * (quadraturePointPosition + 1) * quadratureRangeEnd;
            return parametricPrecision;
        }

        private Point calculateRealPosition(IShapeFunction shapeFunction)
        {
            var _shapeFunction = (BezieCurve)shapeFunction;
            return shapeFunction.CalculateRealPosition(this.ParametricPosition);
        }

        private Vector calculateNormalVector(IShapeFunction shapeFunction)
        {
            var _shapeFunction = (BezieCurve)shapeFunction;
            return _shapeFunction.CalculateNormalVector(this.ParametricPosition);
        }

        private double calculateJacobian(IShapeFunction shapeFunction)
        {
            var _shapeFunction = (BezieCurve)shapeFunction;
            return _shapeFunction.CalculateJacobian(this.ParametricPosition);
        }
    }
}
