using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIES_TransientTemperature.BoundaryConditions
{
    public enum BoundaryConditionValueType
    {
        Value,
        TimeIntervalValue,
        TimeDependentFunction,
        ConvectionRadiation,
    }
}
