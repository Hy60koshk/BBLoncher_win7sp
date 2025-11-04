using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YobaLoncher {
	public class CheckResult {
		public bool IsAllOk = true;
		public LinkedList<FileInfo> InvalidFiles = new LinkedList<FileInfo>();
	}
	class FileCheckedEventArgs : EventArgs {
		public FileInfo File {
			get;
		}
		public FileCheckedEventArgs(FileInfo file) {
			File = file;
		}
	}
	class FileChecker {
		private static MD5 md5_;
		private static Regex forbiddenChars = new Regex("(\\\\)|(//)|([\\*\\?:<>\"])");

		public static MD5 MD5 {
			get => md5_ == null ? (md5_ = MD5.Create()) : md5_;
		}

		public static async Task<CheckResult> CheckFiles(List<FileInfo> files) {
			return await CheckFiles(Program.GamePath, files, null);
		}
		public static async Task<CheckResult> CheckFiles(string root, List<FileInfo> files) {
			return await CheckFiles(root, files, null);
		}
		public static async Task<CheckResult> CheckFiles(List<FileInfo> files, EventHandler<FileCheckedEventArgs> checkEventHandler) {
			return await CheckFiles(Program.GamePath, files, checkEventHandler);
		}
		public static async Task<CheckResult> CheckFiles(string root, List<FileInfo> files, EventHandler<FileCheckedEventArgs> checkEventHandler) {
			CheckResult result = new CheckResult();
			Dictionary<string, string> fileDates = LauncherConfig.FileDates;
			Dictionary<string, string> fileDateHashes = LauncherConfig.FileDateHashes;
			foreach (FileInfo file in files) {
				string fullPath = root + file.Path;
				file.IsPresent = false;
				file.IsOK = false;
				if (fileDates.ContainsKey(file.Path)) {
					if (File.Exists(fullPath)) {
						file.IsPresent = true;
						if (file.Hashes == null || file.Hashes.Count == 0) {
							file.IsOK = true;
						}
						else {
							string filedate = YU.GetFileDateString(fullPath);
							if (fileDates[file.Path].Equals(filedate) && file.IsHashAcceptable(fileDateHashes[file.Path])) {
								file.IsOK = true;
							}
							else {
								await CheckExistingFileOnline(root, file, result);
							}
						}
					}
					else {
						fileDates.Remove(file.Path);
						fileDateHashes.Remove(file.Path);
						result.InvalidFiles.AddLast(file);
						result.IsAllOk = false;
						await UpdatefileSize(file);
					}
				}
				else {
					await CheckExistingFileOnline(root, file, result);
				}
				checkEventHandler?.Invoke(null, new FileCheckedEventArgs(file));
			}
			return result;
		}

		private static async Task CheckExistingFileOnline(string root, FileInfo file, CheckResult result) {
			if (YU.stringHasText(file.Url)) {
				string md5;
				file.IsOK = CheckFileMD5(root, file, out md5);
				if (file.IsOK) {
					string filedate = YU.GetFileDateString(root, file.Path);
					LauncherConfig.FileDates[file.Path] = filedate;
					LauncherConfig.FileDateHashes[file.Path] = md5;
				}
				else {
					result.InvalidFiles.AddLast(file);
					result.IsAllOk = false;
					if (!Program.OfflineMode) {
						await UpdatefileSize(file);
					}
				}
			}
		}

		private static async Task UpdatefileSize(FileInfo file) {
			if (file.Size < 1) {
				WebRequest webRequest = WebRequest.Create(file.Url);
				webRequest.Method = "HEAD";

				using (WebResponse webResponse = await webRequest.GetResponseAsync()) {
					string fileSize = webResponse.Headers.Get("Content-Length");
					file.Size = Convert.ToUInt32(fileSize);
				}
			}
		}

		public static string GetFileMD5(string path) {
			byte[] hash;
			using (FileStream stream = File.OpenRead(path)) {
				hash = MD5.ComputeHash(stream);
			}
			StringBuilder hashSB = new StringBuilder(hash.Length);
			for (int i = 0; i < hash.Length; i++) {
				hashSB.Append(hash[i].ToString("X2"));
			}
			return hashSB.ToString();
		}
		public static bool CheckFileMD5(FileInfo file) {
			return CheckFileMD5(Program.GamePath, file);
		}
		public static bool CheckFileMD5(string root, FileInfo file) {
			if (file.Path == null || file.Path.Length == 0) {
				throw new Exception(Locale.Get("FileCheckNoFilePath"));
			}
			if (file.Path.Contains(':') || file.Path.Contains('?') || file.Path.Contains('*') || file.Path.Contains("\\\\") || file.Path.Contains("//")) {
				throw new Exception(string.Format(Locale.Get("FileCheckNoFilePath"), file.Path));
			}
			string filepath = root + file.Path;
			if (File.Exists(filepath) && (new System.IO.FileInfo(filepath).Length > 0)) {
				file.IsPresent = true;
				if (file.Hashes == null || file.Hashes.Count == 0) {
					return true;
				}
				string strHash = GetFileMD5(filepath);
				return file.IsHashAcceptable(strHash);
			}
			return false;
		}
		public static bool CheckFileMD5(FileInfo file, out string md5) {
			return CheckFileMD5(Program.GamePath, file, out md5);
		}
		public static bool CheckFileMD5(string root, FileInfo file, out string md5) {
			if (file.Path == null || file.Path.Length == 0) {
				throw new Exception(Locale.Get("FileCheckNoFilePath"));
			}
			if (forbiddenChars.IsMatch(file.Path)) {
				throw new Exception(string.Format(Locale.Get("FileCheckNoFilePath"), file.Path));
			}
			string filepath = root + file.Path;
			if (File.Exists(filepath) && (new System.IO.FileInfo(filepath).Length > 0)) {
				file.IsPresent = true;
				md5 = GetFileMD5(filepath);
				if (file.Hashes == null || file.Hashes.Count == 0) {
					return true;
				}
				return file.IsHashAcceptable(md5);
			}
			md5 = null;
			return false;
		}

		public static bool CheckFileMD5(string path, string correctHash) {
			if (File.Exists(path)) {
				if (correctHash is null || correctHash.Length == 0) {
					return true;
				}
				string strHash = GetFileMD5(path);
				if (correctHash.ToUpper().Equals(strHash)) {
					return true;
				}
			}
			return false;
		}
	}
}
