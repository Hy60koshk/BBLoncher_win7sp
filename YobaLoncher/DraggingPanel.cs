using System;
using System.Drawing;
using System.Windows.Forms;

namespace YobaLoncher {

	internal interface DraggableForm {
		void InitDrag();
		void ToggleMaximized();
	}

	internal class DraggingPanel<T> : Panel where T : DraggableForm {
		public int WidthSpace = 128;
		private int _initWidth = 200;
		private int _initHeight = 24;

		private DateTime _lastClickTime = DateTime.Now;
		private int _lastClickX = 0;
		private int _lastClickY = 0;
		private T _parent = default(T);

		public void UpdateSize(int w, int h) {
			_initWidth = w;
			_initHeight = h;
			UpdateSize();
		}
		public void UpdateWidth(int w) {
			_initWidth = w;
			UpdateSize();
		}
		public void UpdateHeight(int h) {
			_initHeight = h;
			UpdateSize();
		}
		public void UpdateSize() {
			int z = LauncherConfig.ZoomPercent;
			int w = _initWidth - 4;
			int h = _initHeight;
			if (z == 100) {
				w -= WidthSpace;
			}
			else {
				w -= (int)Math.Floor((double)WidthSpace / 100 * z);
				h = (int)Math.Floor((double)_initHeight / 100 * z);
			}
			if (w < 0) {
				w = 0;
			}
			this.Size = new Size(w, h);
		}

		public void AddDragging(T parent) {
			_parent = parent;
			this.MouseDown += new MouseEventHandler(draggingPanel_MouseDown);
		}

		private void draggingPanel_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				DateTime now = DateTime.Now;

				bool isDouble = (now - _lastClickTime).TotalMilliseconds <= SystemInformation.DoubleClickTime
					&& Math.Abs(e.X - _lastClickX) <= SystemInformation.DoubleClickSize.Width
					&& Math.Abs(e.Y - _lastClickY) <= SystemInformation.DoubleClickSize.Height;

				_lastClickTime = now;
				_lastClickX = e.Location.X;
				_lastClickY = e.Location.Y;

				if (isDouble) {
					_parent.ToggleMaximized();
					return; // IMPORTANT: don't start drag
				}

				_parent.InitDrag();
			}
		}
	}
}