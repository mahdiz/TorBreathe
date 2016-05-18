using SecretSharing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricks
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
        private List<long> distributorPseudonyms;

        public User(List<long> distributorPseudonyms)
        {
            this.distributorPseudonyms = distributorPseudonyms;
            this.distributorPseudonyms.ForEach(d => SendEmail(d, null, MessageType.UserJoin));
        }

        public void Leave()
        {
            // Let all distributors know that I have leaved the protocol
            distributorPseudonyms.ForEach(d => SendEmail(d, Pseudonym, MessageType.UserLeave));
        }

        //public override void Receive<BrideAssignment>(int fromNode, BrideAssignment message, MessageType type)
        //{
        //    throw new NotImplementedException();
        //}

        public override void ReceiveEmail(long fromPseudonym, object message, MessageType type)
        {
            if (type == MessageType.UserAssignments)   // From a distributor: Here's a list of bridge id share for you.
            {
                var assignments = message as List<UserAssignment>;
                if (distributorPseudonyms.Count == 1)
                {
                    foreach (var a in assignments)
                        Bridges.Add(Simulator.GetNode<Bridge>((int)a.BridgeShare.Value));
                }
                else
                {
                    foreach (var a in assignments)
                    {
                        if (!BridgeShares.ContainsKey(a.BridgePseudonym))
                            BridgeShares[a.BridgePseudonym] = new List<Zp>();

                        BridgeShares[a.BridgePseudonym].Add(a.BridgeShare);

                        if (BridgeShares[a.BridgePseudonym].Count == distributorPseudonyms.Count)
                        {
                            // We have enough number of shares to reconstruct the bridge ID
                            int bridgeId = (int)ShamirSharing.Reconstruct(BridgeShares[a.BridgePseudonym],
                                Simulator.PolynomialDegree, Simulator.Prime).Value;

                            Debug.Assert(Simulator.GetNodes<Bridge>().Any(b => b.Id == bridgeId), "Invalid bridge ID reconstructed from shares.");
                            Bridges.Add(Simulator.GetNode<Bridge>(bridgeId));
                            BridgeShares[a.BridgePseudonym] = null;
                        }
                    }
                }
            }
            else throw new Exception("Invalid message received.");
        }

        public override void Receive(int fromNode, object message, MessageType type)
        {
            throw new Exception("Invalid email received.");
        }
    }
}
