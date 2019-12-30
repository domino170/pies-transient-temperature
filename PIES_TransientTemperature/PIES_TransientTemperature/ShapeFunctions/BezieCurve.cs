using PIESTransientTemperature.Objects;
using PIESTransientTemperature.MatrixCalculations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.ShapeFunctions
{
    public class BezieCurve : IBoundaryShape
    {
        public RealPoint a0 { get; set; }
        public RealPoint a1 { get; set; }
        public RealPoint a2 { get; set; }
        public RealPoint a3 { get; set; }

        public RealPoint V0 { get; set; }
        public RealPoint V1 { get; set; }
        public RealPoint Vp0 { get; set; }
        public RealPoint Vp1 { get; set; }

        public double lenght { get; set; }

        public BezieCurve(RealPoint v0, RealPoint v1, RealPoint vp0, RealPoint vp1)
        {
            this.V0 = v0;
            this.V1 = v1;
            this.Vp0 = vp0;
            this.Vp1 = vp1;
            this.CalculateParameters();
        }

        public RealPoint GetV0()
        {
            return this.V0;
        }

        public RealPoint GetV1()
        {
            return this.V1;
        }

        private void CalculateParameters()
        {
            this.a0 = new RealPoint(V0.x, V0.y);
            this.a1 = new RealPoint(3.0 * (Vp0.x - V0.x), 3.0 * (Vp0.y - V0.y));
            this.a2 = new RealPoint(3.0 * V0.x - 6.0 * Vp0.x + 3.0 * Vp1.x, 3.0 * V0.y - 6.0 * Vp0.y + 3.0 * Vp1.y);
            this.a3 = new RealPoint((-V0.x + 3.0 * Vp0.x - 3.0 * Vp1.x + V1.x), (-V0.y + 3.0 * Vp0.y - 3.0 * Vp1.y + V1.y));
        }

        public RealPoint CalculateRealPosition(double parametricPosition)
        {
            var x = parametricPosition * parametricPosition * parametricPosition * this.a3.x
                  + parametricPosition * parametricPosition * this.a2.x
                  + parametricPosition * this.a1.x
                  + this.a0.x;

            var y = parametricPosition * parametricPosition * parametricPosition * this.a3.y
              + parametricPosition * parametricPosition * this.a2.y
              + parametricPosition * this.a1.y
              + this.a0.y;

            return new RealPoint(x, y);
        }

        public Vector CalculateNormalVector(double parametricPosition)
        {
            var NormalVector = new Vector();
            NormalVector.X = (3.0 * this.a3.y * parametricPosition * parametricPosition + 2.0 * this.a2.y * parametricPosition + this.a1.y);
            NormalVector.Y = -(3.0 * this.a3.x * parametricPosition * parametricPosition + 2.0 * this.a2.x * parametricPosition + this.a1.x);
            double lenght = (double)Math.Sqrt(NormalVector.Y * NormalVector.Y + NormalVector.X * NormalVector.X);
            NormalVector.X = (double)NormalVector.X / lenght;
            NormalVector.Y = (double)NormalVector.Y / lenght;

            return NormalVector;
        }

        public double CalculateJacobian(double parametricPosition)
        {
            double Jacobian = Math.Sqrt((3.0 * this.a3.x * parametricPosition * parametricPosition + 2.0 * this.a2.x * parametricPosition + this.a1.x)
                                * (3.0 * this.a3.x * parametricPosition * parametricPosition + 2.0 * this.a2.x * parametricPosition + this.a1.x)
                                + (3.0 * this.a3.y * parametricPosition * parametricPosition + 2.0 * this.a2.y * parametricPosition + this.a1.y)
                                * (3.0 * this.a3.y * parametricPosition * parametricPosition + 2.0 * this.a2.y * parametricPosition + this.a1.y));
            return Jacobian;
        }

        public double CalculateLenght()
        {
            this.lenght = Math.Sqrt(Math.Pow(this.V1.x - this.V0.x, 2) + Math.Pow(this.V1.y - this.V0.y, 2));
            return this.lenght;
        }

        public List<RealPoint> GetPoints()
        {
            return new List<RealPoint>() { V0, Vp0, Vp1, V1 };
        }
    }
}
