using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
	public class User
	{
		private static int idGen;

		public int Id { get; private set; }
		public List<Bridge> Bridges;

		public bool IsThirsty
		{
			get
			{
				return Bridges.All(b => b.IsBlocked);
			}
		}

		public User()
		{
			Id = idGen++;
			Bridges = new List<Bridge>();
		}

        public override string ToString()
        {
            return Id.ToString();
        }
	}
}
