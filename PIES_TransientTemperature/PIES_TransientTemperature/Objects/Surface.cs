using Jace;
using PIESTransientTemperature.Integration;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Objects
{
    public class Surface
    {
        public int Index { get; set; }
        public ISurfaceShape SurfaceShape { get; set; }
        public List<SurfaceIntegrationPoint> InitialConditionSurfaceIntegrationPoints { get; set; }
        public List<SurfaceIntegrationPoint> HeatSourceSurfaceIntegrationPoints { get; set; }
        public bool subractSurface { get; set; }
        public double initialTemperatureValue { get; set; }
        public Func<Dictionary<string, double>, double> heatSourceFunction { get; set; }
        public bool timeDependentHeatSourceFunction { get; set; }

        public Surface(
            int Index,
            ConfigurationData configurationData,
            ISurfaceShape surfaceShape,
            int initialConditionSurfaceIntegrationPointsCount_v,
            int initialConditionSurfaceIntegrationPointsCount_w,
            double initialTemperatureValue,
            string heatSourceFunction,
            int heatSourceSurfaceIntegrationPointsCount_v,
            int heatSourceSurfaceIntegrationPointsCount_w,
            bool subractSurface)
        {
            this.Index = Index;
            var engine = new CalculationEngine();
            this.SurfaceShape = surfaceShape;
            this.InitialConditionSurfaceIntegrationPoints = this.calculateRegularSurfaceIntegrationPoints(initialConditionSurfaceIntegrationPointsCount_v, initialConditionSurfaceIntegrationPointsCount_w, initialTemperatureValue, configurationData);
            this.subractSurface = subractSurface;
            this.initialTemperatureValue = initialTemperatureValue;
            if (configurationData.addHeatSource)
            {
                this.heatSourceFunction = engine.Build(heatSourceFunction);
                this.HeatSourceSurfaceIntegrationPoints = this.calculateRegularSurfaceIntegrationPoints(heatSourceSurfaceIntegrationPointsCount_v, heatSourceSurfaceIntegrationPointsCount_w, 0, configurationData);
            }
        }

        private List<SurfaceIntegrationPoint> calculateRegularSurfaceIntegrationPoints(int regularSurfaceIntegrationPointsCount_v, int regularSurfaceIntegrationPointsCount_w, double temperatureValue, ConfigurationData configurationData)
        {
            var surfaceIntegrationPoints = new List<SurfaceIntegrationPoint>();
            //var diffusionCoefficientTimeValue = configurationData.GetDiffusionCoefficient(temperatureValue);

            var gaussQuadrature_v = new GaussQuadrature(regularSurfaceIntegrationPointsCount_v);
            var gaussQuadrature_w = new GaussQuadrature(regularSurfaceIntegrationPointsCount_w);

            for (int indexv = 0; indexv < regularSurfaceIntegrationPointsCount_v; indexv++)
            {
                for (int indexw = 0; indexw < regularSurfaceIntegrationPointsCount_w; indexw++)
                {
                    var SurfaceIntegrationPoint = new SurfaceIntegrationPoint(new int[] { indexv, indexw }, new ParametricPoint(gaussQuadrature_v.xi[indexv], gaussQuadrature_w.xi[indexw]), 0.0, 1.0, new ParametricPoint(gaussQuadrature_v.Ai[indexv], gaussQuadrature_w.Ai[indexw]), temperatureValue, this.SurfaceShape);
                    //SurfaceIntegrationPoint.diffusionCoefficientTimeValue = diffusionCoefficientTimeValue;
                    surfaceIntegrationPoints.Add(SurfaceIntegrationPoint);
                }
            }

            return surfaceIntegrationPoints;
        }
    }
}
