using Jace;
using PIES_TransientTemperature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature
{
    public class ConfigurationData
    {
        private Func<Dictionary<string, double>, double> DiffusionCoefficientEquation;
        private double DiffusionCoefficientConstantValue;
        private bool timeDependentDiffusionCoefficient;
        private Func<Dictionary<string, double>, double> ThermalConductivityEquation;
        private double ThermalConductivityConstantValue;
        private bool timeDependentThermalConductivity;
        public IterationProcess iterationProcess { get; set; }
        public bool addHeatSource;
        public bool isHeatSourceTimeDependent;

        public ConfigurationData(string DiffusionCoefficient, bool timeDependentDiffusionCoefficient, string ThermalConductivity, bool timeDependentThermalConductivity, IterationProcess iterationProcess)
        {
            var engine = new CalculationEngine();
            this.DiffusionCoefficientEquation = engine.Build(DiffusionCoefficient);
            this.ThermalConductivityEquation = engine.Build(ThermalConductivity); ;
            this.timeDependentDiffusionCoefficient = timeDependentDiffusionCoefficient;
            this.timeDependentThermalConductivity = timeDependentThermalConductivity;
            if (!timeDependentDiffusionCoefficient)
            {
                this.DiffusionCoefficientConstantValue = double.Parse(DiffusionCoefficient);
            }
            if (!timeDependentThermalConductivity)
            {
                this.ThermalConductivityConstantValue = double.Parse(ThermalConductivity);
            }
            this.iterationProcess = iterationProcess;
        }

        public double GetDiffusionCoefficient()
        {
            return this.DiffusionCoefficientConstantValue;
        }

        public double GetDiffusionCoefficient(double temperature)
        {
            var parameters = new Dictionary<string, double>();
            parameters.Add("T", temperature);
            var value = this.DiffusionCoefficientEquation(parameters);
            return value;
        }

        public double GetThermalConductivity()
        {
            return this.ThermalConductivityConstantValue;
        }

        public double GetThermalConductivity(double temperature)
        {
            var parameters = new Dictionary<string, double>();
            parameters.Add("T", temperature);
            var value = this.ThermalConductivityEquation(parameters);
            return value;
        }

        public bool arePropertiesTimeDependent()
        {
            return this.timeDependentDiffusionCoefficient || this.timeDependentThermalConductivity;
        }
    }
}
