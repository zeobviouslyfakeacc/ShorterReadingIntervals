using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace ShorterReadingIntervals {
	internal static class Patches {

		[HarmonyPatch(typeof(Panel_Inventory_Examine), "RefreshHoursToRead", new Type[0])]
		private static class FixHoursToRead {
			private static bool Prefix(Panel_Inventory_Examine __instance) {
				float hoursResearchRemaining = GetHoursResearchRemaining(__instance);
				float readingInterval = Settings.GetReadingIntervalHours();
				int maximumIntervals = Mathf.CeilToInt(hoursResearchRemaining / readingInterval);

				Traverse hoursTraverse = Traverse.Create(__instance).Field("m_HoursToRead");
				int intervalsToRead = hoursTraverse.GetValue<int>();
				hoursTraverse.SetValue(Mathf.Clamp(intervalsToRead, 1, maximumIntervals));

				float hoursToRead = Math.Min(intervalsToRead * readingInterval, hoursResearchRemaining);
				__instance.m_TimeToReadLabel.text = hoursToRead.ToString("0.##");
				__instance.m_ReadHoursDecrease.gameObject.SetActive(intervalsToRead > 1);
				__instance.m_ReadHoursIncrease.gameObject.SetActive(intervalsToRead < maximumIntervals);

				if (Utils.IsGamepadActive()) {
					ButtonLegend buttonLegend = __instance.m_ButtonLegendContainer.m_ButtonLegend;
					buttonLegend.ConfigureButtonIconSpriteName("Inventory_FilterLeft", ref __instance.m_GamepadReadHoursSpriteDecrease);
					buttonLegend.ConfigureButtonIconSpriteName("Inventory_FilterRight", ref __instance.m_GamepadReadHoursSpriteIncrease);
				}

				return false; // Never run the original
			}
		}

		[HarmonyPatch(typeof(Panel_Inventory_Examine), "OnReadHoursIncrease", new Type[0])]
		private static class FixOnReadHoursIncrease {
			private static bool Prefix(Panel_Inventory_Examine __instance) {
				float hoursResearchRemaining = GetHoursResearchRemaining(__instance);
				int maximumIntervals = Mathf.CeilToInt(hoursResearchRemaining / Settings.GetReadingIntervalHours());

				Traverse hoursTraverse = Traverse.Create(__instance).Field("m_HoursToRead");
				int intervalsToRead = hoursTraverse.GetValue<int>();

				if (intervalsToRead >= maximumIntervals) {
					GameAudioManager.PlayGUIError();
				} else {
					hoursTraverse.SetValue(intervalsToRead + 1);
					GameAudioManager.PlayGUIScroll();
					AccessTools.Method(typeof(Panel_Inventory_Examine), "RefreshHoursToRead").Invoke(__instance, new object[0]);
				}

				return false; // Never run the original
			}
		}

		[HarmonyPatch(typeof(Panel_Inventory_Examine), "StartRead", new Type[] { typeof(int), typeof(string) })]
		private static class ScaleStartRead {
			private static void Prefix(Panel_Inventory_Examine __instance, ref int durationMinutes) {
				float hoursResearchRemaining = GetHoursResearchRemaining(__instance);
				float minutesToRead = Math.Min(durationMinutes * Settings.GetReadingIntervalHours(), hoursResearchRemaining * 60f);
				float hoursToRead = minutesToRead / 60f;

				__instance.m_ReadTimeSeconds = 1 + 3 * Mathf.Log(1 + hoursToRead);
				durationMinutes = Mathf.CeilToInt(minutesToRead);
			}
		}

		[HarmonyPatch(typeof(Panel_Inventory_Examine), "ReadComplete", new Type[] { typeof(float) })]
		private static class ScaleReadComplete {
			private static void Prefix(Panel_Inventory_Examine __instance, ref float normalizedProgress) {
				int hoursToRead = Traverse.Create(__instance).Field("m_HoursToRead").GetValue<int>();
				float intervalsRead = normalizedProgress * hoursToRead;
				if (!Settings.GetAllowInterruptions()) {
					intervalsRead = Mathf.Floor(intervalsRead);
				}

				float hoursRead = intervalsRead * Settings.GetReadingIntervalHours();
				__instance.m_GearItem.m_ResearchItem.Read(hoursRead);

				// Do the rest of the method as if we read for 0 minutes
				normalizedProgress = 0;
			}
		}

		[HarmonyPatch(typeof(Panel_Inventory_Examine), "RefreshReadPanel", new Type[0])]
		private static class DisplayHoursReadProgressAsFraction {
			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				foreach (CodeInstruction instruction in instructions) {
					if (instruction.opcode == OpCodes.Ldstr && ((string) instruction.operand) == "F0") {
						instruction.operand = "0.##";
					}
					yield return instruction;
				}
			}
		}

		private static float GetHoursResearchRemaining(Panel_Inventory_Examine panel) {
			ResearchItem researchItem = panel.m_GearItem.m_ResearchItem;
			return researchItem.m_TimeRequirementHours - researchItem.GetElapsedHours();
		}
	}
}
