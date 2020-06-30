using MelonLoader;
using UnityEngine;

namespace ShorterReadingIntervals {
	internal class ShorterReadingIntervalsMod : MelonMod {
		public override void OnApplicationStart() {
			Settings.OnLoad();
			Debug.Log($"[{InfoAttribute.Name}] version {InfoAttribute.Version} loaded!");
		}
	}
}
