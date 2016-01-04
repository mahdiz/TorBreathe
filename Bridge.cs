using SecretSharing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
    public class BridgeJoinMessage
    {
        public readonly long Pseudonym;
        public readonly Zp IdShare;

        public BridgeJoinMessage(long pseudonym, Zp idShare)
        {
            Pseudonym = pseudonym;
            IdShare = idShare;
        }
    }

	public class Bridge : Node
	{
        public readonly long Pseudonym;
        public bool IsBlocked { get; protected set; }
        private readonly List<int> distributorIds;

        public Bridge(List<int> distributorIds)
		{
            Pseudonym = StaticRandom.Next(0, long.MaxValue);

            this.distributorIds = distributorIds;
            var distCount = distributorIds.Count;

            if (distCount == 1)
            {
                Send(distributorIds[0],
                    new BridgeJoinMessage(Pseudonym, new Zp(Simulator.Prime, Id)),
                    MessageType.BridgeJoin);
            }
            else
            {
                // Secret share my id among all distributors
                var shares = ShamirSharing.Share(new Zp(Simulator.Prime, Id),
                    distCount, Simulator.PolynomialDegree);

                Debug.Assert(shares.Count == distCount);
                Debug.Assert(Id == ShamirSharing.Reconstruct(shares,
                            Simulator.PolynomialDegree, Simulator.Prime).Value,
                            "The secret is not reconstructible! There is probably an overflow in polynomial evaluation of the sharing phase in Shamir sharing.");

                for (int i = 0; i < distCount; i++)
                    Send(distributorIds[i], new BridgeJoinMessage(Pseudonym, shares[i]), MessageType.BridgeJoin);
            }
        }

		public void Block()
		{
            //Debug.Assert(IsBlocked == false);
			IsBlocked = true;
            distributorIds.ForEach(d => Send(d, Pseudonym, MessageType.BridgeBlocked));
		}

        public override void Receive(int fromNode, object message, MessageType type)
        {
            throw new Exception("I'm not expecting any message!");
        }
    }
}
