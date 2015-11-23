using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BridgeDistribution
{
	public partial class TestForm : Form
	{
        private int defaultN = 1024;         // default number of users (must be >= 4)
        private bool stopRequest = true;
        private Censor censor;
        private List<RunLog> runsLog = new List<RunLog>();
        private StreamWriter logFile;

		public TestForm()
		{
			InitializeComponent();
		}

        private void TestForm_Load(object sender, EventArgs e)
        {
            tbN.Value = (int)Math.Log(defaultN, 2);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (stopRequest)
            {
                runsLog.Clear();
                dgvStats.Rows.Clear();
                btnStart.Text = "Stop";
                stopRequest = false;
                User.Reset();
                Bridge.Reset();
                Application.DoEvents();

                logFile = new StreamWriter("stats.txt");
                logFile.WriteLine("n \t round \t alpha \t beta \t thirst \t B \t bB \t b");

                int n = (int)Math.Pow(2, tbN.Value);
                int nCorrupt = tbT.Value;

                var d = new Distributor(rbBnb.Checked ? DistributeMethod.BallsAndBins : DistributeMethod.Matrix, 123);

                // add honest users
                for (int i = 0; i < n - nCorrupt; i++)
                    d.Join(new User());

                // add corrupt users
                censor = new Censor(d, rbAggressive.Checked ? AttackModel.Aggressive : AttackModel.Prudent);
                censor.AddCorruptUsers(nCorrupt);

                d.OnRoundEnd += OnRoundEnd;
                d.OnRunEnd += OnRunEnd;
                d.Run(tbC.Value);

                ShowCombinedRun(d.Users.GetUsers());
                stopRequest = true;
                btnStart.Text = "Start";
            }
            else
            {
                stopRequest = true;
                btnStart.Text = "Start";
            }
        }

        private bool OnRunEnd(int run, RunLog log)
        {
            Application.DoEvents();

            runsLog.Add(log);
            return stopRequest;
        }

        private RoundLog OnRoundEnd(int round, UserList users, List<Bridge> bridges, out bool stop)
        {
            Application.DoEvents();

            var t = users.Count(u => u is CorruptUser);
            var m = bridges.Count();
            var b = bridges.Count(x => x.IsBlocked);

            stop = stopRequest;
            return new RoundLog(round, t, m, b, users);
        }

        private void ShowCombinedRun(User[] users)
        {
            var maxRoundsCount = runsLog.Max(r => r.RoundsCount);
            var combinedRun = new RunLog();
            var n = runsLog[0][0].UsersCount;
            var t = runsLog[0][0].CorruptsCount;

            for (int i = 0; i < maxRoundsCount; i++)
            {
                int m = 0, b = 0;
                var usersSuccessMap = new BitArray(n);

                foreach (var runLog in runsLog)
                {
                    if (runLog.RoundsCount > i)
                    {
                        Debug.Assert(runLog[i].UsersCount == n);        // variable n or t not supported yet!
                        Debug.Assert(runLog[i].CorruptsCount == t);

                        m += runLog[i].BridgeCount;
                        b += runLog[i].BlockedCount;
                        usersSuccessMap.Or(runLog[i].UsersSuccessMap);
                    }
                }
                combinedRun.Add(new RoundLog(i, t, m, b, usersSuccessMap));
            }

            int blockedSoFar = 0;
            foreach (var r in combinedRun)
            {
                // find number of thirsty honest users in this round
                int thirsty = 0;
                for (int i = 0; i < r.UsersSuccessMap.Length; i++)
                {
                    if (!r.UsersSuccessMap[i] && !(users[i] is CorruptUser))
                        thirsty++;

                    Debug.Assert(!(r.UsersSuccessMap[i] && users[i] is CorruptUser && rbAggressive.Checked));
                }
                int N = blockedSoFar + r.BridgeCount;

                dgvStats.Rows.Add(new string[] { 
                    r.Round.ToString(), r.UsersCount.ToString(), r.CorruptsCount.ToString(), (r.BridgeCount / runsLog.Count).ToString(), 
                    r.BridgeCount.ToString(), r.BlockedCount.ToString(), N.ToString(), thirsty.ToString(),
                });

                blockedSoFar += r.BlockedCount;
            }

            //// write logs in file
            //logFile.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", n, round, thirstyCount, m, b));
            //logFile.Flush();

            logFile.Close();
        }

        private void tbN_ValueChanged(object sender, EventArgs e)
        {
            var n = 1 << tbN.Value;
            lUserCount.Text = "n = " + n;
            tbT.TickFrequency = (n - 1) / (int)Math.Log(n - 1, 2);
            tbT.Maximum = n - 1;
            tbT.Value = n / 2;
            lBadCount.Text = "t = " + tbT.Value;
        }

        private void tbT_ValueChanged(object sender, EventArgs e)
        {
            lBadCount.Text = "t = " + tbT.Value;
        }

        private void tbC_Scroll(object sender, EventArgs e)
        {
            lc.Text = "c = " + tbC.Value;
        }

        //private bool OnDraw(UserList users, List<Bridge> bridges, int round, int totalBlockedSoFar)
        //{
        //    SuspendLayout();

        //    // delete previous tables
        //    if (tables != null)
        //        foreach (var table in tables)
        //            Controls.Remove(table);

        //    int n = users.Count;
        //    int d = c * (int)Math.Log(n, 2);      // # of matrices
        //    tables = new DataGridView[d];

        //    for (int l = 0; l < d; l++)
        //    {				
        //        #region UI Settings
        //        var t = new DataGridView();
        //        t.SuspendLayout();
        //        t.Font = new Font("Arial Narrow", 9);

        //        var cellStyle = new DataGridViewCellStyle() 
        //        { 
        //            Alignment = DataGridViewContentAlignment.MiddleCenter,
        //            WrapMode = DataGridViewTriState.True,
        //            SelectionBackColor = t.DefaultCellStyle.BackColor,
        //            SelectionForeColor = t.DefaultCellStyle.ForeColor
        //        };

        //        t.AllowUserToAddRows = false;
        //        t.AllowUserToDeleteRows = false;
        //        t.AllowUserToResizeColumns = false;
        //        t.AllowUserToResizeRows = false;
        //        t.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        //        t.ColumnHeadersVisible = true;
        //        t.RowHeadersVisible = false;
        //        t.ColumnHeadersDefaultCellStyle = cellStyle;
        //        t.DefaultCellStyle = cellStyle;
        //        t.EnableHeadersVisualStyles = false;
        //        t.ReadOnly = true;
        //        #endregion

        //        var cols = new DataGridViewColumn[w];
        //        for (int i = 0; i < w; i++)
        //        {
        //            cols[i] = new DataGridViewTextBoxColumn()
        //            {
        //                HeaderText = bridges[l * w + i].Id.ToString(),
        //                SortMode = DataGridViewColumnSortMode.NotSortable,
        //                ReadOnly = true
        //            };
        //            cols[i].HeaderCell.Style.BackColor = Color.LemonChiffon;
        //            if (bridges[l * w + i].IsBlocked)
        //                cols[i].HeaderCell.Style.ForeColor = Color.Red;
        //        }

        //        t.Columns.AddRange(cols);
        //        t.Width = (Width - 100) / d;
        //        t.Height = (r + 1) * 30;

        //        for (int i = 0; i < r; i++)
        //        {
        //            var labels = new string[w];
        //            for (int j = 0; j < w; j++)
        //                if (j * r + i < n)
        //                {
        //                    labels[j] = userShuffles[l][j * r + i].ToString();
        //                    if (users[userShuffles[l][j * r + i]] is CorruptUser)
        //                        labels[j] += "*";
        //                }
        //            t.Rows.Add(labels);
        //        }

        //        foreach (DataGridViewRow row in t.Rows)
        //            row.Height = (t.Height - t.ColumnHeadersHeight - 2) / r;

        //        t.Top = 10;
        //        t.Left = 10 + l * (t.Width + 10);

        //        Controls.Add(t);
        //        t.PerformLayout();
        //        t.ResumeLayout(false);
        //        tables[l] = t;

        //        ShowStats(users, bridges, w, r, round, totalBlockedSoFar);
        //    }

        //    ResumeLayout();
        //    Application.DoEvents();
        //    Thread.Sleep(800);
        //    return stopped;
        //}
    }
}
