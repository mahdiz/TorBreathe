using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorBricks
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
        public int UsersCount { get; private set; }
        
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
        /// Number of users (including corrupt users) who have no unblocked bridge.
        /// </summary>
        public int ThirstyCount { get; private set; }

        public RoundLog(int round, UserList users, List<Bridge>[] bridges)
        {
            Round = round;
            UsersCount = users.Count;
            CorruptsCount = users.Count(u => u is CorruptUser);

            var m = 0;
            var b = 0;

            var repeatCount = bridges.Length;
            for (int j = 0; j < repeatCount; j++)
            {
                m += bridges[j].Count;
                b += bridges[j].Count(x => x.IsBlocked);
            }
            
            BridgeCount = m;
            BlockedCount = b;
            ThirstyCount = users.ThirstyUsers.Where(u => !(u is CorruptUser)).Count();
        }

        public override string ToString()
        {
            return "Thirsty = " + ThirstyCount;
        }
    }
}