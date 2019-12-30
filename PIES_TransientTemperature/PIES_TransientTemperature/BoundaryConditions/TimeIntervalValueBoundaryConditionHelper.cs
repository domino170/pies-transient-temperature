using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.BoundaryConditions
{
    public class TimeIntervalValueBoundaryConditionHelper
    {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public double[] Value { get; set; }

        public TimeIntervalValueBoundaryConditionHelper(double startTime, double endTime, double[] value)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Value = value;
        }

        public double[] CalculateValue(double time)
        {
            if (time >= this.StartTime && time <= this.EndTime)
            {
                return this.Value;
            }
            return new double[] { 0.0 };
        }
    }
}
