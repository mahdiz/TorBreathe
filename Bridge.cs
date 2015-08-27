using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
	public class Bridge
	{
		private static int idGen;
		public int Id { get; private set; }
		public bool IsBlocked { get; protected set; }

		public Bridge()
		{
			Id = idGen++;
		}

		public void Block()
		{
			//Debug.WriteLineIf(IsBlocked == true, "The bridge is already blocked.");
			IsBlocked = true;
		}
	}
}
