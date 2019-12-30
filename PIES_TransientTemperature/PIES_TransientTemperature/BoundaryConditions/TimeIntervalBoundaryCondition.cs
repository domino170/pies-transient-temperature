using Jace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.BoundaryConditions
{
    public class TimeIntervalBoundaryCondition
    {
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public Func<Dictionary<string, double>, double> Function { get; set; }

        public TimeIntervalBoundaryCondition(double startTime, double endTime, string function)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
            var engine = new CalculationEngine();
            this.Function = engine.Build(function);
        }
    }
}
