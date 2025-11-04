using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YobaLoncher {
	public partial class MainForm {

		private string MoveUploadedFile(string filename, FileInfo fileInfo) {
			string dirpath = filename.Substring(0, filename.LastIndexOf('\\'));
			Directory.CreateDirectory(dirpath);
			if (File.Exists(filename)) {
				File.Delete(filename);
			}
			File.Move(PreloaderForm.UPDPATH + fileInfo.UploadAlias, filename);

			fileInfo.IsPresent = true;
			string md5 = FileChecker.GetFileMD5(filename);
			if (!fileInfo.IsHashAcceptable(md5)) {
				return fileInfo.Path + " : " + md5;
			}

			fileInfo.IsOK = true;
			LauncherConfig.FileDates[fileInfo.Path] = YU.GetFileDateString(filename);
			LauncherConfig.FileDateHashes[fileInfo.Path] = md5;
			return null;
		}

		private async Task<bool> FinalizeModDownload(ModInfo modInfo) {
			List<FileInfo> files = modInfo.FilesForLatestVersion;
			int progressStep = progressBarInfo_.MaxValue / files.Count;
			bool success = false;
			string filename = "";
			try {
				List<string> failedFiles = new List<string>();
				for (int i = 0; i < files.Count; i++) {
					UpdateProgressBar(progressStep * i);
					FileInfo fi = files[i];
					if (fi.IsOK) {
						continue;
					}
					filename = ThePath + fi.Path.Replace('/', '\\');
					string errorStr = await Task<string>.Run(() => {
						return MoveUploadedFile(filename, fi);
					});
					if (errorStr != null) {
						failedFiles.Add(errorStr);
					}
				}
				modInfo.Install();
				success = true;
				if (failedFiles.Count > 0) {
					YobaDialog.ShowDialog(String.Format(Locale.Get("UpdateModHashCheckFailed"), String.Join("\r\n", failedFiles)));
				}
			}
			catch (UnauthorizedAccessException ex) {
				ShowDownloadError(string.Format(Locale.Get("DirectoryAccessDenied"), filename) + ":\r\n" + ex.Message);
			}
			catch (Exception ex) {
				ShowDownloadError(string.Format(Locale.Get("CannotMoveFile"), filename) + ":\r\n" + ex.Message);
			}
			modInfo.DlInProgress = false;
			UpdateModsWebView();
			return success;
		}

		private LinkedListNode<ModInfo> currentMod_ = null;

		private async void DownloadNextMod() {
			if (currentFile_ is null) {
				if (currentMod_ is null) {
					if (modsToUpdate_ == null || modsToUpdate_.Count < 1) {
						FinishModDownload();
						return;
					}
					LaunchButtonEnabled_ = false;
					UpdateLaunchButton();
					currentMod_ = modsToUpdate_.First;
					downloadProgressTracker_.Reset();
				}
				else {
					currentMod_ = currentMod_.Next;
				}
			}
			if (currentMod_ != null) {
				if (currentFile_ is null) {
					var modFileList = new LinkedList<FileInfo>(currentMod_.Value.FilesForLatestVersion.FindAll(fi => !fi.IsOK && fi.HasValidInfo));
					if (modFileList.Count > 0) {
						currentFile_ = modFileList.First;
						DownloadFile(currentFile_.Value);
					}
					else {
						currentMod_.Value.Install();
						currentMod_.Value.IsUpdateAvailable = false;
						DownloadNextMod();
					}
				}
				else {
					currentFile_ = currentFile_.Next;
					if (currentFile_ is null) {
						UpdateProgressBar(0, Locale.Get("StatusCopyingFiles") + " // " + currentMod_.Value.VersionedName);
						if (await FinalizeModDownload(currentMod_.Value)) {
							DownloadNextMod();
						}
						else {
							UpdateProgressBar(0, Locale.Get("ModInstallationError"));
							FinishModDownload();
						}
					}
					else {
						DownloadFile(currentFile_.Value);
					}
				}
			}
			else {
				UpdateProgressBar(progressBarInfo_.MaxValue, Locale.Get("ModInstallationDone"));
				FinishModDownload();
			}
		}

		private void FinishModDownload() {
			currentFile_ = null;
			currentMod_ = null;
			modsToUpdate_ = null;
			foreach (ModInfo mi in Program.LoncherSettings.Mods) {
				mi.DlInProgress = false;
			}
			LaunchButtonEnabled_ = true;
			UpdateModsWebView();
			CheckReady();
		}
	}
}