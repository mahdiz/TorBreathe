using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
	public delegate void YieldHandler();
	public delegate bool LogHandler(UserList users, int[][] userShuffles, List<Bridge> bridges, int c, int r, int round, int totalReplacedBridgeCount);		// distributor rest handler

	public class Distributor
	{
        private int c;					// clog(n) is the number of matrices
		private double alpha, beta;
		private Random randgen;
		private UserList Users { get; set; }
		public event YieldHandler OnYield;
		public event LogHandler OnIteration;
        public event LogHandler OnRound;

		public Distributor(int c, double alpha, double beta, int seed)
		{
            this.c = c; 
			this.alpha = alpha;
			this.beta = beta;
			randgen = new Random(seed);
			Users = new UserList();
		}

		public void Run()
		{
			int i = 1;
			var bridges = new List<Bridge>();

			// initialize the parameters
			int n = Users.Count;
            int d = c * (int)Math.Log(n, 2);            // # of matrices
            int w = (int)Math.Floor(alpha / beta);      // # columns (width) in each matrix
			int r = (int)Math.Ceiling((double)n / w);   // # rows in each matrix

			// recruit wd bridges
			for (int j = 0; j < w * d; j++)
				bridges.Add(new Bridge());

			// distribute them among the users
			var userShuffles = Distribute(bridges, w, r);
			var assignedBridges = new List<Bridge>(bridges);

			// yield: let the users perform their job
			if (OnYield != null)
				OnYield();

            // notify end of the first round
            if (OnRound != null)
                OnRound(Users, userShuffles, assignedBridges, w, r, i, 0);

            // notify end of the first iteration
			if (OnIteration != null)
                OnIteration(Users, userShuffles, assignedBridges, w, r, i, 0);

			int b = 0;      // total # blocked bridges so far
            bool stopped = false, stable = false;
            var roundHistory = new int[2];

			while (!stopped && !stable)
			{
				// replace blocked bridges with fresh ones
				for (int j = 0; j < bridges.Count; j++)
				{
					if (bridges[j].IsBlocked)
					{
						b++;
						bridges[j] = new Bridge();
					}
				}

                // calculate the # of blocked bridges for each user
                foreach (var user in Users)
                {
					var nb = user.Bridges.Count(e => e.IsBlocked);
                    var du = nb / (i * d);
                }

				if (b >= (d * Math.Pow(alpha, i))) 
				{
                    i++;

					// update the parameters
					n = Users.Count;
                    d = c * (int)Math.Log(n, 2);
                    w = (int)Math.Floor(Math.Pow(alpha, i) / (i * beta));   // # columns (width) in each matrix
                    r = (int)Math.Ceiling((double)n / w);                   // # rows in each matrix

					// recruit a sufficient number of unblocked bridges to have wd of them
					int bCount = bridges.Count;
					for (int j = 0; j < w * d - bCount; j++)
						bridges.Add(new Bridge());

					userShuffles = Distribute(bridges, w, r);
                    assignedBridges = new List<Bridge>(bridges);

                    // yield: let the users perform their job
                    if (OnYield != null)
                        OnYield();

                    // notify end of round
                    if (OnRound != null)
                        stopped = OnRound(Users, userShuffles, assignedBridges, w, r, i, b);
				}
                else if (OnYield != null)
                    OnYield();

                // notify end of iteration
				if (OnIteration != null)
                    stopped = OnIteration(Users, userShuffles, assignedBridges, w, r, i, b);

                // check for stability: stop after 2/(log(alpha) - 1)*log(t) + 1 iterations (see Lemma 2 in the paper)
                if (i == roundHistory[0] && i == roundHistory[1])
                    stable = true;
                else
                {
                    roundHistory[0] = roundHistory[1];
                    roundHistory[1] = i;
                }
			}
		}

		private int[][] Distribute(List<Bridge> B, int w, int r)
		{
			// create d user matrices with floor(w) columns and ceiling(n/w) rows
			int n = Users.Count;
            int d = c * (int)Math.Log(n, 2);
			var userShuffles = new int[d][];

			for (int l = 0; l < d; l++)
			{
				var u = Users.GetIds();

				Shuffle(u);
				for (int i = 0; i < r; i++)
				{
					for (int j = 0; j < w; j++)
					{
						// the user in the i-th row of the j-th column of the l-th matrix
						if (j * r + i < n)
							Users[u[j * r + i]].Bridges.Add(B[l * w + j]);
					}
				}
				userShuffles[l] = u;
			}
			return userShuffles;
		}

		public void Join(User u)
		{
			Users.Add(u);
		}

		public void Leave(User u)
		{
			Users.Remove(u);
		}

		private void Shuffle<T>(T[] array)
		{
			// Fisher-Yates algorithm
			int n = array.Length;
			for (int i = 0; i < n; i++)
			{
				int r = i + (int)(randgen.NextDouble() * (n - i));
				T a = array[r];
				array[r] = array[i];
				array[i] = a;
			}
		}
	}
}
