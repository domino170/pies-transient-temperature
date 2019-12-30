using PIESTransientTemperature.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.MatrixCalculations
{
    public static class GaussElimination
    {
        public static double[] Calculate(double[,] A, double[] B, int N)
        {
            int i, j, k, ii, jj;
            double tmp;
            var marked = new int[N];
            var pivot = new int[N];
            var iteration = new int[N];
            int picked = 0;
            var a = new double[N, N];
            var b = new double[N];
            var x = new double[N];
            var X = new double[N];

            for (i = 0; i < N; i++)
            {
                for (j = 0; j < N; j++)
                {
                    a[i, j] = A[i, j];
                }
            }

            for (i = 0; i < N; i++)
            {
                b[i] = B[i];
            }

            // Gaussian elimination 
            for (i = 0; i < N; i++)
            {
                marked[i] = 0;
            }

            for (i = 0; i < (N - 1); i++)
            {
                tmp = 0.0;
                for (j = 0; j < N; j++)
                {
                    if ((marked[j] == 0) && (Math.Abs(a[j, i]) > tmp))
                    {
                        tmp = Math.Abs(a[j, i]);
                        picked = j;
                    }
                }

                marked[picked] = 1;
                pivot[picked] = i;
                iteration[i] = picked;

                for (j = 0; j < N; j++)
                {
                    if (marked[j] == 0)
                    {
                        tmp = a[j, i] / a[picked, i];
                        for (k = i + 1; k < N; k++)
                        {
                            a[j, k] = a[j, k] - a[picked, k] * tmp;
                        }
                        b[j] = b[j] - b[picked] * tmp;
                    }
                }
            }

            for (i = 0; i < N; i++)
            {
                if (marked[i] == 0)
                {
                    pivot[i] = N - 1;
                    iteration[N - 1] = i;
                    break;
                }
            }

            // Back substitution 
            for (ii = N - 1; ii >= 0; ii--)
            {
                i = iteration[ii];
                x[i] = b[i] / a[i, ii];
                for (jj = 0; jj <= ii - 1; jj++)
                {
                    j = iteration[jj];
                    b[j] = b[j] - x[i] * a[j, ii];
                    a[j, ii] = 0.0;
                }
            }


            for (i = 0; i < N; i++)
            {
                X[i] = x[iteration[i]];
            }

            return X;
        }
    }
}
