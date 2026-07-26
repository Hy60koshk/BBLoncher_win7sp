using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using IWshRuntimeLibrary;
using IOFile = System.IO.File;
using System.Runtime.InteropServices;
using System.IO;

namespace YobaLoncher {
	class SettingsDialog : YobaDialog {
		private readonly MainForm _mainForm;
		private readonly TextBox _gamePath;
		private readonly YobaComboBox _openingPanelCB;
		private readonly CheckBox _launchViaGalaxy;
		private readonly CheckBox _offlineMode;
		private readonly CheckBox _closeLauncherOnLaunch;
		private readonly CommonOpenFileDialog _folderBrowserDialog;
		//private YobaButton openingPanelCB;

		public string GamePath => _gamePath.Text;
		public StartPageEnum OpeningPanel => (StartPageEnum)_openingPanelCB.SelectedIndex;
		public bool LaunchViaGalaxy => _launchViaGalaxy.Checked;
		public bool OfflineMode => _offlineMode.Checked;
		public bool CloseOnLaunch => _closeLauncherOnLaunch.Checked;

		private UninstallationRules _urules;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		extern static bool DestroyIcon(IntPtr handle);

		public SettingsDialog(MainForm mainForm) : base(new Size(480, 460), new UIElement[] {
			new UIElement() {
				Caption = Locale.Get("Cancel")
				, Result = DialogResult.Cancel
			}
			, new UIElement() {
				Caption = Locale.Get("Apply")
				, Result = DialogResult.OK
			}
		}) {
			_mainForm = mainForm;
			Text = Locale.Get("SettingsTitle");

			SuspendLayout();

			_folderBrowserDialog = new CommonOpenFileDialog() {
				IsFolderPicker = true
			};

			ToolTip theToolTip = new ToolTip();
			theToolTip.AutoPopDelay = 10000;
			theToolTip.InitialDelay = 200;
			theToolTip.ReshowDelay = 100;

			Label gamePathLabel = new Label();
			gamePathLabel.Text = Locale.Get("SettingsGamePath");
			gamePathLabel.Font = new Font("Tahoma", 12F, FontStyle.Regular, GraphicsUnit.Pixel, 204);
			gamePathLabel.Location = new Point(18, 22);
			gamePathLabel.Size = new Size(444, 40);

			_gamePath = new TextBox();
			_gamePath.Text = Program.GamePath;
			_gamePath.Font = new Font("Lucida Sans Unicode", 12F, FontStyle.Regular, GraphicsUnit.Pixel, 204);
			_gamePath.Location = new Point(2, 4);
			_gamePath.Size = new Size(359, 20);
			_gamePath.BackColor = Color.FromArgb(32, 33, 34);
			_gamePath.BorderStyle = BorderStyle.None;
			_gamePath.ForeColor = Color.White;
			YU.AssertLucida(_gamePath);

			YobaButton browseButton = new YobaButton();
			browseButton.Location = new Point(385, 44);
			browseButton.Name = "browseButton";
			browseButton.Size = new Size(75, 27);
			browseButton.Text = Locale.Get("Browse");
			browseButton.UseVisualStyleBackColor = false;
			browseButton.Click += new System.EventHandler(_browseButton_Click);

			Panel fieldBackground = new Panel();
			fieldBackground.BackColor = Color.FromArgb(32, 33, 34);
			fieldBackground.BorderStyle = BorderStyle.FixedSingle;
			fieldBackground.Controls.Add(_gamePath);
			fieldBackground.Location = new Point(20, 44);
			fieldBackground.Name = "fieldBackground";
			fieldBackground.Size = new Size(361, 27);

			_launchViaGalaxy = new CheckBox();
			_launchViaGalaxy.Text = Locale.Get("SettingsGogGalaxy");
			_launchViaGalaxy.Font = new Font("Tahoma", 12F, FontStyle.Regular, GraphicsUnit.Pixel, 204);
			_launchViaGalaxy.Location = new Point(20, 86);
			_launchViaGalaxy.Size = new Size(440, 24);
			_launchViaGalaxy.Checked = LauncherConfig.LaunchFromGalaxy;
			_launchViaGalaxy.BackColor = Color.Transparent;
			_launchViaGalaxy.Enabled = LauncherConfig.GalaxyDir != null;
			
			_offlineMode = new CheckBox();
			_offlineMode.Text = Locale.Get("SettingsOfflineMode");
			_offlineMode.Font = new Font("Tahoma", 12F, FontStyle.Regular, GraphicsUnit.Pixel, 204);
			_offlineMode.Location = new Point(20, 119);
			_offlineMode.Size = new Size(440, 24);
			_offlineMode.Checked = LauncherConfig.StartOffline;
			_offlineMode.BackColor = Color.Transparent;

			theToolTip.SetToolTip(_offlineMode, Locale.Get("SettingsOfflineModeTooltip"));

			_closeLauncherOnLaunch = new CheckBox();
			_closeLauncherOnLaunch.Text = Locale.Get("SettingsCloseOnLaunch");
			_closeLauncherOnLaunch.Font = new Font("Tahoma", 12F, FontStyle.Regular, GraphicsUnit.Pixel, 204);
			_closeLauncherOnLaunch.Location = new Point(20, 152);
			_closeLauncherOnLaunch.Size = new Size(440, 24);
			_closeLauncherOnLaunch.Checked = LauncherConfig.CloseOnLaunch;
			_closeLauncherOnLaunch.BackColor = Color.Transparent;

			Label openingPanelLabel = new Label();
			openingPanelLabel.Text = Locale.Get("SettingsOpeningPanel");
			openingPanelLabel.Font = new Font("Tahoma", 12F, FontStyle.Regular, GraphicsUnit.Pixel, 204);
			openingPanelLabel.Location = new Point(18, 187);
			openingPanelLabel.Size = new Size(444, 40);

			_openingPanelCB = new YobaComboBox();
			_openingPanelCB.Location = new Point(20, 208);
			_openingPanelCB.Name = "openingPanel";
			_openingPanelCB.Size = new Size(440, 22);
			_openingPanelCB.DataSource = new string[] {
				Locale.Get("SettingsOpeningPanelChangelog")
				, Locale.Get("SettingsOpeningPanelStatus")
				, Locale.Get("SettingsOpeningPanelLinks")
				//, Locale.Get("SettingsOpeningPanelMods")
			};
			/*openingPanelCB = new YobaButton();
			openingPanelCB.Location = new Point(20, 141);
			openingPanelCB.Name = "openingPanel";

			YobaButton opt1 = new YobaComboBox();
			opt1.Location = new Point(20, 141);
			Size = new Size(440, 28);

			Panel cbDD = new Panel();
			cbDD.BackColor = Color.FromArgb(40, 40, 41);
			cbDD.BorderStyle = BorderStyle.FixedSingle;
			cbDD.Controls.Add(gamePath);
			cbDD.Location = new Point(20, 141 + openingPanelCB.Height);
			cbDD.Name = "cbDD";
			cbDD.Size = new Size(361, openingPanelCB.Height * 3);
			*/
			YobaButton makeBackupBtn = new YobaButton();
			makeBackupBtn.MouseClick += _makeBackupBtn_MouseClick;
			makeBackupBtn.Location = new Point(20, 246);
			makeBackupBtn.Size = new Size(240, 24);
			makeBackupBtn.Text = Locale.Get("SettingsMakeBackup");

			YobaButton createShortcutBtn = new YobaButton();
			createShortcutBtn.MouseClick += _createShortcutBtn_MouseClick;
			createShortcutBtn.Location = new Point(20, 278);
			createShortcutBtn.Size = new Size(240, 24);
			createShortcutBtn.Text = Locale.Get("SettingsCreateShortcut");

			YobaButton openFolderBtn = new YobaButton();
			openFolderBtn.MouseClick += _openFolderBtn_MouseClick;
			openFolderBtn.Location = new Point(20, 310);
			openFolderBtn.Size = new Size(240, 24);
			openFolderBtn.Text = Locale.Get("SettingsOpenDataFolder");

			YobaButton uninstallLoncherBtn = new YobaButton();
			uninstallLoncherBtn.MouseClick += _uninstallLoncherBtn_MouseClick;
			uninstallLoncherBtn.Location = new Point(20, 358);
			uninstallLoncherBtn.Size = new Size(240, 24);
			uninstallLoncherBtn.Text = Locale.Get("SettingsUninstallLoncher");

			_gamePath.TabIndex = 1;
			browseButton.TabIndex = 2;
			_launchViaGalaxy.TabIndex = 3;
			_offlineMode.TabIndex = 4;
			_closeLauncherOnLaunch.TabIndex = 5;
			
			_openingPanelCB.TabIndex = 10;
			makeBackupBtn.TabIndex = 15;
			createShortcutBtn.TabIndex = 16;
			uninstallLoncherBtn.TabIndex = 30;

			Controls.Add(fieldBackground);
			Controls.Add(browseButton);
			Controls.Add(_launchViaGalaxy);
			Controls.Add(_offlineMode);
			Controls.Add(_closeLauncherOnLaunch);
			Controls.Add(_openingPanelCB);
			Controls.Add(makeBackupBtn);
			Controls.Add(createShortcutBtn);
			Controls.Add(openFolderBtn);
			Controls.Add(uninstallLoncherBtn);

			/*urules_ = Program.LoncherSettings.UninstallationRules;
			if (urules_.FilesToDelete != null && urules_.FilesToDelete.Count > 0) {
				YobaButton uninstallRussifierBtn = new YobaButton();
				uninstallRussifierBtn.MouseClick += UninstallRussifierBtn_MouseClick;
				uninstallRussifierBtn.Location = new Point(20, 366);
				uninstallRussifierBtn.Size = new Size(240, 24);
				uninstallRussifierBtn.Text = Locale.Get("SettingsUninstallMainProduct");
				uninstallRussifierBtn.TabIndex = 32;
				Controls.Add(uninstallRussifierBtn);
			}*/

			Controls.Add(openingPanelLabel);
			Controls.Add(gamePathLabel);

			Load += new EventHandler((object o, EventArgs a) => {
				_openingPanelCB.SelectedIndex = (int)LauncherConfig.StartPage;
			});
			ResumeLayout();
		}

		private void _createShortcutBtn_MouseClick(object sender, MouseEventArgs e) {
			try {
				string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Запускатр Боевых Братьев.lnk";
				if (!IOFile.Exists(filename)) {
					WshShell wsh = new WshShell();
					IWshShortcut shortcut = wsh.CreateShortcut(filename) as IWshShortcut;
					shortcut.Arguments = "";
					shortcut.TargetPath = Application.ExecutablePath;
					shortcut.WorkingDirectory = Program.LONCHER_PATH;
					shortcut.WindowStyle = 1;
					string iconFile = Program.LONCHER_DATA_PATH + "shortcutIcon.ico";
					bool validIconFile = IOFile.Exists(iconFile);
					if (!validIconFile) {
						string exename = Program.GamePath + Program.LoncherSettings.ExeName;

						if (IOFile.Exists(PreloaderForm.ICON_FILE)) {
							PngIconConverter.Convert(PreloaderForm.ICON_FILE, iconFile);
							validIconFile = true;
						}
						else if (IOFile.Exists(exename) && exename.EndsWith(".exe")) {
							Icon exeIcon = Icon.ExtractAssociatedIcon(exename);
							if (exeIcon != null) {
								Bitmap exeBmp = exeIcon.ToBitmap();
								PngIconConverter.Convert(exeBmp, iconFile);
								validIconFile = true;
							}
						}
					}
					if (validIconFile) {
						shortcut.IconLocation = iconFile;
					}
					shortcut.Save();
				}
			} catch (Exception ex) {
				YobaDialog.ShowDialog(ex.Message);
			}
		}

		private void _makeBackupBtn_MouseClick(object sender, MouseEventArgs e) {
			string bkpdir = Program.GamePath + "_loncher_backups\\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "\\";
			if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("SettingsMakeBackupInfo"), bkpdir), YesNoBtns)) {
				try {
					string origDir = Program.GamePath;
					if (!Directory.Exists(bkpdir)) {
						Directory.CreateDirectory(bkpdir);
					}

					List<string> dirs = new List<string>();

					void backupFile(FileInfo fi) {
						string path = fi.Path.Replace('/', '\\');
						int fileNameStart = path.LastIndexOf('\\');
						if (fileNameStart > 0) {
							string dir = path.Substring(0, fileNameStart);
							if (!dirs.Contains(dir)) {
								if (!Directory.Exists(bkpdir + dir)) {
									Directory.CreateDirectory(bkpdir + dir);
								}
								dirs.Add(dir);
							}
						}
						if (IOFile.Exists(origDir + path)) {
							IOFile.Copy(origDir + path, bkpdir + path);
						}
					}

					GameVersion gameVersion = Program.LoncherSettings.GameVersion;
					foreach (FileGroup fg in gameVersion.FileGroups) {
						foreach (FileInfo fi in fg.Files) {
							backupFile(fi);
						}
					}
					foreach (FileInfo fi in gameVersion.Files) {
						backupFile(fi);
					}
					YobaDialog.ShowDialog(String.Format(Locale.Get("SettingsMakeBackupDone"), bkpdir));
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}
		}

		private void _browseButton_Click(object sender, EventArgs e) {
			_folderBrowserDialog.InitialDirectory = _gamePath.Text;
			if (_folderBrowserDialog.ShowDialog() == CommonFileDialogResult.Ok) {
				_gamePath.Text = _folderBrowserDialog.FileName;
			}
		}

		private void _openFolderBtn_MouseClick(object sender, EventArgs e) {
			YU.RunCommand("/C explorer \"" + Program.GamePath + "data\"");
		}

		private void _openingPanelCB_DrawItem(object sender, DrawItemEventArgs e) {
			int index = e.Index >= 0 ? e.Index : 0;
			using (SolidBrush brush = new SolidBrush(_openingPanelCB.BackColor)) {
				e.DrawBackground();
				e.Graphics.DrawString(_openingPanelCB.Items[index].ToString(), e.Font, brush, e.Bounds, StringFormat.GenericDefault);
				e.DrawFocusRectangle();
			}
		}

		private void _uninstallRussifierBtn_MouseClick(object sender, EventArgs e) {
			try {
				string msg = Locale.Get("ProductUninstallationConfirmation") + ":";
				foreach (FileInfo fi in _urules.FilesToDelete) {
					if (IOFile.Exists(Program.GamePath + fi.Path)) {
						msg += "\r\n" + Program.GamePath + fi.Path;
					}
				}
				if (YobaDialog.ShowDialog(msg, YobaDialog.YesNoBtns) == DialogResult.Yes) {
					foreach (FileInfo fi in _urules.FilesToDelete) {
						if (IOFile.Exists(Program.GamePath + fi.Path)) {
							IOFile.Delete(Program.GamePath + fi.Path);
						}
					}
				}
			}
			catch (Exception ex) {
				YobaDialog.ShowDialog(ex.Message);
			}
		}

		private void _uninstallLoncherBtn_MouseClick(object sender, EventArgs e) {
			try {
				string msg = Locale.Get("LoncherUninstallationConfirmation");
				if (YobaDialog.ShowDialog(msg, YobaDialog.YesNoBtns) == DialogResult.Yes) {
					if (Directory.Exists(Program.LONCHER_DATA_PATH)) {
						Directory.Delete(Program.LONCHER_DATA_PATH, true);
					}
					YU.RunCommand(String.Format("/C choice /C Y /N /D Y /T 1 & Del \"{0}\"", Application.ExecutablePath));
					Application.Exit();
				}
			}
			catch (Exception ex) {
				YobaDialog.ShowDialog(ex.Message);
			}
		}
	}
}
