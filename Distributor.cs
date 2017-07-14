using SecretSharing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorBricks
{
	public delegate void YieldHandler(double threshold, int repeatCount);
	public delegate bool RoundEndHandler(int round, int bridgeCount, int blockedCount, int bridgesSoFar);
    public delegate void RunEndHandler(int numRounds, int bridgesUsed);
    public delegate void Distribute(List<long> bridges);

    //public class BridgeShare
    //{
    //    public readonly Zp Share;
    //    public bool IsBlocked;

    //    public BridgeShare(Zp share)
    //    {
    //        Share = share;
    //    }
    //}

    //public class UserAssignMessage
    //{
    //    public readonly long UserPseudonym;
    //    public readonly List<long> BridgePseudonym;

    //    public UserAssignMessage(long userPseudonym, List<long> bridgePseudonym)
    //    {
    //        UserPseudonym = userPseudonym;
    //        BridgePseudonym = bridgePseudonym;
    //    }
    //}

    public class UserAssignment
    {
        public readonly long BridgePseudonym;
        public readonly Zp BridgeShare;

        public UserAssignment(long bridgePseudonym, Zp bridgeShare)
        {
            BridgePseudonym = bridgePseudonym;
            BridgeShare = bridgeShare;
        }
    }

    public class Distributor : Node
    {
        public List<long> BridgePseudonyms { protected get; set; }

        protected List<long> Users = new List<long>();
        protected Dictionary<long, Zp> BridgeShares = new Dictionary<long, Zp>();

        public override void Receive(int fromNode, object message, MessageType type)
        {
            if (type == MessageType.LeaderAssignments)    // From the leader: Send the bridge to the user.
            {
                var assignments = message as Dictionary<long, List<long>>;

                // Send each user all of its assigned bridge shares in one email
                foreach (var a in assignments)
                {
                    // a.Key is the user pseudonym
                    // a.Value is the list of bridge pseudonyms assigned to the user
                    var userAssignments = new List<UserAssignment>();
                    foreach (var bridgePseudonym in a.Value)
                        userAssignments.Add(new UserAssignment(bridgePseudonym, BridgeShares[bridgePseudonym]));

                    SendEmail(a.Key, userAssignments, MessageType.UserAssignments);
                }
            }
            else throw new Exception("Invalid message received.");
        }

        public override void ReceiveEmail(long fromPseudonym, object message, MessageType type)
        {
            switch (type)
            {
                case MessageType.BridgeJoin:    // From a bridge: I'm joining the protocol.
                    var bj = message as BridgeJoinMessage;
                    Debug.Assert(!BridgeShares.ContainsKey(bj.Pseudonym), "The bridge has already joined.");

                    BridgeShares[bj.Pseudonym] = bj.IdShare;
                    BridgePseudonyms.Remove(bj.Pseudonym);
                    break;

                case MessageType.UserJoin:      // From a user: I'm joining the protocol.
                    Users.Add(fromPseudonym);
                    break;

                case MessageType.UserLeave:     // From a user: I'm leaving the protocol.
                    Debug.Assert(Users.Remove(fromPseudonym), "User not found.");
                    break;

                case MessageType.BridgeBlocked:     // From bridge: I'm blocked; remove me from your list.
                    BridgeShares.Remove(fromPseudonym);
                    break;

                default:
                    throw new Exception("Invalid email received.");
            }
        }
    }

	public class LeaderDistributor : Distributor
	{
        public DistributeAlgorithm Algorithm { get; private set; }
		public event YieldHandler OnYield;
        public event RoundEndHandler OnRoundEnd;
        public event RunEndHandler OnRunEnd;

        /// <summary>
        /// The distribution method
        /// </summary>
        public Distribute Distribute;

        private Random randGen;
        private List<int> distributorIds;
        private HashSet<long> blockedBridges = new HashSet<long>();
        private Dictionary<long, List<long>> assignments;

        /// <summary>
        /// A function that calculates the round threshold for the number of bridges blocked in each round.
        /// </summary>
        private Func<int, double> threshold;

        /// <summary>
        /// A function that calculates the number of bridges to be distributed in each round.
        /// </summary>
        private Func<int, int> distCount;

		public LeaderDistributor(List<int> distributorIds, DistributeAlgorithm method, int seed)
		{
			this.randGen = new Random(seed);
            this.distributorIds = distributorIds;
            this.distributorIds.Insert(0, Id);
            
            switch (Algorithm = method)
            {
                case DistributeAlgorithm.BallsAndBins:
                    Distribute = DistributeBnB;
                    threshold = delegate(int i) { return 0.6 * Math.Pow(2, i); };       // See the robustness lemma
                    distCount = delegate(int i) { return (int)Math.Pow(2, i); };
                    break;

                case DistributeAlgorithm.Matrix:
                    Distribute = DistributeMatrix;
                    threshold = delegate(int i) { return Math.Pow(2, i); };
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
        public void Run(int repeatCount)
        {
            if (BridgePseudonyms == null)
                throw new InvalidOperationException("Bridge pseudonyms must be set before running the protocol.");

            int i = 0, d = 0;       // Round number, number of bridges distributed in the current round (i=4 gives a constant failure probability of ~10^-4)
            int n = Users.Count;    // Number of users
            int bridgesBlockedSoFar = 0, bridgeCount = 0, bridgesUsedSoFar = 0;
            bool stopped = false, stable = false, nextRound = true;
            var repeatBridges = new List<long>[repeatCount];      // List of bridges distributed in each repeat

            while (!stopped & !stable)
			{
                if (nextRound)
                {
                    i = i + 1;
                    nextRound = false;

                    // Update the parameters
                    n = Users.Count;
                    d = distCount(i);       // Number of bridges distributed in current round for each repeat
                    assignments = new Dictionary<long, List<long>>();   // (userID, List of bridges assigned to the user)

                    if (d * repeatCount < n)
                    {
                        if (BridgeShares.Count < d * repeatCount)
                            RecruitBridges(d * repeatCount - BridgeShares.Count);

                        Debug.Assert(BridgeShares.Count == d * repeatCount, "Not enough bridges were recruited.");

                        var bridgePseudonyms = BridgeShares.Keys.ToList();
                        for (int j = 0; j < repeatCount; j++)
                        {
                            repeatBridges[j] = bridgePseudonyms.GetRange(j * d, d);
                            Distribute(repeatBridges[j]);
                        }
                    }
                    else
                    {
                        // Give each user a unique fresh bridge that has never been assigned to any user
                        if (BridgeShares.Count < n)
                            RecruitBridges(n - BridgeShares.Count);

                        var e = BridgeShares.Keys.GetEnumerator();
                        foreach (var user in Users)
                        {
                            e.MoveNext();
                            Assign(user, e.Current);
                        }
                        stable = true;
                    }

                    // Tell the distributors to send the assigned bridges to their corresponding users
                    distributorIds.ForEach(dist => Send(dist, assignments, MessageType.LeaderAssignments));

                    // Let the users perform their job
                    if (OnYield != null)
                        OnYield(threshold(i), repeatCount);

                    // Compute some measurments for the round
					bridgeCount = d * repeatCount < n ? d * repeatCount : n;
					bridgesUsedSoFar += bridgesBlockedSoFar + bridgeCount;
                    bridgesBlockedSoFar += blockedBridges.Count;

                    // Log the end of round
                    if (OnRoundEnd != null)
                        stopped = OnRoundEnd(i, bridgeCount, blockedBridges.Count, bridgesUsedSoFar);
                }
                else stable = true;

                if (!stable)
                {
                    // Calculate # of blocked bridges in each repeat
                    // Go to next round if in any of the repeats the number blocked bridges is >= the threshold.
                    var rbCounts = repeatBridges.Select(rb => rb.Intersect(blockedBridges).Count());
                    if (rbCounts.Any(rbCount => rbCount >= threshold(i)))
                        nextRound = true;
                }
                blockedBridges.Clear();
            }
			if (OnRunEnd != null)
				OnRunEnd(i, bridgesUsedSoFar);
        }

        private void Assign(long userPseudonym, long bridgePseudonym)
        {
            if (!assignments.ContainsKey(userPseudonym))
                assignments[userPseudonym] = new List<long>();

            assignments[userPseudonym].Add(bridgePseudonym);
        }

        private void RecruitBridges(int num)
        {
            // Invite "num" bridges via the pseudonym network to join the protocol
            Debug.Assert(BridgePseudonyms.Count >= num);
            for (int i = 0; i < num; i++)
                SendEmail(BridgePseudonyms[i], null, MessageType.JoinRequest);
        }

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

        public override void ReceiveEmail(long fromPseudonym, object message, MessageType type)
        {
            if (type == MessageType.BridgeBlocked)
            {
                blockedBridges.Add(fromPseudonym);
                BridgePseudonyms.Remove(fromPseudonym);
            }
            base.ReceiveEmail(fromPseudonym, message, type);
        }
    }
}
