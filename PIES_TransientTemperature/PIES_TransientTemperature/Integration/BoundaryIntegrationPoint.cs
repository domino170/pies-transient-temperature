using PIESTransientTemperature.ApproximationSeries;
using PIESTransientTemperature.Objects;
using PIESTransientTemperature.ShapeFunctions;
using PIESTransientTemperature.MatrixCalculations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Integration
{
    public class BoundaryIntegrationPoint
    {
        public int Index { get; set; }

        public double ParametricPosition { get; set; }

        public double QuadratureValue { get; set; }

        public RealPoint RealPosition { get; set; }

        public Vector NormalVector { get; set; }

        public double Jacobian { get; set; }

        public BoundaryIntegrationPoint(int index, double quadraturePointPosition, double quadratureRangeBegining, double quadratureRangeEnd, double quadraturePointValue, IBoundaryShape shapeFunction)
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

        private RealPoint calculateRealPosition(IBoundaryShape shapeFunction)
        {
            return shapeFunction.CalculateRealPosition(this.ParametricPosition);
        }

        private Vector calculateNormalVector(IBoundaryShape shapeFunction)
        {
            return shapeFunction.CalculateNormalVector(this.ParametricPosition);
        }

        private double calculateJacobian(IBoundaryShape shapeFunction)
        {
            return shapeFunction.CalculateJacobian(this.ParametricPosition);
        }
    }
}
