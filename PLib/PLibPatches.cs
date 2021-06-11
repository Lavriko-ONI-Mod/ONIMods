﻿/*
 * Copyright 2021 Peter Han
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

using Harmony;
using KSerialization;
using PeterHan.PLib.Buildings;
using PeterHan.PLib.Datafiles;
using PeterHan.PLib.Detours;
using PeterHan.PLib.Lighting;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using IEnumerable = System.Collections.IEnumerable;
using IntHandle = HandleVector<int>.Handle;
using LightGridEmitter = LightGridManager.LightGridEmitter;

namespace PeterHan.PLib {
	/// <summary>
	/// All patches for PLib are stored here and only applied once for all PLib mods loaded.
	/// </summary>
	sealed class PLibPatches {
#region Patches
		// Method only exists in vanilla
		private static readonly MethodBase ACHIEVEMENT_SERIALIZE = typeof(Database.
			ColonyAchievementRequirement).GetMethodSafe("Serialize", false, typeof(
			BinaryWriter));

		private static readonly IDetouredField<LightShapePreview, int> PREVIOUS_CELL =
			PDetours.DetourFieldLazy<LightShapePreview, int>("previousCell");

		/// <summary>
		/// Applied to LightGridEmitter to unattribute lighting sources.
		/// </summary>
		private static void AddToGrid_Postfix() {
			var lm = PLightManager.Instance;
			if (lm != null)
				lm.CallingObject = null;
		}

		/// <summary>
		/// Applied to Light2D to properly attribute lighting sources.
		/// </summary>
		private static bool AddToScenePartitioner_Prefix(Light2D __instance,
				ref IntHandle ___solidPartitionerEntry,
				ref IntHandle ___liquidPartitionerEntry) {
			var lm = PLightManager.Instance;
			bool cont = true;
			if (lm != null) {
				// Replace the whole method since the radius could use different algorithms
				lm.CallingObject = __instance.gameObject;
				cont = !PLightManager.AddScenePartitioner(__instance,
					ref ___solidPartitionerEntry, ref ___liquidPartitionerEntry);
			}
			return cont;
		}

		/// <summary>
		/// Applied to ModsScreen if mod options are registered, after BuildDisplay runs.
		/// </summary>
		private static void BuildDisplay_Postfix(IEnumerable ___displayedMods,
				GameObject ___entryPrefab) {
			POptions.BuildDisplay(___displayedMods, ___entryPrefab);
		}

		/// <summary>
		/// Applied to KeyDef (constructor) to adjust array lengths if necessary.
		/// </summary>
		private static void CKeyDef_Postfix(KInputController.KeyDef __instance) {
			__instance.mActionFlags = PActionManager.ExtendFlags(__instance.mActionFlags,
				PActionManager.Instance.GetMaxAction());
		}

		/// <summary>
		/// Applied to CodexCache to collect dynamic codex entries from the file system.
		/// </summary>
		private static void CollectEntries_Postfix(string folder, List<CodexEntry> __result,
				string ___baseEntryPath) {
			// Check to see if we are loading from either the "Creatures" directory or
			// "Plants" directory
			string path = string.IsNullOrEmpty(folder) ? ___baseEntryPath : Path.Combine(
				___baseEntryPath, folder);
			bool modified = false;
			if (path.EndsWith("Creatures")) {
				__result.AddRange(PCodex.LoadCreaturesEntries());
				modified = true;
			}
			if (path.EndsWith("Plants")) {
				__result.AddRange(PCodex.LoadPlantsEntries());
				modified = true;
			}
			if (modified) {
				foreach (var codexEntry in __result)
					// Fill in a default sort string if necessary
					if (string.IsNullOrEmpty(codexEntry.sortString))
						codexEntry.sortString = Strings.Get(codexEntry.title);
				__result.Sort((x, y) => x.sortString.CompareTo(y.sortString));
			}
		}

		/// <summary>
		/// Applied to CodexCache to collect dynamic codex sub entries from the file system.
		/// </summary>
		private static void CollectSubEntries_Postfix(List<SubEntry> __result) {
			int startSize = __result.Count;
			__result.AddRange(PCodex.LoadCreaturesSubEntries());
			__result.AddRange(PCodex.LoadPlantsSubEntries());
			if (__result.Count != startSize)
				__result.Sort((x, y) => x.title.CompareTo(y.title));
		}

		/// <summary>
		/// Applied to LightGridEmitter to compute the lux values properly.
		/// </summary>
		private static bool ComputeLux_Prefix(LightGridEmitter __instance, int cell,
				LightGridEmitter.State ___state, ref int __result) {
			var lm = PLightManager.Instance;
			return lm == null || !lm.GetBrightness(__instance, cell, ___state, out __result);
		}

		/// <summary>
		/// Applied to LightGridManager to properly preview a new light source.
		/// </summary>
		private static bool CreatePreview_Prefix(int origin_cell, float radius,
				LightShape shape, int lux) {
			var lm = PLightManager.Instance;
			return lm == null || !lm.PreviewLight(origin_cell, radius, shape, lux);
		}

		/// <summary>
		/// Applied to BuildingTemplates to properly debug missing building anims.
		/// </summary>
		private static void CreateBuildingDef_Postfix(BuildingDef __result, string anim,
				string id) {
			var animFiles = __result?.AnimFiles;
			if (animFiles != null && animFiles.Length > 0 && animFiles[0] == null)
				Debug.LogWarningFormat("(when looking for KAnim named {0} on building {1})",
					anim, id);
		}

		/// <summary>
		/// Applied to EquipmentTemplates to properly debug missing item anims.
		/// </summary>
		private static void CreateEquipmentDef_Postfix(EquipmentDef __result, string Anim,
				string Id) {
			var anim = __result?.Anim;
			if (anim == null)
				Debug.LogWarningFormat("(when looking for KAnim named {0} on equipment {1})",
					Anim, Id);
		}

		/// <summary>
		/// Applied to Game to run postload handlers.
		/// </summary>
		private static void Game_DestroyInstances_Postfix() {
			PPatchManager.RunAll(RunAt.OnEndGame);
		}

		/// <summary>
		/// Applied to Game to run postload handlers.
		/// </summary>
		private static void Game_OnPrefabInit_Postfix() {
			PPatchManager.RunAll(RunAt.OnStartGame);
		}

		/// <summary>
		/// Applied to GetKeycodeLocalized to quash warning spam on key codes that are valid
		/// but not handled by default.
		/// </summary>
		private static bool GetKeycodeLocalized_Prefix(KKeyCode key_code, ref string __result)
		{
			string newResult = PActionManager.GetExtraKeycodeLocalized(key_code);
			if (newResult != null)
				__result = newResult;
			return newResult == null;
		}

		/// <summary>
		/// Applied to DiscreteShadowCaster to handle lighting requests.
		/// </summary>
		private static bool GetVisibleCells_Prefix(int cell, List<int> visiblePoints,
				int range, LightShape shape) {
			bool exec = true;
			var lm = PLightManager.Instance;
			if (shape != LightShape.Circle && shape != LightShape.Cone && lm != null)
				// This is not a customer scenario
				exec = !lm.GetVisibleCells(cell, visiblePoints, range, shape);
			return exec;
		}

		/// <summary>
		/// Applied to Db to register PLib buildings and run postload handlers.
		/// </summary>
		private static void Initialize_Prefix(ref bool __state) {
			bool needBuilds = PBuilding.CheckBuildings();
			if (needBuilds && !PBuilding.RequiresDBInit)
				PBuilding.AddAllTechs();
			__state = needBuilds;
			PPatchManager.RunAll(RunAt.BeforeDbInit);
		}

		/// <summary>
		/// Applied to Db to run postload handlers.
		/// </summary>
		private static void Initialize_Postfix(bool __state) {
			if (__state && PBuilding.RequiresDBInit)
				PBuilding.AddAllTechs();
			PPatchManager.RunAll(RunAt.AfterDbInit);
		}

		/// <summary>
		/// Applied to KInputController to adjust array lengths if necessary.
		/// </summary>
		private static void IsActive_Prefix(ref bool[] ___mActionState) {
			___mActionState = PActionManager.ExtendFlags(___mActionState, PActionManager.
				Instance.GetMaxAction());
		}

		/// <summary>
		/// Applied to LightBuffer to allow custom ray shapes to be used.
		/// </summary>
		private static IEnumerable<CodeInstruction> LightBuffer_LateUpdate_Transpile(
				IEnumerable<CodeInstruction> body) {
			var target = typeof(Light2D).GetPropertySafe<LightShape>(nameof(Light2D.shape),
				false)?.GetGetMethod();
			return (target == null) ? body : PPatchTools.ReplaceMethodCall(body, target,
				typeof(PLightManager).GetMethodSafe(nameof(PLightManager.LightShapeToRayShape),
				true, typeof(Light2D)));
		}

		/// <summary>
		/// Applied to LightShapePreview to properly attribute lighting sources.
		/// </summary>
		private static void LightShapePreview_Update_Prefix(LightShapePreview __instance) {
			var lm = PLightManager.Instance;
			if (lm != null)
				lm.CallingObject = __instance.gameObject;
		}

		/// <summary>
		/// Applied to GeneratedBuildings to register PLib buildings.
		/// </summary>
		private static void LoadGeneratedBuildings_Prefix() {
			PBuilding.AddAllStrings();
		}

		/// <summary>
		/// Applied to SteamUGCService to silence "Preview image load failed".
		/// </summary>
		private static IEnumerable<CodeInstruction> LoadPreviewImage_Transpile(
				IEnumerable<CodeInstruction> body) {
			var target = typeof(Debug).GetMethodSafe(nameof(Debug.LogFormat), true,
				typeof(string), typeof(object[]));
			return (target == null) ? body : PPatchTools.ReplaceMethodCall(body, target);
		}

		/// <summary>
		/// Applied to MainMenu to run postload handlers.
		/// </summary>
		private static void MainMenu_OnSpawn_Postfix() {
			PPatchManager.RunAll(RunAt.InMainMenu);
		}

		/// <summary>
		/// Applied to Rotatable to rotate light previews if a visualizer is rotated.
		/// </summary>
		private static void OrientVisualizer_Postfix(Rotatable __instance) {
			var preview = __instance.gameObject.GetComponentSafe<LightShapePreview>();
			// Force regeneration on next Update()
			if (preview != null)
				PREVIOUS_CELL.Set(preview, -1);
		}

		/// <summary>
		/// Applied to KInputController to adjust array lengths if necessary.
		/// </summary>
		private static void QueueButtonEvent_Prefix(ref bool[] ___mActionState,
				KInputController.KeyDef key_def) {
			if (KInputManager.isFocused) {
				int max = PActionManager.Instance.GetMaxAction();
				key_def.mActionFlags = PActionManager.ExtendFlags(key_def.mActionFlags, max);
				___mActionState = PActionManager.ExtendFlags(___mActionState, max);
			}
		}

		/// <summary>
		/// Applied to LightGridEmitter to properly attribute lighting sources.
		/// </summary>
		private static void RefreshShapeAndPosition_Postfix(Light2D __instance) {
			var lm = PLightManager.Instance;
			if (lm != null)
				lm.CallingObject = __instance.gameObject;
		}

		/// <summary>
		/// Applied to LightGridEmitter to clean up the trash when it is removed from grid.
		/// </summary>
		private static void RemoveFromGrid_Postfix(LightGridEmitter __instance) {
			PLightManager.Instance?.DestroyLight(__instance);
		}

		/// <summary>
		/// Applied to Serialize to fix deserializing non-Klei achievements. Only applied in
		/// the vanilla version of the game as DLC changed the achievement format.
		/// </summary>
		private static bool Serialize_Prefix(ColonyAchievementStatus __instance,
				BinaryWriter writer) {
			var requirements = __instance.Requirements;
			writer.Write((byte)(__instance.success ? 1 : 0));
			writer.Write((byte)(__instance.failed ? 1 : 0));
			if (requirements == null)
				writer.Write(0);
			else {
				Assembly asm = typeof(Game).Assembly, asmFirstPass = typeof(KObject).Assembly;
				writer.Write(requirements.Count);
				foreach (var requirement in requirements) {
					var type = requirement.GetType();
					var typeAsm = type.Assembly;
					// Handles Assembly-CSharp and Assembly-CSharp-firstpass
					if (typeAsm == asm || typeAsm == asmFirstPass)
						writer.WriteKleiString(type.ToString());
					else
						writer.WriteKleiString(type.AssemblyQualifiedName);
					// Slower than a detour, but there are not many requirements and the
					// existence is used as a check to enable this patch
					ACHIEVEMENT_SERIALIZE.Invoke(requirement, new object[] { writer });
				}
			}
			return false;
		}

		/// <summary>
		/// Applied to GameInputMapping to update the action count if new actions are
		/// registered.
		/// </summary>
		private static void SetDefaultKeyBindings_Postfix() {
			PActionManager.Instance.UpdateMaxAction();
		}

		/// <summary>
		/// Applied to Assets to load sprites for custom tools.
		/// </summary>
		private static void ToolMenu_DLC_Postfix() {
			PToolMode.AddAllToolIconsToAssets();
		}

		/// <summary>
		/// Applied to ToolMenu to load sprites for custom tools.
		/// </summary>
		private static void ToolMenu_Vanilla_Postfix(List<Sprite> ___icons) {
			// TODO Vanilla/DLC code
			PToolMode.AddAllToolIcons(___icons);
		}

		/// <summary>
		/// Applied to LightGridEmitter to update lit cells upon a lighting request.
		/// </summary>
		private static bool UpdateLitCells_Prefix(LightGridEmitter __instance,
				List<int> ___litCells, LightGridEmitter.State ___state) {
			var lm = PLightManager.Instance;
			return lm == null || !lm.UpdateLitCells(__instance, ___state, ___litCells);
		}

		/// <summary>
		/// Applies all patches.
		/// </summary>
		/// <param name="instance">The Harmony instance to use when patching.</param>
		private static void PatchAll(HarmonyInstance instance) {
			if (instance == null)
				throw new ArgumentNullException("instance");

			// ColonyAchievementStatus
			if (ACHIEVEMENT_SERIALIZE != null) {
#if DEBUG
				PUtil.LogDebug("Applying ColonyAchievementRequirement.Serialize patch");
#endif
				// TODO Vanilla/DLC code
				instance.Patch(typeof(ColonyAchievementStatus), nameof(ColonyAchievementStatus.
					Serialize), PatchMethod(nameof(Serialize_Prefix)), null);
			}

			// Db
			instance.Patch(typeof(Db), nameof(Db.Initialize), PatchMethod(nameof(
				Initialize_Prefix)), PatchMethod(nameof(Initialize_Postfix)));

			// Game
			instance.Patch(typeof(Game), "DestroyInstances", null, PatchMethod(nameof(
				Game_DestroyInstances_Postfix)));
			instance.Patch(typeof(Game), "OnPrefabInit", null, PatchMethod(nameof(
				Game_OnPrefabInit_Postfix)));

			// GameInputMapping
			instance.Patch(typeof(GameInputMapping), nameof(GameInputMapping.
				SetDefaultKeyBindings), null, PatchMethod(nameof(
				SetDefaultKeyBindings_Postfix)));

			// GameUtil
			instance.Patch(typeof(GameUtil), nameof(GameUtil.GetKeycodeLocalized),
				PatchMethod(nameof(GetKeycodeLocalized_Prefix)), null);

			// KInputController
			instance.PatchConstructor(typeof(KInputController.KeyDef), new Type[] {
				typeof(KKeyCode), typeof(Modifier)
			}, null, PatchMethod(nameof(CKeyDef_Postfix)));
			instance.Patch(typeof(KInputController), nameof(KInputController.IsActive),
				PatchMethod(nameof(IsActive_Prefix)), null);
			instance.Patch(typeof(KInputController), nameof(KInputController.QueueButtonEvent),
				PatchMethod(nameof(QueueButtonEvent_Prefix)), null);

			if (PLightManager.InitInstance()) {
				// DiscreteShadowCaster
				instance.Patch(typeof(DiscreteShadowCaster), nameof(DiscreteShadowCaster.
					GetVisibleCells), PatchMethod(nameof(GetVisibleCells_Prefix)), null);

				// Light2D
				instance.Patch(typeof(Light2D), "AddToScenePartitioner",
					PatchMethod(nameof(AddToScenePartitioner_Prefix)), null);
				instance.Patch(typeof(Light2D), nameof(Light2D.RefreshShapeAndPosition), null,
					PatchMethod(nameof(RefreshShapeAndPosition_Postfix)));

				// LightBuffer
				try {
					instance.PatchTranspile(typeof(LightBuffer), "LateUpdate", PatchMethod(
						nameof(LightBuffer_LateUpdate_Transpile)));
				} catch (Exception e) {
					// Only visual, log as warning
					PUtil.LogExcWarn(e);
				}

				// LightGridEmitter
				instance.Patch(typeof(LightGridEmitter), nameof(LightGridEmitter.AddToGrid),
					null, PatchMethod(nameof(AddToGrid_Postfix)));
				instance.Patch(typeof(LightGridEmitter), "ComputeLux",
					PatchMethod(nameof(ComputeLux_Prefix)), null);
				instance.Patch(typeof(LightGridEmitter), nameof(LightGridEmitter.
					RemoveFromGrid), null, PatchMethod(nameof(RemoveFromGrid_Postfix)));
				instance.Patch(typeof(LightGridEmitter), nameof(LightGridEmitter.
					UpdateLitCells), PatchMethod(nameof(UpdateLitCells_Prefix)), null);

				// LightGridManager
				instance.Patch(typeof(LightGridManager), nameof(LightGridManager.
					CreatePreview), PatchMethod(nameof(CreatePreview_Prefix)), null);

				// LightShapePreview
				instance.Patch(typeof(LightShapePreview), "Update",
					PatchMethod(nameof(LightShapePreview_Update_Prefix)), null);

				// Rotatable
				instance.Patch(typeof(Rotatable), "OrientVisualizer", null,
					PatchMethod(nameof(OrientVisualizer_Postfix)));
			}

			// MainMenu
			instance.Patch(typeof(MainMenu), "OnSpawn", null, PatchMethod(
				nameof(MainMenu_OnSpawn_Postfix)));

			// PBuilding
			try {
				// Non essential, do not crash if fail
			instance.Patch(typeof(BuildingTemplates), nameof(BuildingTemplates.
				CreateBuildingDef), null, PatchMethod(nameof(CreateBuildingDef_Postfix)));
			instance.Patch(typeof(EquipmentTemplates), nameof(EquipmentTemplates.
				CreateEquipmentDef), null, PatchMethod(nameof(CreateEquipmentDef_Postfix)));
			} catch (Exception e) {
#if DEBUG
				PUtil.LogExcWarn(e);
#endif
			}
			if (PBuilding.CheckBuildings())
				instance.Patch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.
					LoadGeneratedBuildings), PatchMethod(nameof(
					LoadGeneratedBuildings_Prefix)), null);

			// PCodex
			instance.Patch(typeof(CodexCache), nameof(CodexCache.CollectEntries), null,
				PatchMethod(nameof(CollectEntries_Postfix)));
			instance.Patch(typeof(CodexCache), nameof(CodexCache.CollectSubEntries), null,
				PatchMethod(nameof(CollectSubEntries_Postfix)));

			// PLocalization
			var locale = Localization.GetLocale();
			if (locale != null) {
				PLocalization.LocalizeAll(locale);
				PLocalizationItself.LocalizeItself(locale);
			}

			// ToolMenu
			if (PToolMode.HasToolIcons) {
				if (typeof(ToolMenu).GetFieldSafe("icons", false) != null)
					instance.Patch(typeof(ToolMenu), "OnPrefabInit", null,
						PatchMethod(nameof(ToolMenu_Vanilla_Postfix)));
				else
					instance.Patch(typeof(Assets), "OnPrefabInit", null,
						PatchMethod(nameof(ToolMenu_DLC_Postfix)));
			}

			// ModsScreen
			POptions.Init();
			instance.Patch(typeof(ModsScreen), "BuildDisplay", null, PatchMethod(nameof(
				BuildDisplay_Postfix)));

			// SteamUGCService
			var ugc = PPatchTools.GetTypeSafe("SteamUGCService", "Assembly-CSharp");
			if (ugc != null)
				try {
					instance.PatchTranspile(ugc, "LoadPreviewImage", PatchMethod(nameof(
						LoadPreviewImage_Transpile)));
				} catch (Exception e) {
#if DEBUG
					PUtil.LogExcWarn(e);
#endif
				}

			// TMPro.TMP_InputField
			Utils.TextMeshProPatcher.Patch(instance);

			// Postload, legacy and normal
			PPatchManager.ExecuteLegacyPostload();
			PPatchManager.RunAll(RunAt.AfterModsLoad);
		}

#endregion

#region Infrastructure

		/// <summary>
		/// Returns a patch method from this class. It must be static.
		/// </summary>
		/// <param name="name">The patch method name.</param>
		/// <returns>The matching method.</returns>
		private static HarmonyMethod PatchMethod(string name) {
			return new HarmonyMethod(typeof(PLibPatches), name);
		}

		/// <summary>
		/// The version of PLib that these patches will represent.
		/// </summary>
		public string MyVersion { get; }

		public PLibPatches() {
			MyVersion = PVersion.VERSION;
		}

		/// <summary>
		/// Applies the patches for this version of PLib.
		/// </summary>
		/// <param name="instance">The Harmony instance to use for patching.</param>
		public void Apply(HarmonyInstance instance) {
			PRegistry.LogPatchDebug("Using version " + MyVersion);
			try {
				PatchAll(instance);
			} catch (TypeLoadException e) {
				PUtil.LogException(e);
			} catch (TargetInvocationException e) {
				PUtil.LogException(e);
			}
			PActionManager.Instance.Init();
			PRegistry.LogPatchDebug("PLib patches applied");
		}

		public override bool Equals(object obj) {
			return obj is PLibPatches other && other.MyVersion == MyVersion;
		}

		public override int GetHashCode() {
			return MyVersion.GetHashCode();
		}

		public override string ToString() {
			return "PLibPatches version " + MyVersion;
		}

#endregion
	}
}
