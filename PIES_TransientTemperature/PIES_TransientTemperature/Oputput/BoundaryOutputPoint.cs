using PIESTransientTemperature.BoundaryConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.Oputput
{
    public class BoundaryOutputPoint
    {
        public int areaIndex { get; set; }
        public int segmentIndex { get; set; }
        public double parametricPosition { get; set; }
        public BoundaryConditionType boundaryConditionType { get; set; }
    }
}
