using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Integration
{
    public class GaussQuadrature
    {

        public double[] Ai;
        public double[] xi;

        public GaussQuadrature(int BoundaryIntegrationPointsCount)
        {
            this.CalculateQuadrature(BoundaryIntegrationPointsCount);
        }

        private void CalculateQuadrature(int Ni)
        {

            Ai = new double[Ni];
            xi = new double[Ni];

            double EPS = 3.0e-10;//3.0e-20;

            //long long double x[MAX+1],w[MAX+1];
            double z1, z, xm, xl, pp, p3, p2, p1;
            int m, jj, ii;
            double a = -1.0;
            double b = 1.0;

            int n = Ni;
            m = (n + 1) / 2;
            xm = 0.5 * (b + a);
            xl = 0.5 * (b - a);

            for (ii = 1; ii <= m; ii++)
            {
                z = Math.Cos(Math.PI * (ii - 0.25) / (n + 0.5));
                do
                {
                    p1 = 1.0;
                    p2 = 0.0;
                    for (jj = 1; jj <= n; jj++)
                    {
                        p3 = p2;
                        p2 = p1;
                        p1 = ((2.0 * jj - 1.0) * z * p2 - (jj - 1.0) * p3) / jj;
                    }
                    pp = n * (z * p1 - p2) / (z * z - 1.0);
                    z1 = z;
                    z = z1 - p1 / pp;
                } while (Math.Abs(z - z1) > EPS);
                xi[ii - 1] = xm - xl * z;
                xi[n + 1 - (ii + 1)] = xm + xl * z;
                Ai[ii - 1] = 2.0 * xl / ((1.0 - z * z) * pp * pp);
                Ai[n + 1 - (ii + 1)] = Ai[ii - 1];
            }

        }
    }
}
