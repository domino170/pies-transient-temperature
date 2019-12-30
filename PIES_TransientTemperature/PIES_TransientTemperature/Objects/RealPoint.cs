using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Objects
{
    public class RealPoint
    {
        public double x { get; set; }
        public double y { get; set; }

        public RealPoint(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
