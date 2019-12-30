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
    public class SurfaceIntegrationPoint
    {
        public int[] Index { get; set; }

        public ParametricPoint QuadraturePointPosition { get; set; }

        public ParametricPoint ParametricPosition { get; set; }

        public ParametricPoint QuadratureValue { get; set; }

        public RealPoint RealPosition { get; set; }

        public double Jacobian { get; set; }

        public double TemperatureValue { get; set; }

        public double NewTemperatureValue { get; set; }

        public List<double> InitialConditionConstantValues { get; set; }

        public List<double> FunctionqConstantValue { get; set; }

        public List<double> FunctionTConstantValue { get; set; }

        public double HeatSourceConstantValue { get; set; }

       // public double diffusionCoefficientTimeValue { get; set; }

        public SurfaceIntegrationPoint(int[] index, ParametricPoint quadraturePointPosition, double quadratureRangeBegining, double quadratureRangeEnd, ParametricPoint quadraturePointValue, double temperatureValue, ISurfaceShape surfaceShape)
        {
            this.Index = index;
            this.QuadraturePointPosition = quadraturePointPosition;
            this.ParametricPosition = this.calculateParametricPosition(quadraturePointPosition, quadratureRangeBegining, quadratureRangeEnd);
            this.QuadratureValue = quadraturePointValue;
            this.RealPosition = this.calculateRealPosition(surfaceShape);
            this.Jacobian = this.calculateJacobian(surfaceShape);
            this.TemperatureValue = temperatureValue;
            this.FunctionqConstantValue = new List<double>();
            this.FunctionTConstantValue = new List<double>();
            this.InitialConditionConstantValues = new List<double>();
        }

        public SurfaceIntegrationPoint(ParametricPoint parametricPosition, ParametricPoint quadraturePointValue, ISurfaceShape surfaceShape)
        {
            this.ParametricPosition = parametricPosition;
            this.QuadratureValue = quadraturePointValue;
            this.RealPosition = this.calculateRealPosition(surfaceShape);
            this.Jacobian = this.calculateJacobian(surfaceShape);
        }

        private ParametricPoint calculateParametricPosition(ParametricPoint quadraturePointPosition, double quadratureRangeBegining, double quadratureRangeEnd)
        {
            double v = (double)0.5 * (quadraturePointPosition.v + 1);
            double w = (double)0.5 * (quadraturePointPosition.w + 1);

            return new ParametricPoint(v, w);
        }

        private RealPoint calculateRealPosition(ISurfaceShape shapeFunction)
        {
            var _shapeFunction = (BezieSurface)shapeFunction;
            return shapeFunction.CalculateRealPosition(this.ParametricPosition);
        }

        private double calculateJacobian(ISurfaceShape shapeFunction)
        {
            var _shapeFunction = (BezieSurface)shapeFunction;
            return _shapeFunction.CalculateJacobian(this.ParametricPosition);
        }
    }
}
