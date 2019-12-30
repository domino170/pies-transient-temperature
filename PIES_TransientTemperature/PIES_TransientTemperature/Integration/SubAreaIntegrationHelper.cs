using PIESTransientTemperature.Integration;
using PIESTransientTemperature.Objects;
using PIESTransientTemperature.ShapeFunctions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.Integration
{
    public class SubAreaIntegrationHelper
    {
        public int NumberOfSubSurfaces { get; set; }

        private List<BezieSurface> SurfaceShapes { get; set; }

        public List<RealPoint[]> corners { get; set; }

        public SubAreaIntegrationHelper(int NumberOfSubSurfaces, List<ISurfaceShape> surfaceShapes)
        {
            this.NumberOfSubSurfaces = NumberOfSubSurfaces;
            this.SurfaceShapes = new List<BezieSurface>();
            this.SurfaceShapes.AddRange(surfaceShapes.Select(x => (BezieSurface)x));
            this.MapCorners();
        }

        public SurfaceIntegrationPoint TransformIntegrationPoint(int surfaceIndex, int subAreaIndex, int boundarySegmentIndex, SurfaceIntegrationPoint surfaceIntegrationPoint, RealPoint singularPoint)
        {
            var cornerOne = this.CornerOnePoint(surfaceIndex, subAreaIndex, boundarySegmentIndex);
            var cornerTwo = this.CornerTwoPoint(surfaceIndex, subAreaIndex, boundarySegmentIndex);

            double w = this.TransformPositionForCollocationPoint(surfaceIntegrationPoint, singularPoint.x, cornerOne.x, cornerTwo.x, singularPoint.x);
            double v = this.TransformPositionForCollocationPoint(surfaceIntegrationPoint, singularPoint.y, cornerOne.y, cornerTwo.y, singularPoint.y);
            double J = this.TransformJacobian(surfaceIntegrationPoint, singularPoint, cornerOne, cornerTwo);
            
            var newSurfaceIntegrationPoint = new SurfaceIntegrationPoint(new ParametricPoint(v, w), surfaceIntegrationPoint.QuadratureValue, this.SurfaceShapes[surfaceIndex]);
            newSurfaceIntegrationPoint.Jacobian *= J;

            return newSurfaceIntegrationPoint;
        }

        private double TransformPositionForCollocationPoint(SurfaceIntegrationPoint surfaceIntegrationPoint, double _v1, double _v2, double _v3, double _v4)
        {
            double L1, L2, L3, L4;
            double v_tmp;

            L1 = 0.25 * (1 - surfaceIntegrationPoint.QuadraturePointPosition.v) * (1 - surfaceIntegrationPoint.QuadraturePointPosition.w);
            L2 = 0.25 * (1 + surfaceIntegrationPoint.QuadraturePointPosition.v) * (1 - surfaceIntegrationPoint.QuadraturePointPosition.w);
            L3 = 0.25 * (1 + surfaceIntegrationPoint.QuadraturePointPosition.v) * (1 + surfaceIntegrationPoint.QuadraturePointPosition.w);
            L4 = 0.25 * (1 - surfaceIntegrationPoint.QuadraturePointPosition.v) * (1 + surfaceIntegrationPoint.QuadraturePointPosition.w);

            v_tmp = L1 * _v1 + L2 * _v2 + L3 * _v3 + L4 * _v4;

            return v_tmp;
        }

        private double TransformJacobian(SurfaceIntegrationPoint surfaceIntegrationPoint, RealPoint singularPoint, RealPoint corner1, RealPoint corner2)
        {
            double dL1_dfi1, dL2_dfi1, dL3_dfi1, dL4_dfi1;
            double dL1_dfi2, dL2_dfi2, dL3_dfi2, dL4_dfi2;
            double dv_dfi1, dw_dfi1, dv_dfi2, dw_dfi2;
            double Jacobian_tmp;

            dL1_dfi1 = -0.25 * (1 - surfaceIntegrationPoint.QuadraturePointPosition.w);
            dL2_dfi1 = 0.25 * (1 - surfaceIntegrationPoint.QuadraturePointPosition.w);
            dL3_dfi1 = 0.25 * (1 + surfaceIntegrationPoint.QuadraturePointPosition.w);
            dL4_dfi1 = -0.25 * (1 + surfaceIntegrationPoint.QuadraturePointPosition.w);

            dL1_dfi2 = -0.25 * (1 - surfaceIntegrationPoint.QuadraturePointPosition.v);
            dL2_dfi2 = -0.25 * (1 + surfaceIntegrationPoint.QuadraturePointPosition.v);
            dL3_dfi2 = 0.25 * (1 + surfaceIntegrationPoint.QuadraturePointPosition.v);
            dL4_dfi2 = 0.25 * (1 - surfaceIntegrationPoint.QuadraturePointPosition.v);

            dv_dfi1 = dL1_dfi1 * singularPoint.x + dL2_dfi1 * corner1.x + dL3_dfi1 * corner2.x + dL4_dfi1 * singularPoint.x;
            dw_dfi1 = dL1_dfi1 * singularPoint.y + dL2_dfi1 * corner1.y + dL3_dfi1 * corner2.y + dL4_dfi1 * singularPoint.y;
            dv_dfi2 = dL1_dfi2 * singularPoint.x + dL2_dfi2 * corner1.x + dL3_dfi2 * corner2.x + dL4_dfi2 * singularPoint.x;
            dw_dfi2 = dL1_dfi2 * singularPoint.y + dL2_dfi2 * corner1.y + dL3_dfi2 * corner2.y + dL4_dfi2 * singularPoint.y;

            Jacobian_tmp = dv_dfi1 * dw_dfi2 - dw_dfi1 * dv_dfi2;

            return Jacobian_tmp;
        }

        private RealPoint CornerOnePoint(int surfaceIndex, int subAreaIndex, int boundarySegmentIndex)
        {
            var cornerIndex = 0;
            if (NumberOfSubSurfaces == 3)
            {
                if (subAreaIndex == 0)
                {
                    cornerIndex = (2 + boundarySegmentIndex) % 4;
                }
                else if (subAreaIndex == 1)
                {
                    cornerIndex = (1 + boundarySegmentIndex) % 4;
                }
                else
                {
                    cornerIndex = (3 + boundarySegmentIndex) % 4;
                }
            }
            if (NumberOfSubSurfaces == 4)
            {
                if (subAreaIndex == 0)
                {
                    cornerIndex = 2;
                }
                else if (subAreaIndex == 1)
                {
                    cornerIndex = 1;
                }
                else if (subAreaIndex == 2)
                {
                    cornerIndex = 0;
                }
                else
                {
                    cornerIndex = 3;
                }
            }
            return this.corners[surfaceIndex][cornerIndex];
        }

        private RealPoint CornerTwoPoint(int surfaceIndex, int subAreaIndex, int boundarySegmentIndex)
        {
            var cornerIndex = 0;
            if (NumberOfSubSurfaces == 3)
            {
                if (subAreaIndex == 0)
                {
                    cornerIndex = (3 + boundarySegmentIndex) % 4;
                }
                else if (subAreaIndex == 1)
                {
                    cornerIndex = (2 + boundarySegmentIndex) % 4;
                }
                else
                {
                    cornerIndex = (0 + boundarySegmentIndex) % 4;
                }
            }
            if (NumberOfSubSurfaces == 4)
            {
                if (subAreaIndex == 0)
                {
                    cornerIndex = 3;
                }
                else if (subAreaIndex == 1)
                {
                    cornerIndex = 2;
                }
                else if (subAreaIndex == 2)
                {
                    cornerIndex = 1;
                }
                else
                {
                    cornerIndex = 0;
                }
            }
            return this.corners[surfaceIndex][cornerIndex];
        }

        private void MapCorners()
        {
            this.corners = new List<RealPoint[]>();
            foreach (var surface in this.SurfaceShapes)
            {
                var surfaceCorners = new RealPoint[4];
                if (surface.SurfaceDegree == 2)
                {
                    surfaceCorners[0] = surface.V[0, 0];
                    surfaceCorners[1] = surface.V[0, 1];
                    surfaceCorners[2] = surface.V[1, 1];
                    surfaceCorners[3] = surface.V[1, 0];
                }
                else if (surface.SurfaceDegree == 4)
                {
                    surfaceCorners[0] = surface.V[0, 0];
                    surfaceCorners[1] = surface.V[0, 3];
                    surfaceCorners[2] = surface.V[3, 3];
                    surfaceCorners[3] = surface.V[3, 0];
                }
                this.corners.Add(surfaceCorners);
            }
        }
    }
}
