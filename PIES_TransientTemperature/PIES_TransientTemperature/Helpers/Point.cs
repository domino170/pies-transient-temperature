using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Helpers
{
    class Point
    {
        public double x { get; set; }
        public double y { get; set; }

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
