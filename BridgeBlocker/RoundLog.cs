using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricks
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

        public int EmailCount { get; private set; }

        public RoundLog(int round, int usersCount, int corruptsCount, int thirstyCount, int bridgesCount, int blockedCount, int emailCount)
        {
            Round = round;
            UsersCount = usersCount;
            CorruptsCount = corruptsCount;

            BridgeCount = bridgesCount;
            BlockedCount = blockedCount;
            ThirstyCount = thirstyCount;
            EmailCount = emailCount;
        }

        public override string ToString()
        {
            return "Thirsty = " + ThirstyCount;
        }
    }
}