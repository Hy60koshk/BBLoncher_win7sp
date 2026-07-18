namespace YobaLoncher {
	partial class MainForm {
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
			this.draggingPanel = new DraggingPanel<MainForm>();
			this.closeButton = new YobaLoncher.YobaCloseButton();
			this.helpButton = new YobaLoncher.YobaCloseButton();
			this.minimizeButton = new YobaLoncher.YobaCloseButton();
			this.maximizeButton = new YobaLoncher.YobaCloseButton();
			this.refreshButton = new YobaLoncher.YobaButton();
			this.mainBrowser = new System.Windows.Forms.WebBrowser();
			this.SuspendLayout();
			// 
			// draggingPanel
			// 
			this.draggingPanel.BackColor = System.Drawing.Color.Transparent;
			this.draggingPanel.Location = new System.Drawing.Point(0, 0);
			this.draggingPanel.Margin = new System.Windows.Forms.Padding(0);
			this.draggingPanel.Name = "draggingPanel";
			this.draggingPanel.TabIndex = 103;
			this.draggingPanel.AddDragging(this);
			//this.draggingPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.draggingPanel_MouseDown);
			//this.draggingPanel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.draggingPanel_DoubleClick);
			this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
			// 
			// closeButton
			// 
			this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(63)))), ((int)(((byte)(64)))));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Abort;
			this.closeButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.closeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(11)))));
			this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.closeButton.Font = new System.Drawing.Font("Lucida Console", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
			this.closeButton.ForeColor = System.Drawing.Color.White;
			this.closeButton.Location = new System.Drawing.Point(750, -2);
			this.closeButton.Margin = new System.Windows.Forms.Padding(0);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(32, 24);
			this.closeButton.TabIndex = 100;
			this.closeButton.Text = "X";
			this.closeButton.UseVisualStyleBackColor = false;
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			// 
			// helpButton
			// 
			this.helpButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(63)))), ((int)(((byte)(64)))));
			this.helpButton.DialogResult = System.Windows.Forms.DialogResult.Abort;
			this.helpButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.helpButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(11)))));
			this.helpButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.helpButton.Font = new System.Drawing.Font("Lucida Console", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
			this.helpButton.ForeColor = System.Drawing.Color.White;
			this.helpButton.Location = new System.Drawing.Point(660, -2);
			this.helpButton.Margin = new System.Windows.Forms.Padding(0);
			this.helpButton.Name = "helpButton";
			this.helpButton.Size = new System.Drawing.Size(32, 24);
			this.helpButton.TabIndex = 102;
			this.helpButton.Text = "?";
			this.helpButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.helpButton.UseVisualStyleBackColor = false;
			this.helpButton.Click += new System.EventHandler(this.helpButton_Click);
			this.helpButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			// 
			// minimizeButton
			// 
			this.minimizeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(63)))), ((int)(((byte)(64)))));
			this.minimizeButton.DialogResult = System.Windows.Forms.DialogResult.Abort;
			this.minimizeButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.minimizeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(11)))));
			this.minimizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.minimizeButton.Font = new System.Drawing.Font("Lucida Console", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
			this.minimizeButton.ForeColor = System.Drawing.Color.White;
			this.minimizeButton.Location = new System.Drawing.Point(690, -2);
			this.minimizeButton.Margin = new System.Windows.Forms.Padding(0);
			this.minimizeButton.Name = "minimizeButton";
			this.minimizeButton.Size = new System.Drawing.Size(32, 24);
			this.minimizeButton.TabIndex = 101;
			this.minimizeButton.Text = "_";
			this.minimizeButton.UseVisualStyleBackColor = false;
			this.minimizeButton.Click += new System.EventHandler(this.minimizeButton_Click);
			this.minimizeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			// 
			// maximizeButton
			// 
			this.maximizeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(63)))), ((int)(((byte)(64)))));
			this.maximizeButton.DialogResult = System.Windows.Forms.DialogResult.Abort;
			this.maximizeButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.maximizeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(11)))));
			this.maximizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.maximizeButton.Font = new System.Drawing.Font("Lucida Console", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
			this.maximizeButton.ForeColor = System.Drawing.Color.White;
			this.maximizeButton.Location = new System.Drawing.Point(720, -2);
			this.maximizeButton.Margin = new System.Windows.Forms.Padding(0);
			this.maximizeButton.Name = "maximizeButton";
			this.maximizeButton.Size = new System.Drawing.Size(32, 24);
			this.maximizeButton.TabIndex = 101;
			this.maximizeButton.Text = "_";
			this.maximizeButton.UseVisualStyleBackColor = false;
			this.maximizeButton.Click += new System.EventHandler(this.maximizeButton_Click);
			this.maximizeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			// 
			// refreshButton
			// 
			this.refreshButton.BackColor = System.Drawing.Color.Transparent;
			this.refreshButton.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
			this.refreshButton.FlatAppearance.BorderSize = 0;
			this.refreshButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(11)))));
			this.refreshButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
			this.refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.refreshButton.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
			this.refreshButton.ForeColor = System.Drawing.Color.White;
			this.refreshButton.Location = new System.Drawing.Point(0, 400);
			this.refreshButton.Margin = new System.Windows.Forms.Padding(0);
			this.refreshButton.Name = "refreshButton";
			this.refreshButton.Size = new System.Drawing.Size(15, 35);
			this.refreshButton.TabIndex = 1110;
			this.refreshButton.UseVisualStyleBackColor = false;
			this.refreshButton.Visible = false;
			this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
			// 
			// mainBrowser
			// 
			this.mainBrowser.Location = new System.Drawing.Point(189, 137);
			this.mainBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.mainBrowser.Name = "mainBrowser";
			this.mainBrowser.Size = new System.Drawing.Size(115, 96);
			this.mainBrowser.TabIndex = 1;
			this.mainBrowser.Visible = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(63)))), ((int)(((byte)(64)))));
			//this.MaximumSize = new System.Drawing.Size(1200, 800);
			//this.ClientSize = new System.Drawing.Size(800, 440);
			this.Resize += MainForm_Resize;
			//this.ResizeEnd += MainForm_Resize;
			this.Controls.Add(this.refreshButton);
			this.Controls.Add(this.draggingPanel);
			this.Controls.Add(this.mainBrowser);
			this.Controls.Add(this.closeButton);
			this.Controls.Add(this.minimizeButton);
			this.Controls.Add(this.maximizeButton);
			this.Controls.Add(this.helpButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Yoba Loncher";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
			this.Shown += new System.EventHandler(this.MainForm_Shown);
			this.ResumeLayout(false);

		}
		

		#endregion

		private YobaCloseButton closeButton;
		private YobaCloseButton maximizeButton;
		private YobaCloseButton minimizeButton;
		private YobaCloseButton helpButton;
		private DraggingPanel<MainForm> draggingPanel;
		private YobaButton refreshButton;
		
		private System.Windows.Forms.WebBrowser mainBrowser;
	}
}