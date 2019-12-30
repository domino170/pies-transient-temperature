using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.BoundaryConditions
{
    public class RobinBoundaryConditionHelper
    {
        private double fluidTemperature { get; set; }
        private double heatTransferCoefficient { get; set; }
        private double emissivityCoefficient { get; set; }
        private double outherBoundaryDiameter { get; set; }
        private double fluidThermalConductivity { get; set; }
        private double fluidViscosity { get; set; }
        private double fluidDensity { get; set; }
        private double fluidSpecificHeat { get; set; }
        private double boltzmanConstant = 0.00000005670373;
        private double gravity = 9.81;
        private double BExpansion;
        private double Pr;

        public RobinBoundaryConditionHelper(double fluidTemperature, double heatTransferCoefficient)
        {
            this.heatTransferCoefficient = heatTransferCoefficient;
            this.fluidTemperature = fluidTemperature;
        }

        public RobinBoundaryConditionHelper(double outherBoundaryDiameter, double fluidTemperature, double fluidThermalConductivity, double fluidViscosity, double fluidDensity, double fluidSpecificHeat, double emissivityCoefficient)
        {
            this.heatTransferCoefficient = 0.0;
            this.fluidTemperature = fluidTemperature;
            this.emissivityCoefficient = emissivityCoefficient;
            this.outherBoundaryDiameter = outherBoundaryDiameter;
            this.fluidThermalConductivity = fluidThermalConductivity;
            this.fluidViscosity = fluidViscosity;
            this.fluidDensity = fluidDensity;
            this.fluidSpecificHeat = fluidSpecificHeat;
            BExpansion = 1 / (fluidTemperature + 273.15);
            Pr = (fluidViscosity * fluidSpecificHeat) / fluidThermalConductivity;
        }

        public double CalculateHeatFlux(double boundaryTemperature)
        {
            if (this.heatTransferCoefficient != 0)
            {
                return this.heatTransferCoefficient * (this.fluidTemperature - boundaryTemperature);
            }
            return this.CalculateHeatFluxForCircleShape(boundaryTemperature);
        }

        private double CalculateHeatFluxForCircleShape(double boundaryTemperature)
        {
            if (boundaryTemperature == fluidTemperature) return 0.0;

            double Gr = (BExpansion * Math.Pow(fluidDensity, 2.0) * gravity * Math.Abs(boundaryTemperature - fluidTemperature) * Math.Pow(outherBoundaryDiameter, 3)) / Math.Pow(fluidViscosity, 2.0);
            double Ra = Gr * Pr;
            double Nu = Math.Pow(0.6 + (0.387 * Math.Pow(Ra, 1.0 / 6.0) / Math.Pow((1.0 + Math.Pow(0.559 / Pr, 9.0 / 16.0)), 8.0 / 27.0)), 2.0);

            double hc = Nu * fluidThermalConductivity / outherBoundaryDiameter;

            double hr = emissivityCoefficient * boltzmanConstant * (Math.Pow(boundaryTemperature, 4) - Math.Pow(fluidTemperature, 4)) / (boundaryTemperature - fluidTemperature);

            double q = (hc + hr) * (this.fluidTemperature - boundaryTemperature);

            return q;
        }

    }
}
