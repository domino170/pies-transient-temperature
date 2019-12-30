using PIESTransientTemperature.BoundaryConditions;
using PIESTransientTemperature.ApproximationSeries;
using PIESTransientTemperature.Integration;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.Objects
{
    public class Segment
    {
        public int Index { get; set; }

        public bool isConnectionBoundary { get; set; }

        public IBoundaryShape ShapeFunction { get; set; }

        public List<CollocationPoint> CollocationPoints { get; set; }

        public List<BoundaryIntegrationPoint> BoundaryIntegrationPoints { get; set; }

        public GaussQuadrature GaussQuadratureForSingularIntegral { get; set; }

        public List<BoundaryIntegrationPoint> SingularBoundaryIntegrationPointsForArea { get; set; }

        public int SingularBoundaryIntegrationPointsCount { get; set; }

        public double DistanceFromSingularPoint { get; set; }

        public double TemperatureValue { get; set; }

        public BoundaryCondition KnownBoundaryCondition { get; set; }

        public BoundaryCondition UnknownBoundaryCondition { get; set; }

        public BoundaryCondition TemperatureBoundaryCondition
        {
            get { return KnownBoundaryCondition.BoundaryConditionType == BoundaryConditionType.Temperature ? KnownBoundaryCondition : UnknownBoundaryCondition; }
        }

        public BoundaryCondition HeatFluxBoundaryCondition
        {
            get { return KnownBoundaryCondition.BoundaryConditionType == BoundaryConditionType.HeatFlux ? KnownBoundaryCondition : UnknownBoundaryCondition; }
        }

        public double Lenght { get; set; }

        public Segment(int segmentIndex,
            IBoundaryShape shapeFunction,
            int collocationPointsCount,
            CollocationPlacementType collocationPlacementType,
            double closerCollocationPointsRealDistanceToEdge,
            int regularBoundaryIntegrationPointsCount,
            int singularBoundaryIntegrationPointsCount,
            int singularBoundaryIntegrationPointsForAreaCount,
            double distanceFromSingularPoint,
            BoundaryCondition KnownBoundaryCondition,
            bool isConnectionBoundaryCondition)
        {
            this.Index = segmentIndex;
            this.ShapeFunction = shapeFunction;
            this.calculateLenght();
            this.calculateCollocationPoints(collocationPointsCount, collocationPlacementType, closerCollocationPointsRealDistanceToEdge);
            this.BoundaryIntegrationPoints = calculateBoundaryIntegrationPoints(regularBoundaryIntegrationPointsCount);
            this.SingularBoundaryIntegrationPointsForArea = calculateBoundaryIntegrationPoints(singularBoundaryIntegrationPointsForAreaCount);
            this.SingularBoundaryIntegrationPointsCount = singularBoundaryIntegrationPointsCount;
            this.GaussQuadratureForSingularIntegral = new GaussQuadrature(singularBoundaryIntegrationPointsCount);
            this.KnownBoundaryCondition = KnownBoundaryCondition;
            this.isConnectionBoundary = isConnectionBoundaryCondition;
            this.DistanceFromSingularPoint = distanceFromSingularPoint;
        }

        private void calculateCollocationPoints(int collocationPointsCount, CollocationPlacementType collocationPlacementType, double realDistanceFromEdge)
        {
            CollocationPoints = new List<CollocationPoint>();
            var parametricDistanceFromEdge = realDistanceFromEdge / this.Lenght;

            for (int index = 0; index < collocationPointsCount; index++)
            {
                var collocationPoint = new CollocationPoint(index, collocationPointsCount, collocationPlacementType, this.ShapeFunction, parametricDistanceFromEdge);
                CollocationPoints.Add(collocationPoint);
            }
        }

        private List<BoundaryIntegrationPoint> calculateBoundaryIntegrationPoints(int regularBoundaryIntegrationPointsCount)
        {
            var BoundaryIntegrationPoints = new List<BoundaryIntegrationPoint>();

            var gaussQuadrature = new GaussQuadrature(regularBoundaryIntegrationPointsCount);

            for (int index = 0; index < regularBoundaryIntegrationPointsCount; index++)
            {
                var BoundaryIntegrationPoint = new BoundaryIntegrationPoint(index, gaussQuadrature.xi[index], 0.0, 1.0, gaussQuadrature.Ai[index], this.ShapeFunction);
                BoundaryIntegrationPoints.Add(BoundaryIntegrationPoint);
            }

            return BoundaryIntegrationPoints;
        }

        private void calculateLenght()
        {
            this.Lenght = this.ShapeFunction.CalculateLenght();
        }
    }
}
