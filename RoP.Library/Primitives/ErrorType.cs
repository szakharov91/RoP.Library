using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoP.Library.Primitives;

public enum ErrorType
{
    None = -1,
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4
}
