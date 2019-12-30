using PIES_TransientTemperature.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature
{
    public class Modeling
    {
        private Problem Problem { get; set; }

        private Size PictureBoxSize { get; set; }

        public Modeling(Problem Problem, Size PictureBoxSize)
        {
            this.Problem = Problem;
            this.PictureBoxSize = PictureBoxSize;
        }

        public void Draw(Graphics graphics)
        {
            graphics.Clear(Color.White);

            double longestSegment = 0;
            float startPoint = PictureBoxSize.Height / 2;

            foreach (var area in this.Problem.Areas)
            {
                foreach (var segment in area.Segments)
                {
                    if (segment.Lenght > longestSegment)
                    {
                        longestSegment = segment.Lenght;
                    }
                }
            }

            var scale = (float)(420.0 / (longestSegment * 4));

            DrawCoordinateSystem(graphics);

            DrawBoundary(graphics, startPoint, scale);

            DrawSurface(graphics, startPoint, scale);

            DrawCollocationPoint(graphics, startPoint, scale);
        }

        private void DrawCoordinateSystem(Graphics graphics)
        {
            Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0));
            graphics.DrawLine(pen, new Point(PictureBoxSize.Width / 2, PictureBoxSize.Height), new Point(PictureBoxSize.Width / 2, 0));
            graphics.DrawLine(pen, new Point(PictureBoxSize.Width, PictureBoxSize.Height / 2), new Point(0, PictureBoxSize.Height / 2));
        }

        private void DrawBoundary(Graphics graphics, float startPoint, float scale)
        {
            graphics.ScaleTransform(1.0F, -1.0F);
            graphics.TranslateTransform(0.0F, -(float)PictureBoxSize.Height);
            Pen pen2 = new Pen(Color.FromArgb(255, 0, 0, 0), 2);
            foreach (var area in this.Problem.Areas)
            {
                foreach (var segment in area.Segments)
                {
                    var points = segment.ShapeFunction.GetPoints();
                    if (points.Count == 2)
                    {
                        graphics.DrawLine(pen2,
                            scale * (float)points[0].x + startPoint, scale * (float)points[0].y + startPoint,
                            scale * (float)points[1].x + startPoint, scale * (float)points[1].y + startPoint);
                    }
                    else if (points.Count == 4)
                    {
                        graphics.DrawBezier(pen2,
                            scale * (float)points[0].x + startPoint, scale * (float)points[0].y + startPoint,
                            scale * (float)points[1].x + startPoint, scale * (float)points[1].y + startPoint,
                            scale * (float)points[2].x + startPoint, scale * (float)points[2].y + startPoint,
                            scale * (float)points[3].x + startPoint, scale * (float)points[3].y + startPoint);
                    }
                }
            }
        }

        private void DrawSurface(Graphics graphics, float startPoint, float scale)
        {
            graphics.ScaleTransform(1.0F, -1.0F);
            graphics.TranslateTransform(0.0F, -(float)PictureBoxSize.Height);

            SolidBrush brush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            foreach (var area in this.Problem.Areas)
            {
                foreach (var surface in area.Surfaces)
                {
                    foreach (var integrationPoint in surface.InitialConditionSurfaceIntegrationPoints)
                    {
                        drawPoint(graphics, brush, new Size(1, 1), (int)((scale * integrationPoint.RealPosition.x) + startPoint + 2), (int)((scale * integrationPoint.RealPosition.y) + startPoint - 1));
                    }
                }
            }
        }

        private void DrawCollocationPoint(Graphics graphics, float startPoint, float scale)
        {
            //graphics.ScaleTransform(1.0F, -1.0F);
            //graphics.TranslateTransform(0.0F, -(float)PictureBoxSize.Height);

            SolidBrush brush = new SolidBrush(Color.Red);
            foreach (var area in this.Problem.Areas)
            {
                foreach (var segment in area.Segments)
                {
                    foreach (var collocationPoint in segment.CollocationPoints)
                    {
                        drawPoint(graphics, brush, new Size(4, 4), (int)((scale * collocationPoint.RealPosition.x) + startPoint), (int)((scale * collocationPoint.RealPosition.y) + startPoint));
                    }
                }
            }
        }
        private void drawPoint(Graphics g, SolidBrush brush, Size size, int x, int y)
        {
            Point dPoint = new Point(x, (PictureBoxSize.Height - y));
            dPoint.X = dPoint.X - 2;
            dPoint.Y = dPoint.Y - 2;
            Rectangle rect = new Rectangle(dPoint, size);
            g.FillRectangle(brush, rect);
        }
    }
}
