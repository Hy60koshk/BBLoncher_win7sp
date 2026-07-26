namespace YobaLoncher {
	partial class PreloaderForm {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreloaderForm));
			this._progressBar1 = new YobaLoncher.YobaProgressBar();
			this.loadingLabel = new System.Windows.Forms.Label();
			this.loadingLabelError = new System.Windows.Forms.Label();
			this.labelAbout = new System.Windows.Forms.Label();
			this.statusLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// _progressBar1
			// 
			this._progressBar1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(63)))), ((int)(((byte)(64)))));
			this._progressBar1.Location = new System.Drawing.Point(0, 290);
			this._progressBar1.Margin = new System.Windows.Forms.Padding(0);
			this._progressBar1.MaxValue = 100;
			this._progressBar1.Name = "_progressBar1";
			this._progressBar1.Size = new System.Drawing.Size(400, 10);
			this._progressBar1.TabIndex = 112;
			this._progressBar1.Value = 0;
			// 
			// loadingLabel
			// 
			this.loadingLabel.AutoSize = true;
			this.loadingLabel.BackColor = System.Drawing.Color.Transparent;
			this.loadingLabel.ForeColor = System.Drawing.Color.White;
			this.loadingLabel.Location = new System.Drawing.Point(3, 272);
			this.loadingLabel.Name = "loadingLabel";
			this.loadingLabel.Size = new System.Drawing.Size(121, 13);
			this.loadingLabel.TabIndex = 1;
			this.loadingLabel.Text = "YobaLoncher file label...";
			// 
			// loadingLabelError
			// 
			this.loadingLabelError.AutoSize = true;
			this.loadingLabelError.BackColor = System.Drawing.Color.Transparent;
			this.loadingLabelError.ForeColor = System.Drawing.SystemColors.ButtonShadow;
			this.loadingLabelError.Location = new System.Drawing.Point(9, 9);
			this.loadingLabelError.Name = "loadingLabelError";
			this.loadingLabelError.Size = new System.Drawing.Size(0, 13);
			this.loadingLabelError.TabIndex = 2;
			// 
			// labelAbout
			// 
			this.labelAbout.BackColor = System.Drawing.Color.Transparent;
			this.labelAbout.ForeColor = System.Drawing.Color.White;
			this.labelAbout.Location = new System.Drawing.Point(15, 9);
			this.labelAbout.Name = "labelAbout";
			this.labelAbout.Size = new System.Drawing.Size(373, 46);
			this.labelAbout.TabIndex = 3;
			this.labelAbout.Text = "Press F1";
			this.labelAbout.TextAlign = System.Drawing.ContentAlignment.TopRight;
			// 
			// statusLabel
			// 
			this.statusLabel.AutoSize = true;
			this.statusLabel.BackColor = System.Drawing.Color.Transparent;
			this.statusLabel.ForeColor = System.Drawing.Color.White;
			this.statusLabel.Location = new System.Drawing.Point(3, 254);
			this.statusLabel.Name = "statusLabel";
			this.statusLabel.Size = new System.Drawing.Size(127, 13);
			this.statusLabel.TabIndex = 113;
			this.statusLabel.Text = "YobaLoncher status label";
			// 
			// PreloaderForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.DimGray;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
			this.ClientSize = new System.Drawing.Size(400, 300);
			this.Controls.Add(this.statusLabel);
			this.Controls.Add(this.labelAbout);
			this.Controls.Add(this.loadingLabelError);
			this.Controls.Add(this.loadingLabel);
			this.Controls.Add(this._progressBar1);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "PreloaderForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Yoba Loncher";
			this.Shown += new System.EventHandler(this._preloaderForm_ShownAsync);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this._preloaderForm_KeyUp);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private YobaProgressBar _progressBar1; //System.Windows.Forms.ProgressBar _progressBar1;
		private System.Windows.Forms.Label loadingLabel;
		private System.Windows.Forms.Label loadingLabelError;
		private System.Windows.Forms.Label labelAbout;
		private System.Windows.Forms.Label statusLabel;
	}
}

