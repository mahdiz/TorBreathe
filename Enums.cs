using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorBricks
{
    public enum MessageType
    {
        UserJoin,           // User to distributor (Email)
        UserLeave,          // User to distributor (Email)
        BridgeJoin,         // Bridge to distributor (Email)
        BridgeBlocked,      // Bridge to distributor (Email)
        LeaderAssignments,  // Leader to distributor
        JoinRequest,        // Leader to bridge (Email)
        UserAssignments,    // Distributors to user (Email)
    }

    public enum DistributeAlgorithm
    {
        BallsAndBins,
        Matrix,
    }
}
