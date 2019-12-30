using Jace;
using PIES_TransientTemperature;
using PIES_TransientTemperature.BoundaryConditions;
using PIESTransientTemperature.ApproximationSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.BoundaryConditions
{
    public class BoundaryCondition
    {
        public BoundaryConditionType BoundaryConditionType;
        public int degree { get; set; }
        public BoundaryConditionValueType boundaryConditionValueType { get; set; }
        public bool isTimeDependent { get; set; }
        private RobinBoundaryConditionHelper robinBoundaryConditionHelper { get; set; }
        private TimeIntervalBoundaryConditionHelper timeIntervalValueBoundaryConditionHelper { get; set; }

        private Func<Dictionary<string, double>, double> BoundaryValueExpression { get; set; }
        public double[] BoundaryConditionVector
        {
            get;
            set;
        }
        public BoundaryCondition(BoundaryConditionType BoundaryConditionType, int degree, double[] boundaryVector, BoundaryConditionValueType boundaryConditionValueType)
        {
            this.BoundaryConditionType = BoundaryConditionType;
            this.degree = degree;
            this.boundaryConditionValueType = boundaryConditionValueType;
            CreateBoundaryConditionVector(degree, boundaryVector);
        }

        public BoundaryCondition(BoundaryConditionType BoundaryConditionType, int degree, string expression, BoundaryConditionValueType boundaryConditionValueType, TimeIntervalBoundaryConditionHelper timeIntervalValueBoundaryConditionHelper, RobinBoundaryConditionHelper robinBoundaryConditionHelper)
        {
            this.BoundaryConditionType = BoundaryConditionType;
            this.degree = degree;
            this.boundaryConditionValueType = boundaryConditionValueType;
            this.robinBoundaryConditionHelper = robinBoundaryConditionHelper;
            this.timeIntervalValueBoundaryConditionHelper = timeIntervalValueBoundaryConditionHelper;
            if (expression != string.Empty)
            {
                var engine = new CalculationEngine();
                this.BoundaryValueExpression = engine.Build(expression);
            }

            CreateBoundaryConditionVector(degree, new double[] { });
        }

        private void CreateBoundaryConditionVector(int degree, double[] value)
        {
            this.BoundaryConditionVector = new double[degree];
            for (int i = 0; i < degree; i++)
            {
                if (i < value.Length)
                {
                    this.BoundaryConditionVector[i] = value[i];
                }
                else
                {
                    this.BoundaryConditionVector[i] = 0.0;
                }
            }
        }

        public double CalculateValue(double Point)
        {
            double result = 0.0;
            for (int j = 0; j < degree; j++)
            {
                result += this.BoundaryConditionVector[j] * CzebyshewSeries.calculate(Point, j);
            }
            return result;
        }

        public void CalculateValueBoundaryCondition(double time)
        {
            if (this.BoundaryValueExpression != null)
            {
                var parameters = new Dictionary<string, double>();
                parameters.Add("t", time);
                var newBoundaryConditionValue = new double[] { this.BoundaryValueExpression(parameters) };
                this.CreateBoundaryConditionVector(this.degree, newBoundaryConditionValue);
            }
        }

        public void CalculateTimeIntervalValueBoundaryCondition(double time)
        {
            var newBoundaryConditionValue = this.timeIntervalValueBoundaryConditionHelper.CalculateValue(time);
            this.CreateBoundaryConditionVector(this.degree, newBoundaryConditionValue);
        }

        public void CalculateHeatFluxFromRobinBoundaryCondition(double boundaryTemperature)
        {
            var newBoundaryConditionValue = new double[] { this.robinBoundaryConditionHelper.CalculateHeatFlux(boundaryTemperature) };
            this.CreateBoundaryConditionVector(this.degree, newBoundaryConditionValue);
        }

    }
}
