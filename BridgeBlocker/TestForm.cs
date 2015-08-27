using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
		int seed = 123;
		private int nHonest, nCorrupt, c;
        private double alpha, beta;
		private Distributor D;
		private DataGridView[] tables;
        private bool stopped = true;
        private bool stable = true;
        private StreamWriter logFile;

		public TestForm()
		{
			InitializeComponent();
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
            if (stopped || stable)
            {
                stopped = false;
                stable = false;
                btnStart.Text = "STOP";
                logFile = new StreamWriter("stats.txt");
                logFile.WriteLine("n \t round \t alpha \t beta \t thirst \t B \t bB \t b");

                for (alpha = 2; alpha <= 5.01 && !stopped; alpha += 0.2)
                {
                    nHonest = (int)Math.Pow(2, 4);
                    nCorrupt = nHonest;

                    c = 1;		     // clog(n) will be the number of matrices
                    //alpha = 2.01;    // alpha must be > 2
                    beta = 0.5;      // beta must be < 1
                    D = new Distributor(c, alpha, beta, seed);

                    // add some honest users
                    for (int i = 0; i < nHonest; i++)
                        D.Join(new User());

                    var censor = new Censor(D, AttackModel.Aggressive);
                    censor.AddCorruptUsers(nCorrupt);

                    D.OnRound += OnRound;
                    //D.OnIteration += OnDraw;
                    D.Run();
                }

                logFile.Close();
                stopped = true;
                stable = true;
                btnStart.Text = "START";
            }
            else
            {
                stopped = true;
                btnStart.Text = "START";
            }
		}

        private bool OnRound(UserList users, int[][] userShuffles, List<Bridge> bridges,
            int w, int r, int round, int totalBlockedSoFar)
        {
            var thirstyCount = users.ThirstyUsers.Where(u => !(u is CorruptUser)).Count();

            // write logs in file
            logFile.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}",
                users.Count, round, alpha, beta, thirstyCount, bridges.Count(), bridges.Count(b => b.IsBlocked), totalBlockedSoFar));

            logFile.Flush();
            ShowStats(users, bridges, w, r, round, totalBlockedSoFar);
            Application.DoEvents();
            Thread.Sleep(10);
            return stopped;
        }

		private bool OnDraw(UserList users, int[][] userShuffles, List<Bridge> bridges,
            int w, int r, int round, int totalBlockedSoFar)
		{
			SuspendLayout();

			// delete previous tables
			if (tables != null)
				foreach (var table in tables)
					Controls.Remove(table);
			
			int n = users.Count;
            int d = c * (int)Math.Log(n, 2);      // # of matrices
            tables = new DataGridView[d];

			for (int l = 0; l < d; l++)
			{				
                #region UI Settings
                var t = new DataGridView();
                t.SuspendLayout();
                t.Font = new Font("Arial Narrow", 9);

                var cellStyle = new DataGridViewCellStyle() 
				{ 
					Alignment = DataGridViewContentAlignment.MiddleCenter,
					WrapMode = DataGridViewTriState.True,
					SelectionBackColor = t.DefaultCellStyle.BackColor,
					SelectionForeColor = t.DefaultCellStyle.ForeColor
				};

				t.AllowUserToAddRows = false;
				t.AllowUserToDeleteRows = false;
				t.AllowUserToResizeColumns = false;
				t.AllowUserToResizeRows = false;
				t.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
				t.ColumnHeadersVisible = true;
				t.RowHeadersVisible = false;
				t.ColumnHeadersDefaultCellStyle = cellStyle;
				t.DefaultCellStyle = cellStyle;
				t.EnableHeadersVisualStyles = false;
				t.ReadOnly = true;
                #endregion

                var cols = new DataGridViewColumn[w];
				for (int i = 0; i < w; i++)
				{
					cols[i] = new DataGridViewTextBoxColumn()
					{
						HeaderText = bridges[l * w + i].Id.ToString(),
						SortMode = DataGridViewColumnSortMode.NotSortable,
						ReadOnly = true
					};
                    cols[i].HeaderCell.Style.BackColor = Color.LemonChiffon;
					if (bridges[l * w + i].IsBlocked)
						cols[i].HeaderCell.Style.ForeColor = Color.Red;
				}

				t.Columns.AddRange(cols);
				t.Width = (Width - 100) / d;
				t.Height = (r + 1) * 30;

				for (int i = 0; i < r; i++)
				{
					var labels = new string[w];
					for (int j = 0; j < w; j++)
						if (j * r + i < n)
						{
							labels[j] = userShuffles[l][j * r + i].ToString();
							if (users[userShuffles[l][j * r + i]] is CorruptUser)
								labels[j] += "*";
						}
					t.Rows.Add(labels);
				}

				foreach (DataGridViewRow row in t.Rows)
					row.Height = (t.Height - t.ColumnHeadersHeight - 2) / r;

				t.Top = 10;
				t.Left = 10 + l * (t.Width + 10);

				Controls.Add(t);
				t.PerformLayout();
				t.ResumeLayout(false);
				tables[l] = t;

                ShowStats(users, bridges, w, r, round, totalBlockedSoFar);
			}

			ResumeLayout();
			Application.DoEvents();
			Thread.Sleep(800);
            return stopped;
		}

        private void ShowStats(UserList users, List<Bridge> bridges, int w, int r, int round, int totalBlockedSoFar)
        {
            // show stats
            label1.Text = "# Users = " + users.Count + " (Honest: " + users.Count(u => !(u is CorruptUser)) + ")";
            var thirstyHonests = users.ThirstyUsers.Where(u => !(u is CorruptUser));
            label2.Text = "# Thirsty Honest Users = " + thirstyHonests.Count() + (thirstyHonests.Count() > 0 ? " (List: [" + string.Join(",", thirstyHonests) + "])" : "");
            label3.Text = "# Bridges in This Round = " + bridges.Count() + " (Blocked: " + bridges.Count(b => b.IsBlocked) + ")";
            label4.Text = "# Bridges Used So Far = " + (bridges.Count() + totalBlockedSoFar) + " (Blocked: " + totalBlockedSoFar + ")";
            label5.Text = "# Rounds = " + round;
            label6.Text = "alpha = " + alpha + ", " + "beta = " + beta + ", " + "w = " + w.ToString();
        }
	}
}
