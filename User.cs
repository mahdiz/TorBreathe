using SecretSharing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
	public class User : Node
	{
        public List<Bridge> Bridges = new List<Bridge>();
        //{
        //    get
        //    {
        //        var bridges = new List<Bridge>();
        //        foreach (var bi in BridgeShares)
        //            bridges.Add(Simulator.Bridges[bi]);

        //        return bridges;
        //    }
        //}

        public bool IsThirsty
        {
            get { return Bridges.All(b => b.IsBlocked); }
        }

        private Dictionary<long, List<Zp>> BridgeShares = new Dictionary<long, List<Zp>>();
        private List<int> distributorIds;

        public User(List<int> distributorIds)
        {
            this.distributorIds = distributorIds;
            this.distributorIds.ForEach(d => Send(d, Id, MessageType.UserJoin));
        }

        public void Leave()
        {
            // Let all distributors know that I have leaved the protocol
            distributorIds.ForEach(d => Send(d, Id, MessageType.UserLeave));
        }

        //public override void Receive<BrideAssignment>(int fromNode, BrideAssignment message, MessageType type)
        //{
        //    throw new NotImplementedException();
        //}

        public override void Receive(int fromNode, object message, MessageType type)
        {
            if (type == MessageType.BridgeAssign)   // From a distributor: Here's a bridge id share for you.
            {
                int bridgeId = -1;
                var ba = message as BridgeAssignMessage;

                if (distributorIds.Count == 1)
                    bridgeId = (int)ba.BridgeShare.Value;
                else
                {
                    if (!BridgeShares.ContainsKey(ba.BridgePseudonym))
                        BridgeShares[ba.BridgePseudonym] = new List<Zp>();

                    BridgeShares[ba.BridgePseudonym].Add(ba.BridgeShare);

                    if (BridgeShares[ba.BridgePseudonym].Count == distributorIds.Count)
                    {
                        bridgeId = (int)ShamirSharing.Reconstruct(BridgeShares[ba.BridgePseudonym],
                            Simulator.PolynomialDegree, Simulator.Prime).Value;

                        Debug.Assert(Simulator.GetNodes<Bridge>().Any(b => b.Id == bridgeId), "Invalid bridge ID reconstructed from shares.");
                        BridgeShares[ba.BridgePseudonym] = null;
                    }
                }

                if (bridgeId >= 0)
                    Bridges.Add(Simulator.GetNode<Bridge>(bridgeId));
            }
            else
                throw new Exception("Invalid message received.");
        }
    }
}
