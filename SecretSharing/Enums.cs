using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretSharing
{
    public enum VectorType
    {
        Row,
        Column
    }

    public enum Operation
    {
        None,
        Add,
        Mul,
        Div,
        Sub
    }
}
