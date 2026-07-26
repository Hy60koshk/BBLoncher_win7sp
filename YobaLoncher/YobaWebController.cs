using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Drawing;
using CommonOpenFileDialog = Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog;
using CommonFileDialogResult = Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult;
using System.Linq;

namespace YobaLoncher {
	partial class MainForm {
		[ComVisible(true)]
		public class YobaWebController {
			private static YobaWebController _instance = null;

			private MainForm _form = null;

			public static YobaWebController Instance {
				get {
					if (_instance is null) {
						_instance = new YobaWebController();
					}
					return _instance;
				}
			}

			public string GetLoc(string key) {
				return Locale.Get(key);
			}
			public string GetLocs(string keysStr) {
				string[] keys = keysStr.Split(',');
				Dictionary<string, string> strings = new Dictionary<string, string>();
				for (int i = 0; i < keys.Length; i++) {
					string key = keys[i].Trim();
					if (!strings.ContainsKey(key)) {
						strings.Add(key, Locale.Get(key));
					}
				}
				return JsonConvert.SerializeObject(strings);
			}
			public void Info(string text) {
				YobaDialog.ShowDialog(text);
			}
			public void Info(string text, string onOk) {
				YobaDialog.ShowDialog(text);
				_form.mainBrowser.Document.InvokeScript(onOk);
			}
			public void Ask(string text, string onYes, string onNo) {
				if (YobaDialog.ShowDialog(text, YobaDialog.YesNoBtns) == DialogResult.Yes) {
					_form.mainBrowser.Document.InvokeScript(onYes);
				}
				else {
					_form.mainBrowser.Document.InvokeScript(onNo);
				}
			}
			public void Warn(string text) {
				MessageBox.Show(text);
			}
			public void Warn(string text, string onOk) {
				MessageBox.Show(text);
				_form.mainBrowser.Document.InvokeScript(onOk);
			}
			public void Warn(string text, string onYes, string onNo) {
				if (MessageBox.Show(_form, text, "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
					_form.mainBrowser.Document.InvokeScript(onYes);
				}
				else {
					_form.mainBrowser.Document.InvokeScript(onNo);
				}
			}
			public void Error(string text) {
				MessageBox.Show(_form, text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			public void Error(string text, string onOk) {
				MessageBox.Show(_form, text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_form.mainBrowser.Document.InvokeScript(onOk);
			}

			public void Close() {
				_form.ExitApp();
			}
			public void Minimize() {
				_form.WindowState = FormWindowState.Minimized;
			}
			public void Maximize() {
				_form.ToggleMaximized();
			}
			public void Help() {
				YU.ShowHelpDialog();
			}
			public void Settings() {
				_form.ShowSettingsDialog();
			}
			public bool IsMaximized() {
				return _form.WindowState == FormWindowState.Maximized;
			}

			public string RetrieveBackground() {
				return Program.LoncherSettings.BackgroundPath.Substring(12).Replace('\\', '/');
			}
			public string RetrieveStartViewId() {
				return LauncherConfig.StartPage.ToString();
			}
			public void UpdateAppControlsSize(string width, string height) {
				if (Int32.TryParse(width, out int ww)) {
					_form.draggingPanel.WidthSpace = ww;
				}
				if (Int32.TryParse(height, out int hh)) {
					_form.draggingPanel.UpdateSize(_form.Width, hh);
				}
				else {
					_form.draggingPanel.UpdateWidth(_form.Width);
				}
			}
			public int GetProgressBarMax() {
				return _form.progressBarInfo_.MaxValue;
			}
			public string GetProgressBarState() {
				return JsonConvert.SerializeObject(_form.progressBarInfo_);
			}

			public void UpdateStatusWebView() {
				_form.UpdateStatusWebView();
			}
			public void UpdateModsWebView() {
				_form.UpdateModsWebView();
			}
			public void CheckModUpdates() {
				_form.CheckModUpdates();
			}

			public void UncheckFile(int groupidx, int fileidx) {
				try {
					FileInfo fi = Program.LoncherSettings.GameVersion.FileGroups[groupidx].Files[fileidx];
					if (fi != null && !fi.IsMandatory) {
						fi.IsCheckedToDl = false;
						_form.CheckReady();
					}
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}
			public void CheckFile(int groupidx, int fileidx) {
				try {
					FileInfo fi = Program.LoncherSettings.GameVersion.FileGroups[groupidx].Files[fileidx];
					if (fi != null) {
						fi.IsCheckedToDl = true;
						_form.SetReady(false);
					}
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}

			public bool LaunchGame() {
				_form.OnLaunchGameBtn();
				return true;
			}

			private List<WebModInfo> _modList;

			internal List<WebModInfo> ModList {
				get => _modList;
			}

			internal void UpdateModList() {
				_modList = Program.LoncherSettings.AvailableMods.Select(mod => new WebModInfo(mod)).ToList();
			}

			private ModInfo _getModInfoById(string id) {
				if (_modList is null) {
					YobaDialog.ShowDialog("Mod List has not been initialized yet");
				}
				else {
					List<WebModInfo> mods = _modList.FindAll(m => m.Id == id);
					switch (mods.Count) {
						case 1:
							return mods[0].ModInfo;
						case 0:
							YobaDialog.ShowDialog("A mod with ID '" + id + "' is not present in modlist. Call for admin assistance.");
							break;
						default:
							YobaDialog.ShowDialog("ID '" + id + "' is not unique in modlist. Call for admin assistance.");
							break;
					}
				}
				return null;
			}

			private bool _checkConflicts(ModInfo mi, string locKey) {
				List<string> conflictedMods = new List<string>();
				foreach (ModInfo ami in Program.LoncherSettings.AvailableMods) {
					if (ami.IsActive && ami.DoesConflict(mi)) {
						conflictedMods.Add(ami.VersionedName);
					}
				}
				if (conflictedMods.Count > 0) {
					if (DialogResult.Yes != YobaDialog.ShowDialog(
							String.Format(Locale.Get(locKey), string.Join("\r\n", conflictedMods))
							, YobaDialog.YesNoBtns)) {
						return false;
					}
				}
				return true;
			}
			private bool _checkDependencies(ModInfo mi, string locKey) {
				List<string> dependentMods = new List<string>();
				foreach (ModInfo dmi in Program.LoncherSettings.AvailableMods) {
					if (dmi.IsActive && dmi.DoesDepend(mi)) {
						dependentMods.Add(dmi.VersionedName);
					}
				}
				if (dependentMods.Count > 0) {
					if (DialogResult.Yes != YobaDialog.ShowDialog(
							String.Format(Locale.Get(locKey), string.Join("\r\n", dependentMods))
							, YobaDialog.YesNoBtns)) {
						return false;
					}
				}
				return true;
			}

			public void ModInstall(string id, string verId) {
				ModInfo mi = _getModInfoById(id);
				if (mi is null) {
					YobaDialog.ShowDialog("There's no mod with ID " + id);
					return;
				}
				/*List<Tuple<ModVersion, GameVersion>> versions = new List<Tuple<ModVersion, GameVersion>>();
				foreach (ModVersion mv in mi.Versions) {
					GameVersion gv = mv.GetGameVersion();
					if (gv != null) {
						versions.Add(new Tuple<ModVersion, GameVersion>(mv, gv));
					}
				}
				if (versions.Count < 1) {
					YobaDialog.ShowDialog("There's no versions for mod " + id);
					return;
				}

				int verIdx = 0;
				if (versions.Count > 1) {

				}*/
				
				//mi.InitCurrentInstallForVersion(versions[verIdx].Item1);
				mi.InitInstallForVersion(verId);
				if (mi.CurrentVersion != null && _checkConflicts(mi, "SomeModsConflictWithThisInstall")) {
					InstallModAsync(mi);
				}
			}
			
			public void ModUninstall(string id) {
				ModInfo mi = _getModInfoById(id);
				if (mi != null && _checkDependencies(mi, "SomeModsDependOnThisDelete")) {
					if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("AreYouSureUninstallMod"), mi.Name), YobaDialog.YesNoBtns)) {
						mi.Delete();
						_form.UpdateModsWebView();
					}
				}
			}
			public void ModDisable(string id) {
				ModInfo mi = _getModInfoById(id);
				if (mi != null && _checkDependencies(mi, "SomeModsDependOnThisDisable")) {
					mi.Disable();
					_form.UpdateModsWebView();
				}
			}
			public void ModEnable(string id) {
				ModInfo mi = _getModInfoById(id);
				if (mi != null && _checkConflicts(mi, "SomeModsConflictWithThisEnable")) {
					ModEnableAsync(mi);
				}
			}

			internal async void ModEnableAsync(ModInfo mi) {
				CheckResult modFileCheckResult = await mi.Enable();
				if (modFileCheckResult is null || modFileCheckResult.IsAllOk) {
					_form.UpdateModsWebView();
				}
				else {
					LinkedList<FileInfo> files = modFileCheckResult.InvalidFiles;
					uint size = 0;
					foreach (FileInfo fi in files) {
						size += fi.Size;
					}
					if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("ModActivationFilesAreOutdated"), mi.VersionedName, YU.FormatFileSize(size)), YobaDialog.YesNoBtns)) {
						if (_form.modsToUpdate_ is null) {
							_form.modsToUpdate_ = new LinkedList<ModInfo>();
							_form.modsToUpdate_.AddLast(mi);
							mi.MarkedForUpdate = true;
							mi.DlInProgress = true;
							_form.UpdateModsWebView();
							if (!_form.UpdateInProgress_) {
								_form._downloadNextMod();
							}
						}
						else {
							mi.DlInProgress = true;
							_form.modsToUpdate_.AddLast(mi);
							_form.UpdateModsWebView();
						}
					}
					else {
						if (DialogResult.Yes == YobaDialog.ShowDialog(Locale.Get("ModDisableToPreventCorruption"), YobaDialog.YesNoBtns)) {
							mi.Disable();
						}
						_form.UpdateModsWebView();
					}
				}
			}

			internal async void InstallModAsync(ModInfo mi) {
				uint size = 0;
				if (mi.LatestVersion.Files[0].Size == 0) {
					await FileChecker.CheckFiles(mi.LatestVersion.Files);
				}
				foreach (FileInfo fi in mi.LatestVersion.Files) {
					if (!fi.IsHashOk) {
						size += fi.Size;
					}
				}
				string modSize = YU.FormatFileSize(size);
				if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("AreYouSureInstallMod"), mi.VersionedName, modSize), YobaDialog.YesNoBtns)) {
					if (_form.modsToUpdate_ is null) {
						_form.modsToUpdate_ = new LinkedList<ModInfo>();
						_form.modsToUpdate_.AddLast(mi);
						mi.DlInProgress = true;
						_form.UpdateModsWebView();
						if (!_form.UpdateInProgress_) {
							_form._downloadNextMod();
						}
					}
					else {
						mi.DlInProgress = true;
						_form.modsToUpdate_.AddLast(mi);
						_form.UpdateModsWebView();
					}
				}
			}

			/*
			 * OPTIONS
			 */
			public string OptionsGetCurrentSettings() {
				Dictionary<string, string> settings = new Dictionary<string, string> {
					{ "CurrentlyOffline", Program.OfflineMode ? "1" : "0" },
					{ "StartOffline", LauncherConfig.StartOffline ? "1" : "0" },
					{ "CloseOnLaunch", LauncherConfig.CloseOnLaunch ? "1" : "0" },
					{ "ShowHiddenMods", LauncherConfig.ShowHiddenMods ? "1" : "0" },
					{ "LaunchFromGalaxy", LauncherConfig.LaunchFromGalaxy ? "1" : "0" },
					{ "ModsCompactMode", LauncherConfig.ModsCompactMode ? "1" : "0" },
					{ "ZoomPercent", LauncherConfig.ZoomPercent.ToString() },
					{ "LoggingLevel", LauncherConfig.LoggingLevel.ToString() },
					{ "GameDir", LauncherConfig.GameDir },
					{ "StartPage", ((int)LauncherConfig.StartPage).ToString() }
				};
				return JsonConvert.SerializeObject(settings);
			}
			public bool OptionsCheckOffline(bool offlineOn) {
				LauncherConfig.StartOffline = offlineOn;
				if (Program.OfflineMode != offlineOn) {
					if (YobaDialog.ShowDialog(Locale.Get(offlineOn ? "OfflineModeSet" : "OnlineModeSet"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
						_form.Hide();
						new PreloaderForm(_form).Show();
					}
				}
				return LauncherConfig.StartOffline;
			}
			public bool OptionsCheckLaunchFromGalaxy(bool isChecked) {
				LauncherConfig.LaunchFromGalaxy = isChecked;
				return LauncherConfig.LaunchFromGalaxy;
			}
			public bool OptionsCheckCloseOnLaunch(bool isChecked) {
				LauncherConfig.CloseOnLaunch = isChecked;
				return LauncherConfig.CloseOnLaunch;
			}
			public int OptionsSelectStartPage(int pageId) {
				LauncherConfig.StartPage = (StartPageEnum)pageId;
				return (int)LauncherConfig.StartPage;
			}
			public bool OptionsCheckShowHiddenMods(bool isChecked) {
				LauncherConfig.ShowHiddenMods = isChecked;
				UpdateModsWebView();
				return LauncherConfig.ShowHiddenMods;
			}
			public bool OptionsCheckModsCompactMode(bool isChecked) {
				LauncherConfig.ModsCompactMode = isChecked;
				UpdateModsWebView();
				return LauncherConfig.ModsCompactMode;
			}

			public void OptionsSetLoggingLevel(int level) {
				LauncherConfig.LoggingLevel = level;
			}
			public int OptionsSetZoom(int zoom) {
				return _form.SetBrowserZoom(zoom);
			}

			public string OptionsBrowseGamePath() {
				CommonOpenFileDialog folderBrowserDialog = new CommonOpenFileDialog() {
					IsFolderPicker = true
					, InitialDirectory = Program.GamePath
				};
				if (folderBrowserDialog.ShowDialog() == CommonFileDialogResult.Ok) {
					string path = folderBrowserDialog.FileName;
					if (path[path.Length - 1] != Path.DirectorySeparatorChar) {
						path += Path.DirectorySeparatorChar;
					}
					if (Program.GamePath != path) {
						if (File.Exists(path + Program.LoncherSettings.ExeName)) {
							LauncherConfig.GameDir = path;
							if (YobaDialog.ShowDialog(Locale.Get("GamePathChanged"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
								LauncherConfig.Save();
								_form.Hide();
								new PreloaderForm(_form).Show();
							}
							return path;
						}
						else {
							YobaDialog.ShowDialog(Locale.Get("NoExeInPath"));
						}
					}
				}
				return "";
			}

			public void OptionsOpenDataFolder() {
				YU.RunCommand("/C explorer \"" + Program.GamePath + "data\"");
			}

			public void OptionsUninstallRussifier() {
				try {
					UninstallationRules urules_ = Program.LoncherSettings.UninstallationRules;
					string msg = Locale.Get("ProductUninstallationConfirmation") + ":";
					foreach (FileInfo fi in urules_.FilesToDelete) {
						if (File.Exists(Program.GamePath + fi.Path)) {
							msg += "\r\n" + Program.GamePath + fi.Path;
						}
					}
					if (YobaDialog.ShowDialog(msg, YobaDialog.YesNoBtns) == DialogResult.Yes) {
						foreach (FileInfo fi in urules_.FilesToDelete) {
							if (File.Exists(Program.GamePath + fi.Path)) {
								File.Delete(Program.GamePath + fi.Path);
							}
						}
					}
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}

			public void OptionsUninstallLoncher() {
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

			public void OptionsCreateShortcut() {
				try {
					string filename = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
						+ Path.DirectorySeparatorChar + Program.LoncherSettings.LoncherLinkName + ".lnk";
					
					if (File.Exists(filename)) {
						YobaDialog.ShowDialog(Locale.Get("ShortcutAlreadyExists"));
					}
					else {
						IWshRuntimeLibrary.WshShell wsh = new IWshRuntimeLibrary.WshShell();
						IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(filename) as IWshRuntimeLibrary.IWshShortcut;
						shortcut.Arguments = "";
						shortcut.TargetPath = Application.ExecutablePath;
						shortcut.WorkingDirectory = Program.LONCHER_PATH;
						shortcut.WindowStyle = 1;
						string iconFile = Program.LONCHER_DATA_PATH + "shortcutIcon.ico";
						bool validIconFile = File.Exists(iconFile);
						if (!validIconFile) {
							string exename = Program.GamePath + Program.LoncherSettings.ExeName;

							if (File.Exists(PreloaderForm.ICON_FILE)) {
								PngIconConverter.Convert(PreloaderForm.ICON_FILE, iconFile);
								validIconFile = true;
							}
							else if (File.Exists(exename) && exename.EndsWith(".exe")) {
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
						YobaDialog.ShowDialog(Locale.Get("ShortcutCreatedSuccessfully"));
					}
				}
				catch (Exception ex) {
					YobaDialog.ShowDialog(ex.Message);
				}
			}

			public void OptionsMakeBackup() {
				string bkpdir = Program.GamePath + "_loncher_backups\\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "\\";
				if (DialogResult.Yes == YobaDialog.ShowDialog(String.Format(Locale.Get("SettingsMakeBackupInfo"), bkpdir), YobaDialog.YesNoBtns)) {
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
							if (File.Exists(origDir + path)) {
								File.Copy(origDir + path, bkpdir + path);
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
		}
	}
}
