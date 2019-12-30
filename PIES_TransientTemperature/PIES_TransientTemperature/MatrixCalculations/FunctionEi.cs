using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.MatrixCalculations
{
    public static class FunctionEi
    {

        public static double gamma
        {
            get
            {
                return 0.57721566;
            }
        }

        public static double[] w
        {
            get
            {
                var w = new double[5];
                w[0] = 0.99999193;
                w[1] = -0.24991055;
                w[2] = 0.05519968;
                w[3] = -0.00976;
                w[4] = 0.00107857;
                return w;

            }
        }

        private static double[] s
        {
            get
            {
                var s = new double[5];
                s[0] = 0.2677737343;
                s[1] = 8.6347608925;
                s[2] = 18.059016973;
                s[3] = 8.5733287401;
                s[4] = 1;
                return s;

            }
        }

        private static double[] p
        {
            get
            {
                var p = new double[5];
                p[0] = 3.958496228;
                p[1] = 21.0996530827;
                p[2] = 25.6329561486;
                p[3] = 9.5733223454;
                p[4] = 1;
                return p;

            }
        }

        //z książki prof. E. Majchrzak
        public static double calculate(double a)
        {
            double wynik = 0;

            if (a < 1)
            {
                double suma1 = 0;

                for (int i = 0; i < 5; i++)
                {
                    suma1 += w[i] * Math.Pow(a, i + 1);
                }
                wynik = -gamma - Math.Log(a) + suma1;
                return wynik;
            }
            else
            {
                double suma1 = 0;
                double suma2 = 0;

                for (int i = 0; i < 5; i++)
                {
                    suma1 += s[i] * Math.Pow(a, i);
                    suma2 += p[i] * Math.Pow(a, i);
                }
                wynik = (double)(Math.Exp(-a) / a) * (suma1 / suma2);

                return wynik;
            }

            //long double wynik=0, suma=0,b,c,d,e;
            //long double C=-0.57721566;
            //for(int i=1; i<10;i++){
            //	e=Math::Pow(-1,i-1);
            //	b=Math::Pow(a,i);
            //	c=i*silnia(i);
            //	suma+=(long double)e*(b/c);
            //}
            //d=Math::Log10(a);
            //wynik=(long double)C-d+suma;

            //return wynik;
        }
    }
}
