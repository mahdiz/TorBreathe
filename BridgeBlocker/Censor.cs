using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricks
{
	public enum AttackModel
	{
		Aggressive,
        Prudent,
        Stochastic
	}

    internal class CorruptUser : User
	{
        public CorruptUser(List<long> distributorPseudonyms)
            : base(distributorPseudonyms)
        {
        }
    }

    internal class Censor
    {
        public AttackModel AttackModel { get; set; }
        public double BlockingProbability { get; set; }
        private Random randGen;

		private List<Distributor> distributors;
		private List<CorruptUser> CorruptUsers = new List<CorruptUser>();

		public Censor(List<Distributor> distributors, AttackModel attackModel, double blockProbability, int seed)
		{
			this.distributors = distributors;
			AttackModel = attackModel;
            BlockingProbability = blockProbability;
            (distributors[0] as LeaderDistributor).OnYield += OnDistYield;
            randGen = new Random(seed);     // The seed must be different from distributor's RNG seed
		}

		public void AddCorruptUsers(int t)
		{
            var distIds = distributors.Select(d => d.Pseudonym).ToList();
            for (int i = 0; i < t; i++)
			{
				var u = new CorruptUser(distIds);
				CorruptUsers.Add(u);
			}
		}

        private void OnDistYield(int threshold, int repeatCount)
		{
			switch (AttackModel)
			{
				case AttackModel.Aggressive:
					// Block all bridges learned by the corrupt users
					CorruptUsers.ForEach(
                        delegate(CorruptUser u)
                        {
                            foreach (var b in u.Bridges)
                                b.Block();
                        });
					break;

				case AttackModel.Prudent:
                    PrudentBlocking(threshold, repeatCount);
					break;

                case AttackModel.Stochastic:
                    // For each unique bridge learned, block with the given probability.
                    var bridgeSet = new HashSet<Bridge>(CorruptUsers.SelectMany(u => u.Bridges));
                    foreach (var bridge in bridgeSet)
                        if (randGen.NextDouble() < BlockingProbability)
                            bridge.Block();
                    break;

                default:
                    throw new InvalidOperationException("Invaid attack model!");
			}
		}

        /// <summary>
        /// Block the least number of bridges, while forcing the algorithm to go to the next round.
        /// This can be done by blocking exactly "threshold" bridges.
        /// </summary>
        /// <param name="threshold">Distributor's blocking threshold for proceeding to the next round.</param>
        private void PrudentBlocking(int threshold, int repeatCount)
        {
            int numBlocked = 0;
            foreach (var u in CorruptUsers)
            {
                foreach (var bridge in u.Bridges)
                {
                    if (!bridge.IsBlocked)
                    {
                        if (numBlocked < threshold * repeatCount)
                        {
                            bridge.Block();
                            numBlocked++;

                            // Since the distributor reuses unblocked bridge, the just-blocked bridge
                            // may be among previously-memorized bridges. If so, just remove it from mem list.
                            //MemorizedBridges.Remove(bridge);
                        }
                        else return;
                        //else MemorizedBridges.Add(bridge);
                    }
                }
            }
            //Debug.Assert(!MemorizedBridges.Any(b => b.IsBlocked), "A memorized bridge is already blocked!");

            // if blocked less than enough to go to next round, block from memorized bridges
            //if (numBlocked < threshold && MemorizedBridges.Count > 0)
            //{
            //    for (int j = 0; j < threshold - numBlocked; j++)
            //    {
            //        var bridge = MemorizedBridges.First();
            //        bridge.Block();
            //        MemorizedBridges.Remove(bridge);
            //    }
            //}
        }
    }
}
