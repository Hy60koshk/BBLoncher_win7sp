﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YobaLoncher {
	public partial class YobaDialog : Form {
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		public static DialogResult ShowDialog(string msg) {
			return new YobaDialog(msg).ShowDialog();
		}
		internal static DialogResult ShowDialog(string msg, UIElement[] buttons) {
			return new YobaDialog(msg, buttons).ShowDialog();
		}

		private static UIElement[] yesNoBtns_;
		private static UIElement[] abortIgnoreBtns_;
		private static UIElement[] OKCopyStackBtns_;

		internal static UIElement[] YesNoBtns {
			get {
				return yesNoBtns_ != null ? yesNoBtns_ : (yesNoBtns_ = new UIElement[] {
					new UIElement() {
						Caption = Locale.Get("Yes"), Result = DialogResult.Yes
					},
					new UIElement() {
						Caption = Locale.Get("No"), Result = DialogResult.No
					}
				});
			}
		}
		internal static UIElement[] AbortIgnoreBtns {
			get {
				return abortIgnoreBtns_ != null ? abortIgnoreBtns_ : (abortIgnoreBtns_ = new UIElement[] {
					new UIElement() {
						Caption = Locale.Get("ExitApp"), Result = DialogResult.Abort
					},
					new UIElement() {
						Caption = Locale.Get("Ignore"), Result = DialogResult.Ignore
					}
				});
			}
		}
		internal static UIElement[] OKCopyStackBtns {
			get {
				return OKCopyStackBtns_ != null ? OKCopyStackBtns_ : (OKCopyStackBtns_ = new UIElement[] {
					new UIElement() {
						Caption = Locale.Get("OK")
						, Result = DialogResult.OK
						, Size = new Vector() {
							Y = 38, X = 170
						}
					},
					new UIElement() {
						Caption = Locale.Get("CopyStackTrace")
						, Result = DialogResult.Retry
						, Size = new Vector() {
							Y = 38, X = 170
						}
					}
				});
			}
		}

		public Label MessageLabel => messageLabel;

		public YobaDialog() : this("", null) { }
		public YobaDialog(string message) : this(message, null) { }
		internal YobaDialog(string message, UIElement[] buttons) {
			startInit(message);
			initButtons(buttons);
		}
		internal YobaDialog(Size size) : this("", size, null) { }
		internal YobaDialog(string message, Size size) : this(message, size, null) { }
		internal YobaDialog(Size size, UIElement[] buttons) : this("", size, buttons) { }
		internal YobaDialog(string message, Size size, UIElement[] buttons) {
			startInit(message);
			Size = size;
			MinimumSize = size;
			MaximumSize = size;
			initButtons(buttons);
		}

		private int _originalWinStyle = -1;

		public void ResetWinStyle() {
			NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, _originalWinStyle);
		}

		/*protected override void OnPaint(PaintEventArgs e) {
			e.Graphics.DrawRectangle(Pens.Black, this.Bounds);
		}*/
		protected override void OnPaintBackground(PaintEventArgs e) {
			base.OnPaintBackground(e);
			//Rectangle rect = new Rectangle(0, -1, this.ClientSize.Width - 1, this.ClientSize.Height);

			Pen pen1 = new Pen(Color.FromArgb(40, 41, 42), 1);
			Pen pen2 = new Pen(Color.FromArgb(100, 101, 102), 1);
			int corner1 = ClientSize.Height - 1;
			int corner2 = ClientSize.Width - 1;
			e.Graphics.DrawLine(pen1, 0, 0, 0, corner1);
			e.Graphics.DrawLine(pen1, 0, corner1, corner2, corner1);
			e.Graphics.DrawLine(pen2, corner2, corner1 - 1, corner2, 0);
			e.Graphics.DrawLine(pen2, corner2, 0, 1, 0);
			//e.Graphics.DrawRectangle(pen, rect);
		}

		private void startInit(string message) {
			InitializeComponent();
			_originalWinStyle = NativeWinAPI.GetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE);
			NativeWinAPI.SetWindowLong(this.Handle, NativeWinAPI.GWL_EXSTYLE, _originalWinStyle | NativeWinAPI.WS_EX_COMPOSITED);

			Icon = Program.LoncherSettings != null ? (Program.LoncherSettings.Icon ?? Resource1.yIcon) : Resource1.yIcon;
			messageLabel.Text = message;
			if (message.Length > 200) {
				int height = Size.Height + (Size.Height - 90) * ((message.Length - message.Length % 200) / 200);
				int width = Size.Width;
				if (height > 400) {
					height = 400;
					width = 640;
				}
				Size = new Size(width, height);
			}
			MinimumSize = Size;
			MaximumSize = Size;
		}

		private void initButtons(UIElement[] buttons) {
			SuspendLayout();
			if (buttons == null) {
				YobaButton button = new YobaButton();
				button.Location = new Point((int)((double)Size.Width / 2 - (double)button.Width / 2), Size.Height - 46);
				button.Name = "okButton";
				button.Text = Locale.Get("OK");
				button.TabIndex = 100;
				button.DialogResult = DialogResult.OK;
				Controls.Add(button);
				//button.Click += new EventHandler(onBtnClick);
			}
			else {
				int minwidth = 38;
				int maxheight = 1;
				byte btnnum = 0;
				YobaButton[] ybuttons = new YobaButton[buttons.Length];
				foreach (UIElement btnInfo in buttons) {
					YobaButton button = new YobaButton();
					button.Name = "button" + btnnum;
					ybuttons[btnnum] = button;
					button.TabIndex = 100 + btnnum;
					if (btnInfo.Size != null) {
						button.Size = new Size(btnInfo.Size.X, btnInfo.Size.Y);
					}
					if (btnInfo.Caption != null) {
						button.Text = btnInfo.Caption;
					}
					button.DialogResult = btnInfo.Result;
					minwidth += 18 + button.Size.Width;
					maxheight = Math.Max(maxheight, button.Size.Height);
					Controls.Add(button);
					btnnum++;
				}
				Size = MinimumSize = MaximumSize = new Size(Math.Max(minwidth, Size.Width), maxheight > 28 ? Size.Height + maxheight - 28 : Size.Height);
				if (ybuttons.Length == 1) {
					YobaButton btn = ybuttons[0];
					btn.Location = new Point((int)((double)Size.Width / 2 - (double)btn.Width / 2), Size.Height - maxheight - 18);
				}
				else {
					int space = (Size.Width - minwidth) / (btnnum - 1);
					int bottomY = Size.Height - maxheight - 18;
					int offsetX = 28;
					foreach (YobaButton btn in ybuttons) {
						btn.Location = new Point(offsetX, bottomY);
						offsetX += 18 + btn.Size.Width + space;
					}
				}
			}
			if (messageLabel.Text.Length > 0) {
				messageLabel.Size = new Size(Size.Width - 56, Size.Height - 84);
			}
			else {
				Controls.Remove(messageLabel);
			}
			closeButton.UpdateLocation();//Location = new Point(Size.Width - 30, -3);
			draggingPanel.Size = new Size(Size.Width - 32, 24);
			ResumeLayout(false);
		}
		private void onBtnClick(object s, EventArgs args) {
			YobaButton _this = (YobaButton)s;
			((YobaDialog)_this.Parent).DialogResult = _this.DialogResult;
		}

		private void draggingPanel_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				ReleaseCapture();
				SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
			}
		}
	}

	internal abstract class YobaButtonAbs : Button {
		protected static class BtnColors {
			public static Color Fore = Color.White;
			public static Color Back = Color.FromArgb(48, 48, 50);
			public static Color MouseDown = Color.FromArgb(10, 10, 11);
			public static Color MouseOver = Color.FromArgb(62, 63, 64);
			public static Color Border = Color.Gray;
		}

		/*public override void NotifyDefault(bool value) {
			base.NotifyDefault(false);
		}*/
		protected override bool ShowFocusCues {
			get {
				return false;
			}
		}

		protected UIElement StyleInfo = null;
		protected bool MouseHoverState = false;

		public YobaButtonAbs() : base() {
			BackColor = BtnColors.Back;
			ForeColor = BtnColors.Fore;
			FlatAppearance.BorderColor = BtnColors.Border;
			FlatAppearance.MouseDownBackColor = BtnColors.MouseDown;
			FlatAppearance.MouseOverBackColor = BtnColors.MouseOver;
			FlatStyle = FlatStyle.Flat;
			Font = new Font("Tahoma", 12F, FontStyle.Regular, GraphicsUnit.Pixel, 204);
			Margin = new Padding(0);
			Size = new Size(138, 28);
			UseVisualStyleBackColor = false;
		}

		public virtual void ApplyUIStyles(UIElement styleInfo) {
			StyleInfo = styleInfo;
			UseVisualStyleBackColor = true;
			if (styleInfo.Position != null) {
				Location = new Point(styleInfo.Position.X, styleInfo.Position.Y);
			}
			if (styleInfo.Size != null) {
				Size = new Size(styleInfo.Size.X, styleInfo.Size.Y);
			}
			if (styleInfo.Caption != null) {
				Text = styleInfo.Caption;
			}
			if (styleInfo.CustomStyle) {
				FlatStyle = FlatStyle.Flat;
				ForeColor = YU.colorFromString(styleInfo.Color, BtnColors.Fore);
				FlatAppearance.BorderColor = YU.colorFromString(styleInfo.BorderColor, BtnColors.Border);
			}
			if (styleInfo.BgImage != null && YU.stringHasText(styleInfo.BgImage.Path)) {
				styleInfo.BgImage.ImageLayout = ImageLayout.Stretch;
				if (YU.stringHasText(styleInfo.BgImage.Layout)) {
					if (Enum.TryParse(styleInfo.BgImage.Layout, out ImageLayout layout)) {
						styleInfo.BgImage.ImageLayout = layout;
					}
				}
				BackgroundImageLayout = styleInfo.BgImage.ImageLayout;
				BackgroundImage = YU.readBitmap(PreloaderForm.IMGPATH + styleInfo.BgImage.Path);

				if (styleInfo.BgImageClick != null && YU.stringHasText(styleInfo.BgImageClick.Path)) {
					styleInfo.BgImageClick.ImageLayout = ImageLayout.Stretch;
					if (YU.stringHasText(styleInfo.BgImageClick.Layout)) {
						if (Enum.TryParse(styleInfo.BgImageClick.Layout, out ImageLayout layout)) {
							styleInfo.BgImageClick.ImageLayout = layout;
						}
					}
					MouseDown += YobaButtonAbs_MouseDownBGChange;
					MouseUp += YobaButtonAbs_MouseUpBGChange;
				}
				if (styleInfo.BgImageHover != null && YU.stringHasText(styleInfo.BgImageHover.Path)) {
					styleInfo.BgImageHover.ImageLayout = ImageLayout.Stretch;
					if (YU.stringHasText(styleInfo.BgImageHover.Layout)) {
						if (Enum.TryParse(styleInfo.BgImageHover.Layout, out ImageLayout layout)) {
							styleInfo.BgImageHover.ImageLayout = layout;
						}
					}
					MouseEnter += YobaButtonAbs_MouseHoverBGChange;
					MouseLeave += YobaButtonAbs_MouseLeaveBGChange;
				}
			}
		}

		private void YobaButtonAbs_MouseLeaveBGChange(object sender, EventArgs e) {
			MouseHoverState = false;
			BackgroundImageLayout = StyleInfo.BgImage.ImageLayout;
			BackgroundImage = YU.readBitmap(PreloaderForm.IMGPATH + StyleInfo.BgImage.Path);
		}
		private void YobaButtonAbs_MouseHoverBGChange(object sender, EventArgs e) {
			MouseHoverState = true;
			BackgroundImageLayout = StyleInfo.BgImageHover.ImageLayout;
			BackgroundImage = YU.readBitmap(PreloaderForm.IMGPATH + StyleInfo.BgImageHover.Path);
		}
		private void YobaButtonAbs_MouseDownBGChange(object sender, EventArgs e) {
			BackgroundImageLayout = StyleInfo.BgImageClick.ImageLayout;
			BackgroundImage = YU.readBitmap(PreloaderForm.IMGPATH + StyleInfo.BgImageClick.Path);
		}
		private void YobaButtonAbs_MouseUpBGChange(object sender, EventArgs e) {
			if (MouseHoverState) {
				BackgroundImageLayout = StyleInfo.BgImageHover.ImageLayout;
				BackgroundImage = YU.readBitmap(PreloaderForm.IMGPATH + StyleInfo.BgImageHover.Path);
			}
			else {
				BackgroundImageLayout = StyleInfo.BgImage.ImageLayout;
				BackgroundImage = YU.readBitmap(PreloaderForm.IMGPATH + StyleInfo.BgImage.Path);
			}
		}
	}

	internal class YobaButton : YobaButtonAbs {
		public string Url = null;

		public YobaButton() : base() { }

		public YobaButton(string url) : this() {
			Url = url;
		}

		public override void ApplyUIStyles(UIElement styleInfo) {
			base.ApplyUIStyles(styleInfo);

			YU.setFont(this, styleInfo.Font, styleInfo.FontSize);
			if (styleInfo.CustomStyle) {
				BackColor = YU.colorFromString(styleInfo.BgColor, BtnColors.Back);
				FlatAppearance.BorderSize = styleInfo.BorderSize > -1 ? styleInfo.BorderSize : 0;
				FlatAppearance.MouseOverBackColor = YU.colorFromString(styleInfo.BgColorHover, BtnColors.MouseOver);
				FlatAppearance.MouseDownBackColor = YU.colorFromString(styleInfo.BgColorDown, BtnColors.MouseDown);
			}
		}
	}

	internal class YobaCloseButton : YobaButtonAbs {
		public YobaCloseButton() : base() {
			BackColor = Color.FromArgb(62, 63, 64);
			FlatAppearance.MouseOverBackColor = BtnColors.Back;
			Font = new Font("Lucida Console", 16F, FontStyle.Regular, GraphicsUnit.Pixel, 204);
			Location = new Point(-2, -3);
			Name = "closeButton";
			Size = new Size(32, 24);
			TabIndex = 150;
			Text = "X";

			DialogResult = DialogResult.Abort;
		}

		public void UpdateLocation() {
			Size formsize = ((Form)Parent).Size;
			Location = new Point(formsize.Width - 30, -3);
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);

			if (this.Parent is YobaDialog) {
				Pen pen1 = new Pen(Color.FromArgb(99, 100, 101), 1);
				Pen pen2 = new Pen(Color.FromArgb(100, 101, 102), 1);
				/*Rectangle rect = new Rectangle(0, 3, ClientSize.Width - 3, ClientSize.Height - 1);
				e.Graphics.DrawRectangle(pen2, rect);*/
				int corner1 = ClientSize.Height - 1;
				int corner2 = ClientSize.Width - 3;
				e.Graphics.DrawLine(pen1, 0, 4, 0, corner1);
				e.Graphics.DrawLine(pen1, 0, corner1, corner2 - 1, corner1);
				e.Graphics.DrawLine(pen2, corner2, corner1, corner2, 3);
				e.Graphics.DrawLine(pen2, corner2, 3, 0, 3);
			}
			else {
				Pen pen2 = new Pen(Color.FromArgb(100, 101, 102), 1);
				Rectangle rect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
				e.Graphics.DrawRectangle(pen2, rect);
			}
		}

		public override void ApplyUIStyles(UIElement styleInfo) {
			base.ApplyUIStyles(styleInfo);

			if (YU.stringHasText(styleInfo.FontSize)) {
				YU.setFont(this, styleInfo.Font, styleInfo.FontSize);
			}
			if (styleInfo.CustomStyle) {
				BackColor = YU.colorFromString(styleInfo.BgColor, Color.FromArgb(62, 63, 64));
				FlatAppearance.BorderSize = styleInfo.BorderSize > -1 ? styleInfo.BorderSize : 0;
				FlatAppearance.MouseOverBackColor = YU.colorFromString(styleInfo.BgColorHover, BtnColors.Back);
				FlatAppearance.MouseDownBackColor = YU.colorFromString(styleInfo.BgColorDown, BtnColors.MouseDown);
			}
		}
	}

	internal class YobaComboBox : ComboBox {
		private const int WM_PAINT = 0xF;
		private int buttonWidth = SystemInformation.HorizontalScrollBarArrowWidth + 4;
		Color borderColor = Color.Gray;
		public Color BorderColor {
			get {
				return borderColor;
			}
			set {
				borderColor = value;
				Invalidate();
			}
		}
		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
			if (m.Msg == WM_PAINT && DropDownStyle != ComboBoxStyle.Simple) {
				using (var g = Graphics.FromHwnd(Handle)) {
					using (var p = new Pen(BorderColor)) {
						using (var brush = new SolidBrush(BackColor)) {
							g.FillRectangle(brush, Width - buttonWidth, 0, buttonWidth, Height);
						}
						using (var brush = new SolidBrush(ForeColor)) {
							float xcenter = (float)Width - (float)buttonWidth / 2;
							float ycenter = (float)Height / 2;
							g.FillPolygon(brush, new PointF[] {
								new PointF(xcenter - 4, ycenter)
								, new PointF(xcenter, ycenter + 3)
								, new PointF(xcenter + 4, ycenter)
							});
						}
						g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
						g.DrawLine(p, Width - buttonWidth, 0, Width - buttonWidth, Height);
					}
				}
			}
		}
		protected override void OnDrawItem(DrawItemEventArgs e) {
			base.OnDrawItem(e);
		}
		/*protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);
			using (var g = Graphics.FromHwnd(Handle)) {
				using (var p = new Pen(BorderColor)) {
					using (var brush = new SolidBrush(BackColor)) {
						g.FillRectangle(brush, ClientRectangle);
					}
					g.DrawRectangle(p, ClientRectangle);
					g.DrawLine(p, Width - buttonWidth - 2, 0, Width - buttonWidth - 2, Height);
				}
			}
		}*/
		public YobaComboBox() : base() {
			BackColor = Color.FromArgb(40, 40, 41);
			DropDownStyle = ComboBoxStyle.DropDownList;
			FlatStyle = FlatStyle.Flat;
			//SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
			Font = new Font("Verdana", 12F, FontStyle.Regular, GraphicsUnit.Pixel, 204);
			ForeColor = Color.White;
			FormattingEnabled = true;
			//DrawMode = DrawMode.OwnerDrawFixed;
		}
	}

	internal class YobaProgressBar : Panel {

		private int value = 0;
		private int maxValue = 100;
		private bool overflow = false;
		private Pen penBG_ = new Pen(Color.FromArgb(62, 63, 64), 1);
		private Pen penBorder_ = new Pen(Color.FromArgb(100, 101, 102), 1);
		private Pen penValue_ = new Pen(Color.FromArgb(10, 154, 57), 1);
		private Pen penOverflow_ = new Pen(Color.FromArgb(12, 113, 160), 1);

		public YobaProgressBar() : base() {
			BackColor = Color.FromArgb(62, 63, 64);
			Location = new Point(220, 369);
			Margin = new Padding(0);
			Size = new Size(311, 24);
			TabIndex = 112;
		}

		protected override void OnPaint(PaintEventArgs e) {
			base.OnPaint(e);

			Rectangle rect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
			e.Graphics.DrawRectangle(penBorder_, rect);
			int valueWidth;
			if (value > maxValue) {
				overflow = true;
				value = value % maxValue;
				if (value == 0) {
					value = maxValue;
				}
			}
			rect = new Rectangle(1, 1, ClientSize.Width - 2, ClientSize.Height - 2);

			if (value == maxValue) {
				e.Graphics.FillRectangle((overflow ? penOverflow_ : penValue_).Brush, rect);
			}
			else {
				e.Graphics.FillRectangle(penBG_.Brush, rect);
				valueWidth = (ClientSize.Width - 2) / maxValue * value;
				if (valueWidth > 0) {
					e.Graphics.FillRectangle((overflow ? penOverflow_ : penValue_).Brush, new Rectangle(1, 1, valueWidth, ClientSize.Height - 2));
				}
			}
		}

		public int Value {
			get => value;
			set {
				if (!overflow) {
					if (value == this.value) {
						return;
					}
					else if (value < 0) {
						if (this.value > 0) {
							this.value = 0;
							this.Update();
						}
						return;
					}
				}
				else if (value < 0) {
					this.value -= value;
					if (this.value < 0) {
						this.value = maxValue - this.value % maxValue;
					}
					this.Update();
					return;
				}
				overflow = false;
				this.value = value;
				this.Refresh();
			}
		}
		public int MaxValue {
			get => maxValue;
			set {
				if (value == this.maxValue) {
					return;
				}
				if (value < 1) {
					throw new Exception("The maxinun value of a progress bar cannot be less than 1.");
				}
				this.maxValue = value;
				this.Update();
			}
		}
	}
}