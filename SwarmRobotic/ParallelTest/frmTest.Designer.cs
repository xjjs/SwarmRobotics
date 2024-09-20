namespace ParallelTest
{
    partial class frmTest
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
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.listThreads = new System.Windows.Forms.ListBox();
            this.txtError = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(8, 8);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(75, 23);
            this.btnUp.TabIndex = 0;
            this.btnUp.Text = "+Thread";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(89, 8);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(75, 23);
            this.btnDown.TabIndex = 1;
            this.btnDown.Text = "-Thread";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(253, 8);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 2;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(334, 8);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(6, 34);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(95, 12);
            this.lblInfo.TabIndex = 4;
            this.lblInfo.Text = "Current Threads";
            // 
            // listThreads
            // 
            this.listThreads.FormattingEnabled = true;
            this.listThreads.ItemHeight = 12;
            this.listThreads.Location = new System.Drawing.Point(0, 53);
            this.listThreads.Name = "listThreads";
            this.listThreads.Size = new System.Drawing.Size(245, 316);
            this.listThreads.TabIndex = 5;
            this.listThreads.SelectedIndexChanged += new System.EventHandler(this.listThreads_SelectedIndexChanged);
            // 
            // txtError
            // 
            this.txtError.Location = new System.Drawing.Point(251, 53);
            this.txtError.Multiline = true;
            this.txtError.Name = "txtError";
            this.txtError.ReadOnly = true;
            this.txtError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtError.Size = new System.Drawing.Size(488, 316);
            this.txtError.TabIndex = 6;
            this.txtError.WordWrap = false;
            this.txtError.TextChanged += new System.EventHandler(this.txtError_TextChanged);
            // 
            // frmTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 366);
            this.Controls.Add(this.txtError);
            this.Controls.Add(this.listThreads);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Name = "frmTest";
            this.Text = "frmTest";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmTest_FormClosing);
            this.Load += new System.EventHandler(this.frmTest_Load);
            this.Resize += new System.EventHandler(this.frmTest_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ListBox listThreads;
        private System.Windows.Forms.TextBox txtError;
    }
}