using System.Collections.Generic;
using System.Linq;
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

				List<ModDep> conflicts = mv.GetConflicts();
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
								ModVersion cmv = cmi.FindVersionForDep(conf);
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

				List<ModDep[]> dependencies = mv.GetDependencies();
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
									ModVersion cmv = cmi.FindVersionForDep(variant);
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
			public string InstalledVersionID;
			public List<WebModVersion> Versions;
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
					InstalledVersionID = mi.CurrentVersion.VersionName;
				}

				Versions = mi.AvailableVersions.Select(mv => new WebModVersion(mi, mv)).ToList();
			}
		}
    }
}
