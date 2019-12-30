using Jace;
using PIES_TransientTemperature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature
{
    public class IterationProcess
    {
     
        public double TimeStep;
        public int NumberOfIterations;
        public int CurrentIteration { get; set; }
        public double CurrentTime{ get; set;}
        public bool isCombinedProblem { get; set; }

        public IterationProcess(double TimeStep, int NumberOfIterations, bool isCombinedProblem)
        {
            this.isCombinedProblem = isCombinedProblem;
            var engine = new CalculationEngine();
            
            this.TimeStep = TimeStep;
            this.NumberOfIterations = NumberOfIterations;
        }

        public void CalculateCurrentTime(int iteration)
        {
            this.CurrentIteration = iteration;
            this.CurrentTime = (double)Decimal.Multiply((decimal)iteration, (decimal)this.TimeStep);
        }
    }
}
