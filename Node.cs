using SecretSharing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorBricks
{
    /// <summary>
    /// Represents an abstract network node capable of sending and receiving messages.
    /// </summary>
    public abstract class Node
    {
        public readonly int Id;
        public readonly long Pseudonym;     // Some unique anonymous ID like gmail ID
        private static int idGen;

        public Node()
        {
            Id = idGen++;
            Pseudonym = StaticRandom.Next(0, long.MaxValue);
            Simulator.RegisterNode(this);
        }

        public static void Reset()
        {
            idGen = 0;
        }

        protected void Send(int toNode, object message, MessageType type)
        {
            Simulator.Send(Id, toNode, message, type);
        }

        protected void SendEmail(long toPseudonym, object message, MessageType type)
        {
            Simulator.SendEmail(Pseudonym, toPseudonym, message, type);
        }

        public abstract void Receive(int fromNode, object message, MessageType type);

        public abstract void ReceiveEmail(long fromPseudonym, object message, MessageType type);

        //public abstract void Receive<T>(int fromNode, T message, MessageType type);

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
