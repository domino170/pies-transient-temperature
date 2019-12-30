using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Objects
{
    public class ParametricPoint
    {
        public double v { get; set; }
        public double w { get; set; }

        public ParametricPoint(double v, double w)
        {
            this.v = v;
            this.w = w;
        }
    }
}
