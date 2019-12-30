using PIESTransientTemperature.Objects;
using PIESTransientTemperature.MatrixCalculations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.ShapeFunctions
{
    public class BezieSurface : ISurfaceShape
    {
        public RealPoint[,] V { get; set; }

        public int SurfaceDegree { get; set; }

        public BezieSurface(RealPoint[,] v, int surfaceDegree)
        {
            this.V = v;
            this.SurfaceDegree = surfaceDegree;
        }

        public RealPoint CalculateRealPosition(ParametricPoint parametricPosition)
        {
            double x = 0.0;
            double y = 0.0;

            for (int direction1 = 0; direction1 < this.SurfaceDegree; direction1++)
            {
                for (int direction2 = 0; direction2 < this.SurfaceDegree; direction2++)
                {
                    x += this.V[direction1, direction2].x * BaseFunction(parametricPosition.v, direction1) * BaseFunction(parametricPosition.w, direction2);
                    y += this.V[direction1, direction2].y * BaseFunction(parametricPosition.v, direction1) * BaseFunction(parametricPosition.w, direction2);
                }
            }

            return new RealPoint(x, y);
        }

        public double CalculateJacobian(ParametricPoint parametricPosition)
        {
            double d1w = 0.0;
            double d2w = 0.0;
            double d3w = 0.0;
            double d1v = 0.0;
            double d2v = 0.0;
            double d3v = 0.0;
            double Jacobian = 0.0;

            for (int direction1 = 0; direction1 < this.SurfaceDegree; direction1++)
            {
                for (int direction2 = 0; direction2 < this.SurfaceDegree; direction2++)
                {
                    d1w += this.V[direction1, direction2].x * BaseFunction(parametricPosition.v, direction1) * BaseFunctionDerivative(parametricPosition.w, direction2);
                    d2w += this.V[direction1, direction2].y * BaseFunction(parametricPosition.v, direction1) * BaseFunctionDerivative(parametricPosition.w, direction2);

                    d1v += this.V[direction1, direction2].x * BaseFunctionDerivative(parametricPosition.v, direction1) * BaseFunction(parametricPosition.w, direction2);
                    d2v += this.V[direction1, direction2].y * BaseFunctionDerivative(parametricPosition.v, direction1) * BaseFunction(parametricPosition.w, direction2);
                }
            }
            double A = d2w * d3v - d2v * d3w;
            double B = d3w * d1v - d3v * d1w;
            double C = d1w * d2v - d1v * d2w;
            Jacobian = (double)Math.Sqrt(A * A + B * B + C * C);

            return Jacobian;
        }

        private double BaseFunction(double x, int degree)
        {
            if (this.SurfaceDegree == 2)
            {
                if (degree == 0) return (1 - x);
                if (degree == 1) return x;
            }
            else if (this.SurfaceDegree == 4)
            {
                if (degree == 0) return ((1 - x) * (1 - x) * (1 - x));
                if (degree == 1) return (3 * x * (1 - x) * (1 - x));
                if (degree == 2) return (3 * x * x * (1 - x));
                if (degree == 3) return (x * x * x);
            }
            return 0.0;
        }

        private double BaseFunctionDerivative(double x, int degree)
        {
            if (this.SurfaceDegree == 2)
            {
                if (degree == 0) return -1;
                if (degree == 1) return 1;
            }
            else if (this.SurfaceDegree == 4)
            {
                if (degree == 0) return (-3 * (1 - x) * (1 - x));
                if (degree == 1) return (3 - 12 * x + 9 * x * x);
                if (degree == 2) return (3 * x * (2 - 3 * x));
                if (degree == 3) return (3 * x * x);
            }
            return 0.0;
        }
    }
}
