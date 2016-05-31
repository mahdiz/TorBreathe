using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorBricks
{
    public class BridgeComparer : IEqualityComparer<Bridge>
    {
        public bool Equals(Bridge a, Bridge b)
        {
            return a.Id == b.Id;
        }

        public int GetHashCode(Bridge b)
        {
            return b.Id;
        }
    }
}
