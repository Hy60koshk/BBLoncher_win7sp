using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YobaLoncher {
	public partial class MainForm {

		private LinkedListNode<ModInfo> _currentMod = null;

		private string _moveUploadedFile(string filename, FileInfo fileInfo) {
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

			fileInfo.IsHashOk = true;
			LauncherConfig.FileDates[fileInfo.Path] = YU.GetFileDateString(filename);
			LauncherConfig.FileDateHashes[fileInfo.Path] = md5;
			return null;
		}

		private async Task<bool> _finalizeModDownload(ModInfo modInfo) {
			List<FileInfo> files = modInfo.FilesForUpdate;
			int progressStep = progressBarInfo_.MaxValue / files.Count;
			bool success = false;
			string filename = "";
			try {
				List<string> failedFiles = new List<string>();
				for (int i = 0; i < files.Count; i++) {
					UpdateProgressBar(progressStep * i);
					FileInfo fi = files[i];
					if (fi.IsHashOk) {
						continue;
					}
					filename = ThePath + fi.Path.Replace('/', '\\');
					string errorStr = await Task<string>.Run(() => {
						return _moveUploadedFile(filename, fi);
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

		private async void _downloadNextMod() {
			if (currentFile_ is null) {
				if (_currentMod is null) {
					if (modsToUpdate_ == null || modsToUpdate_.Count < 1) {
						_finishModDownload();
						return;
					}
					LaunchButtonEnabled_ = false;
					UpdateLaunchButton();
					_currentMod = modsToUpdate_.First;
					downloadProgressTracker_.Reset();
				}
				else {
					_currentMod = _currentMod.Next;
				}
			}
			if (_currentMod != null) {
				if (currentFile_ is null) {
					LinkedList<FileInfo> modFileList = new LinkedList<FileInfo>(
						_currentMod.Value.FilesForUpdate.FindAll(fi => !fi.IsHashOk && fi.HasValidInfo)
					);
					if (modFileList.Count > 0) {
						currentFile_ = modFileList.First;
						DownloadFile(currentFile_.Value);
					}
					else {
						_currentMod.Value.Install();
						_currentMod.Value.IsUpdateAvailable = false;
						_downloadNextMod();
					}
				}
				else {
					currentFile_ = currentFile_.Next;
					if (currentFile_ is null) {
						UpdateProgressBar(0, Locale.Get("StatusCopyingFiles") + " // " + _currentMod.Value.VersionedName);
						if (await _finalizeModDownload(_currentMod.Value)) {
							_downloadNextMod();
						}
						else {
							UpdateProgressBar(0, Locale.Get("ModInstallationError"));
							_finishModDownload();
						}
					}
					else {
						DownloadFile(currentFile_.Value);
					}
				}
			}
			else {
				UpdateProgressBar(progressBarInfo_.MaxValue, Locale.Get("ModInstallationDone"));
				_finishModDownload();
			}
		}

		private void _finishModDownload() {
			currentFile_ = null;
			_currentMod = null;
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