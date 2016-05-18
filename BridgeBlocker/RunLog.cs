using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bricks
{
    public class RunLog : IEnumerable<RoundLog>
    {
        public RoundLog this[int i] { get { return roundsLog[i]; } }
        public int RoundsCount { get { return roundsLog.Count; } }

        private List<RoundLog> roundsLog = new List<RoundLog>();

        public void Add(RoundLog log)
        {
            roundsLog.Add(log);
        }

        public IEnumerator<RoundLog> GetEnumerator()
        {
            return roundsLog.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return roundsLog.GetEnumerator();
        }

        public override string ToString()
        {
            return "Thirsty: " + roundsLog.Select(x => x.ThirstyCount.ToString())
                .Aggregate((x, y) => x + ", " + y);
        }
    }
}
