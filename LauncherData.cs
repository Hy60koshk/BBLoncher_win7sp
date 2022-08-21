﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace YobaLoncher {
	public enum StartPageEnum {
		Changelog = 0
		, Status = 1
		, Links = 2
		, Mods = 3
		, FAQ = 4
	}
	public enum ModConflictTypeEnum {
		Incompatible = 0
		, Substitute = 1
	}

	public class ModCfgInfo {
		public string Id = null;
		public string Name;
		public string GameVersion;
		public bool Active = false;
		public bool Altered = false;
		public List<string> FileList;

		public ModCfgInfo(string id, string name, string gameVersion, List<string> fileList, bool active) {
			Id = id;
			Name = name;
			GameVersion = gameVersion;
			Active = active;
			FileList = fileList;
		}
	}
	public class LoncherForOSInfo {
		public string OSName;
		public string LoncherHash;
		public string LoncherExe;

		public LoncherForOSInfo(string osName, string loncherExe, string loncherHash) {
			OSName = osName;
			LoncherHash = loncherHash;
			LoncherExe = loncherExe;
		}
	}
	static class LauncherConfig {
		public static bool HasUnsavedChanges = false;

		public static string GameDir = null;
		public static string GalaxyDir = null;
		public static bool LaunchFromGalaxy = false;
		public static bool StartOffline = false;
		public static bool CloseOnLaunch = false;
		public static string LastSurveyId = null;
		public static Dictionary<string, string> FileDates = new Dictionary<string, string>();
		public static Dictionary<string, string> FileDateHashes = new Dictionary<string, string>();
		public static int WindowHeight = 440;
		public static int WindowWidth = 780;
		public static StartPageEnum StartPage = StartPageEnum.Status;
		private const string CFGFILE = @"loncherData\loncher.cfg";
		private const string MODINFOFILE = @"loncherData\installedMods.json";
		public static List<ModCfgInfo> InstalledMods = new List<ModCfgInfo>();

		public static void Save() {
			try {
				File.WriteAllLines(CFGFILE, new string[] {
					"path = " + GameDir
					, "startpage = " + (int)StartPage
					, "startviagalaxy = " + (LaunchFromGalaxy ? 1 : 0)
					, "offlinemode = " + (StartOffline ? 1 : 0)
					, "closeonlaunch = " + (CloseOnLaunch ? 1 : 0)
					, "lastsrvchk = " + LastSurveyId
					, "windowheight = " + WindowHeight
					, "windowwidth = " + WindowWidth
					, "filedates = " + JsonConvert.SerializeObject(FileDates)
					, "filedatehashes = " + JsonConvert.SerializeObject(FileDateHashes)
				});
				HasUnsavedChanges = false;
			}
			catch (Exception ex) {
				YobaDialog.ShowDialog(Locale.Get("CannotWriteCfg") + ":\r\n" + ex.Message);
			}
		}

		public static void SaveMods() {
			try {
				File.WriteAllText(MODINFOFILE, JsonConvert.SerializeObject(InstalledMods));
			}
			catch (Exception ex) {
				YobaDialog.ShowDialog(Locale.Get("CannotWriteCfg") + ":\r\n" + ex.Message);
			}
		}

		private static bool ParseBooleanParam(string val) {
			return val.Length > 0 && !"0".Equals(val) && val.Length != 5;
		}

		private static int ParseIntParam(string val, int def) {
			if (val.Length > 0 && int.TryParse(val, out int intval)) {
				return intval;
			}
			return def;
		}

		public static void Load() {
			GalaxyDir = YU.GetGogGalaxyPath();
			try {
				if (File.Exists(CFGFILE)) {
					string[] lines = File.ReadAllLines(CFGFILE);
					foreach (string line in lines) {
						if (line.Length > 0) {
							int eqidx = line.IndexOf('=');
							if (eqidx > -1) {
								string key = line.Substring(0, eqidx).Trim();
								string val = line.Substring(eqidx + 1).Trim();
								switch (key) {
									case "path":
										GameDir = val;
										break;
									case "startpage":
										int spidx = ParseIntParam(val, 100);
										if (spidx > -1 && spidx < 4) {
											StartPage = (StartPageEnum)spidx;
										}
										break;
									case "windowheight":
										WindowHeight = ParseIntParam(val, WindowHeight);
										break;
									case "windowwidth":
										WindowWidth = ParseIntParam(val, WindowWidth);
										break;
									case "lastsrvchk":
										LastSurveyId = val;
										break;
									case "startviagalaxy":
										if (GalaxyDir != null) {
											LaunchFromGalaxy = ParseBooleanParam(val);
										}
										break;
									case "offlinemode":
										StartOffline = ParseBooleanParam(val);
										break;
									case "closeonlaunch":
										CloseOnLaunch = ParseBooleanParam(val);
										break;
									case "filedates":
										try {
											FileDates = JsonConvert.DeserializeObject<Dictionary<string, string>>(val);
										}
										catch (Exception) {
											// похуй
										}
										break;
									case "filedatehashes":
										try {
											FileDateHashes = JsonConvert.DeserializeObject<Dictionary<string, string>>(val);
										}
										catch (Exception) {
											// похуй
										}
										break;
								}
							}
						}
					}
					if (FileDates.Count > 0) {
						Dictionary<string, string>.KeyCollection keys = FileDates.Keys;
						foreach (string key in keys) {
							if (!FileDateHashes.ContainsKey(key)) {
								FileDates.Remove(key);
							}
						}
					}
				}
				if (File.Exists(MODINFOFILE)) {
					InstalledMods = JsonConvert.DeserializeObject<List<ModCfgInfo>>(File.ReadAllText(MODINFOFILE));
					List<string> names = new List<string>();
					if (InstalledMods.Count > 0) {
						if (InstalledMods[0].Id == null) {
							// In case of old mod settings format
							for (int i = InstalledMods.Count - 1; i > -1; i--) {
								ModCfgInfo mci = InstalledMods[i];
								if (names.FindIndex(x => x.Equals(mci.Name)) > -1) {
									InstalledMods.RemoveAt(i);
								}
								else {
									names.Add(mci.Name);
								}
							}
						}
						else {
							for (int i = InstalledMods.Count - 1; i > -1; i--) {
								ModCfgInfo mci = InstalledMods[i];
								if (names.FindIndex(x => x.Equals(mci.Id)) > -1) {
									InstalledMods.RemoveAt(i);
								}
								else {
									names.Add(mci.Id);
								}
							}
						}
					}
				}
			}
			catch (Exception ex) {
				YobaDialog.ShowDialog(Locale.Get("CannotReadCfg") + ":\r\n" + ex.Message);
			}
		}
	}

	class UninstallationRules {
		public List<FileInfo> FilesToDelete;
	}

	class LauncherData {
#pragma warning disable 649

		public Dictionary<string, GameVersion> GameVersions = new Dictionary<string, GameVersion>();
		public GameVersion GameVersion = null;
		public List<FileInfo> Files = new List<FileInfo>();
		public List<ModInfo> Mods = new List<ModInfo>();
		public Dictionary<string, UIElement> UI;
		public Dictionary<string, FileInfo> UIStyle;
		public List<FileInfo> Assets;
		public string GameName;
		public string SteamGameFolder;
		public string ExeName;
		public string SteamID;
		public string GogID;
		public StaticTabData MainPage = new StaticTabData();
		public Dictionary<string, LoncherForOSInfo> LoncherVersions;
		public StartPageEnum StartPage;
		public SurveyInfo Survey;
		public Dictionary<string, string> Fonts;
		public Image Background;
		public string BackgroundPath;
		public Image PreloaderBackground;
		public Icon Icon;
		private WebClient wc_;
		private LauncherDataRaw raw_;
		public LauncherDataRaw RAW => raw_;
		public List<string> ModFilesInUse = new List<string>();
		public List<string> ModFilesToDelete = new List<string>();
		public Dictionary<string, FileInfo> ExistingModFiles = new Dictionary<string, FileInfo>();

		public UninstallationRules UninstallationRules;

		public class StaticTabData {
			public string Html = null;
			public string Site = null;
			public string Error = null;
		}

		public class LauncherDataRaw {
			public string GameName;
			public string SteamGameFolder;
			public List<GameVersion> GameVersions;
			public List<RawModInfo> Mods;
			public BgImageInfo Background;
			public FileInfo PreloaderBackground;
			public FileInfo Localization;
			public FileInfo Icon;
			public SurveyInfo Survey;
			public string ExeName;
			public StartPageEnum StartPage = StartPageEnum.Status;
			public string SteamID;
			public string GogID;
			public string MainPage;
			public Dictionary<string, LoncherForOSInfo> LoncherVersions;
			public List<RandomBgImageInfo> RandomBackgrounds;
			public Dictionary<string, UIElement> UI;
			public Dictionary<string, FileInfo> UIStyle;
			public List<FileInfo> Assets;
			public UninstallationRules UninstallationRules;
		}

		public LauncherData(string json) {

			LauncherDataRaw raw = JsonConvert.DeserializeObject<LauncherDataRaw>(json);
			raw_ = raw;
			wc_ = new WebClient();
			wc_.Encoding = System.Text.Encoding.UTF8;

			StartPage = raw.StartPage;
			UI = raw.UI ?? new Dictionary<string, UIElement>();
			UIStyle = raw.UIStyle ?? new Dictionary<string, FileInfo>();
			Assets = raw.Assets ?? new List<FileInfo>();

			LoncherVersions = raw.LoncherVersions;
			ExeName = raw.ExeName;
			SteamID = raw.SteamID;
			GogID = raw.GogID;
			Survey = raw.Survey;

			GameName = raw.GameName;
			SteamGameFolder = raw.SteamGameFolder;

			UninstallationRules = raw.UninstallationRules ?? new UninstallationRules();

			GameVersions = PrepareGameVersions(raw.GameVersions);
			if (raw.Mods != null && raw.Mods.Count > 0) {
				foreach (RawModInfo rmi in raw.Mods) {
					if (YU.stringHasText(rmi.Name) && rmi.GameVersions != null) {
						Mods.Add(new ModInfo(rmi.Name, rmi.Description, rmi.DetailedDescription, PrepareGameVersions(rmi.GameVersions), rmi.Screenshots));
					}
				}
			}
		}

		public Dictionary<string, GameVersion> PrepareGameVersions(List<GameVersion> rawGameVersions) {
			Dictionary<string, GameVersion> partialGameVersions = new Dictionary<string, GameVersion>();
			Dictionary<string, GameVersion> mergedGameVersions = new Dictionary<string, GameVersion>();
			foreach (GameVersion gv in rawGameVersions) {
				string key = YU.stringHasText(gv.ExeVersion) ? gv.ExeVersion : "==";
				if (partialGameVersions.ContainsKey(key)) {
					throw new Exception(string.Format(Locale.Get("MultipleFileBlocksForSingleGameVersion"), key));
				}
				partialGameVersions.Add(key, gv);
			}
			List<string> gvkeys = partialGameVersions.Keys.ToList();
			string[] anyKeys = new string[] { "DEFAULT", "ANY", "==" };
			gvkeys.RemoveAll(s => anyKeys.Contains(s));

			GameVersion defaultGV = new GameVersion();
			foreach (string anyKey in anyKeys) {
				if (partialGameVersions.ContainsKey(anyKey)) {
					defaultGV.MergeFrom(partialGameVersions[anyKey]);
				}
			}
			defaultGV.SortFiles();
			mergedGameVersions.Add("DEFAULT", defaultGV);

			foreach (string gvkey in gvkeys) {
				GameVersion gv = new GameVersion();
				gv.MergeFrom(defaultGV);
				gv.MergeFrom(partialGameVersions[gvkey]);
				gv.ExeVersion = gvkey;
				gv.Description = partialGameVersions[gvkey].Description;
				gv.SortFiles();
				mergedGameVersions.Add(gvkey, gv);

			}
			return mergedGameVersions;
		}

		public void LoadFileListForVersion(string curVer) {
			if (YU.stringHasText(curVer)) {
				if (GameVersions.ContainsKey(curVer)) {
					GameVersion = GameVersions[curVer];
				}
			}
			if (GameVersion is null) {
				if (GameVersions.ContainsKey("DEFAULT")) {
					GameVersion = GameVersions["DEFAULT"];
				}
				else if (GameVersions.ContainsKey("OTHER")) {
					GameVersion = GameVersions["OTHER"];
				}
			}
			if (GameVersion is null) {
				throw new Exception(string.Format(Locale.Get("OldGameVersion"), curVer));
			}
			foreach (FileGroup fileGroup in GameVersion.FileGroups) {
				if (fileGroup.Files != null) {
					Files.AddRange(fileGroup.Files);
				}
			}
			if (GameVersion.Files != null) {
				Files.AddRange(GameVersion.Files);
			}
			foreach (FileInfo fi in Files) {
				fi.NormalizeHashes();
			}
			foreach (ModInfo mi in Mods) {
				mi.InitCurrentVersion(GameVersion, curVer);
			}
			foreach (string file in ModFilesToDelete) {
				if (ModFilesInUse.FindIndex(x => x.Equals(file)) < 0) {
					File.Delete(Program.GamePath + file);
				}
			}
		}
	}

	class GameVersion {
		public string ExeVersion = null;
		public List<FileInfo> Files = new List<FileInfo>();
		public List<FileGroup> FileGroups = new List<FileGroup>();
		public List<FileInfo> AllModFiles = new List<FileInfo>();
		public string Name = null;
		public string Description = null;

		public void SortFiles() {
			foreach (FileGroup fg in FileGroups) {
				if (fg.OrderIndex == -99) {
					fg.OrderIndex = 1;
				}
				foreach (FileInfo fi in fg.Files) {
					if (fi.OrderIndex == -99) {
						fi.OrderIndex = 1;
					}
				}
				fg.Files.Sort();
			}
			FileGroups.Sort();
		}
		public void ResetCheckedToDl() {
			foreach (FileGroup fg in FileGroups) {
				fg.ResetCheckedToDl();
			}
			foreach (FileInfo fi in Files) {
				fi.ResetCheckedToDl();
			}
		}
		public void MergeFrom(GameVersion seniorVersion) {
			if (this.FileGroups.Count > 0) {
				foreach (FileGroup fg in seniorVersion.FileGroups) {
					FileGroup targetGroup = this.FileGroups.Find(x => x.Name == fg.Name);
					if (targetGroup is null) {
						this.FileGroups.Add(FileGroup.CopyOf(fg));
					}
					else {
						if (fg.OrderIndex != -99) {
							targetGroup.OrderIndex = fg.OrderIndex;
						}
						if (targetGroup.Files.Count > 0) {
							foreach (FileInfo file in fg.Files) {
								int sameFileIdx = targetGroup.Files.FindIndex(x => x.Path == file.Path);
								if (sameFileIdx > -1) {
									targetGroup.Files.RemoveAt(sameFileIdx);
								}
							}
						}
						targetGroup.Files.AddRange(fg.Files);
					}
				}
			}
			else {
				foreach (FileGroup fg in seniorVersion.FileGroups) {
					this.FileGroups.Add(FileGroup.CopyOf(fg));
				}
			}
			if (this.Files.Count > 0) {
				foreach (FileInfo file in seniorVersion.Files) {
					int sameFileIdx = seniorVersion.Files.FindIndex(x => x.Path == file.Path);
					if (sameFileIdx > -1) {
						this.Files.RemoveAt(sameFileIdx);
					}
				}
			}
			this.Files.AddRange(seniorVersion.Files);
		}
	}

	class FileGroup : IComparable<FileGroup> {
		public string Name = null;
		public List<FileInfo> Files = new List<FileInfo>();
		public int OrderIndex = -99;
		public bool Collapsed = false;

		public int CompareTo(FileGroup fg) {
			return OrderIndex.CompareTo(fg.OrderIndex);
		}
		public int CompareTo(FileInfo fi) {
			return OrderIndex.CompareTo(fi.OrderIndex);
		}

		public static FileGroup CopyOf(FileGroup fg) {
			FileGroup targetGroup = new FileGroup() {
				Name = fg.Name,
				OrderIndex = fg.OrderIndex
			};
			targetGroup.Files.AddRange(fg.Files);
			return targetGroup;
		}

		public void ResetCheckedToDl() {
			foreach (FileInfo fi in Files) {
				fi.ResetCheckedToDl();
			}
		}
	}

	class FileInfo : IComparable<FileInfo> {
		public string Url;
		public string Path;
		public string Description;
		public string Tooltip;
		public List<string> Hashes;
		public bool IsOK = false;
		public bool IsPresent = false;
		public bool IsDateChecked = false;
		public string UploadAlias;
		public uint Size = 0;
		public int Importance = 0;
		public int OrderIndex = -99;
		public bool IsCheckedToDl = false;
		[JsonIgnore]
		public List<ModInfo> UsedByMods = new List<ModInfo>();

		[JsonIgnore]
		public bool IsComplete {
			get {
				return Url != null && Path != null && Hashes != null
					&& Url.Length > 0 && Path.Length > 0 && Hashes.Count > 0;
			}
		}
		[JsonIgnore]
		public bool HasValidInfo {
			get {
				return Url != null && Path != null && Url.Length > 0 && Path.Length > 0
					&& (Importance > 0 || (Hashes != null && Hashes.Count > 0));
			}
		}
		[JsonIgnore]
		public bool HasHashes {
			get {
				return Hashes != null && Hashes.Count > 0;
			}
		}

		public void NormalizeHashes() {
			if (HasHashes) {
				for (int i = 0; i < Hashes.Count; i++) {
					Hashes[i] = Hashes[i].ToUpper();
				}
			}
		}
		public bool HasHash(string hash) {
			if (HasHashes) {
				return null != Hashes.Find(x => x.Equals(hash.ToUpper()));
			}
			return false;
		}
		public bool IsHashAcceptable(string hash) {
			if (Importance > 0) {
				return true;
			}
			if (HasHashes) {
				return null != Hashes.Find(x => x.Equals(hash.ToUpper()));
			}
			YobaDialog.ShowDialog(Path + "\r\n" + Url + "\r\n\r\nNo hashes specified for a file that has to be checked for a valid hash!\r\n(Иными словами, админ накосячил. Сообщите об этой ошибке как можно скорее.)");
			return false;
		}
		public bool AreHashesSame(FileInfo fi) {
			if (HasHashes && fi.HasHashes) {
				return Enumerable.SequenceEqual(fi.Hashes, Hashes);
			}
			return false;
		}

		public int CompareTo(FileGroup fg) {
			return OrderIndex.CompareTo(fg.OrderIndex);
		}
		public int CompareTo(FileInfo fi) {
			return OrderIndex.CompareTo(fi.OrderIndex);
		}

		public void ResetCheckedToDl() {
			if (IsOK) {
				IsCheckedToDl = false;
			}
			else if (Importance == 0 || (Importance == 1 && !IsPresent)) {
				IsCheckedToDl = true;
			}
		}
	}
	class BgImageInfo : FileInfo {
		public string Layout;
		public ImageLayout ImageLayout = ImageLayout.Stretch;
	}

	class UIElement {
		public string Color;
		public string BgColor;
		public string BgColorDown;
		public string BgColorHover;
		public string BorderColor = "gray";
		public int BorderSize = -1;
		public BgImageInfo BgImage;
		public BgImageInfo BgImageClick;
		public BgImageInfo BgImageHover;
		public string Caption;
		public string Font;
		public string FontSize;
		public Vector Position;
		public Vector Size;
		public bool CustomStyle = false;
		public DialogResult Result;
	}
	class LinkButton : UIElement {
		public string Url;
	}
	class SurveyInfo {
		public string Text;
		public string ID;
		public string Url;
	}
	class RandomBgImageInfo {
		public BgImageInfo Background;
		public int Chance = 100;
	}
	class Vector {
		public int X = 0;
		public int Y = 0;
	}

	class ModDependency {
		public string[] Options;
		public ModDependency(string[] options) {
			Options = options;
		}
		public ModDependency(string option) {
			Options = new string[]{ option };
		}
	}
	class ModConflict {
		public static int ModConflictMax = Enum.GetValues(typeof(ModConflictTypeEnum)).Cast<int>().Max();

		public string ModId;
		public ModConflictTypeEnum ConflictType;
		public ModConflict(string modId, int conflictType) {
			ModId = modId;
			ConflictType = (-1 < conflictType && conflictType < ModConflictMax) ? (ModConflictTypeEnum)conflictType : ModConflictTypeEnum.Incompatible;
		}
	}

	class RawModInfo {
		public string Id;
		public string Name;
		public string Description = "";
		public List<GameVersion> GameVersions;
		public string DetailedDescription;
		public List<string> Screenshots;
	}
	class ModInfo {
		public string Id;
		public string Name;
		public string VersionedName;
		public string VersionedDescription;
		public string Description;
		public string DetailedDescription;
		public List<string> Screenshots;
		public List<ModDependency> Dependencies;
		public List<string> Conflicts;
		public Dictionary<string, GameVersion> GameVersions;
		public ModInfo(
				string name
				, string descr
				, string detdescr
				, Dictionary<string, GameVersion> gv
				, List<string> screenshots
			) {
			Description = descr;
			DetailedDescription = detdescr;
			Name = name;
			GameVersions = gv;
			Screenshots = screenshots;
		}
		public List<FileInfo> CurrentVersionFiles;
		public GameVersion CurrentVersionData;
		public ModCfgInfo ModConfigurationInfo;
		public bool DlInProgress = false;

		private void AddFilesForCurrentVersion(List<FileInfo> files) {
			foreach (FileInfo fi in files) {
				FileInfo existing = CurrentVersionFiles.Find(cvf => cvf.Path.Equals(fi.Path));
				if (null == existing) {
					CurrentVersionFiles.Add(fi);
					fi.NormalizeHashes();
				}
				else {
					if (!Enumerable.SequenceEqual(fi.Hashes, existing.Hashes)) {
						YobaDialog.ShowDialog(String.Format(Locale.Get("SameFileWithDifferentHashWarning"), fi.Path));
					}
				}
			}
		}

		public void InitCurrentVersion(GameVersion gv, string curVer) {
			ModConfigurationInfo = LauncherConfig.InstalledMods.Find(x => x.Id == null ? x.Name.Equals(Name) : x.Id.Equals(Id));
			CurrentVersionData = null;
			CurrentVersionFiles = new List<FileInfo>();
			if (YU.stringHasText(curVer)) {
				if (GameVersions.ContainsKey(curVer)) {
					CurrentVersionData = GameVersions[curVer];
				}
			}
			if (CurrentVersionData is null) {
				if (GameVersions.ContainsKey("DEFAULT")) {
					CurrentVersionData = GameVersions["DEFAULT"];
				}
				else if (GameVersions.ContainsKey("OTHER")) {
					CurrentVersionData = GameVersions["OTHER"];
				}
			}
			if (CurrentVersionData is null) {
				CurrentVersionFiles = null;
				VersionedName = Name;
				VersionedDescription = Description;
			}
			else {
				VersionedName = CurrentVersionData.Name ?? Name;
				VersionedDescription = CurrentVersionData.Description ?? Description;
				foreach (FileGroup fileGroup in CurrentVersionData.FileGroups) {
					if (fileGroup.Files != null) {
						AddFilesForCurrentVersion(fileGroup.Files);
					}
				}
				if (CurrentVersionData.Files != null) {
					AddFilesForCurrentVersion(CurrentVersionData.Files);
				}
				if (CurrentVersionFiles.Count > 0) {
					for (int i = 0; i < CurrentVersionFiles.Count; i++) {
						FileInfo fi = CurrentVersionFiles[i];
						FileInfo existing = gv.AllModFiles.Find(gvf => gvf.Path == fi.Path);
						if (null == existing) {
							gv.AllModFiles.Add(fi);
							fi.UsedByMods.Add(this);
						}
						else {
							existing.UsedByMods.Add(this);
							CurrentVersionFiles[i] = existing;
							if (!fi.AreHashesSame(existing)) {
								YobaDialog.ShowDialog(String.Format(Locale.Get("SameFileWithDifferentHashBetweenModsWarning"), fi.Path));
							}
						}
					}
				}
				else {
					CurrentVersionFiles = null;
				}
			}
			if (ModConfigurationInfo != null) {
				if (CurrentVersionFiles is null) {
					if (ModConfigurationInfo.Active) {
						DisableOld();
					}
					else {
						ModConfigurationInfo = null;
					}
				}
				else if (ModConfigurationInfo.Active) {
					foreach (string file in ModConfigurationInfo.FileList) {
						if (CurrentVersionFiles.FindIndex(x => x.Path.Equals(file)) < 0) {
							Program.LoncherSettings.ModFilesToDelete.Add(file);//File.Delete(Program.GamePath + file);
							ModConfigurationInfo.Altered = true;
						}
						else {
							Program.LoncherSettings.ModFilesInUse.Add(file);
						}
					}
					if (ModConfigurationInfo.Altered) {
						ModConfigurationInfo.FileList = new List<string>();
						foreach (FileInfo fi in CurrentVersionFiles) {
							ModConfigurationInfo.FileList.Add(fi.Path);
						}
					}
				}
			}
		}
		public void Install() {
			List<string> fileList = new List<string>();
			foreach (FileInfo fi in CurrentVersionFiles) {
				fileList.Add(fi.Path);
			}
			ModConfigurationInfo = new ModCfgInfo(Id, Name, Program.GameVersion, fileList, true);
			int oldModConfInfoIdx = LauncherConfig.InstalledMods.FindIndex(x => x.Name.Equals(Name));
			if (oldModConfInfoIdx > -1) {
				LauncherConfig.InstalledMods.RemoveAt(oldModConfInfoIdx);
			}
			LauncherConfig.InstalledMods.Add(ModConfigurationInfo);
			LauncherConfig.SaveMods();
		}
		public void Delete() {
			string prefix = ModConfigurationInfo.Active ? Program.GamePath : Program.ModsDisabledPath;
			try {
				Directory.CreateDirectory(Program.ModsDisabledPath);
				List<string> disdirs = new List<string>();
				foreach (FileInfo fi in CurrentVersionFiles) {
					List<ModInfo> usedByMods = fi.UsedByMods.FindAll(mi => mi != this && mi.ModConfigurationInfo != null);
					if (usedByMods.Count > 0) {
						MoveFileToDisabled(fi, disdirs);
					}
					else {
						File.Delete(prefix + fi.Path);
						fi.IsOK = false;
						LauncherConfig.FileDates.Remove(fi.Path);
						LauncherConfig.FileDateHashes.Remove(fi.Path);
					}
				}
				LauncherConfig.InstalledMods.Remove(ModConfigurationInfo);
				ModConfigurationInfo = null;
				LauncherConfig.SaveMods();
			}
			catch (Exception ex) {
				if (YobaDialog.ShowDialog(String.Format(Locale.Get("DeleteModError"), ex.Message), YobaDialog.OKCopyStackBtns) == DialogResult.Retry) {
					YU.CopyExceptionToClipboard(ex);
				}
			}
		}
		public async Task<CheckResult> Enable() {
			CheckResult modFileCheckResult = null;
			try {
				foreach (FileInfo fi in CurrentVersionFiles) {
					string disabledFilePath = Program.ModsDisabledPath + fi.Path;
					string enabledFilePath = Program.GamePath + fi.Path;
					if (File.Exists(enabledFilePath)) {
						File.Delete(disabledFilePath);
					}
					else if (File.Exists(disabledFilePath)) {
						File.Move(disabledFilePath, enabledFilePath);
					}
				}
				modFileCheckResult = await FileChecker.CheckFiles(CurrentVersionFiles);
				ModConfigurationInfo.Active = true;
				LauncherConfig.SaveMods();
			}
			catch (Exception ex) {
				if (YobaDialog.ShowDialog(String.Format(Locale.Get("CannotEnableMod"), ex.Message), YobaDialog.OKCopyStackBtns) == DialogResult.Retry) {
					YU.CopyExceptionToClipboard(ex);
				}
			}
			return modFileCheckResult;
		}
		public void Disable() {
			MoveToDisabled(CurrentVersionFiles);
			ModConfigurationInfo.Active = false;
			LauncherConfig.SaveMods();
		}
		private void MoveToDisabled(List<FileInfo> version) {
			if (version != null) {
				try {
					Directory.CreateDirectory(Program.ModsDisabledPath);
					List<string> disdirs = new List<string>();
					foreach (FileInfo fi in version) {
						MoveFileToDisabled(fi, disdirs);
					}
				}
				catch (Exception ex) {
					if (YobaDialog.ShowDialog(String.Format(Locale.Get("DisableModError"), ex.Message), YobaDialog.OKCopyStackBtns) == DialogResult.Retry) {
						YU.CopyExceptionToClipboard(ex);
					}
				}
			}
		}
		private void MoveFileToDisabled(FileInfo fi, List<string> disdirs) {
			if (File.Exists(Program.GamePath + fi.Path)) {
				if (null == fi.UsedByMods.Find(mi => mi != this && mi.ModConfigurationInfo != null && mi.ModConfigurationInfo.Active)) {
					string path = fi.Path.Replace('/', '\\');
					bool hasSubdir = path.Contains('\\');
					path = Program.ModsDisabledPath + path;
					if (hasSubdir) {
						string disdir = path.Substring(0, path.LastIndexOf('\\'));
						if (!disdirs.Contains(disdir)) {
							Directory.CreateDirectory(disdir);
							disdirs.Add(disdir);
						}
					}
					if (File.Exists(path)) {
						File.Delete(path);
					}
					File.Move(Program.GamePath + fi.Path, path);
				}
			}
		}
		public void DisableOld() {
			List<FileInfo> oldVersionFiles = new List<FileInfo>();
			GameVersion gv = null;
			string oldGVId = ModConfigurationInfo.GameVersion;
			if (GameVersions.ContainsKey(oldGVId)) {
				gv = GameVersions[oldGVId];
			}
			if (gv is null) {
				if (GameVersions.ContainsKey("DEFAULT")) {
					gv = GameVersions["DEFAULT"];
				}
				else if (GameVersions.ContainsKey("OTHER")) {
					gv = GameVersions["OTHER"];
				}
			}
			if (gv is null) {
				oldVersionFiles = null;
			}
			else {
				foreach (FileGroup fileGroup in gv.FileGroups) {
					if (fileGroup.Files != null) {
						oldVersionFiles.AddRange(fileGroup.Files);
					}
				}
				if (gv.Files != null) {
					oldVersionFiles.AddRange(gv.Files);
				}
			}
			MoveToDisabled(oldVersionFiles);
			YobaDialog.ShowDialog(String.Format(Locale.Get("ModForOldVerDisabled"), ModConfigurationInfo.Name));
			ModConfigurationInfo.Active = false;
			ModConfigurationInfo = null;
		}
	}
}
