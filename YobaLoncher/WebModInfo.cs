using System.Collections.Generic;
using Newtonsoft.Json;

namespace YobaLoncher {

    partial class MainForm {

		internal class WebModVersion {
			public string Id;
			public string Name;
			public string Description;

			public WebModVersion(ModInfo mi, ModVersion mv, GameVersion gv) {
				Id = mv.VersionName;
				Name = gv.Name ?? (mi.Name + ' ' + mv.VersionName);
				Description = gv.Description ?? mv.Description ?? "";
			}
		}
        internal class WebModInfo {
			public bool DlInProgress = false;
			public bool Installed = false;
			public bool Active = false;
			public bool IsHidden = false;
			public bool NeedsDonation = false;
			public string Id;
			public string Name;
			public ModGroup Group;
			public string Description;
			public string DetailedDescription;
			public string Screenshots;
			public List<WebModVersion> Versions;
			public List<string> Conflicts = new List<string>();
			public List<string[]> Dependencies = new List<string[]>();
			[JsonIgnore]
			public ModInfo ModInfo;

			public WebModInfo(ModInfo mi) {
				ModInfo = mi;
				Name = mi.VersionedName;
				Group = mi.Group;
				//GroupId = mi.GroupId;
				Id = mi.Id;
				IsHidden = mi.IsHidden;
				NeedsDonation = mi.GVForLatestVersion.NeedsDonation;
				Description = mi.VersionedDescription;
				DetailedDescription = mi.VersionedDetailedDescription ?? mi.DetailedDescription ?? "";
				Screenshots = (mi.Screenshots == null) ? "" : JsonConvert.SerializeObject(mi.Screenshots);

				DlInProgress = mi.DlInProgress;
				if (mi.ModConfigurationInfo != null) {
					Installed = true;
					Active = mi.ModConfigurationInfo.Active;
				}

				Versions = new List<WebModVersion>();
				foreach (ModVersion mv in mi.Versions) {
					GameVersion gv = mv.GetGameVersion();
					if (gv != null && (gv.Files.Count + gv.FileGroups.Count) > 0) {
						Versions.Add(new WebModVersion(mi, mv, gv));
					}
				}

				if (mi.ConflictList.Count > 0) {
					foreach (ModInfo cmi in mi.ConflictList) {
						if (cmi.IsActive) {
							Conflicts.Add(cmi.VersionedName);
						}
					}
				}
				if (mi.DependencyList.Count > 0) {
					foreach (List<ModInfo> deps in mi.DependencyList) {
						string[] modDeps = new string[deps.Count];
						bool addDeps = true;
						for (int i = 0; i < deps.Count; i++) {
							if (deps[i].IsActive) {
								addDeps = false;
								break;
							}
							else {
								modDeps[i] = deps[i].VersionedName;
							}
						}
						if (addDeps) {
							Dependencies.Add(modDeps);
						}
					}
				}
			}
		}
    }
}
