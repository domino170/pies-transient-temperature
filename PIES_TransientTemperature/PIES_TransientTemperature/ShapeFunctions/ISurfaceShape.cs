using PIESTransientTemperature.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIESTransientTemperature.ShapeFunctions
{
    public interface ISurfaceShape
    {
        RealPoint CalculateRealPosition(ParametricPoint parametricPosition);
        double CalculateJacobian(ParametricPoint parametricPosition);
    }
}
