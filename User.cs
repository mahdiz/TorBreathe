using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorBricks
{
	public class User
	{
		private static int idGen;

		public int Id { get; private set; }
		public HashSet<Bridge> Bridges;

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
			Bridges = new HashSet<Bridge>(new BridgeComparer());
		}

        public override string ToString()
        {
            return Id.ToString();
        }

        public static void Reset()
        {
            idGen = 0;
        }
	}
}
