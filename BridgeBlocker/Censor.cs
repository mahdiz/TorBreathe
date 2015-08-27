using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
	public enum AttackModel
	{
		Aggressive,
		Conservative,
		Burst,
	}

	internal class CorruptUser : User
	{
	}

    internal class Censor
    {
		private Distributor D;
		private List<CorruptUser> CorruptUsers;
		public AttackModel AttackModel;

		public Censor(Distributor D, AttackModel attackModel)
		{
			this.D = D;
			AttackModel = attackModel;
			D.OnYield += OnDistRest;
			CorruptUsers = new List<CorruptUser>();
		}

		public void AddCorruptUsers(int n)
		{
			for (int i = 0; i < n; i++)
			{
				var u = new CorruptUser();
				CorruptUsers.Add(u);
				D.Join(u);
			}
		}

		private void OnDistRest()
		{
			switch (AttackModel)
			{
				case AttackModel.Aggressive:
					// block all bridges assigned to the corrupt users
					CorruptUsers.ForEach(u => u.Bridges.ForEach(b => b.Block()));
					break;

				case AttackModel.Conservative:
					break;

				case AttackModel.Burst:
					break;
			}
		}
    }
}
