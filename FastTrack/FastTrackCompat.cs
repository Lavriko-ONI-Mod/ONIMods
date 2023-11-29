﻿/*
 * Copyright 2023 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;

namespace PeterHan.FastTrack {
	/// <summary>
	/// Deals with compatibility with known "problem" mods.
	/// </summary>
	internal static class FastTrackCompat {
		/// <summary>
		/// There are two achievement enabler mods on the workshop, allow achievements even
		/// in sandbox if either of these types are defined.
		/// </summary>
		private const string ACHIEVEMENT_ENABLER_TYPE_1 = "AchievementEnablerLib.AchievementEnablerMod";
		private const string ACHIEVEMENT_ENABLER_TYPE_2 = "EnableAchievements.Patches";

		/// <summary>
		/// Keep diseases enabled (Diseases Expanded) if this type is defined.
		/// </summary>
		private const string DISEASES_EXPANDED_TYPE = "DiseasesExpanded.ModInfo";

		/// <summary>
		/// Keep diseases enabled (Diseases Restored) if this type is defined.
		/// </summary>
		private const string DISEASES_RESTORED_TYPE = "DiseasesReimagined.DiseasesPatch";

		/// <summary>
		/// Reduce fetch manager patches if this type is defined.
		/// </summary>
		private const string EFFICIENT_SUPPLY_TYPE = "PeterHan.EfficientFetch.EfficientFetchManager";

		/// <summary>
		/// Turn off electrical patches if either this type is defined, or the registry
		/// entry is set.
		/// </summary>
		private const string GW_WIRE_TYPE = "Egladil.WirePatchs";

		/// <summary>
		/// Enable extended temperature precision if this type is defined.
		/// </summary>
		private const string HPT_TYPE = "Main.Temperature_Patch";

		/// <summary>
		/// Avoid totally replacing material properties screens if this type is defined.
		/// </summary>
		private const string MATERIAL_PROPERTIES_TYPE = "MaterialSelectionProperties.MaterialSelectionProperties";

		/// <summary>
		/// Let Rocketry Expanded take over side screens if this type is defined.
		/// </summary>
		private const string ROCKETRY_EXPANDED_TYPE = "Rockets_TinyYetBig.Mod";

		/// <summary>
		/// Turn off most of the attributes and stats panel patches if this type is defined.
		/// </summary>
		private const string STATS_UNLIMITED_TYPE = "OniStatsPlusSo.MyMod";

		/// <summary>
		/// Turn off tile mesh renderers if this mod is active.
		/// </summary>
		private const string TRUE_TILES_ID = "TrueTiles";

		/// <summary>
		/// Checks for compatibility and allows achievements in sandbox and debug (even with
		/// Disable Achievements set to In Sandbox/Debug) if Achievement Enabler or similar
		/// mods are enabled.
		/// </summary>
		internal static void CheckAchievementCompat() {
			if (PPatchTools.GetTypeSafe(ACHIEVEMENT_ENABLER_TYPE_1) != null || PPatchTools.
					GetTypeSafe(ACHIEVEMENT_ENABLER_TYPE_2) != null) {
				PUtil.LogWarning("Achievement Enabler is active, achievements allowed in sandbox");
				GamePatches.AchievementDisablePatches.forceEnableAchievements = true;
			}
		}

		/// <summary>
		/// Checks for compatibility and turns off electrical network patches if conflicting
		/// mods (either set the registry key or define a conflicting type) are active.
		///
		/// The registry key is in case the grid gets rewritten by a future mod idea.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		internal static void CheckENetCompat(Harmony harmony) {
			if (PPatchTools.GetTypeSafe(GW_WIRE_TYPE) != null || PRegistry.GetData<bool>(
					"OverrideElectricalNetwork"))
				PUtil.LogWarning("Disabling electrical network patches: Conflicting mod active");
			else
				GamePatches.FastElectricalNetworkCalculator.Apply(harmony);
		}

		/// <summary>
		/// Checks for compatibility and applies fast fetch manager updates only if Efficient
		/// Supply is not enabled.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		internal static void CheckFetchCompat(Harmony harmony) {
			if (PPatchTools.GetTypeSafe(EFFICIENT_SUPPLY_TYPE) == null) {
				PathPatches.AsyncBrainGroupUpdater.allowFastListSwap = true;
				harmony.Patch(typeof(FetchManager.FetchablesByPrefabId),
					nameof(FetchManager.FetchablesByPrefabId.UpdatePickups),
					prefix: new HarmonyMethod(typeof(GamePatches.FetchManagerFastUpdate),
					nameof(GamePatches.FetchManagerFastUpdate.BeforeUpdatePickups)));
#if DEBUG
				PUtil.LogDebug("Patched FetchManager for fast pickup updates");
#endif
			} else {
				PUtil.LogWarning("Disabling fast pickup updates: Efficient Supply active");
				PathPatches.AsyncBrainGroupUpdater.allowFastListSwap = false;
			}
		}

		/// <summary>
		/// Checks for compatibility and applies material property optimizations only if mods
		/// which add more descriptors are not enabled.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		internal static void CheckMaterialPropertiesCompat(Harmony harmony) {
			if (PPatchTools.GetTypeSafe(MATERIAL_PROPERTIES_TYPE) == null)
				UIPatches.DescriptorAllocPatches.ApplyMaterialPatches(harmony);
			else
				PUtil.LogDebug("Disabling material properties optimizations: Properties mod active");
		}

		/// <summary>
		/// Checks for compatibility and applies no disease only if Diseases Restored or
		/// Diseases Expanded is not enabled.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		internal static void CheckNoDiseaseCompat(Harmony harmony) {
			if (PPatchTools.GetTypeSafe(DISEASES_RESTORED_TYPE) != null)
				PUtil.LogWarning("Enabling diseases: Diseases Restored active");
			else if (PPatchTools.GetTypeSafe(DISEASES_EXPANDED_TYPE) != null)
				PUtil.LogWarning("Enabling diseases: Diseases Expanded active");
			else {
				try {
					GamePatches.NoDiseasePatches.Apply(harmony);
#if DEBUG
					PUtil.LogDebug("Disabled diseases");
#endif
				} catch (Exception e) {
					PUtil.LogWarning("Error when disabling diseases, diseases may not be completely removed:");
					PUtil.LogExcWarn(e);
				}
			}
		}

		/// <summary>
		/// Checks for compatibility and applies drillcone optimizations only if a rocket
		/// mod is not enabled.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		internal static void CheckRocketCompat(Harmony harmony) {
			bool rtb = PPatchTools.GetTypeSafe(ROCKETRY_EXPANDED_TYPE) != null;
			if (rtb)
				PUtil.LogWarning("Disabling drillcone optimizations: Rockets mod active");
			else
				UIPatches.HarvestSideScreenWrapper.Apply(harmony);
			UIPatches.SimpleInfoScreenWrapper.AllowBaseRocketPanel = rtb;
		}

		/// <summary>
		/// Checks for compatibility and applies stats panel optimizations only if mods which
		/// show or alter attributes are not enabled.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		internal static void CheckStatsCompat(Harmony harmony) {
			if (PPatchTools.GetTypeSafe(STATS_UNLIMITED_TYPE) == null)
				UIPatches.MinionStatsPanelWrapper.Apply(harmony);
			else
				PUtil.LogDebug("Disabling attributes panel optimizations: Stats mod active");
		}

		/// <summary>
		/// Checks for compatibility and enables extended precision temperature only if a mod
		/// which requests it is enabled.
		/// </summary>
		internal static void CheckTemperatureCompat() {
			bool hpt = PPatchTools.GetTypeSafe(HPT_TYPE) != null;
			if (hpt)
				PUtil.LogDebug("Extending temperature precision: High Precision Temperature active");
			UIPatches.FormatStringPatches.ForceHighPrecisionTemperature = hpt;
		}

		/// <summary>
		/// Checks for compatibility and applies tile mesh renderer patches only if other tile
		/// mods are not enabled.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		/// <param name="mods">The mods list after all load.</param>
		internal static void CheckTileCompat(Harmony harmony, IReadOnlyList<Mod> mods) {
			int n = mods.Count;
			bool found = false;
			// No Contains in read only lists...
			for (int i = 0; i < n && !found; i++) {
				var m = mods[i];
				if (m.staticID == TRUE_TILES_ID && m.IsEnabledForActiveDlc()) {
					PUtil.LogWarning("Disabling tile mesh renderers: True Tiles active");
					found = true;
				}
			}
			if (!found) {
				VisualPatches.TileMeshPatches.Apply(harmony);
#if DEBUG
				PUtil.LogDebug("Patched BlockTileRenderer for mesh renderers");
#endif
			}
		}

		/// <summary>
		/// Checks for compatibility and applies Trapped diagnostic patches only if a mod which
		/// fixes them is not active.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		internal static void CheckTrappedCompat(Harmony harmony) {
			if (!PRegistry.GetData<bool>("Bugs.TrappedDiagnostic"))
				UIPatches.TrappedDuplicantPatch.Apply(harmony);
			else
				PUtil.LogDebug("Disabling Trapped optimizations");
		}
	}
}
