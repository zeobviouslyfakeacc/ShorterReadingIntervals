using System;
using System.IO;
using System.Reflection;
using ModSettings;
using UnityEngine;

namespace ShorterReadingIntervals {
	internal class ReadingSettings : ModSettingsBase {

		[Name("Reading interval length")]
		[Description("Sets the shortest amount of time that a book can be read for.")]
		[Choice("15 minutes", "30 minutes", "60 minutes")]
		public IntervalLength intervalLength = IntervalLength.MINS_30;

		[Name("Count interrupted progress")]
		[Description("Whether progress within a reading interval should still be counted when you're interrupted.")]
		public bool allowInterruptions = true;

		protected override void OnConfirm() {
			Settings.Save();
		}
	}

	internal static class Settings {

		private static readonly string MODS_FOLDER_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		private static readonly string SETTINGS_PATH = Path.Combine(MODS_FOLDER_PATH, "ShorterReadingIntervals.json");

		private static ReadingSettings settings;

		public static void OnLoad() {
			settings = LoadOrCreateSettings();
			settings.AddToModSettings("Shorter Reading Intervals");

			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Debug.Log("[ShorterReadingIntervals] Version " + version + " loaded!");
		}

		private static ReadingSettings LoadOrCreateSettings() {
			if (!File.Exists(SETTINGS_PATH)) {
				Debug.Log("[ShorterReadingIntervals] Settings file did not exist, using default settings.");
				return new ReadingSettings();
			}

			try {
				string json = File.ReadAllText(SETTINGS_PATH, System.Text.Encoding.UTF8);
				return JsonUtility.FromJson<ReadingSettings>(json);
			} catch (Exception ex) {
				Debug.LogError("[ShorterReadingIntervals] Error while trying to read config file:");
				Debug.LogException(ex);

				// Re-throw to make error show up in main menu
				throw new IOException("Error while trying to read config file", ex);
			}
		}

		internal static void Save() {
			try {
				string json = JsonUtility.ToJson(settings, prettyPrint: true);
				File.WriteAllText(SETTINGS_PATH, json, System.Text.Encoding.UTF8);
				Debug.Log("[ShorterReadingIntervals] Config file saved to " + SETTINGS_PATH);
			} catch (Exception ex) {
				Debug.LogError("[ShorterReadingIntervals] Error while trying to write config file:");
				Debug.LogException(ex);
			}
		}

		internal static float GetReadingIntervalHours() {
			switch (settings.intervalLength) {
				case IntervalLength.MINS_60:
					return 1f;
				case IntervalLength.MINS_30:
					return 0.5f;
				case IntervalLength.MINS_15:
					return 0.25f;
				default:
					Debug.LogError("[ShorterReadingIntervals] Unknown interval length: " + settings.intervalLength);
					return 1f;
			}
		}

		internal static bool GetAllowInterruptions() {
			return settings.allowInterruptions;
		}
	}

	internal enum IntervalLength {
		MINS_15, MINS_30, MINS_60
	}
}
