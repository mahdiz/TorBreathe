using SecretSharing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricks
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
        public bool IsBlocked { get; protected set; }
        private readonly List<long> distPseudonyms;

        public Bridge(List<long> distPseudonyms)
		{
            this.distPseudonyms = distPseudonyms;
        }

		public void Block()
		{
            //Debug.Assert(IsBlocked == false);
			IsBlocked = true;
            distPseudonyms.ForEach(d => SendEmail(d, null, MessageType.BridgeBlocked));
		}

        public override void Receive(int fromNode, object message, MessageType type)
        {
            throw new Exception("Invalid message received.");
        }

        public override void ReceiveEmail(long fromPseudonym, object message, MessageType type)
        {
            if (type == MessageType.JoinRequest)
            {
                // Join the protocol.
                var distCount = distPseudonyms.Count;
                if (distCount == 1)
                {
                    SendEmail(distPseudonyms[0],
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
                        SendEmail(distPseudonyms[i], new BridgeJoinMessage(Pseudonym, shares[i]), MessageType.BridgeJoin);
                }
            }
            else throw new Exception("Invalid email received.");
        }
    }
}
