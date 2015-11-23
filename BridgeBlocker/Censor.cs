using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
	public enum AttackModel
	{
		Aggressive,
        Prudent,
	}

	internal class CorruptUser : User
	{
	}

    internal class Censor
    {
        public AttackModel AttackModel;
        //public int MemorizedCount { get { return MemorizedBridges.Count; } }
        public int MemorizedCount
        {
            get
            {
                int mem = 0;
                CorruptUsers.ForEach(u => mem += u.Bridges.Count(b => !b.IsBlocked));
                return mem;
            }
        }

		private Distributor d;
		private List<CorruptUser> CorruptUsers = new List<CorruptUser>();
        //private HashSet<Bridge> MemorizedBridges = new HashSet<Bridge>(new BridgeComparer());

		public Censor(Distributor d, AttackModel attackModel)
		{
			this.d = d;
			AttackModel = attackModel;
            d.OnYield += OnDistYield;
		}

		public void AddCorruptUsers(int n)
		{
			for (int i = 0; i < n; i++)
			{
				var u = new CorruptUser();
				CorruptUsers.Add(u);
				d.Join(u);
			}
		}

        private void OnDistYield(int threshold)
		{
			switch (AttackModel)
			{
				case AttackModel.Aggressive:
					// block all bridges learned by the corrupt users
					CorruptUsers.ForEach(
                        delegate(CorruptUser u)
                        {
                            foreach (var b in u.Bridges)
                                b.Block();
                        });
					break;

				case AttackModel.Prudent:
                    PrudentBlocking(threshold);
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
        private void PrudentBlocking(int threshold)
        {
            int numBlocked = 0;
            foreach (var u in CorruptUsers)
            {
                foreach (var bridge in u.Bridges)
                {
                    if (!bridge.IsBlocked)
                    {
                        if (numBlocked < threshold)
                        {
                            bridge.Block();
                            numBlocked++;

                            // Since the distributor reuses unblocked bridge, the just-blocked bridge
                            // may be among previously-memorized bridges. If so, just remove it from mem list.
                            //MemorizedBridges.Remove(bridge);
                        }
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
