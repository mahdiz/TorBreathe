using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretSharing
{
    public class ShareDetails
    {
        public IList<Zp> RandomPolynomial { get; set; }

        public IList<Zp> CreatedShares { get; set; }

        public ShareDetails(IList<Zp> randomPolynomial, IList<Zp> createdShares)
        {
            RandomPolynomial = randomPolynomial;
            CreatedShares = createdShares;
        }
    }
}
