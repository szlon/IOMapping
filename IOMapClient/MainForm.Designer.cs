namespace IOMapClient
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.picRxLED = new System.Windows.Forms.PictureBox();
            this.picTxLED = new System.Windows.Forms.PictureBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picRxLED)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTxLED)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 340);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(684, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel1
            // 
            this.statusLabel1.Name = "statusLabel1";
            this.statusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // txtInfo
            // 
            this.txtInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(10)))), ((int)(((byte)(10)))));
            this.txtInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInfo.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInfo.ForeColor = System.Drawing.Color.Lime;
            this.txtInfo.Location = new System.Drawing.Point(0, 0);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtInfo.Size = new System.Drawing.Size(684, 340);
            this.txtInfo.TabIndex = 5;
            // 
            // picRxLED
            // 
            this.picRxLED.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picRxLED.BackColor = System.Drawing.Color.Magenta;
            this.picRxLED.Location = new System.Drawing.Point(614, 4);
            this.picRxLED.Name = "picRxLED";
            this.picRxLED.Size = new System.Drawing.Size(23, 4);
            this.picRxLED.TabIndex = 7;
            this.picRxLED.TabStop = false;
            // 
            // picTxLED
            // 
            this.picTxLED.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picTxLED.BackColor = System.Drawing.Color.Aqua;
            this.picTxLED.Location = new System.Drawing.Point(639, 4);
            this.picTxLED.Name = "picTxLED";
            this.picTxLED.Size = new System.Drawing.Size(23, 4);
            this.picTxLED.TabIndex = 6;
            this.picTxLED.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 362);
            this.Controls.Add(this.picRxLED);
            this.Controls.Add(this.picTxLED);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.statusStrip1);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.Text = "IOMapClient";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picRxLED)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTxLED)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel1;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.PictureBox picRxLED;
        private System.Windows.Forms.PictureBox picTxLED;
    }
}

