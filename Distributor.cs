using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
	public delegate void YieldHandler(int threshold);
    public delegate bool RunEndHandler(int run, RunLog log);
	public delegate RoundLog RoundEndHandler(int round, UserList users, List<Bridge> bridges, out bool stop);
    public delegate void Distribute(List<Bridge> B);

    public enum DistributeMethod
    {
        BallsAndBins,
        Matrix,
    }

	public class Distributor
	{
        public DistributeMethod DistMethod { get; private set; }
		public event YieldHandler OnYield;
        public event RunEndHandler OnRunEnd;
        public event RoundEndHandler OnRoundEnd;
        public UserList Users { get; private set; }
        public int NumRuns { get; private set; }

        private Random randGen;
        private Distribute distribute;

        /// <summary>
        /// A function that calculates the round threshold for the number of bridges blocked in each round.
        /// </summary>
        private Func<int, int> threshold;

        /// <summary>
        /// A function that calculates the number of bridges to be distributed in each round.
        /// </summary>
        private Func<int, int> distCount;

		public Distributor(DistributeMethod method, int seed)
		{
            NumRuns = 1;
			randGen = new Random(seed);
			Users = new UserList();
            
            switch (DistMethod = method)
            {
                case DistributeMethod.BallsAndBins:
                    distribute = DistributeBnB;
                    threshold = delegate(int i) { return (int)Math.Pow(2, i); };
                    distCount = delegate(int i) { return (int)Math.Pow(2, i + 1); };
                    break;

                case DistributeMethod.Matrix:
                    distribute = DistributeMatrix;
                    threshold = delegate(int i) { return (int)Math.Pow(2, i); };
                    distCount = delegate(int i) { return (int)Math.Pow(2, i + 1); };
                    break;

                default:
                    throw new InvalidOperationException("Invalid distribution method!");
            }
		}

        /// <summary>
        /// Runs the bridge distribution algorithm a number of times.
        /// </summary>
        /// <param name="c">Repeat the algorithm c*Log(n) times. If c=0, then the algorithm runs once.</param>
        public void Run(int c)
        {
            NumRuns = c == 0 ? 1 : c * (int)Math.Ceiling(Math.Log(Users.Count, 2));

            var stop = false;
            for (int r = 0; r < NumRuns && !stop; r++)
            {
                var runLog = RunOnce();

                if (OnRunEnd != null)
                    stop = OnRunEnd(r, runLog);

                // Clear all bridge assignments for next run (runs must be completely independent)
                foreach (var user in Users)
                    user.Bridges.Clear();
            }
        }

        /// <summary>
        /// Runs the bridge distribution algorithm once.
        /// </summary>
        /// <returns>A log of all rounds in the run.</returns>
		private RunLog RunOnce()
		{
            var runLog = new RunLog();
			var bridges = new List<Bridge>();

            int i = 0, b = 1;
            int n = Users.Count;
            bool stopped = false, stable = false;

			while (!stopped & !stable)
			{
                if (b >= threshold(i))
                {
                    i++;        // Go to the next round

                    // Update parameters
                    n = Users.Count;
                    int m = distCount(i);       // # bridges distributed in current round

                    Debug.Assert(!bridges.Any(x => x.IsBlocked), "Distribute a blocked bridge??");

                    if (m * NumRuns < n)
                    {
                        // Recruit a sufficient number of unblocked bridges to have m of them
                        int bCount = bridges.Count;
                        for (int j = 0; j < m - bCount; j++)
                            bridges.Add(new Bridge());

                        distribute(bridges);
                    }
                    else
                    {
                        // Give a unique bridge to each user
                        int j = 0;
                        foreach (var user in Shuffle(Users))
                            user.Bridges.Add(bridges[j++]);
                    }

                    // Let the users perform their job
                    if (OnYield != null)
                        OnYield(threshold(i));

                    // Log the end of round
                    if (OnRoundEnd != null)
                        runLog.Add(OnRoundEnd(i, Users, bridges, out stopped));
                }
                else
                    stable = true;

                // Replace blocked bridges with fresh ones
                b = 0;
                for (int j = 0; j < bridges.Count; j++)
                {
                    if (bridges[j].IsBlocked)
                    {
                        b++;
                        bridges[j] = new Bridge();
                    }
                }
			}
            return runLog;
		}

        private void DistributeBnB(List<Bridge> B)
        {
            int n = Users.Count;
            int m = B.Count;

            for (int j = 0; j < n; j++)
            {
                var k = randGen.Next(0, m);
                Users[j].Bridges.Add(B[k]);
            }
        }

		private void DistributeMatrix(List<Bridge> B)
		{
			// Create a matrix with floor(m) columns and ceiling(n/m) rows
			int n = Users.Count;
            int m = B.Count;
            int r = (int)Math.Ceiling((double)n / m);

            var users = Shuffle(Users);
			for (int i = 0; i < r; i++)
			{
				for (int j = 0; j < m; j++)
				{
					// the user in the i-th row of the j-th column
					if (j * r + i < n)
						users[j * r + i].Bridges.Add(B[j]);
				}
			}
		}

		public void Join(User u)
		{
			Users.Add(u);
		}

		public void Leave(User u)
		{
			Users.Remove(u);
		}

        /// <summary>
        /// Shuffles an enumerable list uniformly at random and returns a reference to the shuffled array.
        /// </summary>
		private T[] Shuffle<T>(IEnumerable<T> list)
		{
            var array = list.ToArray();

			// Fisher-Yates algorithm
			int n = array.Length;
			for (int i = 0; i < n; i++)
			{
				int r = i + (int)(randGen.NextDouble() * (n - i));
				var temp = array[r];
				array[r] = array[i];
				array[i] = temp;
			}
            return array;
		}
	}
}
