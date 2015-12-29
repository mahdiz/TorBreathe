using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
    public static class Simulator
    {
        public const int Prime = 2147482223;

        public static Dictionary<int, User> Users { get; private set; }
        public static Dictionary<int, Bridge> Bridges { get; private set; }
        public static List<Distributor> Distributors { get; private set; }

        /// <summary>
        /// Degree of the polynomial used in Shamir's secret-sharing.
        /// </summary>
        public static int PolynomialDegree
        {
            get { return ((int)Math.Floor(2 * Distributors.Count / 3.0)) - 1; }
        }

        /// <summary>
        /// Number of messages sent.
        /// </summary>
        public static int MessageCount { get; private set; }

        private static Dictionary<int, Node> nodes = new Dictionary<int, Node>();

        public static void RegisterNode(Node node)
        {
            nodes.Add(node.Id, node);

            if (node is User)
                Users[node.Id] = node as User;
            else if (node is Bridge)
                Bridges[node.Id] = node as Bridge;
            else
                Distributors.Add(node as Distributor);
        }

        public static void Send(int fromNode, int toNode, object message, MessageType type)
        {
            MessageCount++;
            nodes[toNode].Receive(fromNode, message, type);
        }

        public static void Reset()
        {
            Node.Reset();
            nodes = new Dictionary<int, Node>();
            MessageCount = 0;
        }
    }
}
