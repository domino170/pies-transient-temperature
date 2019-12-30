using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Helpers
{
   public static class CzebyshevSeries
    {
       public static double calculate(double s_, int i)
        {
            //i to stopien wielomianu Czebyszewa
            //wzor rekurencyjny: T[n](x)=2*x*T[n-1](x)-T[n-2](x), n=2,3,...

            if (i == 0) return 1.0;
            if (i == 1) return s_;
            if (i == 2) return (2.0 * s_ * s_ - 1.0);
            if (i == 3) return (4.0 * s_ * s_ * s_ - 3.0 * s_);
            if (i == 4) return (8.0 * Math.Pow(s_, 4) - 8.0 * s_ * s_ + 1.0);
            if (i == 5) return (16.0 * Math.Pow(s_, 5) - 20.0 * s_ * s_ * s_ + 5.0 * s_);
            if (i == 6) return (32.0 * Math.Pow(s_, 6) - 48.0 * Math.Pow(s_, 4) + 18.0 * s_ * s_ - 1.0);
            if (i == 7) return (64.0 * Math.Pow(s_, 7) - 112.0 * Math.Pow(s_, 5) + 56.0 * s_ * s_ * s_ - 7.0 * s_);
            if (i == 8) return (128.0 * Math.Pow(s_, 8) - 256.0 * Math.Pow(s_, 6) + 160.0 * Math.Pow(s_, 4) - 32.0 * s_ * s_ + 1.0);
            if (i == 9) return (256.0 * Math.Pow(s_, 9) - 576.0 * Math.Pow(s_, 7) + 432.0 * Math.Pow(s_, 5) - 120.0 * s_ * s_ * s_ + 9.0 * s_);
            if (i == 10) return (512.0 * Math.Pow(s_, 10) - 1280.0 * Math.Pow(s_, 8) + 1120.0 * Math.Pow(s_, 6) - 400.0 * Math.Pow(s_, 4) + 50.0 * s_ * s_ - 1.0);

            return 0;
        }
    }
}
