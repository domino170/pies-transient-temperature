using PIESTransientTemperature.Helpers;
using PIESTransientTemperature.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIESTransientTemperature.ShapeFunctions
{
    interface IShapeFunction
    {
        Point CalculateRealPosition(double parametricPosition);
        Vector CalculateNormalVector(double parametricPosition);
        double CalculateJacobian(double parametricPosition);
        double CalculateLenght();
    }
}
