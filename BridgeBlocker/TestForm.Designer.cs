namespace BridgeDistribution
{
	partial class TestForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnStart = new System.Windows.Forms.Button();
            this.dgvStats = new System.Windows.Forms.DataGridView();
            this.rbAggressive = new System.Windows.Forms.RadioButton();
            this.rbPrudent = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbBnb = new System.Windows.Forms.RadioButton();
            this.rbMatrix = new System.Windows.Forms.RadioButton();
            this.tbN = new System.Windows.Forms.TrackBar();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lBadCount = new System.Windows.Forms.Label();
            this.tbT = new System.Windows.Forms.TrackBar();
            this.lUserCount = new System.Windows.Forms.Label();
            this.lc = new System.Windows.Forms.Label();
            this.tbC = new System.Windows.Forms.TrackBar();
            this.col_i = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_n = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_t = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_m = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_mclogn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_b = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_bigN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.col_th = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvStats)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbN)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbT)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbC)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Font = new System.Drawing.Font("Helvetica", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(748, 551);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(230, 35);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // dgvStats
            // 
            this.dgvStats.AllowUserToAddRows = false;
            this.dgvStats.AllowUserToDeleteRows = false;
            this.dgvStats.AllowUserToResizeRows = false;
            this.dgvStats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvStats.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Helvetica", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvStats.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvStats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvStats.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.col_i,
            this.col_n,
            this.col_t,
            this.col_m,
            this.col_mclogn,
            this.col_b,
            this.col_bigN,
            this.col_th});
            this.dgvStats.EnableHeadersVisualStyles = false;
            this.dgvStats.Font = new System.Drawing.Font("Helvetica", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgvStats.Location = new System.Drawing.Point(12, 12);
            this.dgvStats.Name = "dgvStats";
            this.dgvStats.ReadOnly = true;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.dgvStats.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dgvStats.RowTemplate.Height = 25;
            this.dgvStats.RowTemplate.ReadOnly = true;
            this.dgvStats.Size = new System.Drawing.Size(721, 574);
            this.dgvStats.TabIndex = 1;
            // 
            // rbAggressive
            // 
            this.rbAggressive.AutoSize = true;
            this.rbAggressive.Checked = true;
            this.rbAggressive.Location = new System.Drawing.Point(20, 40);
            this.rbAggressive.Name = "rbAggressive";
            this.rbAggressive.Size = new System.Drawing.Size(111, 23);
            this.rbAggressive.TabIndex = 2;
            this.rbAggressive.TabStop = true;
            this.rbAggressive.Text = "Aggressive";
            this.rbAggressive.UseVisualStyleBackColor = true;
            // 
            // rbPrudent
            // 
            this.rbPrudent.AutoSize = true;
            this.rbPrudent.Location = new System.Drawing.Point(20, 73);
            this.rbPrudent.Name = "rbPrudent";
            this.rbPrudent.Size = new System.Drawing.Size(84, 23);
            this.rbPrudent.TabIndex = 3;
            this.rbPrudent.Text = "Prudent";
            this.rbPrudent.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.rbAggressive);
            this.groupBox1.Controls.Add(this.rbPrudent);
            this.groupBox1.Font = new System.Drawing.Font("Helvetica", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(748, 21);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(230, 117);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Censor Strategy";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.rbBnb);
            this.groupBox2.Controls.Add(this.rbMatrix);
            this.groupBox2.Font = new System.Drawing.Font("Helvetica", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(748, 156);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(230, 117);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Distributor Algorithm";
            // 
            // rbBnb
            // 
            this.rbBnb.AutoSize = true;
            this.rbBnb.Checked = true;
            this.rbBnb.Location = new System.Drawing.Point(20, 40);
            this.rbBnb.Name = "rbBnb";
            this.rbBnb.Size = new System.Drawing.Size(129, 23);
            this.rbBnb.TabIndex = 2;
            this.rbBnb.TabStop = true;
            this.rbBnb.Text = "Balls and Bins";
            this.rbBnb.UseVisualStyleBackColor = true;
            // 
            // rbMatrix
            // 
            this.rbMatrix.AutoSize = true;
            this.rbMatrix.Location = new System.Drawing.Point(20, 73);
            this.rbMatrix.Name = "rbMatrix";
            this.rbMatrix.Size = new System.Drawing.Size(70, 23);
            this.rbMatrix.TabIndex = 3;
            this.rbMatrix.Text = "Matrix";
            this.rbMatrix.UseVisualStyleBackColor = true;
            // 
            // tbN
            // 
            this.tbN.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbN.LargeChange = 2;
            this.tbN.Location = new System.Drawing.Point(6, 50);
            this.tbN.Maximum = 20;
            this.tbN.Minimum = 3;
            this.tbN.Name = "tbN";
            this.tbN.Size = new System.Drawing.Size(218, 45);
            this.tbN.TabIndex = 5;
            this.tbN.Value = 6;
            this.tbN.ValueChanged += new System.EventHandler(this.tbN_ValueChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.lc);
            this.groupBox3.Controls.Add(this.lBadCount);
            this.groupBox3.Controls.Add(this.tbC);
            this.groupBox3.Controls.Add(this.tbT);
            this.groupBox3.Controls.Add(this.lUserCount);
            this.groupBox3.Controls.Add(this.tbN);
            this.groupBox3.Location = new System.Drawing.Point(748, 292);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(230, 246);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Users";
            // 
            // lBadCount
            // 
            this.lBadCount.AutoSize = true;
            this.lBadCount.Location = new System.Drawing.Point(82, 102);
            this.lBadCount.Name = "lBadCount";
            this.lBadCount.Size = new System.Drawing.Size(45, 17);
            this.lBadCount.TabIndex = 6;
            this.lBadCount.Text = "t = 32";
            // 
            // tbT
            // 
            this.tbT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbT.LargeChange = 2;
            this.tbT.Location = new System.Drawing.Point(6, 122);
            this.tbT.Maximum = 63;
            this.tbT.Minimum = 1;
            this.tbT.Name = "tbT";
            this.tbT.Size = new System.Drawing.Size(218, 45);
            this.tbT.TabIndex = 5;
            this.tbT.Value = 32;
            this.tbT.ValueChanged += new System.EventHandler(this.tbT_ValueChanged);
            // 
            // lUserCount
            // 
            this.lUserCount.AutoSize = true;
            this.lUserCount.Location = new System.Drawing.Point(82, 30);
            this.lUserCount.Name = "lUserCount";
            this.lUserCount.Size = new System.Drawing.Size(49, 17);
            this.lUserCount.TabIndex = 6;
            this.lUserCount.Text = "n = 64";
            // 
            // lc
            // 
            this.lc.AutoSize = true;
            this.lc.Location = new System.Drawing.Point(82, 172);
            this.lc.Name = "lc";
            this.lc.Size = new System.Drawing.Size(41, 17);
            this.lc.TabIndex = 20;
            this.lc.Text = "c = 1";
            // 
            // tbC
            // 
            this.tbC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbC.LargeChange = 2;
            this.tbC.Location = new System.Drawing.Point(6, 193);
            this.tbC.Maximum = 20;
            this.tbC.Name = "tbC";
            this.tbC.Size = new System.Drawing.Size(218, 45);
            this.tbC.TabIndex = 7;
            this.tbC.Value = 1;
            this.tbC.Scroll += new System.EventHandler(this.tbC_Scroll);
            // 
            // col_i
            // 
            this.col_i.FillWeight = 67.87527F;
            this.col_i.HeaderText = "Round";
            this.col_i.Name = "col_i";
            this.col_i.ReadOnly = true;
            this.col_i.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // col_n
            // 
            this.col_n.FillWeight = 67.87527F;
            this.col_n.HeaderText = "n";
            this.col_n.Name = "col_n";
            this.col_n.ReadOnly = true;
            this.col_n.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // col_t
            // 
            this.col_t.FillWeight = 67.87527F;
            this.col_t.HeaderText = "t";
            this.col_t.Name = "col_t";
            this.col_t.ReadOnly = true;
            this.col_t.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // col_m
            // 
            this.col_m.FillWeight = 67.87527F;
            this.col_m.HeaderText = "m";
            this.col_m.Name = "col_m";
            this.col_m.ReadOnly = true;
            this.col_m.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // col_mclogn
            // 
            this.col_mclogn.FillWeight = 67.87527F;
            this.col_mclogn.HeaderText = "mclog(n)";
            this.col_mclogn.Name = "col_mclogn";
            this.col_mclogn.ReadOnly = true;
            this.col_mclogn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // col_b
            // 
            this.col_b.FillWeight = 67.87527F;
            this.col_b.HeaderText = "b";
            this.col_b.Name = "col_b";
            this.col_b.ReadOnly = true;
            this.col_b.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // col_bigN
            // 
            this.col_bigN.FillWeight = 67.87527F;
            this.col_bigN.HeaderText = "N";
            this.col_bigN.Name = "col_bigN";
            this.col_bigN.ReadOnly = true;
            this.col_bigN.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // col_th
            // 
            this.col_th.FillWeight = 67.87527F;
            this.col_th.HeaderText = "Thirsty";
            this.col_th.Name = "col_th";
            this.col_th.ReadOnly = true;
            this.col_th.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(990, 598);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.dgvStats);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Helvetica", 10.86792F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(500, 580);
            this.Name = "TestForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Tor Bridge Distribution";
            this.Load += new System.EventHandler(this.TestForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvStats)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbN)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbT)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbC)).EndInit();
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.DataGridView dgvStats;
        private System.Windows.Forms.RadioButton rbAggressive;
        private System.Windows.Forms.RadioButton rbPrudent;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbBnb;
        private System.Windows.Forms.RadioButton rbMatrix;
        private System.Windows.Forms.TrackBar tbN;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lUserCount;
        private System.Windows.Forms.Label lBadCount;
        private System.Windows.Forms.TrackBar tbT;
        private System.Windows.Forms.Label lc;
        private System.Windows.Forms.TrackBar tbC;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_i;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_n;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_t;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_m;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_mclogn;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_b;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_bigN;
        private System.Windows.Forms.DataGridViewTextBoxColumn col_th;
	}
}