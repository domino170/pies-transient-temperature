using PIESTransientTemperature.Objects;
using System;
using System.Collections.Generic;
using PIESTransientTemperature.MatrixCalculations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.ShapeFunctions
{
    public interface IBoundaryShape
    {
        RealPoint GetV0();
        RealPoint GetV1();
        RealPoint CalculateRealPosition(double parametricPosition);
        Vector CalculateNormalVector(double parametricPosition);
        double CalculateJacobian(double parametricPosition);
        double CalculateLenght();
        List<RealPoint> GetPoints();
    }
}
