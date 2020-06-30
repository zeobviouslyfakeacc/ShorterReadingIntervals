using ModSettings;
using UnityEngine;

namespace ShorterReadingIntervals {
	internal class ReadingSettings : JsonModSettings {

		[Name("Reading interval length")]
		[Description("Sets the shortest amount of time that a book can be read for.")]
		[Choice("15 minutes", "30 minutes", "60 minutes")]
		public IntervalLength intervalLength = IntervalLength.MINS_30;

		[Name("Count interrupted progress")]
		[Description("Whether progress within a reading interval should still be counted when you're interrupted.")]
		public bool allowInterruptions = true;
	}

	internal static class Settings {

		private static ReadingSettings settings;

		public static void OnLoad() {
			settings = new ReadingSettings();
			settings.AddToModSettings("Shorter Reading Intervals");
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
