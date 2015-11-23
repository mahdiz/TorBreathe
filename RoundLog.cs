using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
    public class RoundLog
    {
        /// <summary>
        /// Round number.
        /// </summary>
        public int Round { get; private set; }

        /// <summary>
        /// Number of users.
        /// </summary>
        public int UsersCount { get { return UsersSuccessMap.Length; } }
        
        /// <summary>
        /// Number of corrupt users in this round.
        /// </summary>
        public int CorruptsCount { get; private set; }

        /// <summary>
        /// Number of bridges distributed in this round.
        /// </summary>
        public int BridgeCount { get; private set; }

        /// <summary>
        /// Number of bridges blocked in this round.
        /// </summary>
        public int BlockedCount { get; private set; }

        /// <summary>
        /// Bit array representing the list of successfull/unsuccessful users so far. 
        /// If the i-th bit is true, then the i-th user in the Distributors users list is successful, otherwise it is unsuccessful.
        /// </summary>
        public BitArray UsersSuccessMap { get; private set; }

        /// <summary>
        /// Number of users (including corrupt users) who have no unblocked bridge.
        /// </summary>
        public int ThirstyCount
        {
            get
            {
                return UsersSuccessMap.Cast<bool>().Count(u => u == false);
            }
        }

        public RoundLog(int round, int t, int m, int b, UserList users)
        {
            Debug.Assert(m >= b);

            Round = round;
            CorruptsCount = t;
            BridgeCount = m;
            BlockedCount = b;
            UsersSuccessMap = new BitArray(users.Count);

            int i = 0;
            foreach (var user in users)
            {
                UsersSuccessMap.Set(i, !users[i].IsThirsty);
                i++;
            }
        }

        public RoundLog(int round, int t, int m, int b, BitArray usersSuccessMap)
        {
            Debug.Assert(m >= b);

            Round = round;
            CorruptsCount = t;
            BridgeCount = m;
            BlockedCount = b;
            UsersSuccessMap = new BitArray(usersSuccessMap);
        }

        public override string ToString()
        {
            return "Thirsty = " + ThirstyCount;
        }
    }
}
