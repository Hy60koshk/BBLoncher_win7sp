
using System.Collections.Generic;

namespace YobaLoncher {
	class Locale {
		private static Dictionary<string, string> CustomLoc = new Dictionary<string, string>();
		public static void LoadCustomLoc(string[] lines) {
			foreach (string line in lines) {
				string[] keyval = line.Split('=');
				if (keyval.Length > 1) {
					string key = keyval[0].Trim();
					if (key.Length > 0) {
						string value = keyval[1].Trim().Replace("[n]", "\r\n");
						if (CustomLoc.ContainsKey(key)) {
							CustomLoc[key] = value;
						}
						else {
							CustomLoc.Add(key, value);
						}
					}
				}
			}
		}

		public static string Get(string key) {
			return Get(key, key);
		}

		public static string Get(string key, string def) {
			if (CustomLoc.ContainsKey(key)) {
				return CustomLoc[key];
			}
			if (DefaultLoc.ContainsKey(key)) {
				return DefaultLoc[key];
			}
			return def;
		}

		private static Dictionary<string, string> DefaultLoc = new Dictionary<string, string>() {
			{ "LaunchBtn", "Launch!" }
			, { "UpdateBtn", "Update" }
			, { "OK", "OK" }
			, { "Yes", "Yes" }
			, { "No", "No" }
			, { "ExitApp", "Close program" }
			, { "Ignore", "Ignore" }
			, { "CopyStackTrace", "Copy exception info" }
			, { "Done", "Done" }
			, { "Proceed", "Proceed" }
			, { "Cancel", "Cancel" }
			, { "Close", "Close" }
			, { "Minimize", "Minimize" }
			, { "Help", "Help" }
			, { "About", "About" }
			, { "Apply", "Apply" }
			, { "Quit", "Quit" }
			, { "Retry", "Retry" }
			, { "RunOffline", "Run offline" }
			, { "Browse", "Browse..." }
			, { "ChangelogBtn", "Changelog" }
			, { "LinksBtn", "Links" }
			, { "DonateBtn", "Donate" }
			, { "StatusBtn", "Status" }
			, { "SettingsBtn", "Settings" }
			, { "FAQBtn", "FAQ" }
			, { "ChangelogTooltip", "Changelog" }
			, { "StatusTooltip", "Status" }
			, { "LinksTooltip", "Links" }
			, { "DonateTooltip", "Donation options" }
			, { "SettingsTooltip", "Settings" }
			, { "FAQTooltip", "FAQ" }
			, { "Error", "Error" }
			, { "LoncherLoading", "{0} loading..." }
			, { "PressF1About", "Press F1 to show About" }
			, { "DLRate", "Downloading: {3} - {0} of {1} @ {2}" }
			, { "AllFilesIntact", "All files intact, we're ready to go." }
			, { "FilesMissing", "{0} files are missing or out of date." }
			, { "StatusListDownloadedFile", "File is OK" }
			, { "StatusListRecommendedFile", "Existing file" }
			, { "StatusListOptionalFile", "Optional file" }
			, { "StatusListRequiredFile", "Required file" }
			, { "StatusListDownloadedFileTooltip", "File is OK" }
			, { "StatusListRecommendedFileTooltip", "File already exists, and it is not neccessary for the file to be of particular version, yet if you will run into any problems, you might start from downloading the recommended version of the file." }
			, { "StatusListOptionalFileTooltip", "Optional file" }
			, { "StatusListRequiredFileTooltip", "Required file" }
			, { "StatusComboboxDownload", "Download" }
			, { "StatusComboboxDownloadForced", "Download" }
			, { "StatusComboboxNoDownload", "Don't download" }
			, { "StatusComboboxUpdate", "Update" }
			, { "StatusComboboxUpdateForced", "Update" }
			, { "StatusComboboxNoUpdate", "Don't update" }
			, { "StatusCopyingFiles", "Updating files in the game directory..." }
			, { "StatusUpdatingDone", "Done!" }
			, { "StatusDownloadError", "Download failed" }
			, { "CannotDownloadFile", "Cannot download file \"{0}\"" }
			, { "CannotMoveFile", "Cannot move file \"{0}\"" }
			, { "DirectoryAccessDenied", "Cannot move file \"{0}\": Access denied. Restart the Launcher as Administrator." }
			, { "UpdateSuccessful", "All files are up to date!\r\nShall we start the game now?" }
			, { "UpdateHashCheckFailed", "Files were successfully downloaded, yet the following files have an invalid checksum!\r\n{0}\r\n\r\nShall we allow the launch of the game anyway?" }
			, { "UpdateModHashCheckFailed", "The mod has been successfully installed/updated, yet the following files have an invalid checksum!\r\n{0}" }
			, { "LauncherIsInOfflineMode", "Launcher is in Offline mode" }
			, { "ChangelogFileUnavailable", "Changelog file is unavailable" }
			, { "FAQFileUnavailable", "FAQ file is unavailable" }
			, { "ModsBtn", "Mods" }
			, { "ModsTooltip", "Mods" }
			, { "NoModsForThisVersion", "There are no mods for your version of the game." }
			, { "YouHaveOutdatedMods", "New versions of these mods are available: {0}\r\n\r\nWould you like to update them?\r\n({1} are to be downloaded)" }
			, { "YouHaveOutdatedModsAndMissingFiles", "You have outdated and missing files for those mods: {0}\r\n\r\nWould you like to update them right now?\r\n({1} are to be downloaded)" }
			, { "YouHaveOutdatedModsAndMissingFilesOffline", "You have outdated and/or missing files for those mods: {0}\r\n\r\nWe cannot update them in offline mode, but be cautious using them." }
			, { "YouHaveAlteredMods", "The file structure of these mods has changed: {0}\r\n\r\nIt is highly recommended to update them. Shall we update them immediately?\r\n({1} are to be downloaded)" }
			, { "ModInstallationInProgress", "Installing..." }
			, { "ModInstallationDone", "Mods installed successfully" }
			, { "ModInstallationError", "Mod installation error" }
			, { "ModDetected", "Installed mod detected: {0}" }
			, { "InstallMod", "Install" }
			, { "UninstallMod", "Uninstall" }
			, { "EnableMod", "Enable" }
			, { "DisableMod", "Disable" }
			, { "ModNotFound", "Mod not found" }
			, { "NeedsDonationMod", "This mod needs your donations" }
			, { "DeleteModError", "An error occured while trying to delete the mod:\r\n\r\n{0}" }
			, { "DisableModError", "An error occured while trying to disable the mod:\r\n\r\n{0}" }
			, { "CannotEnableMod", "Cannot enable the mod:\r\n\r\n{0}" }
			, { "ModActivationFilesAreOutdated", "Some files of {0} are outdated or missing.\r\nGame may crash without the update\r\nDo you want to update this mod right now ({1} to be downloaded)?" }
			, { "ModDisableToPreventCorruption", "Shall we disable the mod then, to prevent game grashes and data corruption?" }
			, { "ModDetailedInfo", "Details" }
			, { "AreYouSureInstallMod", "Are you sure you want to install {0} ({1})?" }
			, { "AreYouSureUninstallMod", "Are you sure you want to uninstall {0}?" }
			, { "SomeModsDependOnThisDelete", "Are you sure you want to delete this mod? These mods depend on it:\r\n{0}" }
			, { "SomeModsDependOnThisDisable", "Are you sure you want to disable this mod? These mods depend on it:\r\n{0}" }
			, { "ModHasConflicts", "{0} Mod conflicts with the following mods:\r\n{1}" }
			, { "ModHasDependency", "{0} Mod has dependencies. Please, activate the following mod: {1}" }
			, { "ModHasDependencies", "{0} Mod has dependencies. Please, activate one of the following mods:\r\n{1}" }
			, { "ModHasDependenciesButNoneAvailable", "{0} Mod has dependencies, yet none of them are available. Please, contact the admins." }
			, { "ModConflictsHintTitle", "Mod Conflicts" }
			, { "ModConflictsHintTextSingle", "This mod conflicts with:[n]" }
			, { "ModConflictsHintTextPlural", "This mod conflicts with:[n]" }
			, { "ModDependenciesHintTitle", "Mod Dependencies" }
			, { "ModDependenciesHintTextSingle", "This mod requires the following mod to work:[n]" }
			, { "ModDependenciesHintTextPlural", "This mod requires the following mods to work:[n]" }
			, { "ModDependenciesHintOR", "OR" }
			, { "ModNotesHintTitle", "Installation Instructions" }
			, { "ModNeedsUpdateHintTitle", "Update Available" }
			, { "SameFileWithDifferentHashWarning", "Data file entries with different hashes have been detected in the configuration file for the \"{0}\". Please, report this issue to the launcher's distributor." }
			, { "SameFileWithDifferentHashBetweenModsWarning", "Different mods infos contain different hashes for file \"{0}\". Please, report this issue to the launcher's distributor." }
			, { "ScanningDataFolder", "Scanning Data folder" }
			, { "CheckingMainFiles", "Checking main files" }
			, { "CheckingModFiles", "Checking mod files" }
			, { "FollowingModsAreDisabled", "The following mods do not support the currect version and will be disabled: {0}" }
			, { "FollowingModsMayBeEnabled", "The following mods were enabled the last time you played this version. Do you want to enable them now?\r\n{0}" }
			, { "ModForOldVerDisabled", "The mod was automatically disabled due to its incompatibility with the current version of the game:\r\n{0}" }
			, { "CannotWriteCfg", "Cannot write the configuration file" }
			, { "CannotReadCfg", "Cannot read the configuration file" }
			, { "PreloaderTitle", "YobaLöncher — Loading..." }
			, { "MainFormTitle", "YobaLöncher" }
			, { "SettingsTitle", "Settings" }
			, { "SettingsGamePath", "The game installation folder" }
			, { "SettingsOpeningPanel", "The menu panel shown at the start of the launcher" }
			, { "SettingsOpeningPanelChangelog", "Changelog" }
			, { "SettingsOpeningPanelStatus", "Filecheck results" }
			, { "SettingsOpeningPanelLinks", "Links" }
			, { "SettingsOpeningPanelMods", "Mods" }
			, { "SettingsGogGalaxy", "Run the game via GOG Galaxy" }
			, { "SettingsCloseOnLaunch", "Close the Loncher when game starts" }
			, { "SettingsOfflineMode", "Offline mode" }
			, { "SettingsOfflineModeTooltip", "Offline mode" }
			, { "SettingsHiddenMods", "Show advanced mods" }
			, { "SettingsModsCompactMode", "Show mods in compact mode" }
			, { "SettingsCreateShortcut", "Put shortcut on Desktop" }
			, { "SettingsOpenDataFolder", "Open Data Folder" }
			, { "SettingsMakeBackup", "Make backup" }
			, { "SettingsMakeBackupInfo", "Create a backup copy of all the vulnerable files into {0} ?" }
			, { "SettingsMakeBackupDone", "Files are successfully backed up at {0}" }
			, { "SettingsUninstallLoncher", "Uninstall YobaLöncher" }
			, { "LoncherUninstallationConfirmation", "Are you sure you want to delete YobaLöncher? If you have many Lönchers on your PC, this instance only shall be removed." }
			, { "CannotUpdateInOfflineMode", "Launcher is currently in offline mode. We cannot update files in offline mode. Do you want to allow the game launch anyway?" }
			, { "OfflineModeSet", "Launcher is set to run in offline mode. Restart now?" }
			, { "OnlineModeSet", "Launcher is set to run in online mode. Restart now?" }
			, { "GamePathChanged", "Game path changed. Restart now?" }
			, { "GamePathSelectionTitle", "Select the game folder" }
			, { "EnterThePath", "Enter the path to the game installation folder" }
			, { "NoExeInPath", "No game executable found in the specified folder!" }
			, { "GogGalaxyDetected", "We've found GOG Galaxy on your computer. Should we run the game via Galaxy?" }
			, { "OldGameVersion", "Your current version of the game ({0}) is not supported!" }
			, { "UpdatingLoncher", "Updating Yoba Löncher..." }
			, { "UpdDownloading", "Updating Yoba Löncher - Downloading File:" }
			, { "PreparingToLaunch", "Preparing to lönch..." }
			, { "PreparingChangelog", "Checking out what's new..." }
			, { "CannotGetLocaleFile", "Cannot get or apply localization files" }
			, { "CannotGetImages", "Cannot get Images" }
			, { "CannotSetBackground", "Cannot set custom background" }
			, { "CannotGetFonts", "Cannot get Fonts" }
			, { "CannotGetAssets", "Cannot get Assets" }
			, { "CannotCheckFiles", "Cannot check files" }
			, { "MultipleFileBlocksForSingleGameVersion", "Multiple file blocks for single game version: {0}" }
			, { "CannotParseConfig", "Cannot access or parse config" }
			, { "CannotLoadIcon", "Cannot load the icon" }
			, { "CannotParseSettings", "Cannot parse the Settings file" }
			, { "CannotUpdateLoncher", "Cannot update the Löncher" }
			, { "LoncherOutOfDate1", "Launcher is out of date.\r\n\r\nAdmin eblan ne polozhil the link for autoupdate,\r\nPoetomu we will just ne dadim zapustit the Launcher." }
			, { "LoncherOutOfDate2", "New Launcher hash mathes the old one and doesn't match the required one.\r\n\r\nAdmin eblan did not update either the link or the launcher executable download link.\r\nPoetomu we will just ne dadim zapustit the Launcher, mamke admina privet." }
			, { "LoncherOutOfDate3", "Launcher versions consider each other as a newer ones.\r\n\r\nClosing the app to prevent endless update loop." }
			, { "WebClientError", "Couldn't get settings for the Löncher.\r\nProbably, you're out of Ethernet." }
			, { "WebClientErrorOffline", "Couldn't get settings for the Löncher.\r\nProbably, you're out of Ethernet.\r\n\r\nShall we try to connect again, or make an attempt to start the louncher in offline mode?" }
			, { "FileCheckNoFilePath", "No file path provided.\r\nContact the guy who set the launcher up." }
			, { "FileCheckInvalidFilePath", "The file path is absolute or invalid.\r\nAbsolute path are forbidden due to security reasons.\r\nContact the guy who set the launcher up.\r\n\r\nThe path:\r\n{0}" }
			, { "ShortcutAlreadyExists", "Loncher shortcut already exists!" }
			, { "ShortcutCreatedSuccessfully", "Loncher shortcut created successfully!" }
			, { "NewFeatureCompactMods", "New feature: Compact mods mode.\nShall we enable it now?\n(You can switch it on or off at any time through launcher settings menu)" }
		};
	}
}
