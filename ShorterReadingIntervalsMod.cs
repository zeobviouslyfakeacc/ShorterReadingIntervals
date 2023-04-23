using MelonLoader;
using UnityEngine;

namespace ShorterReadingIntervals {
	internal class ShorterReadingIntervalsMod : MelonMod {

		public override void OnInitializeMelon() {
			Settings.OnLoad();
			Debug.Log($"[{Info.Name}] version {Info.Version} loaded!");
            new MelonLoader.MelonLogger.Instance($"{Info.Name}").Msg($"Version {Info.Version} loaded");
        }
	}
}
