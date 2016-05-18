using SecretSharing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Bricks
{
	public partial class MainForm : Form
	{
        private const int seed = 123;
        private const int defaultUserCount = 32;
        private const int defaultDistCount = 1;    
        private const int plotMarkerSize = 15;
        private const int plotThickness = 6;
        private int repeatCount;
        private bool stopped = true;
        private bool stopRequest, exitRequest;
        private bool logIncConsidered;
        private int blockedSoFar;
        private RunLog runLog;
        private Font plotScreenFont = new Font("Arial", 12F, FontStyle.Regular);
        private Font plotSaveFont = new Font("Arial", 24F, FontStyle.Regular);

		public MainForm()
		{
			InitializeComponent();
		}

        private void TestForm_Load(object sender, EventArgs e)
        {
            tbUserCount.Value = (int)Math.Log(defaultUserCount, 2);
            tbDistCount.Value = defaultDistCount;
            chPlots.Series.Clear();
            rbSingleRun.Checked = true;
            rbPlot.Checked = true;
            cboGridX.Checked = true;
            cboGridY.Checked = true;
            rbLegendLeft.Checked = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (stopped)
            {
                dgvStats.Rows.Clear();
                chPlots.Series.Clear();
                blockedSoFar = 0;
                StaticRandom.Init(seed);
                Simulator.Reset();

                #region UI Initialization
                btnStart.Text = "Stop";
                cbLogY.Enabled = false;
                stopped = stopRequest = exitRequest = logIncConsidered = false;

                // Set up plots
                chPlots.Series.Add(new Series(cbThirsty.Text));
                chPlots.Series.Add(new Series(cbm.Text));
                chPlots.Series.Add(new Series(cbb.Text));
                chPlots.Series.Add(new Series(cbN.Text));
                chPlots.Series.Add(new Series(cbDistMessageCount.Text));
                chPlots.Series.Add(new Series(cbEmailCount.Text));
                chPlots.Series.Add(new Series(cbTime.Text));
                chPlots.Series.Add(new Series(cbmm.Text));
                chPlots.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                chPlots.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chPlots.ChartAreas[0].AxisX.LabelStyle.Font = plotScreenFont;
                chPlots.ChartAreas[0].AxisY.LabelStyle.Font = plotScreenFont;
                chPlots.ChartAreas[0].AxisX.TitleFont = plotScreenFont;
                chPlots.ChartAreas[0].AxisY.TitleFont = plotScreenFont;
                chPlots.Legends[0].Font = plotScreenFont;
                chPlots.Legends[0].IsDockedInsideChartArea = true;
                chPlots.ApplyPaletteColors();

                foreach (var series in chPlots.Series)
                {
                    series.BorderWidth = plotThickness;
                    series.ChartType = SeriesChartType.Line;
                    series.MarkerSize = plotMarkerSize;
                }
                chPlots.Series[0].MarkerStyle = MarkerStyle.Square;
                chPlots.Series[1].MarkerStyle = MarkerStyle.Triangle;
                chPlots.Series[2].MarkerStyle = MarkerStyle.Diamond;
                chPlots.Series[3].MarkerStyle = MarkerStyle.Circle;

                chPlots.Series[5].Color = chPlots.Series[2].Color;
                chPlots.Series[5].MarkerStyle = MarkerStyle.Circle;

                chPlots.Series[6].Color = chPlots.Series[2].Color;
                chPlots.Series[6].MarkerStyle = MarkerStyle.Circle;
                chPlots.Series[7].Color = chPlots.Series[3].Color;
                chPlots.Series[7].MarkerStyle = MarkerStyle.Square;
                UpdatePlots();
                Application.DoEvents();
                #endregion

                int n = (int)Math.Pow(2, tbUserCount.Value);
                int t_max = rbMultipleRuns.Checked ? t_max = tbBadCountMax.Value : tbCorruptCount.Value + 1;

                for (int t = tbCorruptCount.Value; t < t_max && !stopRequest; t++)
                {
                    // Create the distributors
                    var dists = new List<Distributor>();

                    for (int i = 1; i < tbDistCount.Value; i++)
                        dists.Add(new Distributor());

                    var leader = new LeaderDistributor(dists.Select(d => d.Id).ToList(),
                        rbBnb.Checked ? DistributeAlgorithm.BallsAndBins : DistributeAlgorithm.Matrix, seed);
                    dists.Insert(0, leader);

                    var distIds = dists.Select(d => d.Pseudonym).ToList();

                    // Add honest users
                    for (int i = 0; i < n - t; i++)
                        new User(distIds);

                    // Add corrupt users
                    var attackModel = AttackModel.Aggressive;
                    if (rbPrudent.Checked)
                        attackModel = AttackModel.Prudent;
                    else if (rbStochastic.Checked)
                        attackModel = AttackModel.Stochastic;

                    var censor = new Censor(dists, attackModel, tbStochastic.Value / 40.0, seed);
                    censor.AddCorruptUsers(t);

                    // Create a sufficient number of bridges
                    repeatCount = tbC.Value == 0 ? 1 : tbC.Value * (int)Math.Ceiling(Math.Log(n, 2));
                    var bridges = new List<Bridge>();
                    for (int i = 0; i < (8 * t - 2) * repeatCount; i++)
                        bridges.Add(new Bridge(distIds));

                    leader.BridgePseudonyms = bridges.Select(b => b.Pseudonym).ToList();
                    runLog = new RunLog();
                    leader.OnRoundEnd += OnRoundEnd;

                    leader.Run(repeatCount);

                    logIncConsidered = cbLogY.Checked;
                    cbLogY.Enabled = true;
                    if (rbMultipleRuns.Checked)
                    {
                        // Add one point to the plot for this run for each plot
                        chPlots.Series[cbTime.Text].Points.AddXY(t, runLog.RoundsCount);

                        var N = runLog.Sum(r => r.BlockedCount) + runLog.Last().BridgeCount;
                        chPlots.Series[cbmm.Text].Points.AddXY(t, N);
                    }
                }

                //for (int x = 8; x < 12 && !stopRequest; x++)
                //{
                //    blockedSoFar = 0;
                //    Simulator.Reset();

                //    int n = (int)Math.Pow(2, x);
                //    int t = n / 2;
                //    // Create the distributors
                //    var dists = new List<Distributor>();

                //    for (int i = 1; i < tbDistCount.Value; i++)
                //        dists.Add(new Distributor());

                //    var leader = new LeaderDistributor(dists.Select(d => d.Id).ToList(),
                //        rbBnb.Checked ? DistributeAlgorithm.BallsAndBins : DistributeAlgorithm.Matrix, seed);
                //    dists.Insert(0, leader);

                //    var distIds = dists.Select(d => d.Pseudonym).ToList();

                //    // Add honest users
                //    for (int i = 0; i < n - t; i++)
                //        new User(distIds);

                //    // Add corrupt users
                //    var attackModel = AttackModel.Aggressive;
                //    if (rbPrudent.Checked)
                //        attackModel = AttackModel.Prudent;
                //    else if (rbStochastic.Checked)
                //        attackModel = AttackModel.Stochastic;

                //    var censor = new Censor(dists, attackModel, tbStochastic.Value / 40.0, seed);
                //    censor.AddCorruptUsers(t);

                //    // Create a sufficient number of bridges
                //    repeatCount = tbC.Value == 0 ? 1 : tbC.Value * (int)Math.Ceiling(Math.Log(n, 2));
                //    var bridges = new List<Bridge>();
                //    for (int i = 0; i < (8 * t - 2) * repeatCount; i++)
                //        bridges.Add(new Bridge(distIds));

                //    leader.BridgePseudonyms = bridges.Select(b => b.Pseudonym).ToList();
                //    runLog = new RunLog();
                //    leader.OnRoundEnd += OnRoundEnd;

                //    leader.Run(repeatCount);

                //    logIncConsidered = cbLogY.Checked;
                //    cbLogY.Enabled = true;

                //    chPlots.Series[cbEmailCount.Text].Points.AddXY(n, (double)runLog.Sum(r => r.EmailCount) / n);

                //}

                //chPlots.Series[cbEmailCount.Text].Points.AddXY(32, 5);
                //chPlots.Series[cbEmailCount.Text].Points.AddXY(64, 10);
                //chPlots.Series[cbEmailCount.Text].Points.AddXY(128, 30);
                //chPlots.Series[cbEmailCount.Text].Points.AddXY(256, 60);
                //chPlots.Series[cbEmailCount.Text].Points.AddXY(512, 90);
                //chPlots.Series[cbEmailCount.Text].Points.AddXY(1024, 126);
                //chPlots.Series[cbEmailCount.Text].Points.AddXY(2048, 168);
                //chPlots.Series[cbEmailCount.Text].Points.AddXY(4096, 210);
            }
            else stopRequest = true;

            btnStart.Text = "Start";
            stopped = true;

            if (exitRequest)
                Close();
        }

        private bool OnRoundEnd(int round, int bridgeCount, int blockedCount)
        {
            var userCount = Simulator.NodeCount<User>();
            var corruptCount = Simulator.NodeCount<CorruptUser>();
            var thirstyCount = Simulator.GetNodes<User>().Where(u => u.IsThirsty && !(u is CorruptUser)).Count();

            int N = blockedSoFar + bridgeCount;
            blockedSoFar += blockedCount;

            // Add one new row to the grid view
            dgvStats.Rows.Add(new string[] {
                round.ToString(), userCount.ToString(), corruptCount.ToString(),
                (bridgeCount / repeatCount).ToString(), bridgeCount.ToString(),
                blockedCount.ToString(), N.ToString(), thirstyCount.ToString(),
            });
            runLog.Add(new RoundLog(round, userCount, corruptCount, thirstyCount, bridgeCount, blockedCount, Simulator.EmailCount));

            if (rbSingleRun.Checked)
            {
                // Add one point to the plot for this round for each plot
                // (cbLogY.Checked ? 1 : 0) prevents log(zero) in log plots
                chPlots.Series[cbThirsty.Text].Points.AddXY(round, thirstyCount + (cbLogY.Checked ? 1 : 0));
                chPlots.Series[cbm.Text].Points.AddXY(round, bridgeCount);
                chPlots.Series[cbb.Text].Points.AddXY(round, blockedCount + (cbLogY.Checked ? 1 : 0));
                chPlots.Series[cbN.Text].Points.AddXY(round, N);
                chPlots.Series[cbDistMessageCount.Text].Points.AddXY(round, Simulator.MessageCount);
                //chPlots.Series[cbEmailCount.Text].Points.AddXY(round, (double)Simulator.EmailCount / userCount);
            }

            Application.DoEvents();
            return stopRequest;
        }

        public void UpdatePlots()
        {
            if (chPlots.Series.Count > 0)
            {
                chPlots.Series[cbThirsty.Text].Enabled = rbSingleRun.Checked && cbThirsty.Checked;
                chPlots.Series[cbm.Text].Enabled = rbSingleRun.Checked && cbm.Checked;
                chPlots.Series[cbb.Text].Enabled = rbSingleRun.Checked && cbb.Checked;
                chPlots.Series[cbN.Text].Enabled = rbSingleRun.Checked && cbN.Checked;
                chPlots.Series[cbDistMessageCount.Text].Enabled = rbSingleRun.Checked && cbDistMessageCount.Checked;
                chPlots.Series[cbEmailCount.Text].Enabled = rbSingleRun.Checked && cbEmailCount.Checked;

                chPlots.Series[cbTime.Text].Enabled = rbMultipleRuns.Checked && cbTime.Checked;
                chPlots.Series[cbmm.Text].Enabled = rbMultipleRuns.Checked && cbmm.Checked;

                chPlots.ChartAreas[0].AxisX.IsLogarithmic = cbLogX.Checked;
                chPlots.ChartAreas[0].AxisY.IsLogarithmic = cbLogY.Checked;
                chPlots.Legends[0].Enabled = cbLegend.Checked;

                chPlots.Legends[0].Docking = rbLegendLeft.Checked ? Docking.Left :
                    rbLegendRight.Checked ? Docking.Right : rbLegendTop.Checked ? Docking.Top : Docking.Bottom;

                if (rbSingleRun.Checked)
                    chPlots.ChartAreas[0].AxisX.Title = "Round";
                else
                {
                    chPlots.ChartAreas[0].AxisX.Title = "Number of corrupt users";
                    if (cbTime.Checked && !cbmm.Checked)
                        chPlots.ChartAreas[0].AxisY.Title = "Running time";
                    else if (!cbTime.Checked && cbmm.Checked)
                        chPlots.ChartAreas[0].AxisY.Title = "Number of bridges used";
                    else
                        chPlots.ChartAreas[0].AxisY.Title = "";
                }

                chPlots.ChartAreas[0].RecalculateAxesScale();
            }
        }

        #region User Interface Event Handlers

        private void tbN_ValueChanged(object sender, EventArgs e)
        {
            var n = 1 << tbUserCount.Value;
            lUserCount.Text = "# Users = " + n;
            tbCorruptCount.TickFrequency = tbBadCountMax.TickFrequency = (n - 1) / (int)Math.Log(n - 1, 2);
            tbCorruptCount.Maximum = tbBadCountMax.Maximum = n - 1;
            tbCorruptCount.Value = tbBadCountMax.Value = n / 2;
            lCorruptCount.Text = "t = " + tbCorruptCount.Value;
        }

        private void tbT_ValueChanged(object sender, EventArgs e)
        {
            lCorruptCount.Text = "# Corrupt Users = " + tbCorruptCount.Value;
            if (tbCorruptCount.Value > tbBadCountMax.Value)
                tbBadCountMax.Value = tbCorruptCount.Value;
        }

        private void tbC_ValueChanged(object sender, EventArgs e)
        {
            if (tbC.Value == 0)
                lc.Text = "# Repeats = 1";
            else if (tbC.Value == 1)
                lc.Text = "# Repeats = log(n)";
            else
                lc.Text = "# Repeats = " + tbC.Value + "*log(n)";
        }

        private void tbBadCountMax_ValueChanged(object sender, EventArgs e)
        {
            lBadCountMax.Text = "Max # Corrupt Users = " + tbBadCountMax.Value;
        }

        private void tbDistCount_ValueChanged(object sender, EventArgs e)
        {
            lDistCount.Text = "# Distributors = " + tbDistCount.Value;
        }

        private void tbMarkerStep_ValueChanged(object sender, EventArgs e)
        {
            lMarkerStep.Text = "Plot marker step = " + tbMarkerStep.Value;

            if (tbMarkerStep.Value > 0)
            {
                int i = 1;
                foreach (var series in chPlots.Series)
                {
                    series.MarkerStyle = MarkerStyle.None + i++;
                    series.MarkerStep = tbMarkerStep.Value;
                }
            }
            else
            {
                foreach (var series in chPlots.Series)
                    series.MarkerStyle = MarkerStyle.None;
            }
        }

        private void btnCollapseLeft_Click(object sender, EventArgs e)
        {
            btnCollapseLeft.Enabled = false;
            pLeftPanel.Visible = !pLeftPanel.Visible;
            if (pLeftPanel.Visible)
                btnCollapseLeft.Text = "<<<";       // Panel is open
            else
                btnCollapseLeft.Text = ">>>";       // Panel is closed

            btnCollapseLeft.Enabled = true;
        }

        private void btnCollapseRight_Click(object sender, EventArgs e)
        {
            btnCollapseRight.Enabled = false;
            pRightPanel.Visible = !pRightPanel.Visible;
            if (pRightPanel.Visible)
                btnCollapseRight.Text = ">>>";      // Panel is open
            else
                btnCollapseRight.Text = "<<<";      // Panel is closed

            btnCollapseRight.Enabled = true;
        }

        private void rbPlot_CheckedChanged(object sender, EventArgs e)
        {
            dgvStats.Visible = rbGridView.Checked;
            chPlots.Visible = !rbGridView.Checked;
        }

        private void rbSingleRun_CheckedChanged(object sender, EventArgs e)
        {
            cbThirsty.Enabled = cbm.Enabled = cbb.Enabled = cbN.Enabled = rbSingleRun.Checked;
            cbTime.Enabled = cbmm.Enabled = tbBadCountMax.Enabled = lBadCountMax.Enabled = !rbSingleRun.Checked;
        }

        private void rbMultipleRuns_CheckedChanged(object sender, EventArgs e)
        {
            rbSingleRun_CheckedChanged(sender, e);
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePlots();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!stopped)
            {
                exitRequest = stopRequest = true;
                e.Cancel = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var saveDialog = new SaveFileDialog() 
            {
                FileName = "plot.emf",
                Filter = "EMF files|*.emf|WMF files|*.wmf|PNG files|*.png|JPEG files|*.jpg|GIF files|*.gif|BMP files|*.bmp|All files (*.*)|*.*",
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                ImageFormat format;

                switch (Path.GetExtension(saveDialog.FileName).ToUpper())
                {
                    case ".WMF":
                        format = ImageFormat.Wmf;
                        break;

                    case ".PNG":
                        format = ImageFormat.Png;
                        break;

                    case ".JPG":
                        format = ImageFormat.Jpeg;
                        break;

                    case ".GIF":
                        format = ImageFormat.Gif;
                        break;

                    case ".BMP":
                        format = ImageFormat.Bmp;
                        break;

                    case ".EMF":
                        format = ImageFormat.Emf;
                        break;

                    default:
                        format = ImageFormat.Png;
                        break;
                }
                chPlots.BorderlineDashStyle = ChartDashStyle.NotSet;
                chPlots.ChartAreas[0].AxisX.LabelStyle.Font = plotSaveFont;
                chPlots.ChartAreas[0].AxisY.LabelStyle.Font = plotSaveFont;
                chPlots.ChartAreas[0].AxisX.TitleFont = plotSaveFont;
                chPlots.ChartAreas[0].AxisY.TitleFont = plotSaveFont;
                chPlots.Legends[0].Font = plotSaveFont;
                chPlots.Legends[0].BackColor = Color.FromArgb(0, 0, 0, 0);

                chPlots.SaveImage(saveDialog.FileName, format);

                chPlots.BorderlineDashStyle = ChartDashStyle.Solid;
                chPlots.ChartAreas[0].AxisX.LabelStyle.Font = plotScreenFont;
                chPlots.ChartAreas[0].AxisY.LabelStyle.Font = plotScreenFont;
                chPlots.ChartAreas[0].AxisX.TitleFont = plotScreenFont;
                chPlots.ChartAreas[0].AxisY.TitleFont = plotScreenFont;
                chPlots.Legends[0].Font = plotScreenFont;
            }
        }

        private void cboGridX_CheckedChanged(object sender, EventArgs e)
        {
            chPlots.ChartAreas[0].AxisX.MajorGrid.LineWidth = 1 - chPlots.ChartAreas[0].AxisX.MajorGrid.LineWidth;
        }

        private void cboGridY_CheckedChanged(object sender, EventArgs e)
        {
            chPlots.ChartAreas[0].AxisY.MajorGrid.LineWidth = 1 - chPlots.ChartAreas[0].AxisY.MajorGrid.LineWidth;
        }

        private void llPlotSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            btnCollapseRight_Click(sender, e);
        }

        private void tbStochastic_ValueChanged(object sender, EventArgs e)
        {
            lStochastic.Text = "p = " + (tbStochastic.Value / 40.0).ToString("0.000");
        }

        private void rbStochastic_CheckedChanged(object sender, EventArgs e)
        {
            tbStochastic.Enabled = lStochastic.Enabled = rbStochastic.Checked;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
                btnSave_Click(sender, e);
        }

        private void cbLegend_CheckedChanged(object sender, EventArgs e)
        {
            rbLegendLeft.Enabled = rbLegendRight.Enabled = rbLegendTop.Enabled = rbLegendBottom.Enabled = cbLegend.Checked;
            UpdatePlots();
        }

        private void chPlots_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.X > 0 && e.Y > 0 && 
                e.X < chPlots.Width - chPlots.Legends[0].Position.Width - 200 && 
                e.Y < chPlots.Height - chPlots.Legends[0].Position.Height - 100)
            {
                chPlots.Legends[0].Position.X = (int)((double)e.X / chPlots.Width * 100.0);
                chPlots.Legends[0].Position.Y = (int)((double)e.Y / chPlots.Height * 100.0);
            }
        }

        private void cbLogY_CheckedChanged(object sender, EventArgs e)
        {
            // Increment the Y-value of data points to prevent log(zero)
            int incdec = cbLogY.Checked ? (logIncConsidered ? 0 : 1) : (logIncConsidered ? -1 : 0);
            foreach (var series in chPlots.Series)
            {
                foreach (var p in series.Points)
                    p.YValues[0] += incdec;
            }
            logIncConsidered = (incdec >= 0);
            UpdatePlots();
        }
        #endregion
    }
}
