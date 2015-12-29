using SecretSharing;
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
	public delegate bool RoundEndHandler(int round, List<int>[] bridges);
    public delegate void Distribute(List<long> bridges);

    public class BridgeShare
    {
        public readonly Zp Share;
        public bool IsBlocked;

        public BridgeShare(Zp share)
        {
            Share = share;
        }
    }

    public class UserAssignMessage
    {

        public readonly int UserId;
        public readonly long BridgePseudonym;

        public UserAssignMessage(int userId, long bridgePseudonym)
        {
            UserId = userId;
            BridgePseudonym = bridgePseudonym;
        }
    }

    public class BridgeAssignMessage
    {
        public readonly long BridgePseudonym;
        public readonly Zp BridgeShare;

        public BridgeAssignMessage(long bridgePseudonym, Zp bridgeShare)
        {
            BridgePseudonym = bridgePseudonym;
            BridgeShare = bridgeShare;
        }
    }

    public class Distributor : Node
    {
        protected List<int> Users = new List<int>();
        protected Dictionary<long, BridgeShare> BridgeShares = new Dictionary<long, BridgeShare>();

        public override void Receive(int fromNode, object message, MessageType type)
        {
            switch (type)
            {
                case MessageType.BridgeJoin:    // From a bridge: I'm joining the protocol.
                    var bj = message as BridgeJoinMessage;
                    BridgeShares[bj.Pseudonym] = new BridgeShare(bj.IdShare);
                    break;

                case MessageType.UserAssign:    // From the leader: Send the bridge to the user.
                    var ba = message as UserAssignMessage;
                    Send(ba.UserId, 
                         new BridgeAssignMessage(ba.BridgePseudonym, BridgeShares[ba.BridgePseudonym].Share), 
                         MessageType.BridgeAssign);
                    break;

                case MessageType.UserJoin:      // From a user: I'm joining the protocol.
                    Users.Add((int)message);
                    break;

                case MessageType.UserLeave:     // From a user: I'm leaving the protocol.
                    Debug.Assert(Users.Remove((int)message), "User not found.");
                    break;

                case MessageType.BridgeBlocked:     // From bridge: I'm blocked.
                    BridgeShares[(long)message].IsBlocked = true;
                    break;

                default:
                    throw new Exception("Invalid message received.");
            }
        }
    }

	public class LeaderDistributor : Distributor
	{
        public DistributeAlgorithm Algorithm { get; private set; }
		public event YieldHandler OnYield;
        public event RoundEndHandler OnRoundEnd;

        /// <summary>
        /// The distribution method
        /// </summary>
        public Distribute Distribute;

        private Random randGen;
        private List<int> distributorIds;

        /// <summary>
        /// A function that calculates the round threshold for the number of bridges blocked in each round.
        /// </summary>
        private Func<int, int> threshold;

        /// <summary>
        /// A function that calculates the number of bridges to be distributed in each round.
        /// </summary>
        private Func<int, int> distCount;

		public LeaderDistributor(List<int> distributorIds, DistributeAlgorithm method, int seed)
		{
			this.randGen = new Random(seed);
            this.distributorIds = distributorIds;
            
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
			var bridges = new List<int>[repeatCount];    // List of bridges distributed in each repeat

            for (int j = 0; j < repeatCount; j++)
                bridges[j] = new List<int>();

            int i = 0;              // Round number
            int n = Users.Count;    // Number of users
            bool stopped = false, stable = false;

			while (!stopped & !stable)
			{
                // Calculate # of blocked bridges in each repeat and remove them from the list
                // Go to next round if in any of the repeats the number blocked bridges is >= the threshold
                bool nextRound = false;
                int j = 0, blockedCounts = 0, m = distCount(i);
                foreach (var bridgeShare in BridgeShares)
                {
                    if (bridgeShare.Value.IsBlocked)
                    {
                        if (++blockedCounts > threshold(i))
                        {
                            nextRound = true;
                            break;
                        }
                    }
                    if (++j >= m)
                        j = blockedCounts = 0;      // reached bridges of next repeat
                }
                
                if (i == 0 || nextRound)
                {
                    i = i + 1;

                    // Update the parameters
                    n = Users.Count;
                    m = distCount(i);       // Number of bridges distributed in current round (same for all repeats)

                    if (m * repeatCount < n)
                    {
                        for (j = 0; j < repeatCount; j++)
                            Distribute(BridgeShares.Keys.ToList().GetRange(j * m, m));  // TODO: Speed improvement. Give start end rather than copying to sublists.
                    }
                    else
                    {
                        // Give each user a fresh bridge that has never been assigned to any user
                        var e = BridgeShares.Keys.GetEnumerator();
                        foreach (var user in Users)
                        {
                            e.MoveNext();
                            Assign(user, e.Current);
                        }
                        stable = true;
                    }

                    // Let the users perform their job
                    if (OnYield != null)
                        OnYield(threshold(i), repeatCount);

                    // Log the end of round
                    if (OnRoundEnd != null)
                        stopped = OnRoundEnd(i, bridges);
                }
                else stable = true;
			}
		}

        private void Assign(int userId, long bridgePseudonym)
        {
            // Tell other distributors to send their own bridge shares to the user
            var ba = new UserAssignMessage(userId, bridgePseudonym);
            distributorIds.ForEach(d => Send(d, ba, MessageType.UserAssign));

            // Send my bridge share to the user as well
            Send(userId, BridgeShares[bridgePseudonym], MessageType.BridgeAssign);
        }

        ///// <summary>
        ///// Replaces blocked bridges with fresh ones in the input list of bridges.
        ///// </summary>
        //private static int RecruitReplace(List<int> bridges)
        //{
        //    // Replace blocked bridges with fresh ones in all repeats
        //    int blockedCount = 0;
        //    for (int i = 0; i < bridges.Count; i++)
        //    {
        //        if (bridges[i].IsBlocked)
        //        {
        //            blockedCount++;
        //            bridges[i] = new Bridge();
        //        }
        //    }
        //    return blockedCount;
        //}

        private void DistributeBnB(List<long> bridges)
        {
            int m = bridges.Count;
            foreach (var user in Users)
            {
                var k = randGen.Next(0, m);
                Assign(user, bridges[k]);
            }
        }

		private void DistributeMatrix(List<long> bridges)
		{
			// Create a matrix with floor(m) columns and ceiling(n/m) rows
			int n = Users.Count;
            int m = bridges.Count;
            int r = (int)Math.Ceiling((double)n / m);

            var users = Shuffle(Users);
			for (int i = 0; i < r; i++)
			{
				for (int j = 0; j < m; j++)
				{
					// the user in the i-th row of the j-th column
						Assign(users[j * r + i], bridges[j]);
				}
			}
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

        public override void Receive(int fromNode, object message, MessageType type)
        {
            base.Receive(fromNode, message, type);
        }
    }
}
