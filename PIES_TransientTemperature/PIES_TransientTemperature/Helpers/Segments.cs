using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Helpers
{
    class Segments : CollectionBase
    {
        public List<Segment> List;

        public int NumberOfCollocationPoints;

        public Segments()
        {
            List = new List<Segment>();
            this.NumberOfCollocationPoints = this.CalculateNumberOfCollocationPoints();
        }

        private int CalculateNumberOfCollocationPoints()
        {
            int number = 0;
            foreach (var segment in this.List)
            {
                number += segment.CollocationPoints.Count();
            }
            return number;
        }


    }
}
