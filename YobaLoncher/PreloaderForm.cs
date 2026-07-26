using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace YobaLoncher {
	public partial class PreloaderForm : Form {

		private DownloadProgressTracker _downloadProgressTracker;
		private WebClient _wc;
		private readonly MainForm _oldMainForm = null;
		private readonly bool _preventInit = false;
		private int _progressBarLeft = 0;
		private string _gogPath = null;

		public const string IMGPATH = @"loncherData\images\";
		public const string ASSETSPATH = @"loncherData\assets\";
		public const string UPDPATH = @"loncherData\updates\";
		public const string FNTPATH = @"loncherData\fonts\";
		public const string LOCPATH = @"loncherData\loc";
		public const string TMPLPATH = @"loncherData\templates\";
		public const string SETTINGSPATH = @"loncherData\settings";

		public const string BG_FILE = IMGPATH + @"loncherbg.png";
		public const string ICON_FILE = IMGPATH + @"icon.png";

		public PreloaderForm() : this(null, false) { }
		public PreloaderForm(MainForm oldMainForm) : this(oldMainForm, false) { }

		public PreloaderForm(MainForm oldMainForm, bool preventInit) {
			YU.Log("\nLaunched: " + Program.VersionInfoShort, 0);

			InitializeComponent();
			_preventInit = preventInit;
			_oldMainForm = oldMainForm;
			this.BackgroundImageLayout = ImageLayout.Stretch;
			
			if (File.Exists(BG_FILE)) {
				try {
					this.BackgroundImage = YU.ReadBitmap(BG_FILE);
				}
				catch {
					loadingLabelError.Text = Locale.Get("CannotSetBackground");
				}
			}
			if (File.Exists(ICON_FILE)) {
				try {
					Bitmap bm = YU.ReadBitmap(ICON_FILE);
					if (bm != null) {
						Icon = Icon.FromHandle(bm.GetHicon());
					}
				}
				catch {
					loadingLabelError.Text = Locale.Get("CannotSetIcon");
				}
			}
			Text = Locale.Get("PreloaderTitle");
			statusLabel.Text = "";
			loadingLabel.Text = string.Format(Locale.Get("LoncherLoading"), Program.LoncherName);
			labelAbout.Text = Locale.Get("PressF1About");
			if (preventInit && !Program.OfflineMode) {
				_wc = new WebClient { Encoding = Encoding.UTF8 };
			}
		}

		private string _getSteamGameInstallPath() {
			string steamId = Program.LoncherSettings.SteamID;
			if (steamId is null) {
				return null;
			}
			string[] locations = new string[] {
				@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + steamId
				, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + steamId
			};
			return YU.GetRegistryInstallPath(locations, true);
		}
		private string _getGogGameInstallPath() {
			string gogId = Program.LoncherSettings.GogID;
			if (gogId is null) {
				return null;
			}
			string[] locations = new string[] {
				@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\" + gogId + "_is1"
				, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + gogId + "_is1"
			};
			return YU.GetRegistryInstallPath(locations, true);
		}		

		private List<string> _getSteamLibraryPaths() {
			List<string> paths = new List<string>();
			try {
				string steamInstallPath = "";
				bool is64 = Environment.Is64BitOperatingSystem;
				using (RegistryKey view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, is64 ? RegistryView.Registry64 : RegistryView.Registry32)) {
					using (RegistryKey clsid = view.OpenSubKey(is64 ? @"SOFTWARE\WOW6432Node\Valve\Steam" : @"SOFTWARE\Valve\Steam")) {
						if (clsid != null) {
							steamInstallPath = (string)clsid.GetValue("InstallPath");
						}
					}
				}
				if (steamInstallPath.Length > 0) {
					paths.Add(steamInstallPath + @"\steamapps\common\");
					string vdfPath = steamInstallPath + @"\steamapps\libraryfolders.vdf";
					if (File.Exists(vdfPath)) {
						string[] lines = File.ReadAllLines(vdfPath);
						foreach (string line in lines) {
							if (line.Length > 0 && line.StartsWith("\t\"")) {
								string val = line.Substring(3);
								if (val.Length > 0 && val.StartsWith("\"\t\t\"")) {
									val = val.Substring(4, val.Length - 5);
									if (val.Length > 2 && val.Substring(1, 2) == ":\\") {
										paths.Add(val.Replace("\\\\", "\\") + @"\steamapps\common\");
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex) {
				YobaDialog.ShowDialog(ex.Message);
			}
			return paths;
		}

		private void _incProgress() {
			_progressBar1.Value++;
		}
		private void _incProgress(int d) {
			_progressBar1.Value += d;
		}

		private void _onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			_downloadProgressTracker.SetProgress(e.BytesReceived, e.TotalBytesToReceive);
			_progressBar1.Value = e.ProgressPercentage;
		}

		private async Task _loadFile(string src, string filename) {
			await _loadFile(src, filename, Locale.Get("UpdDownloading"));
		}

		private async Task _loadFile(string src, string filename, string customStatus) {
			statusLabel.Text = customStatus;
			_progressBarLeft = _progressBar1.Value;
			int ldidx = filename.IndexOf("loncherData");
			loadingLabel.Text = ldidx > 0 ? filename.Substring(ldidx) : filename;
			await _wc.DownloadFileTaskAsync(src, filename);
			_downloadProgressTracker.Reset();
			_progressBarLeft += 3;
			if (_progressBarLeft > 87) {
				_progressBarLeft = 50;
			}
			_progressBar1.Value = _progressBarLeft;
		}

		private async Task<bool> _assertFile(FileInfo fi, string dir) {
			if (fi != null && YU.StringHasText(fi.Path) && YU.StringHasText(fi.Url)) {
				fi.NormalizeHashes();
				if (!FileChecker.CheckFileMD5(dir, fi)) {
					YU.Log("Preloader > Downloading file: " + fi.Url, 1);
					Directory.CreateDirectory(dir);
					await _loadFile(fi.Url, dir + fi.Path);
				}
				return true;
			}
			return false;
		}
		private async Task<bool> _assertFile(FileInfo fi, string dir, string targetPath) {
			if (fi != null && YU.StringHasText(fi.Path) && YU.StringHasText(fi.Url)) {
				fi.NormalizeHashes();
				if (!FileChecker.CheckFileMD5(dir, fi)) {
					Directory.CreateDirectory(dir);
					await _loadFile(fi.Url, targetPath);
				}
				return true;
			}
			return false;
		}
		private bool _assertOfflineFile(FileInfo fi, string dir) {
			if (fi != null && YU.StringHasText(fi.Path) && File.Exists(dir + fi.Path)) {
				return true;
			}
			return false;
		}
		private bool _assertOfflineFile(FileInfo fi, string dir, string targetPath) {
			if (fi != null && YU.StringHasText(fi.Path) && File.Exists(dir + targetPath)) {
				return true;
			}
			return false;
		}

		internal async Task<LauncherData.StaticTabData> GetMainPageData() {
			LauncherData.StaticTabData staticTabData = new LauncherData.StaticTabData();
			try {
				if (Program.LoncherSettings.UIStyle.TryGetValue("MainPageFix", out FileInfo staticPageFileInfo)) {
					if (await _assertFile(staticPageFileInfo, Program.LONCHER_DATA_PATH)) {
						_writeMainPageData(staticPageFileInfo, staticTabData);
					}
				}
			}
			catch (Exception ex) {
				staticTabData.Error = ex.Message;
			}
			return staticTabData;
		}

		internal LauncherData.StaticTabData GetMainPageDataOffline() {
			LauncherData.StaticTabData staticTabData = new LauncherData.StaticTabData();
			try {
				if (Program.LoncherSettings.UIStyle.TryGetValue("MainPageFix", out FileInfo staticPageFileInfo)) {
					if (_assertOfflineFile(staticPageFileInfo, Program.LONCHER_DATA_PATH)) {
						_writeMainPageData(staticPageFileInfo, staticTabData);
					}
				}
			}
			catch (Exception ex) {
				staticTabData.Error = ex.Message;
			}
			return staticTabData;
		}

		private void _writeMainPageData(FileInfo staticPageFileInfo, LauncherData.StaticTabData staticTabData) {
			string path = Program.LONCHER_DATA_PATH + staticPageFileInfo.Path;
			string pageTemplate = File.ReadAllText(path, Encoding.UTF8);
			pageTemplate = pageTemplate.Replace("[[[YOBALIB]]]", Resource1.yobalib);

			path += "_prefab.html";
			File.WriteAllText(path, pageTemplate, Encoding.UTF8);
			staticTabData.Site = "file:///" + path.Replace('\\', '/');
		}

		private string _showPathSelection(string path) {
			if (!YU.StringHasText(path)) {
				path = Program.GamePath;
			}
			GamePathSelectForm gamePathSelectForm = new GamePathSelectForm();
			gamePathSelectForm.Icon = Program.LoncherSettings.Icon;
			gamePathSelectForm.ThePath = path;
			if (gamePathSelectForm.ShowDialog(this) == DialogResult.Yes) {
				path = gamePathSelectForm.ThePath;
				gamePathSelectForm.Dispose();
				if (path != null && path.Equals(_gogPath) && LauncherConfig.GalaxyDir != null) {
					if (YobaDialog.ShowDialog(Locale.Get("GogGalaxyDetected"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
						LauncherConfig.LaunchFromGalaxy = true;
						LauncherConfig.Save();
					}
				}
				if (path.Length == 0) {
					path = Program.GamePath;
				}
				else {
					if (path[path.Length - 1] != '\\') {
						path += "\\";
					}
					Program.GamePath = path;
				}
				LauncherConfig.GameDir = path;
				LauncherConfig.Save();
				return path;
			}
			else {
				Application.Exit();
				return null;
			}
		}

		private bool _findGamePath() {
			string path = LauncherConfig.GameDir;
			if (path is null) {
				path = _getSteamGameInstallPath();
				if (path is null && YU.StringHasText(Program.LoncherSettings.SteamGameFolder)) {
					List<string> steampaths = _getSteamLibraryPaths();
					for (int i = 0; i < steampaths.Count; i++) {
						string spath = steampaths[i] + Program.LoncherSettings.SteamGameFolder;
						if (Directory.Exists(spath)) {
							path = spath + "\\";
							break;
						}
					}
				}
				if (path is null) {
					path = _getGogGameInstallPath();
					if (path != null) {
						_gogPath = "" + path;
					}
				}
				path = _showPathSelection(path);
			}
			if (path is null || path.Length == 0) {
				path = Program.GamePath;
			}
			if (path[path.Length - 1] != '\\') {
				path += "\\";
			}
			while (!File.Exists(path + Program.LoncherSettings.ExeName)) {
				YobaDialog.ShowDialog(Locale.Get("NoExeInPath"));
				path = _showPathSelection(path);
				if (path is null) {
					return false;
				}
			}
			Program.GamePath = path;
			YU.Log("GamePath: " + path, 0);
			return true;
		}

		private void _updateGameVersion() {
			string curVer = FileVersionInfo.GetVersionInfo(Program.GamePath + Program.LoncherSettings.ExeName).FileVersion.Replace(',', '.');
			Program.GameVersion = curVer;

			Program.ModsDisabledPath = Program.GamePath + "_loncher_disabled_mods\\"
				+ string.Join("_", curVer.Split(Path.GetInvalidFileNameChars())) + '\\';
			Program.LoncherSettings.InitForVersion(curVer);
			LauncherConfig.SaveMods();
		}

		private void _showMainForm() {
			_progressBar1.Value = 95;
			if (LauncherConfig.NewFeaturesNotes == null) {
				LauncherConfig.NewFeaturesNotes = new Dictionary<string, bool>();
			}
			/*if (!LauncherConfig.NewFeaturesNotes.ContainsKey("CompactMods")) {
				LauncherConfig.NewFeaturesNotes.Add("CompactMods", true);
				if (YobaDialog.ShowDialog(Locale.Get("NewFeatureCompactMods"), YobaDialog.YesNoBtns) == DialogResult.Yes) {
					LauncherConfig.ModsCompactMode = true;
				}
			}*/
			MainForm mainForm = new MainForm();
			mainForm.Icon = Program.LoncherSettings.Icon;
			_progressBar1.Value = 99;
			mainForm.Show(this);
			Hide();
		}

		private void _preloaderForm_ShownAsync(object sender, EventArgs e) {
			if (!_preventInit) {
				LauncherConfig.Load();
				if (LauncherConfig.StartOffline) {
					_initializeOffline();
				}
				else {
					_wc = new WebClient { Encoding = Encoding.UTF8 };
					_initialize();
				}
			}
		}

		public void InitProgressTracker() {
			_downloadProgressTracker = new DownloadProgressTracker(50, TimeSpan.FromMilliseconds(500));
			_wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(_onDownloadProgressChanged);
		}

		private async void _initialize() {
			_progressBar1.Value = 0;
			Program.OfflineMode = false;
			long startingTicks = DateTime.Now.Ticks;
			long lastTicks = startingTicks;
			void logDeltaTicks(string point) {
				long current = DateTime.Now.Ticks;
				YU.Log(point + ": " + (current - lastTicks) + " (" + (current - startingTicks) + ')', 2);
				lastTicks = current;
			}
			try {
				if (!Directory.Exists(IMGPATH)) {
					Directory.CreateDirectory(IMGPATH);
				}
				if (!Directory.Exists(UPDPATH)) {
					Directory.CreateDirectory(UPDPATH);
				}
				if (!Directory.Exists(ASSETSPATH)) {
					Directory.CreateDirectory(ASSETSPATH);
				}

				string settingsJson = (await _wc.DownloadStringTaskAsync(Program.SETTINGS_URL));
				logDeltaTicks("settings");
				_incProgress(5);
				try {
					Program.LoncherSettings = new LauncherData(settingsJson);
					try {
						File.WriteAllText(SETTINGSPATH, settingsJson, Encoding.UTF8);
					}
					catch { }
					_incProgress(5);
					try {
						if (Program.LoncherSettings.RAW.Localization != null) {
							FileInfo locInfo = Program.LoncherSettings.RAW.Localization;
							if (YU.StringHasText(locInfo.Url)) {
								locInfo.Path = LOCPATH;
								if (!FileChecker.CheckFileMD5("", locInfo)) {
									string loc = await _wc.DownloadStringTaskAsync(locInfo.Url);
									File.WriteAllText(LOCPATH, loc, Encoding.UTF8);
									Locale.LoadCustomLoc(loc.Replace("\r\n", "\n").Split('\n'));
								}
								Locale.LoadCustomLoc(File.ReadAllLines(LOCPATH, Encoding.UTF8));
							}
							else if (File.Exists(LOCPATH)) {
								Locale.LoadCustomLoc(File.ReadAllLines(LOCPATH, Encoding.UTF8));
							}
						}
						else if (File.Exists(LOCPATH)) {
							Locale.LoadCustomLoc(File.ReadAllLines(LOCPATH, Encoding.UTF8));
						}
						_incProgress(5);
						logDeltaTicks("locales");
					}
					catch (Exception ex) {
						YobaDialog.ShowDialog(Locale.Get("CannotGetLocaleFile") + ":\r\n" + ex.Message);
					}
					try {
						InitProgressTracker();
						
#if DEBUG
#else
						string winVer = "";
						using (RegistryKey view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
							using (RegistryKey clsid = view.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion")) {
								winVer = (string)clsid.GetValue("ProductName"); // Тупо, но это единственный способ добыть реальный номер версии
							}
						}
						int verIdx = 1;
						if (winVer.StartsWith("Windows 11")) {
							verIdx = 3;
						}
						else if (winVer.StartsWith("Windows 10")) {
							verIdx = 2;
						}
						string loncherHash = "";
						string loncherUrl = "";
						string[] availKeys = { "Any", "Win7", "Win10", "Win11" };
						while (verIdx > -1) {
							if (Program.LoncherSettings.LoncherVersions.ContainsKey(availKeys[verIdx])) {
								LoncherForOSInfo loncherForOSInfo = Program.LoncherSettings.LoncherVersions[availKeys[verIdx]];
								loncherHash = loncherForOSInfo.LoncherHash;
								loncherUrl = loncherForOSInfo.LoncherExe;
								break;
							}
							verIdx--;
						}
						if (YU.StringHasText(loncherHash)) {
							string selfHash = FileChecker.GetFileMD5(Application.ExecutablePath);

							if (!loncherHash.Equals(selfHash, StringComparison.OrdinalIgnoreCase)) {
								if (YU.StringHasText(loncherUrl)) {
									string newLoncherPath = Application.ExecutablePath + ".new";
									string appname = Application.ExecutablePath;
									appname = appname.Substring(appname.LastIndexOf('\\') + 1);
									await loadFile(loncherUrl, newLoncherPath, Locale.Get("UpdatingLoncher"));

									string newHash = FileChecker.GetFileMD5(newLoncherPath);

									if (selfHash.Equals(Program.PreviousVersionHash)) {
										YU.ErrorAndKill(Locale.Get("LoncherOutOfDate2"));
									}
									else if (newHash.Equals(Program.PreviousVersionHash)) {
										YU.ErrorAndKill(Locale.Get("LoncherOutOfDate3"));
									}
									else {
										string args = String.Format("/C choice /C Y /N /D Y /T 1 & Del \"{0}\" & Rename \"{1}\" \"{2}\" & \"{0}\" -oldhash {3}"
													, Application.ExecutablePath, newLoncherPath, appname, selfHash);
										YU.RunCommand(args);
										Application.Exit();
									}
									return;
								}
								else {
									YU.ErrorAndKill(Locale.Get("LoncherOutOfDate1"));
									return;
								}
							}
						}
#endif
					}
					catch (Exception ex) {
						YU.ErrorAndKill(Locale.Get("CannotUpdateLoncher") + ":\r\n" + ex.Message, ex);
						return;
					}
					LauncherData.LauncherDataRaw launcherDataRaw = Program.LoncherSettings.RAW;
					try {
						if (await _assertFile(launcherDataRaw.Icon, IMGPATH, ICON_FILE)) {
							Bitmap bm = YU.ReadBitmap(ICON_FILE);
							if (bm != null) {
								Program.LoncherSettings.Icon = Icon.FromHandle(bm.GetHicon());
								this.Icon = Program.LoncherSettings.Icon;
							}
						}
						if (Program.LoncherSettings.Icon == null) {
							Program.LoncherSettings.Icon = this.Icon;
						}
						if (await _assertFile(launcherDataRaw.PreloaderBackground, IMGPATH, BG_FILE)) {
							this.BackgroundImage = YU.ReadBitmap(BG_FILE);
						}
						bool gotRandomBG = false;
						if (launcherDataRaw.RandomBackgrounds != null && launcherDataRaw.RandomBackgrounds.Count > 0) {
							int randomBGRoll = new Random().Next(0, 1000);
							int totalRoll = 0;
							foreach (RandomBgImageInfo rbgi in launcherDataRaw.RandomBackgrounds) {
								if (await _assertFile(rbgi.Background, IMGPATH)) {
									totalRoll += rbgi.Chance;
									if (totalRoll > randomBGRoll) {
										Program.LoncherSettings.Background = YU.ReadBitmap(IMGPATH + rbgi.Background.Path);
										Program.LoncherSettings.BackgroundPath = IMGPATH + rbgi.Background.Path;
										gotRandomBG = true;
										break;
									}
								}
							}
						}
						if (!gotRandomBG && await _assertFile(launcherDataRaw.Background, IMGPATH)) {
							Program.LoncherSettings.Background = YU.ReadBitmap(IMGPATH + launcherDataRaw.Background.Path);
							Program.LoncherSettings.BackgroundPath = IMGPATH + launcherDataRaw.Background.Path;
						}

						if (Program.LoncherSettings.UI.Count > 0) {
							string[] keys = Program.LoncherSettings.UI.Keys.ToArray();
							foreach (string key in keys) {
								if (!(await _assertFile(Program.LoncherSettings.UI[key].BgImage, IMGPATH))) {
									Program.LoncherSettings.UI[key].BgImage = null;
								}
								if (!(await _assertFile(Program.LoncherSettings.UI[key].BgImageClick, IMGPATH))) {
									Program.LoncherSettings.UI[key].BgImageClick = null;
								}
								if (!(await _assertFile(Program.LoncherSettings.UI[key].BgImageHover, IMGPATH))) {
									Program.LoncherSettings.UI[key].BgImageHover = null;
								}
							}
						}
					
						logDeltaTicks("images");
					}
					catch (Exception ex) {
						YU.ErrorAndKill(Locale.Get("CannotGetImages") + ":\r\n" + ex.Message, ex);
						return;
					}
					if (Program.LoncherSettings.Fonts != null) {
						List<string> keys = Program.LoncherSettings.Fonts.Keys.ToList();
						if (keys.Count > 0) {
							try {
								if (!Directory.Exists(FNTPATH)) {
									Directory.CreateDirectory(FNTPATH);
								}
								foreach (string key in keys) {
									using (Font fontTester = new Font(key, 12, FontStyle.Regular, GraphicsUnit.Pixel)) {
										if (fontTester.Name == key) {
											Program.LoncherSettings.Fonts[key] = "win";
										}
										else if (File.Exists(FNTPATH + key)) {
											Program.LoncherSettings.Fonts[key] = "local";
										}
										else {
											string status = "none";
											string src = Program.LoncherSettings.Fonts[key];
											string filename = FNTPATH + key;
											if (YU.StringHasText(src)) {
												await _loadFile(src, filename);
												if (File.Exists(filename)) {
													status = "local";
												}
											}
											Program.LoncherSettings.Fonts[key] = status;
										}
									}
								}
								logDeltaTicks("fonts");
							}
							catch (Exception ex) {
								YU.ErrorAndKill(Locale.Get("CannotGetFonts") + ":\r\n" + ex.Message, ex);
								return;
							}
						}
					}
					try {
						foreach (FileInfo fi in Program.LoncherSettings.Assets) {
							await _assertFile(fi, Program.LONCHER_DATA_PATH);
						}
						logDeltaTicks("assets");
					}
					catch (Exception ex) {
						YU.ErrorAndKill(Locale.Get("CannotGetAssets") + ":\r\n" + ex.Message, ex);
						return;
					}
					try {
						statusLabel.Text = "";
						loadingLabel.Text = Locale.Get("PreparingToLaunch");
						Program.LoncherSettings.MainPage = await GetMainPageData();
						_incProgress(5);
						logDeltaTicks("changelog and etc");
						try {
							if (_findGamePath()) {
								try {
									_updateGameVersion();
									if (_oldMainForm != null) {
										_oldMainForm.Dispose();
									}
									int progressBarPerFile = 100 - _progressBar1.Value;
									if (progressBarPerFile < Program.LoncherSettings.Files.Count) {
										progressBarPerFile = 88;
										_progressBar1.Value = 6;

									}
									progressBarPerFile = progressBarPerFile / Program.LoncherSettings.Files.Count;
									if (progressBarPerFile < 1) {
										progressBarPerFile = 1;
									}

									List<FileInfo> mainFiles = Program.LoncherSettings.Files;

									logDeltaTicks("Main files check start");
									statusLabel.Text = "";
									loadingLabel.Text = Locale.Get("CheckingMainFiles");
									Program.GameFileCheckResult = await FileChecker.CheckFiles(
										mainFiles
										, new EventHandler<FileCheckedEventArgs>((object o, FileCheckedEventArgs a) => {
											_progressBar1.Value += progressBarPerFile;
											if (_progressBar1.Value > 100) {
												_progressBar1.Value = 40;
											}
										})
									);

									logDeltaTicks("Main files check end");

									loadingLabel.Text = Locale.Get("CheckingModFiles");
									List<FileInfo> allModFiles = Program.LoncherSettings.GameVersion.AllModFiles;
									if (allModFiles.Count > 0) {
										await FileChecker.CheckFiles(
											allModFiles
											, new EventHandler<FileCheckedEventArgs>((object o, FileCheckedEventArgs a) => {
												_progressBar1.Value += progressBarPerFile;
												if (_progressBar1.Value > 100) {
													_progressBar1.Value = 40;
												}
											})
										);
									}

									logDeltaTicks("Mod check end");
									LauncherConfig.Save();
									_showMainForm();
								}
								catch (Exception ex) {
									YU.ErrorAndKill(Locale.Get("CannotCheckFiles") + ":\r\n" + ex.Message, ex);
								}
							}
						}
						catch (Exception ex) {
							YU.ErrorAndKill(Locale.Get("CannotParseConfig") + ":\r\n" + ex.Message, ex);
						}
					}
					catch (Exception ex) {
						YU.ErrorAndKill(Locale.Get("CannotLoadIcon") + ":\r\n" + ex.Message, ex);
					}
				}
				catch (Exception ex) {
					YU.ErrorAndKill(Locale.Get("CannotParseSettings") + ":\r\n" + ex.Message, ex);
				}
			}
			catch (Exception ex) {
				UIElement[] btns;
				UIElement btnQuit = new UIElement();
				btnQuit.Caption = Locale.Get("Quit");
				btnQuit.Result = DialogResult.Abort;
				UIElement btnRetry = new UIElement();
				btnRetry.Caption = Locale.Get("Retry");
				btnRetry.Result = DialogResult.Retry;
				string msg;
				if (File.Exists(SETTINGSPATH)) {
					msg = Locale.Get("WebClientErrorOffline");
					UIElement btnOffline = new UIElement();
					btnOffline.Caption = Locale.Get("RunOffline");
					btnOffline.Result = DialogResult.Ignore;
					btns = new UIElement[] { btnQuit, btnRetry, btnOffline };
				}
				else {
					msg = Locale.Get("WebClientError");
					btns = new UIElement[] { btnQuit, btnRetry };
				}
				YobaDialog yobaDialog = new YobaDialog(msg, btns);
				yobaDialog.Icon = Program.LoncherSettings != null ? (Program.LoncherSettings.Icon ?? this.Icon) : this.Icon;
				DialogResult result = yobaDialog.ShowDialog(this);
				switch (result) {
					case DialogResult.Retry:
						_initialize();
						break;
					case DialogResult.Ignore:
						_initializeOffline();
						break;
					case DialogResult.Abort: {
							Application.Exit();
							return;
						}
				}
			}
		}

		private async void _initializeOffline() {
			try {
				Program.OfflineMode = true;
				string settingsJson = File.ReadAllText(SETTINGSPATH);
				Program.LoncherSettings = new LauncherData(settingsJson);
				LauncherData.LauncherDataRaw launcherDataRaw = Program.LoncherSettings.RAW;
				_incProgress(10);
				try {
					try {
						if (File.Exists(LOCPATH)) {
							Locale.LoadCustomLoc(File.ReadAllLines(LOCPATH, Encoding.UTF8));
						}
					}
					catch (Exception ex) {
						YobaDialog.ShowDialog(Locale.Get("CannotGetLocaleFile") + ":\r\n" + ex.Message);
					}
					if (_assertOfflineFile(launcherDataRaw.Background, IMGPATH)) {
						Program.LoncherSettings.Background = YU.ReadBitmap(IMGPATH + launcherDataRaw.Background.Path);
						Program.LoncherSettings.BackgroundPath = IMGPATH + launcherDataRaw.Background.Path;
					}
					if (_assertOfflineFile(launcherDataRaw.Icon, IMGPATH, ICON_FILE)) {
						Bitmap bm = YU.ReadBitmap(IMGPATH + launcherDataRaw.Icon.Path);
						if (bm != null) {
							Program.LoncherSettings.Icon = Icon.FromHandle(bm.GetHicon());
						}
					}
					if (_assertOfflineFile(launcherDataRaw.PreloaderBackground, IMGPATH, BG_FILE)) {
						this.BackgroundImage = YU.ReadBitmap(IMGPATH + launcherDataRaw.PreloaderBackground.Path);
					}
					if (Program.LoncherSettings.Icon == null) {
						Program.LoncherSettings.Icon = this.Icon;
					}
					if (Program.LoncherSettings.UI.Count > 0) {
						string[] keys = Program.LoncherSettings.UI.Keys.ToArray();
						foreach (string key in keys) {
							if (!(_assertOfflineFile(Program.LoncherSettings.UI[key].BgImage, IMGPATH))) {
								Program.LoncherSettings.UI[key].BgImage = null;
							}
						}
					}
				}
				catch (Exception ex) {
					YU.ErrorAndKill(Locale.Get("CannotGetImages") + ":\r\n" + ex.Message, ex);
					return;
				}
				if (Program.LoncherSettings.Fonts != null) {
					List<string> keys = Program.LoncherSettings.Fonts.Keys.ToList();
					if (keys.Count > 0) {
						try {
							if (!Directory.Exists(FNTPATH)) {
								Directory.CreateDirectory(FNTPATH);
							}
							foreach (string key in keys) {
								using (Font fontTester = new Font(key, 12, FontStyle.Regular, GraphicsUnit.Pixel)) {
									if (fontTester.Name == key) {
										Program.LoncherSettings.Fonts[key] = "win";
									}
									else if (File.Exists(FNTPATH + key)) {
										Program.LoncherSettings.Fonts[key] = "local";
									}
									else {
										Program.LoncherSettings.Fonts[key] = "none";
									}
								}
							}
						}
						catch (Exception ex) {
							YU.ErrorAndKill(Locale.Get("CannotGetFonts") + ":\r\n" + ex.Message, ex);
							return;
						}
					}
				}
				try {
					try {
						Program.LoncherSettings.MainPage = GetMainPageDataOffline();
						
						if (_findGamePath()) {
							try {
								_updateGameVersion();
								_incProgress(10);
								/*Program.GameFileCheckResult = FileChecker.CheckFilesOffline(Program.LoncherSettings.Files);
								foreach (ModInfo mi in Program.LoncherSettings.Mods) {
									if (mi.ModConfigurationInfo != null) {
										FileChecker.CheckFilesOffline(mi.CurrentVersionFiles);
									}
								}*/
								int progressBarPerFile = 100 - _progressBar1.Value;
								if (progressBarPerFile < Program.LoncherSettings.Files.Count) {
									progressBarPerFile = 88;
									_progressBar1.Value = 6;

								}
								progressBarPerFile = progressBarPerFile / Program.LoncherSettings.Files.Count;
								if (progressBarPerFile < 1) {
									progressBarPerFile = 1;
								}

								List<FileInfo> mainFiles = Program.LoncherSettings.Files;

								statusLabel.Text = "";
								loadingLabel.Text = Locale.Get("CheckingMainFiles");
								Program.GameFileCheckResult = await FileChecker.CheckFiles(
									mainFiles
									, new EventHandler<FileCheckedEventArgs>((object o, FileCheckedEventArgs a) => {
										_progressBar1.Value += progressBarPerFile;
										if (_progressBar1.Value > 100) {
											_progressBar1.Value = 40;
										}
									})
								);

								loadingLabel.Text = Locale.Get("CheckingModFiles");
								List<FileInfo> allModFiles = Program.LoncherSettings.GameVersion.AllModFiles;
								if (allModFiles.Count > 0) {
									await FileChecker.CheckFiles(
										allModFiles
										, new EventHandler<FileCheckedEventArgs>((object o, FileCheckedEventArgs a) => {
											_progressBar1.Value += progressBarPerFile;
											if (_progressBar1.Value > 100) {
												_progressBar1.Value = 40;
											}
										})
									);
								}
								LauncherConfig.Save();
								_showMainForm();
							}
							catch (Exception ex) {
								YU.ErrorAndKill(Locale.Get("CannotCheckFiles") + ":\r\n" + ex.Message, ex);
							}
						}
					}
					catch (Exception ex) {
						YU.ErrorAndKill(Locale.Get("CannotParseConfig") + ":\r\n" + ex.Message, ex);
					}
				}
				catch (Exception ex) {
					YU.ErrorAndKill(Locale.Get("CannotLoadIcon") + ":\r\n" + ex.Message, ex);
				}
			}
			catch (Exception ex) {
				YU.ErrorAndKill(Locale.Get("CannotParseSettings") + ":\r\n" + ex.Message, ex);
			}
		}

		private void _preloaderForm_KeyUp(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.F1) {
				YU.ShowHelpDialog();
			}
		}
	}
}