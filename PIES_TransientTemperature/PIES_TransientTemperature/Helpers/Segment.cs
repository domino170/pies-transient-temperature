using PIESTransientTemperature.CollocationPoints;
using PIESTransientTemperature.Helpers;
using PIESTransientTemperature.CollocationPoints;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Helpers
{
    class Segment
    {
        public int Index { get; set; }

        public IShapeFunction ShapeFunction { get; set; }

        public List<CollocationPoint> CollocationPoints { get; set; }

        public List<IntegrationPoint> IntegrationPoints { get; set; }

        public GaussQuadrature GaussQuadratureForSingularIntegral { get; set; }

        public int SingularIntegrationPointsCount { get; set; }

        public double Lenght { get; set; }

        public Segment(int segmentIndex, IShapeFunction shapeFunction, int collocationPointsCount, CollocationPlacementType collocationPlacementType, int regularIntegrationPointsCount, int singularIntegrationPointsCount)
        {
            this.Index = segmentIndex;
            this.ShapeFunction = shapeFunction;
            this.calculateLenght();
            this.calculateCollocationPoints(collocationPointsCount, collocationPlacementType);
            this.calculateRegularIntegrationPoints(regularIntegrationPointsCount);
            this.SingularIntegrationPointsCount = singularIntegrationPointsCount;
            this.GaussQuadratureForSingularIntegral = new GaussQuadrature(singularIntegrationPointsCount);
        }

        private void calculateCollocationPoints(int collocationPointsCount, CollocationPlacementType collocationPlacementType)
        {
            CollocationPoints = new List<CollocationPoint>();
            for (int index = 0; index < collocationPointsCount; index++)
            {
                var collocationPoint = new CollocationPoint(index, collocationPointsCount, collocationPlacementType, this.ShapeFunction);
                CollocationPoints.Add(collocationPoint);
            }
        }

        private void calculateRegularIntegrationPoints(int regularIntegrationPointsCount)
        {
            IntegrationPoints = new List<IntegrationPoint>();

            var gaussQuadrature = new GaussQuadrature(regularIntegrationPointsCount);

            for (int index = 0; index < regularIntegrationPointsCount; index++)
            {
                var integrationPoint = new IntegrationPoint(index, gaussQuadrature.xi[index], 0.0, 1.0, gaussQuadrature.Ai[index], this.ShapeFunction);
                IntegrationPoints.Add(integrationPoint);
            }
        }

        private void calculateLenght()
        {
            var _shapeFunction = (BezieCurve)this.ShapeFunction;
            this.Lenght = _shapeFunction.CalculateLenght();
        }
    }
}
