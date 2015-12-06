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

namespace BridgeDistribution
{
	public partial class MainForm : Form
	{
        private int defaultN = 4096;         // default number of users (must be >= 4)
        private bool stopped = true;
        private bool stopRequest, exitRequest;
        private bool logIncConsidered;
        private Censor censor;
        private int blockedSoFar;
        private RunLog runLog;
        private Font plotScreenFont = new Font("Arial", 12F, FontStyle.Regular);
        private Font plotSaveFont = new Font("Times New Roman", 20F, FontStyle.Regular);

		public MainForm()
		{
			InitializeComponent();
		}

        private void TestForm_Load(object sender, EventArgs e)
        {
            tbUserCount.Value = (int)Math.Log(defaultN, 2);
            chPlots.Series.Clear();
            rbSingleRun.Checked = true;
            rbPlot.Checked = true;
            cboGridX.Checked = true;
            cboGridY.Checked = true;
            btnCollapseRight_Click(sender, e);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (stopped)
            {
                stopped = stopRequest = exitRequest = logIncConsidered = false;

                dgvStats.Rows.Clear();
                chPlots.Series.Clear();
                btnStart.Text = "Stop";
                cbLogY.Enabled = false;
                blockedSoFar = 0;
                User.Reset();
                Bridge.Reset();
                Application.DoEvents();

                // Set up plots
                chPlots.Series.Add(new Series(cbThirsty.Text));
                chPlots.Series.Add(new Series(cbm.Text));
                chPlots.Series.Add(new Series(cbb.Text));
                chPlots.Series.Add(new Series(cbN.Text));
                chPlots.Series.Add(new Series(cbTime.Text));
                chPlots.Series.Add(new Series(cbmm.Text));
                chPlots.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
                chPlots.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
                chPlots.ChartAreas[0].AxisX.LabelStyle.Font = plotScreenFont;
                chPlots.ChartAreas[0].AxisY.LabelStyle.Font = plotScreenFont;
                chPlots.Legends[0].Font = plotScreenFont;

                int j = 1;
                foreach (var series in chPlots.Series)
                {
                    series.BorderWidth = 3;
                    series.ChartType = SeriesChartType.Line;
                    series.MarkerStyle = MarkerStyle.None + j++;
                    series.MarkerSize = 12;
                }
                UpdatePlots();

                int n = (int)Math.Pow(2, tbUserCount.Value);
                int t_max = rbMultipleRuns.Checked ? t_max = tbBadCountMax.Value : tbBadCount.Value + 1;

                for (int t = tbBadCount.Value; t < t_max && !stopRequest; t++)
                {
                    var d = new Distributor(rbBnb.Checked ? DistributeAlgorithm.BallsAndBins : DistributeAlgorithm.Matrix, 123);

                    // Add honest users
                    for (int i = 0; i < n - t; i++)
                        d.Join(new User());

                    // Add corrupt users
                    var attackModel = AttackModel.Aggressive;
                    if (rbPrudent.Checked)
                        attackModel = AttackModel.Prudent;
                    else if (rbStochastic.Checked)
                        attackModel = AttackModel.Stochastic;

                    censor = new Censor(d, attackModel, tbStochastic.Value / 40.0, 456);
                    censor.AddCorruptUsers(t);

                    runLog = new RunLog();
                    d.OnRoundEnd += OnRoundEnd;
                    d.Run(tbC.Value);

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
            }
            btnStart.Text = "Start";
            stopped = true;

            if (exitRequest)
                Close();
        }

        private bool OnRoundEnd(int round, UserList users, List<Bridge>[] bridges)
        {
            var log = new RoundLog(round, users, bridges);
            runLog.Add(log);

            int N = blockedSoFar + log.BridgeCount;
            blockedSoFar += log.BlockedCount;

            // Add one new row to the grid view
            dgvStats.Rows.Add(new string[] {
                round.ToString(), log.UsersCount.ToString(), log.CorruptsCount.ToString(), (log.BridgeCount / bridges.Length).ToString(), 
                log.BridgeCount.ToString(), log.BlockedCount.ToString(), N.ToString(), 
                log.ThirstyCount.ToString(),
            });
            
            if (rbSingleRun.Checked)
            {
                // Add one point to the plot for this round for each plot
                // (cbLogY.Checked ? 1 : 0) prevents log(zero) in log plots
                chPlots.Series[cbThirsty.Text].Points.AddXY(round, log.ThirstyCount + (cbLogY.Checked ? 1 : 0));
                chPlots.Series[cbm.Text].Points.AddXY(round, log.BridgeCount);
                chPlots.Series[cbb.Text].Points.AddXY(round, log.BlockedCount + (cbLogY.Checked ? 1 : 0));
                chPlots.Series[cbN.Text].Points.AddXY(round, N);
            }

            Application.DoEvents();
            return stopRequest;
        }

        private void tbN_ValueChanged(object sender, EventArgs e)
        {
            var n = 1 << tbUserCount.Value;
            lUserCount.Text = "n = " + n;
            tbBadCount.TickFrequency = tbBadCountMax.TickFrequency = (n - 1) / (int)Math.Log(n - 1, 2);
            tbBadCount.Maximum = tbBadCountMax.Maximum = n - 1;
            tbBadCount.Value = tbBadCountMax.Value = n / 2;
            lBadCount.Text = "t = " + tbBadCount.Value;
        }

        private void tbT_ValueChanged(object sender, EventArgs e)
        {
            lBadCount.Text = "t = " + tbBadCount.Value;
            if (tbBadCount.Value > tbBadCountMax.Value)
                tbBadCountMax.Value = tbBadCount.Value;
        }

        private void tbC_ValueChanged(object sender, EventArgs e)
        {
            lc.Text = "c = " + tbC.Value;
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
            {
                btnCollapseRight.Text = ">>>";      // Panel is open
                llPlotSettings.Text = "Hide plot settings";
            }
            else
            {
                btnCollapseRight.Text = "<<<";      // Panel is closed
                llPlotSettings.Text = "Show plot settings";
            }

            btnCollapseRight.Enabled = true;
        }

        public void UpdatePlots()
        {
            if (chPlots.Series.Count > 0)
            {
                chPlots.Series[cbThirsty.Text].Enabled = rbSingleRun.Checked && cbThirsty.Checked;
                chPlots.Series[cbm.Text].Enabled = rbSingleRun.Checked && cbm.Checked;
                chPlots.Series[cbb.Text].Enabled = rbSingleRun.Checked && cbb.Checked;
                chPlots.Series[cbN.Text].Enabled = rbSingleRun.Checked && cbN.Checked;

                chPlots.Series[cbTime.Text].Enabled = rbMultipleRuns.Checked && cbTime.Checked;
                chPlots.Series[cbmm.Text].Enabled = rbMultipleRuns.Checked && cbmm.Checked;

                chPlots.ChartAreas[0].AxisX.IsLogarithmic = cbLogX.Checked;
                chPlots.ChartAreas[0].AxisY.IsLogarithmic = cbLogY.Checked;

                chPlots.ChartAreas[0].RecalculateAxesScale();
            }
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

        private void tbBadCountMax_ValueChanged(object sender, EventArgs e)
        {
            lBadCountMax.Text = "t_max = " + tbBadCountMax.Value;
        }

        private void cb_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePlots();
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
                chPlots.Legends[0].Font = plotSaveFont;

                chPlots.SaveImage(saveDialog.FileName, format);

                chPlots.BorderlineDashStyle = ChartDashStyle.Solid;
                chPlots.ChartAreas[0].AxisX.LabelStyle.Font = plotScreenFont;
                chPlots.ChartAreas[0].AxisY.LabelStyle.Font = plotScreenFont;
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
    }
}
