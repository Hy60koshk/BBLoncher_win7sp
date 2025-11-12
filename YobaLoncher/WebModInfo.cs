using System.Collections.Generic;
using Newtonsoft.Json;

namespace YobaLoncher {

    partial class MainForm {

		internal class WebModVersion {
			public string Id;
			public string Name;
			public string Description;
			public List<string> Conflicts = new List<string>();
			public List<string[]> Dependencies = new List<string[]>();
			public List<string> ActiveConflicts = new List<string>();
			public List<bool> DepsFulfilled = new List<bool>();

			public WebModVersion(ModInfo mi, ModVersion mv) {
				Id = mv.VersionName;
				Name = mi.Name + ' ' + mv.VersionName;
				Description = mv.Description ?? "";

				List<ModInfo> availableMods = Program.LoncherSettings.AvailableMods;

				List<ModDep> conflicts = null;
				if (mv.Conflicts != null) {
					if (!mv.Conflicts[0].ModId.Equals("-")) {
						conflicts = mv.Conflicts;
					}
				}
				else if (mi.Conflicts != null) {
					conflicts = mi.Conflicts;
				}
				if (conflicts != null) {
					foreach (ModDep conf in conflicts) {
						ModInfo cmi = availableMods.Find(m => m.Id.Equals(conf.ModId));
						if (cmi != null) {
							if (conf.VerId is null) {
								Conflicts.Add(cmi.VersionedName);
								if (cmi.IsActive) {
									ActiveConflicts.Add(cmi.VersionedName);
								}
							}
							else {
								ModVersion cmv = cmi.Versions.Find(
									v => v.VersionName.Equals(conf.VerId, System.StringComparison.OrdinalIgnoreCase)
								);
								if (cmv != null) {
									Conflicts.Add(cmi.Name + ' ' + cmv.VersionName);
									if (cmi.IsActive && cmv == cmi.CurrentVersion) {
										ActiveConflicts.Add(cmi.VersionedName);
									}
								}
							}
						}
					}
				}

				List<ModDep[]> dependencies = null;
				if (mv.Dependencies != null) {
					if (!mv.Dependencies[0][0].ModId.Equals("-")) {
						dependencies = mv.Dependencies;
					}
				}
				else if (mi.Dependencies != null) {
					dependencies = mi.Dependencies;
				}
				if (dependencies != null) {
					foreach (ModDep[] depVariants in dependencies) {
						List<string> depsTitles = new List<string>();
						bool isFulfilled = false;
						foreach (ModDep variant in depVariants) {
							ModInfo cmi = availableMods.Find(m => m.Id.Equals(variant.ModId));
							if (cmi != null) {
								if (variant.VerId is null) {
									depsTitles.Add(cmi.VersionedName);
									if (cmi.IsActive) {
										isFulfilled = true;
									}
								}
								else {
									ModVersion cmv = cmi.Versions.Find(
										v => v.VersionName.Equals(variant.VerId, System.StringComparison.OrdinalIgnoreCase)
									);
									if (cmv != null) {
										depsTitles.Add(cmi.Name + ' ' + cmv.VersionName);
										if (cmi.IsActive && cmi.CurrentVersion == cmv) {
											isFulfilled = true;
										}
									}
								}
							}
						}
						Dependencies.Add(depsTitles.ToArray());
						DepsFulfilled.Add(isFulfilled);
					}
				}
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
				NeedsDonation = mi.LatestVersion.NeedsDonation;
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
					if (mv.HasVersion()) {
						Versions.Add(new WebModVersion(mi, mv));
					}
				}
			}
		}
    }
}
