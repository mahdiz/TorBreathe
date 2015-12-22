using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
	public delegate void YieldHandler(int threshold, int repeatCount);
	public delegate bool RoundEndHandler(int round, UserList users, List<Bridge>[] bridges);
    public delegate void Distribute(List<Bridge> B);

    public enum DistributeAlgorithm
    {
        BallsAndBins,
        Matrix,
    }

	public class Distributor
	{
        public DistributeAlgorithm Algorithm { get; private set; }
		public event YieldHandler OnYield;
        public event RoundEndHandler OnRoundEnd;
        public UserList Users { get; private set; }

        /// <summary>
        /// The distribution method
        /// </summary>
        public Distribute Distribute;

        private Random randGen;

        /// <summary>
        /// A function that calculates the round threshold for the number of bridges blocked in each round.
        /// </summary>
        private Func<int, int> threshold;

        /// <summary>
        /// A function that calculates the number of bridges to be distributed in each round.
        /// </summary>
        private Func<int, int> distCount;

		public Distributor(DistributeAlgorithm method, int seed)
		{
			randGen = new Random(seed);
			Users = new UserList();
            
            switch (Algorithm = method)
            {
                case DistributeAlgorithm.BallsAndBins:
                    Distribute = DistributeBnB;
                    threshold = delegate(int i) { return (int)Math.Pow(2, i); };
                    distCount = delegate(int i) { return (int)Math.Pow(2, i + 1); };
                    break;

                case DistributeAlgorithm.Matrix:
                    Distribute = DistributeMatrix;
                    threshold = delegate(int i) { return (int)Math.Pow(2, i); };
                    distCount = delegate(int i) { return (int)Math.Pow(2, i + 1); };
                    break;

                default:
                    throw new InvalidOperationException("Invalid distribution method!");
            }
		}

        /// <summary>
        /// Runs the bridge distribution algorithm.
        /// </summary>
        /// <param name="c">Repeats the distribute method c*Log(n) times. If c=0, then repeats the method only once.</param>
        public void Run(int c)
        {
            var repeatCount = c == 0 ? 1 : c * (int)Math.Ceiling(Math.Log(Users.Count, 2));

			var bridges = new List<Bridge>[repeatCount];    // List of bridges distributed in each repeat
            int[] blockedCount;        // # of blocked bridges in each repeat

            for (int j = 0; j < repeatCount; j++)
                bridges[j] = new List<Bridge>();

            int i = 0;              // Round number
            int n = Users.Count;    // Number of users
            bool stopped = false, stable = false;

			while (!stopped & !stable)
			{
                blockedCount = bridges.Select(b => RecruitReplace(b)).ToArray();

                // Go to next round if in any of the repeats the number blocked bridges is >= the threshold
                if (i == 0 || blockedCount.Any(b => b >= threshold(i)))
                {
                    i = i + 1;

                    // Update the parameters
                    n = Users.Count;
                    int m = distCount(i);       // Number of bridges distributed in current round (same for all repeats)

                    if (m * repeatCount < n)
                    {
                        for (int j = 0; j < repeatCount; j++)
                        {
                            Debug.Assert(!bridges[j].Any(x => x.IsBlocked), "Distribute a blocked bridge??");

                            // Increased the number of unblocked bridges to m for each repeat
                            var extraCount = m - bridges[j].Count;
                            for (int k = 0; k < extraCount; k++)
                                bridges[j].Add(new Bridge());

                            Distribute(bridges[j]);
                        }
                    }
                    else
                    {
                        // Give each user a fresh bridge that has never been assigned to any user
                        foreach (var user in Users)
                            user.Bridges.Add(new Bridge());
                        stable = true;
                    }

                    // Let the users perform their job
                    if (OnYield != null)
                        OnYield(threshold(i), repeatCount);

                    // Log the end of round
                    if (OnRoundEnd != null)
                        stopped = OnRoundEnd(i, Users, bridges);
                }
                else stable = true;
			}
		}


        /// <summary>
        /// Replaces blocked bridges with fresh ones in the input list of bridges.
        /// </summary>
        private static int RecruitReplace(List<Bridge> bridges)
        {
            // Replace blocked bridges with fresh ones in all repeats
            int blockedCount = 0;
            for (int i = 0; i < bridges.Count; i++)
            {
                if (bridges[i].IsBlocked)
                {
                    blockedCount++;
                    bridges[i] = new Bridge();
                }
            }
            return blockedCount;
        }

        private void DistributeBnB(List<Bridge> B)
        {
            int m = B.Count;
            foreach (var user in Users)
            {
                var k = randGen.Next(0, m);
                user.Bridges.Add(B[k]);
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
