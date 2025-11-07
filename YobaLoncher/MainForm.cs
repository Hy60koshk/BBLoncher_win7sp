using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using SHDocVw;

namespace YobaLoncher {
	public partial class MainForm : Form {
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
		[DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		private DownloadProgressTracker downloadProgressTracker_;
		public string ThePath = "";
		private LinkedList<FileInfo> filesToUpload_;
		private LinkedList<ModInfo> modsToUpdate_;
		private LinkedListNode<FileInfo> currentFile_ = null;
		private bool ReadyToGo_ = false;
		private bool LaunchButtonEnabled_ = false;
		private volatile bool UpdateInProgress_ = false;

		private long lastDlstringUpdate_ = -1;

		private ProgressBarInfo progressBarInfo_ = new ProgressBarInfo();

		private WebClient wc_ = null;
		private WebClient WC {
			get {
				if (wc_ is null) {
					wc_ = new WebClient { Encoding = Encoding.UTF8 };
					wc_.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnDownloadProgressChanged);
					wc_.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadCompleted);
				}
				return wc_;
			}
		}

		internal class ProgressBarInfo {
			public int MaxValue = 1000;
			public int Progress = 0;
			public string Caption = "";
		}

		public MainForm() {
			ThePath = Program.GamePath;

			InitializeComponent();

			//int winstyle = NativeWinAPI.GetWindowLong(basePanel.Handle, NativeWinAPI.GWL_EXSTYLE);
			//NativeWinAPI.SetWindowLong(basePanel.Handle, NativeWinAPI.GWL_EXSTYLE, winstyle | NativeWinAPI.WS_EX_COMPOSITED);

			SuspendLayout();

			if (!Program.OfflineMode) {
				downloadProgressTracker_ = new DownloadProgressTracker(50, TimeSpan.FromMilliseconds(500));
			}

			int missingFilesCount = Program.GameFileCheckResult.InvalidFiles.Count;

			if (missingFilesCount > 0) {
				filesToUpload_ = Program.GameFileCheckResult.InvalidFiles;
				bool allowRun = true;
				foreach (FileInfo fi in filesToUpload_) {
					if (fi.Importance < 2 && (!fi.IsPresent || (!fi.IsOK && fi.Importance < 1))) {
						allowRun = false;
					}
					else {
						missingFilesCount--;
					}
				}
				SetReady(allowRun);
			}
			else {
				SetReady(true);
			}
			progressBarInfo_.Caption = ReadyToGo_ ? Locale.Get("AllFilesIntact") : String.Format(Locale.Get("FilesMissing"), missingFilesCount);

			Text = Locale.Get("MainFormTitle");

			int windowH = LauncherConfig.WindowHeight;
			int windowW = LauncherConfig.WindowWidth;
			if (windowH > this.MaximumSize.Height) {
				windowH = this.MaximumSize.Height;
			}
			else if (windowH < this.MinimumSize.Height) {
				windowH = this.MinimumSize.Height;
			}
			if (windowW > this.MaximumSize.Width) {
				windowW = this.MaximumSize.Width;
			}
			else if (windowW < this.MinimumSize.Width) {
				windowW = this.MinimumSize.Width;
			}
			this.ClientSize = new Size(windowW, windowH);
			mainBrowser.Size = new Size(windowW - 4, windowH - 2);
			draggingPanel.UpdateSize(windowW, 24);
			mainBrowser.Location = new Point(1, 0);
			mainBrowser.ObjectForScripting = YobaWebController.Instance;
			YobaWebController.Instance.Form = this;
			mainBrowser.Navigated += MainBrowser_Navigated;
			mainBrowser.Navigating += webBrowser_Navigating;
			UpdateMainWebView();

			BackgroundImageLayout = ImageLayout.Stretch;
			BackgroundImage = Program.LoncherSettings.Background;

			PerformLayout();
		}

		public void CheckModUpdate(string modId) {
			if (!UpdateInProgress_) {
				return;
			}
			ModInfo mi = Program.LoncherSettings.AvailableMods.Find(m => m.Id == modId);
			if (mi == null) {
				YobaDialog.ShowDialog(String.Format(Locale.Get("ModNotFound")));
				return;
			}
			
			uint outdatedmodssize = 0;
			if (mi.LatestVersion != mi.CurrentVersion) {
				foreach (FileInfo mif in mi.FilesForLatestVersion) {
					mif.IsOK = FileChecker.CheckFileMD5(mif);
					if (!mif.IsOK) {
						outdatedmodssize += mif.Size;
					}
				}
			}
			else {
				foreach (FileInfo mif in mi.FilesForCurrentVersion) {
					if (!mif.IsOK) {
						outdatedmodssize += mif.Size;
					}
				}
			}
			if (outdatedmodssize < 1) {
				if (Program.OfflineMode) {
					YobaDialog.ShowDialog(String.Format(Locale.Get("LauncherIsInOfflineMode")));
				}
				else {
					if (DialogResult.Yes == YobaDialog.ShowDialog(
							String.Format(Locale.Get("YouHaveOutdatedMods"), mi.VersionedName, mi.LatestVersion.VersionName, YU.formatFileSize(outdatedmodssize))
							, YobaDialog.YesNoBtns)) {
						if (!UpdateInProgress_) {
							modsToUpdate_ = new LinkedList<ModInfo>();
							modsToUpdate_.AddLast(mi);
							mi.DlInProgress = true;
							UpdateModsWebView();
							DownloadNextMod();
						}
					}
				}
			}
		}

		public void CheckModUpdates() {
			CheckModUpdates(false);
		}

		public bool CheckModUpdates(bool isBeforeRun) {
			bool allowRun = true;
			LinkedList<ModInfo> outdatedMods = new LinkedList<ModInfo>();
			LinkedList<ModInfo> brokenMods = new LinkedList<ModInfo>();
			LinkedList<ModInfo> criticalMods = new LinkedList<ModInfo>();
			ulong outdatedModsSize = 0;
			ulong brokenModsSize = 0;
			List<ModInfo> availableMods = Program.LoncherSettings.AvailableMods;
			List<ModInfo> allMods = Program.LoncherSettings.Mods;
			
			foreach (ModInfo mi in availableMods) {
				if (mi.IsActive) {
					bool hasIt = false;
					if (mi.LatestVersion != mi.CurrentVersion) {
						outdatedMods.AddLast(mi);
						mi.IsUpdateAvailable = true;
						foreach (FileInfo mif in mi.FilesForLatestVersion) {
							mif.IsOK = FileChecker.CheckFileMD5(mif);
							if (!mif.IsOK) {
								outdatedModsSize += mif.Size;
							}
						}
						foreach (FileInfo mif in mi.FilesForCurrentVersion) {
							mif.IsOK = FileChecker.CheckFileMD5(mif);
							if (!mif.IsOK) {
								brokenModsSize += mif.Size;
								if (!hasIt) {
									hasIt = true;
									brokenMods.AddLast(mi);
								}
								if (!mif.IsPresent && criticalMods.Last?.Value != mi) {
									criticalMods.AddLast(mi);
								}
							}
						}
					}
					else {
						foreach (FileInfo mif in mi.FilesForLatestVersion) {
							if (!mif.IsOK) {
								brokenModsSize += mif.Size;
								if (!hasIt) {
									hasIt = true;
									brokenMods.AddLast(mi);
									mi.IsUpdateAvailable = true;
								}
								if (!mif.IsPresent && criticalMods.Last?.Value != mi) {
									criticalMods.AddLast(mi);
								}
							}
						}
					}
				}
				else {
					bool modIsIntact = true;
					foreach (FileInfo fi in mi.FilesForLatestVersion) {
						if (!fi.IsOK) {
							modIsIntact = false;
							break;
						}
					}
					if (modIsIntact) {
						YobaDialog.ShowDialog(String.Format(Locale.Get("ModDetected"), mi.VersionedName));
						mi.Install();
					}
				}
			}

			UpdateModsWebView();

			if (outdatedModsSize + brokenModsSize > 0) {
				if (Program.OfflineMode) {
					if (criticalMods.Count > 0) {
						string critBrokenModsStr = "";
						foreach (ModInfo mi in criticalMods) {
							critBrokenModsStr += "\r\n" + mi.VersionedName;
						}
						if (isBeforeRun) {
							allowRun = DialogResult.Yes == YobaDialog.ShowDialog(
								String.Format(Locale.Get("YouHaveOutdatedModsAndMissingFilesOffline"), critBrokenModsStr)
								, YobaDialog.YesNoBtns
							);
						}
						if (DialogResult.Yes == YobaDialog.ShowDialog(
							String.Format(Locale.Get("YouHaveBrokenModsOffline"), critBrokenModsStr)
							, YobaDialog.YesNoBtns
						)) {
							foreach (ModInfo mi in criticalMods) {
								mi.Disable();
							}
							UpdateModsWebView();
						}
					}
				}
				else {
					string outdatedModsStr = "";
					string brokenModsStr = "";
					LinkedList<ModInfo> modsToUpdate;
					LinkedList<ModInfo> modsToFix;
					foreach (ModInfo mi in outdatedMods) {
						outdatedModsStr += "\r\n" + mi.VersionedName + " → " + mi.LatestVersion.VersionName;
					}
					foreach (ModInfo mi in brokenMods) {
						brokenModsStr += "\r\n" + mi.VersionedName;
					}
					string locKey;
					UIElement[] btns = null;
					if (outdatedMods.Count > 0) {
						if (brokenMods.Count > 0) {
							locKey = criticalMods.Count > 0 ? "YouHaveOutdatedAndBrokenModsAndMissingFiles" : "YouHaveOutdatedAndBrokenMods";
							modsToUpdate = outdatedMods;
							modsToFix = brokenMods;
						}
						else if (criticalMods.Count > 0) {
							// is impossible because of brokenMods
							locKey = "YouHaveOutdatedModsAndMissingFiles";
							modsToUpdate = outdatedMods;
							modsToFix = criticalMods;
						}
						else {
							locKey = "YouHaveOutdatedMods";
							btns = YobaDialog.YesNoBtns;
							modsToUpdate = outdatedMods;
							modsToFix = null;
						}
						if (btns == null) {
							btns = new UIElement[] {
								new UIElement() {
									Caption = Locale.Get("ModsUpdateAll"), Result = DialogResult.Yes
								},
								new UIElement() {
									Caption = Locale.Get("ModsFixesOnly"), Result = DialogResult.OK
								},
								new UIElement() {
									Caption = Locale.Get("ModsSkipAll"), Result = DialogResult.No
								}
							};
						}
					}
					else {
						locKey = criticalMods.Count > 0 ? "YouHaveBrokenModsAndMissingFiles" : "YouHaveBrokenMods";
						btns = YobaDialog.YesNoBtns;
						modsToUpdate = null;
						modsToFix = brokenMods;
					}
					DialogResult modUpdateDlgRes = YobaDialog.ShowDialog(
						String.Format(
							Locale.Get(locKey)
							, brokenModsStr
							, YU.formatFileSize(brokenModsSize)
							, outdatedModsStr
							, YU.formatFileSize(outdatedModsSize)
						)
						, btns
					);

					if (DialogResult.No != modUpdateDlgRes) {
						if (DialogResult.Yes == modUpdateDlgRes) {
							modsToUpdate_ = new LinkedList<ModInfo>(outdatedMods.Except(brokenMods).Concat(brokenMods));
							foreach (ModInfo mi in outdatedMods) {
								mi.MarkedForUpdate = true;
							}
						}
						else {
							modsToUpdate_ = brokenMods;
						}
						foreach (ModInfo mi in modsToUpdate_) {
							mi.DlInProgress = true;
						}
						UpdateModsWebView();
						if (!UpdateInProgress_) {
							DownloadNextMod();
						}
						allowRun = false;
					}
				}
			}
			return allowRun;
		}

		internal bool CheckDepsAndConflicts() {
			List<ModInfo> availableMods = Program.LoncherSettings.AvailableMods;
			foreach (ModInfo mi in availableMods) {
				if (mi.IsActive) {
					if (mi.Dependencies != null && mi.Dependencies.Count > 0) {
						foreach (string[] deps in mi.Dependencies) {
							if (deps != null && deps.Length > 0) {
								bool hasDeps = false;
								List<string> availDeps = new List<string>();
								foreach (string dep in deps) {
									ModInfo depmi = availableMods.Find(x => x.Id.Equals(dep));
									if (depmi != null) {
										if (depmi.IsActive) {
											hasDeps = true;
											break;
										}
										else {
											availDeps.Add(depmi.VersionedName);
										}
									}
								}
								if (!hasDeps) {
									if (availDeps.Count > 1) {
										YobaDialog.ShowDialog(String.Format(Locale.Get("ModHasDependencies"), mi.VersionedName, string.Join("\r\n", availDeps)));
									}
									else if (availDeps.Count > 0) {
										YobaDialog.ShowDialog(String.Format(Locale.Get("ModHasDependency"), mi.VersionedName, availDeps[0]));
									}
									else {
										YobaDialog.ShowDialog(String.Format(Locale.Get("ModHasDependenciesButNoneAvailable"), mi.VersionedName));
									}
									return false;
								}
							}
						}
					}
					if (mi.Conflicts != null && mi.Conflicts.Count > 0) {
						List<string> activeConflicts = new List<string>();
						foreach (string conflict in mi.Conflicts) {
							ModInfo conmi = availableMods.Find(x => x.Id.Equals(conflict));
							if (conmi != null && conmi.IsActive) {
								activeConflicts.Add(conmi.VersionedName);
							}
						}
						if (activeConflicts.Count > 0) {
							YobaDialog.ShowDialog(String.Format(Locale.Get("ModHasConflicts"), mi.VersionedName, string.Join("\r\n", activeConflicts)));
							return false;
						}
					}
				}
			}
			return true;
		}

		/*public bool AnalyzeModsBeforeRun() {
			LinkedList<ModInfo> outdatedMods = new LinkedList<ModInfo>();
			ulong outdatedmodssize = 0;
			bool isAllPresent = true;
			bool allowRun = true;
			List<ModInfo> availableMods = Program.LoncherSettings.AvailableMods;
			List<ModInfo> allMods = Program.LoncherSettings.Mods;
			

			foreach (ModInfo mi in availableMods) {
				if (mi.IsActive) {
					bool hasIt = false;
					foreach (FileInfo mif in mi.FilesForLatestVersion) {
						if (!mif.IsOK) {
							outdatedmodssize += mif.Size;
							if (!hasIt) {
								hasIt = true;
								outdatedMods.AddLast(mi);
								mi.IsUpdateAvailable = true;
							}
							if (!mif.IsPresent) {
								isAllPresent = false;
							}
						}
					}
					CheckDepsAndConflicts(mi);
				}
				else {
					bool modIsIntact = true;
					foreach (FileInfo fi in mi.FilesForLatestVersion) {
						if (!fi.IsOK) {
							modIsIntact = false;
							break;
						}
					}
					if (modIsIntact) {
						YobaDialog.ShowDialog(String.Format(Locale.Get("ModDetected"), mi.VersionedName));
						mi.Install();
						CheckDepsAndConflicts(mi);
					}
				}
			}
			UpdateModsWebView();
			if (outdatedMods.Count > 0) {
				string outdatedModsStr = "";
				foreach (ModInfo mi in outdatedMods) {
					outdatedModsStr += "\r\n" + mi.VersionedName;
				}
				if (Program.OfflineMode) {
					allowRun = DialogResult.Yes == YobaDialog.ShowDialog(
						String.Format(Locale.Get("YouHaveOutdatedModsAndMissingFilesOffline"), outdatedModsStr)
						, YobaDialog.YesNoBtns
					);
				}
				else {
					if (DialogResult.Yes == YobaDialog.ShowDialog(
							String.Format(Locale.Get(isAllPresent ? "YouHaveOutdatedMods" : "YouHaveOutdatedModsAndMissingFiles"), outdatedModsStr, YU.formatFileSize(outdatedmodssize))
							, YobaDialog.YesNoBtns)) {
						modsToUpdate_ = outdatedMods;
						foreach (ModInfo mi in outdatedMods) {
							mi.DlInProgress = true;
						}
						UpdateModsWebView();
						if (!UpdateInProgress_) {
							DownloadNextMod();
						}
						return false;
					}
				}
			}
			return allowRun;
		}*/

		private void MainBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e) {
			mainBrowser.Visible = true;
			minimizeButton.Visible = false;
			closeButton.Visible = false;
			helpButton.Visible = false;
#if DEBUG
			refreshButton.Visible = true;
#endif
			SetBrowserZoom(LauncherConfig.ZoomPercent);
		}

		public int SetBrowserZoom(int zoom) {
			try {
				// https://stackoverflow.com/a/52255558
				//The value should be between 10 and 1000
				if (zoom > 180) {
					zoom = 180;
				}
				else if (zoom < 50) {
					zoom = 50;
				}
				((IWebBrowser2)mainBrowser.ActiveXInstance).ExecWB(
					OLECMDID.OLECMDID_OPTICAL_ZOOM, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER
					, zoom, zoom);
				LauncherConfig.ZoomPercent = zoom;
				draggingPanel.UpdateSize();
			}
			catch (Exception ex) {
				if (YobaDialog.ShowDialog(ex.Message, YobaDialog.OKCopyStackBtns) == DialogResult.Retry) {
					YU.CopyExceptionToClipboard(ex);
				}
			}
			return zoom;
		}

		private void UpdateStatusWebView() {
			GameVersion gameVersion = Program.LoncherSettings.GameVersion;
			gameVersion.ResetCheckedToDl();
			string gvstr = JsonConvert.SerializeObject(gameVersion);
			string result = new StringBuilder("{\"GameVersion\":", gvstr.Length + 100)
				.Append(gvstr)
				.Append(",\"LaunchBtn\":")
				.Append(GetLaunchButtonStateJson())
				.Append("}").ToString();

			RunScript("__updateStatusView", new object[] { result });
			return;
		}

		private void UpdateLaunchButton() {
			RunScript("__updateLaunchBtn", new object[] { GetLaunchButtonStateJson() });
			return;
		}

		private string GetLaunchButtonStateJson() {
			return "{\"IsReady\":" + (ReadyToGo_ ? "true" : "false")
				+ ",\"Enabled\":" + (LaunchButtonEnabled_ ? "true" : "false") + "}";
		}

		private void UpdateModsWebView() {
			YobaWebController.Instance.UpdateModList();
			List<WebModInfo> modList = YobaWebController.Instance.ModList;
			if (!LauncherConfig.ShowHiddenMods) {
				modList = modList.FindAll(x => !x.IsHidden);
			}
			string modstr = "{ \"ModList\": " + JsonConvert.SerializeObject(modList)
				+ ", \"ModGroups\": " + JsonConvert.SerializeObject(Program.LoncherSettings.ModGroups)
				+ ", \"ModsCompactMode\": " + (LauncherConfig.ModsCompactMode ? "true" : "false") + "}";
			RunScript("__updateModsView", new object[] { modstr });
		}

		private void RunScript(string funcName, object[] args) {
			if (mainBrowser.Visible) {
				mainBrowser.Document.InvokeScript(funcName, args);
			}
		}
		private void RunScript(string funcName) {
			if (mainBrowser.Visible) {
				mainBrowser.Document.InvokeScript(funcName);
			}
		}
		private void UpdateProgressBar(int progress) {
			progressBarInfo_.Progress = progress;
			RunScript("__updateProgressBar", new object[] { progress });
		}
		private void UpdateProgressBar(int progress, string labelText) {
			progressBarInfo_.Progress = progress;
			progressBarInfo_.Caption = labelText;
			RunScript("__updateProgressBar", new object[] { progress, labelText });
		}

		private void UpdateMainWebView() {
			LauncherData ls = Program.LoncherSettings;

			if (ls.MainPage.Error != null) {
				YU.ErrorAndKill(Locale.Get(ls.MainPage.Error));
			}
			else {
				mainBrowser.Navigate(new Uri(ls.MainPage.Site));
			}
		}

		private async void DownloadFile(FileInfo fileInfo) {
			if (!YU.stringHasText(fileInfo.UploadAlias)) {
				fileInfo.UploadAlias = fileInfo.Hashes.Count > 0 ? fileInfo.Hashes[0] : null;
				if (!YU.stringHasText(fileInfo.UploadAlias)) {
					int lios = Math.Max(fileInfo.Path.LastIndexOf('\\'), fileInfo.Path.LastIndexOf('/'));
					fileInfo.UploadAlias = lios > -1 ? fileInfo.Path.Substring(lios + 1, fileInfo.Path.Length) : fileInfo.Path;
				}
			}
			string uploadFilename = PreloaderForm.UPDPATH + fileInfo.UploadAlias;
			if (File.Exists(uploadFilename)) {
				if (FileChecker.CheckFileMD5(PreloaderForm.UPDPATH, fileInfo)) {
					if (UpdateInProgress_) {
						DownloadNext();
					}
					else {
						DownloadNextMod();
					}
					return;
				}
				else {
					File.Delete(uploadFilename);
				}
			}
			try {
				string labelText = string.Format(
					Locale.Get("DLRate")
					, YU.formatFileSize(0)
					, YU.formatFileSize(fileInfo.Size)
					, ""
					, fileInfo.Description
				);
				UpdateProgressBar(0, labelText);
				await WC.DownloadFileTaskAsync(new Uri(fileInfo.Url), uploadFilename);
			}
			catch (Exception ex) {
				ShowDownloadError(string.Format(Locale.Get("CannotDownloadFile"), fileInfo.Path) + "\r\n" + ex.Message);
			}
		}

		private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			downloadProgressTracker_.SetProgress(e.BytesReceived, e.TotalBytesToReceive);
			if (DateTime.Now.Ticks > lastDlstringUpdate_ + 500000L) {
				lastDlstringUpdate_ = DateTime.Now.Ticks;
				int progressVal = (int)Math.Floor((double)e.BytesReceived / e.TotalBytesToReceive * progressBarInfo_.MaxValue);
				string labelText = string.Format(
					Locale.Get("DLRate")
					, YU.formatFileSize(e.BytesReceived)
					, YU.formatFileSize(e.TotalBytesToReceive)
					, downloadProgressTracker_.GetBytesPerSecondString()
					, currentFile_.Value.Description
				);
				UpdateProgressBar(progressVal, labelText);
			}
		}

		private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e) {
			downloadProgressTracker_.Reset();
			if (UpdateInProgress_) {
				DownloadNext();
			}
			else {
				DownloadNextMod();
			}
		}

		private async void DownloadNext() {
			do {
				currentFile_ = currentFile_.Next;
			}
			while ((currentFile_ != null) && !currentFile_.Value.IsCheckedToDl);

			if (currentFile_ != null) {
				DownloadFile(currentFile_.Value);
			}
			else {
				string filename = "";
				UpdateProgressBar(progressBarInfo_.MaxValue, Locale.Get("StatusCopyingFiles"));
				try {
					List<string> failedFiles = new List<string>();
					foreach (FileInfo fileInfo in filesToUpload_) {
						if (!fileInfo.IsCheckedToDl) {
							continue;
						}
						filename = ThePath + fileInfo.Path.Replace('/', '\\');

						string errorStr = await Task<string>.Run(() => {
							return MoveUploadedFile(filename, fileInfo);
						});
						if (errorStr != null) {
							failedFiles.Add(errorStr);
						}
					}

					UpdateProgressBar(progressBarInfo_.MaxValue, Locale.Get("StatusUpdatingDone"));
					UpdateInProgress_ = false;
					if (modsToUpdate_ != null && modsToUpdate_.Count > 0) {
						UpdateStatusWebView();
						if (failedFiles.Count > 0) {
							if (YobaDialog.ShowDialog(String.Format(Locale.Get("UpdateHashCheckFailed"), String.Join("\r\n", failedFiles)), YobaDialog.YesNoBtns) == DialogResult.Yes) {
								DownloadNextMod();
							}
						}
						else {
							DownloadNextMod();
						}
					}
					else {
						if (failedFiles.Count > 0) {
							if (YobaDialog.ShowDialog(String.Format(Locale.Get("UpdateHashCheckFailed"), String.Join("\r\n", failedFiles)), YobaDialog.YesNoBtns) == DialogResult.Yes) {
								SetReady(true);
							}
							else {
								UpdateStatusWebView();
							}
						}
						else {
							SetReady(true);
							if (YobaDialog.ShowDialog(Locale.Get("UpdateSuccessful"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
								launch();
							}
						}
					}
				}
				catch (UnauthorizedAccessException ex) {
					ShowDownloadError(string.Format(Locale.Get("DirectoryAccessDenied"), filename) + ":\r\n" + ex.Message);
				}
				catch (Exception ex) {
					ShowDownloadError(string.Format(Locale.Get("CannotMoveFile"), filename) + ":\r\n" + ex.Message);
				}
				UpdateInProgress_ = false;
			}
		}

		private void ShowDownloadError(string error) {
			YobaDialog.ShowDialog(error);
			LaunchButtonEnabled_ = true;
			UpdateProgressBar(0, Locale.Get("StatusDownloadError"));
			UpdateStatusWebView();
		}

		private void SetReady(bool isReady) {
			ReadyToGo_ = isReady;
			LaunchButtonEnabled_ = true;
			UpdateStatusWebView();
		}
		private void CheckReady() {
			if (UpdateInProgress_) {
				return;
			}
			if (filesToUpload_ != null) {
				foreach (FileInfo fi in filesToUpload_) {
					if (fi.IsCheckedToDl) {
						SetReady(false);
						return;
					}
				}
			}
			SetReady(true);
		}

		public void OnLaunchGameBtn() {
			if (ReadyToGo_) {
				launch();
			}
			else {
				if (Program.OfflineMode) {
					if (YobaDialog.ShowDialog(Locale.Get("CannotUpdateInOfflineMode"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
						SetReady(true);
					}
				}
				else {
					LaunchButtonEnabled_ = false;
					UpdateInProgress_ = true;
					UpdateStatusWebView();
					currentFile_ = filesToUpload_.First;
					while ((currentFile_ != null) && !currentFile_.Value.IsCheckedToDl) {
						currentFile_ = currentFile_.Next;
					}
					DownloadFile(currentFile_.Value);
				}
			}
		}

		private void launchGameBtn_Click(object sender, EventArgs e) {
			OnLaunchGameBtn();
		}

		private void launch() {
			if (!(CheckModUpdates(true) && CheckDepsAndConflicts())) {
				return;
			}
			string args;

			/*List<Tuple<string, string>> execMods = new List<Tuple<string, string>>();
			foreach (ModInfo mi in Program.LoncherSettings.Mods) {
				if (mi.IsActive && mi.HasExecutable) {
					foreach (FileInfo fi in mi.FilesForCurrentVersion) {
						if (fi.IsOK && fi.Path.EndsWith(".exe")) {
							execMods.Add(new Tuple<string, string>(mi.Name, fi.Path));
							break;
						}
					}
				}
			}
			if (execMods.Count > 0) {

			}*/

			if (LauncherConfig.LaunchFromGalaxy) {
				args = string.Format("/command=runGame /gameId={1} /path=\"{0}\"", ThePath, Program.LoncherSettings.GogID);
				YU.RunProcess(args, LauncherConfig.GalaxyDir);
			}
			else {
				if (ThePath.Contains("steamapps")) {
					args = "/C explorer steam://run/" + Program.LoncherSettings.SteamID;
				}
				else {
					args = "/C \"" + ThePath + Program.LoncherSettings.ExeName + "\"";
				}
				YU.RunCommand(args);
			}
			YU.Log(args, 3);
			LaunchButtonEnabled_ = false;
			UpdateLaunchButton();
			System.Threading.Thread.Sleep(1800);
			if (LauncherConfig.CloseOnLaunch) {
				ExitApp();
			}
			else {
				LaunchButtonEnabled_ = true;
				UpdateLaunchButton();
			}
		}

		private void launchGameExe() {

		}

		private void settingsButton_Click(object sender, EventArgs e) {
			ShowSettingsDialog();
		}

		public void ShowSettingsDialog() {
			SettingsDialog settingsDialog = new SettingsDialog(this);
			settingsDialog.Icon = Program.LoncherSettings.Icon;
			if (settingsDialog.ShowDialog(this) == DialogResult.OK) {
				LauncherConfig.StartPage = settingsDialog.OpeningPanel;
				LauncherConfig.GameDir = settingsDialog.GamePath;
				LauncherConfig.LaunchFromGalaxy = settingsDialog.LaunchViaGalaxy;
				bool prevOffline = LauncherConfig.StartOffline;
				LauncherConfig.StartOffline = settingsDialog.OfflineMode;
				LauncherConfig.CloseOnLaunch = settingsDialog.CloseOnLaunch;
				LauncherConfig.Save();
				settingsDialog.Dispose();
				if (LauncherConfig.GameDir != Program.GamePath) {
					Hide();
					new PreloaderForm(this).Show();
				}
				else if (prevOffline != LauncherConfig.StartOffline) {
					if (YobaDialog.ShowDialog(Locale.Get(LauncherConfig.StartOffline ? "OfflineModeSet" : "OnlineModeSet"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
						Hide();
						new PreloaderForm(this).Show();
					}
				}
			}
			this.Focus();
		}

		private void MainForm_FormClosed(object sender, FormClosedEventArgs e) {
			ExitApp();
		}

		private void closeButton_Click(object sender, EventArgs e) {
			ExitApp();
		}

		public void ExitApp() {
			if (LauncherConfig.HasUnsavedChanges) {
				LauncherConfig.Save();
			}
			Application.Exit();
		}

		private void minimizeButton_Click(object sender, EventArgs e) {
			WindowState = FormWindowState.Minimized;
		}

		private void draggingPanel_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				ReleaseCapture();
				SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
			}
		}

		private async void refreshButton_Click(object sender, EventArgs e) {
			if (!Program.OfflineMode) {
				PreloaderForm pf = new PreloaderForm(null, true);
				pf.Show();
				pf.InitProgressTracker();
				Program.LoncherSettings.MainPage = await pf.getMainPageData();
				pf.Hide();
				pf.Dispose();
				UpdateMainWebView();
			}
		}

		private void helpButton_Click(object sender, EventArgs e) {
			YU.ShowHelpDialog();
		}

		private void MainForm_Shown(object sender, EventArgs e) {
			if (!(Program.FirstRun || Program.OfflineMode)
					&& Program.LoncherSettings.Survey != null
					&& YU.stringHasText(Program.LoncherSettings.Survey.Text)
					&& YU.stringHasText(Program.LoncherSettings.Survey.Url)
					&& YU.stringHasText(Program.LoncherSettings.Survey.ID)
					&& (LauncherConfig.LastSurveyId is null || LauncherConfig.LastSurveyId != Program.LoncherSettings.Survey.ID)) {
				int showSurvey = new Random().Next(0, 100);
				string discardId = "-0" + Program.LoncherSettings.Survey.ID;
				bool wasDiscarded = discardId.Equals(LauncherConfig.LastSurveyId);
				if ((!wasDiscarded && showSurvey > 70) || (showSurvey > 7 && showSurvey < 10)) {
					DialogResult result = YobaDialog.ShowDialog(Program.LoncherSettings.Survey.Text, YobaDialog.YesNoBtns);
					if (result == DialogResult.Yes) {
						Process.Start(Program.LoncherSettings.Survey.Url);
						LauncherConfig.LastSurveyId = Program.LoncherSettings.Survey.ID;
						LauncherConfig.Save();
					}
					else if (result == DialogResult.No) {
						LauncherConfig.LastSurveyId = discardId;
						LauncherConfig.Save();
					}
				}
			}
		}

		private void webBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
			string url = e.Url.ToString();
			string urlstart = url.Substring(0, 7);
			switch (urlstart) {
				case "http://":
				case "https:/":
					Process.Start(url);
					e.Cancel = true;
					break;
			}
		}

		private void MainForm_Resize(object sender, System.EventArgs e) {
			int h = this.Size.Height;
			int w = this.Size.Width;
			mainBrowser.Size = new Size(w - 4, h - 2);
			draggingPanel.UpdateWidth(w);
			LauncherConfig.WindowHeight = h;
			LauncherConfig.WindowWidth = w;
			LauncherConfig.HasUnsavedChanges = true;
		}

		public class DraggingPanel : Panel {
			public int WidthSpace = 96;
			private int initWidth_ = 100;
			private int initHeight_ = 24;

			public void UpdateSize(int w, int h) {
				initWidth_ = w;
				initHeight_ = h;
				UpdateSize();
			}
			public void UpdateWidth(int w) {
				initWidth_ = w;
				UpdateSize();
			}
			public void UpdateHeight(int h) {
				initHeight_ = h;
				UpdateSize();
			}
			public void UpdateSize() {
				int z = LauncherConfig.ZoomPercent;
				if (z == 100) {
					this.Size = new Size(initWidth_ - WidthSpace - 4, initHeight_);
				}
				else {
					int w = initWidth_ - (int)Math.Floor((double)WidthSpace / 100 * z) - 4;
					int h = (int)Math.Floor((double)initHeight_ / 100 * z);
					this.Size = new Size(w, h);
				}
			}
		}

		protected override void WndProc(ref Message m) {
			const uint WM_NCHITTEST = 0x0084;
			const uint WM_MOUSEMOVE = 0x0200;

			const uint HTLEFT = 10;
			const uint HTRIGHT = 11;
			const uint HTBOTTOMRIGHT = 17;
			const uint HTBOTTOM = 15;
			const uint HTBOTTOMLEFT = 16;
			const uint HTTOP = 12;
			const uint HTTOPLEFT = 13;
			const uint HTTOPRIGHT = 14;

			const int RESIZE_HANDLE_SIZE = 10;
			bool handled = false;
			if (m.Msg == WM_NCHITTEST || m.Msg == WM_MOUSEMOVE) {
				Size formSize = this.Size;
				Point screenPoint = new Point(m.LParam.ToInt32());
				Point clientPoint = this.PointToClient(screenPoint);

				Dictionary<uint, Rectangle> boxes = new Dictionary<uint, Rectangle>() {
						{HTBOTTOMLEFT, new Rectangle(0, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
						{HTBOTTOM, new Rectangle(RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
						{HTBOTTOMRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
						{HTRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE)},
						{HTTOPRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
						{HTTOP, new Rectangle(RESIZE_HANDLE_SIZE, 0, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
						{HTTOPLEFT, new Rectangle(0, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
						{HTLEFT, new Rectangle(0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE) }
					};

				foreach (KeyValuePair<uint, Rectangle> hitBox in boxes) {
					if (hitBox.Value.Contains(clientPoint)) {
						m.Result = (IntPtr)hitBox.Key;
						handled = true;
						break;
					}
				}
			}

			if (!handled)
				base.WndProc(ref m);
		}
	}
}