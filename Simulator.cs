using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BridgeDistribution
{
    public static class Simulator
    {
        public const int Prime = 2147482223;        // Supports up to 2,147,482,223 bridges and 100 distributors

        /// <summary>
        /// Degree of the polynomial used in Shamir's secret-sharing.
        /// </summary>
        public static int PolynomialDegree
        {
            get { return ((int)Math.Floor(2 * numDistributors / 3.0)) - 1; }
        }

        /// <summary>
        /// Number of messages sent.
        /// </summary>
        public static int MessageCount { get; private set; }

        private static Dictionary<int, Node> nodes = new Dictionary<int, Node>();
        private static int numDistributors;

        public static void RegisterNode(Node node)
        {
            nodes.Add(node.Id, node);

            if (node is Distributor)
                numDistributors++;
        }

        public static T GetNode<T>(int userId) where T : Node
        {
            return (T)nodes[userId];
        }

        public static IEnumerable<T> GetNodes<T>() where T : Node
        {
            return nodes.Values.OfType<T>();
        }

        public static int NodeCount<T>() where T : Node
        {
            return nodes.Values.Count(n => n is T);
        }

        public static void Send(int fromNode, int toNode, object message, MessageType type)
        {
            if (fromNode != toNode)
                MessageCount++;

            nodes[toNode].Receive(fromNode, message, type);
        }

        public static void Reset()
        {
            Node.Reset();
            nodes = new Dictionary<int, Node>();
            MessageCount = 0;
            numDistributors = 0;
        }
    }
}
