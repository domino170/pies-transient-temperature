using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.BoundaryConditions
{
    public class TimeIntervalBoundaryConditionHelper
    {
        public List<TimeIntervalBoundaryCondition> timeIntervals { get; set; }

        public TimeIntervalBoundaryConditionHelper(List<TimeIntervalBoundaryCondition> timeIntervals)
        {
            this.timeIntervals = timeIntervals;
        }

        public double[] CalculateValue(double time)
        {
            var parameters = new Dictionary<string, double>();
            parameters.Add("t", time);

            foreach (var interval in this.timeIntervals)
            {
                if (time >= interval.StartTime && time <= interval.EndTime)
                {
                    return new double[] { interval.Function(parameters) };
                }
            }
            return new double[] { 0.0 };
        }
    }
}
