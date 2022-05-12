﻿/*
 * Copyright 2022 Peter Han
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

using HarmonyLib;
using KMod;
using PeterHan.PLib.AVC;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;
using PeterHan.PLib.PatchManager;
using System;
using System.Collections.Generic;

namespace PeterHan.FastTrack {
	/// <summary>
	/// Patches which will be applied via annotations for Fast Track.
	/// </summary>
	public sealed class FastTrackMod : KMod.UserMod2 {
// Global, Game, World
#if DEBUG
		[PLibMethod(RunAt.AfterModsLoad)]
		internal static void Profile(Harmony harmony) {
			//harmony.Profile(typeof(KAnimBatchManager), nameof(KAnimBatchManager.UpdateDirty));
			//harmony.Profile(typeof(KBatchedAnimUpdater), nameof(KBatchedAnimUpdater.UpdateRegisteredAnims));
			harmony.Profile(typeof(Game), nameof(Game.UnsafeSim200ms));
			harmony.Profile(typeof(ConduitFlow), nameof(ConduitFlow.Sim200ms));
			harmony.Profile(typeof(EnergySim), nameof(EnergySim.EnergySim200ms));
		}
#endif

		/// <summary>
		/// The maximum time that any of the blocking joins will wait, in real time ms.
		/// </summary>
		public const int MAX_TIMEOUT = 5000;

		/// <summary>
		/// Turn off tile mesh renderers if this mod is active.
		/// </summary>
		private const string TRUE_TILES_ID = "TrueTiles";

		/// <summary>
		/// The number of CPUs to use for the async job manager.
		/// </summary>
		internal static int CoreCount { get; private set; }

		/// <summary>
		/// Set to true when the game gets off its feet, and false while it is still loading.
		/// </summary>
		internal static bool GameRunning { get; private set; }

		/// <summary>
		/// Runs several one-time patches after the Db is initialized.
		/// </summary>
		[PLibMethod(RunAt.AfterDbInit)]
		internal static void AfterDbInit() {
			var options = FastTrackOptions.Instance;
			if (options.BackgroundRoomRebuild)
				GamePatches.BackgroundRoomProber.Init();
			if (options.ThreatOvercrowding)
				CritterPatches.OvercrowdingMonitor_UpdateState_Patch.InitTagBits();
			if (options.RadiationOpts)
				GamePatches.FastProtonCollider.Init();
			if (options.SensorOpts)
				SensorPatches.SensorPatches.Init();
			if (options.AnimOpts)
				VisualPatches.KAnimLoopOptimizer.CreateInstance();
			if (options.InfoCardOpts)
				// Localization related
				UIPatches.FormatStringPatches.Init();
			if (options.AllocOpts) {
				UIPatches.TrappedDuplicantDiagnostic_CheckTrapped_Patch.Init();
				UIPatches.DescriptorAllocPatches.Init();
			}
		}

		/// <summary>
		/// Checks for compatibility and applies fast fetch manager updates only if Efficient
		/// Supply is not enabled.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		private static void CheckFetchCompat(Harmony harmony) {
			if (PPatchTools.GetTypeSafe("PeterHan.EfficientFetch.EfficientFetchManager") ==
					null) {
				PathPatches.AsyncBrainGroupUpdater.AllowFastListSwap = true;
				harmony.Patch(typeof(FetchManager.FetchablesByPrefabId),
					nameof(FetchManager.FetchablesByPrefabId.UpdatePickups),
					prefix: new HarmonyMethod(typeof(GamePatches.FetchManagerFastUpdate),
					nameof(GamePatches.FetchManagerFastUpdate.BeforeUpdatePickups)));
#if DEBUG
				PUtil.LogDebug("Patched FetchManager for fast pickup updates");
#endif
			} else {
				PUtil.LogWarning("Disabling fast pickup updates: Efficient Supply active");
				PathPatches.AsyncBrainGroupUpdater.AllowFastListSwap = false;
			}
		}

		/// <summary>
		/// Checks for compatibility and applies tile mesh renderer patches only if other tile
		/// mods are not enabled.
		/// </summary>
		/// <param name="harmony">The Harmony instance to use for patching.</param>
		/// <param name="mods">The mods list after all load.</param>
		private static void CheckTileCompat(Harmony harmony, IReadOnlyList<Mod> mods) {
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
		/// Fixes the drag order spam bug, if Stock Bug Fix did not get to it first.
		/// </summary>
		[HarmonyPriority(Priority.Low)]
		internal static void FixTimeLapseDrag() {
			PlayerController.Instance?.CancelDragging();
		}

		public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<Mod> mods) {
			const string ASDF = "Bugs.AutosaveDragFix";
			const string PACU_SAYS_NO = "Bugs.TropicalPacuRooms";
			var options = FastTrackOptions.Instance;
			base.OnAllModsLoaded(harmony, mods);
			if (options.MeshRendererOptions == FastTrackOptions.MeshRendererSettings.All &&
					mods != null)
				CheckTileCompat(harmony, mods);
			// Die pacu bug die
			if (options.AllocOpts && !PRegistry.GetData<bool>(PACU_SAYS_NO)) {
				GamePatches.DecorProviderRefreshFix.ApplyPatch(harmony);
				PRegistry.PutData(PACU_SAYS_NO, true);
			}
			if (options.FastUpdatePickups)
				CheckFetchCompat(harmony);
			if (!PRegistry.GetData<bool>(ASDF)) {
				// Fix the annoying autosave bug
				harmony.Patch(typeof(Timelapser), "SaveScreenshot", postfix: new HarmonyMethod(
					typeof(FastTrackMod), nameof(FastTrackMod.FixTimeLapseDrag)));
				PRegistry.PutData(ASDF, true);
			}
			if (options.FastReachability)
				GamePatches.FastCellChangeMonitor.CreateInstance();
			// Fix those world strings
			UIPatches.FormatStringPatches.ApplyPatch(harmony);
		}

		/// <summary>
		/// Cleans up the mod caches after the game ends.
		/// </summary>
		[PLibMethod(RunAt.OnEndGame)]
		internal static void OnEndGame() {
			var options = FastTrackOptions.Instance;
#if DEBUG
			Metrics.FastTrackProfiler.End();
#endif
			ConduitPatches.ConduitFlowVisualizerRenderer.Cleanup();
			if (options.CachePaths)
				PathPatches.PathCacher.Cleanup();
			if (options.UnstackLights)
				VisualPatches.LightBufferManager.Cleanup();
			if (options.ReduceTileUpdates)
				VisualPatches.PropertyTextureUpdater.DestroyInstance();
			if (options.ConduitOpts)
				ConduitPatches.BackgroundConduitUpdater.DestroyInstance();
			if (options.ParallelInventory)
				UIPatches.BackgroundInventoryUpdater.DestroyInstance();
			if (options.MeshRendererOptions != FastTrackOptions.MeshRendererSettings.None) {
				VisualPatches.TerrainMeshRenderer.DestroyInstance();
				ConduitPatches.ConduitFlowMeshPatches.CleanupAll();
				VisualPatches.GroundRendererDataPatches.CleanupAll();
			}
			if (options.MeshRendererOptions == FastTrackOptions.MeshRendererSettings.All)
				VisualPatches.TileMeshRenderer.DestroyInstance();
			if (options.FastReachability) {
				SensorPatches.FastGroupProber.Cleanup();
				GamePatches.FastCellChangeMonitor.FastInstance.Cleanup();
			}
			if (options.MiscOpts)
				GamePatches.GeyserConfigurator_FindType_Patch.Cleanup();
			if (options.PickupOpts) {
				GamePatches.SolidTransferArmUpdater.DestroyInstance();
				PathPatches.DeferAnimQueueTrigger.DestroyInstance();
			}
			if (options.AsyncPathProbe)
				PathPatches.PathProbeJobManager.DestroyInstance();
			GamePatches.AchievementPatches.DestroyInstance();
			PathPatches.AsyncBrainGroupUpdater.DestroyInstance();
			if (options.AllocOpts)
				UIPatches.DescriptorAllocPatches.Cleanup();
			if (options.SideScreenOpts) {
				UIPatches.AdditionalDetailsPanelWrapper.Cleanup();
				UIPatches.DetailsPanelWrapper.Cleanup();
				UIPatches.VitalsPanelWrapper.Cleanup();
			}
			if (options.BackgroundRoomRebuild)
				GamePatches.BackgroundRoomProber.DestroyInstance();
			AsyncJobManager.DestroyInstance();
			if (options.CustomStringFormat)
				UIPatches.FormatStringPatches.DumpStringFormatterCaches();
			GameRunning = false;
		}

		public override void OnLoad(Harmony harmony) {
			base.OnLoad(harmony);
			var options = FastTrackOptions.Instance;
			int overrideCoreCount = TuningData<CPUBudget.Tuning>.Get().overrideCoreCount,
				coreCount = UnityEngine.SystemInfo.processorCount;
			PUtil.InitLibrary();
			LocString.CreateLocStringKeys(typeof(FastTrackStrings.UI));
			new PLocalization().Register();
			new POptions().RegisterOptions(this, typeof(FastTrackOptions));
			new PPatchManager(harmony).RegisterPatchClass(typeof(FastTrackMod));
			new PVersionCheck().Register(this, new SteamVersionChecker());
			if (overrideCoreCount <= 0 || overrideCoreCount >= coreCount)
				CoreCount = Math.Min(8, coreCount);
			else
				CoreCount = overrideCoreCount;
			// In case this goes in stock bug fix later
			if (options.UnstackLights)
				PRegistry.PutData("Bugs.StackedLights", true);
			PRegistry.PutData("Bugs.MassStringsReadOnly", true);
			if (options.MiscOpts)
				PRegistry.PutData("Bugs.ElementTagInDetailsScreen", true);
			// This patch is Windows only apparently
			var target = typeof(Global).GetMethodSafe(nameof(Global.TestDataLocations), false);
			if (options.MiscOpts && target != null && typeof(Global).GetFieldSafe(
					nameof(Global.saveFolderTestResult), true) != null) {
				harmony.Patch(target, prefix: new HarmonyMethod(typeof(FastTrackMod),
					nameof(RemoveTestDataLocations)));
#if DEBUG
				PUtil.LogDebug("Patched Global.TestDataLocations");
#endif
			} else
				PUtil.LogDebug("Skipping TestDataLocations patch");
			// Another potentially Windows only patch
			target = typeof(Game).Assembly.GetType(nameof(InitializeCheck), false)?.
				GetMethodSafe(nameof(InitializeCheck.CheckForSavePathIssue), false);
			if (options.MiscOpts && target != null) {
				harmony.Patch(target, prefix: new HarmonyMethod(typeof(FastTrackMod),
					nameof(SkipInitCheck)));
#if DEBUG
				PUtil.LogDebug("Patched InitializeCheck.Awake");
#endif
			} else
				PUtil.LogDebug("Skipping InitializeCheck patch");
			GameRunning = false;
		}

		/// <summary>
		/// Initializes all components loaded on game start.
		/// </summary>
		[PLibMethod(RunAt.OnStartGame)]
		internal static void OnStartGame() {
			var inst = Game.Instance;
			var options = FastTrackOptions.Instance;
			if (options.AllocOpts)
				UIPatches.SelectedRecipeQueuePatches.Init();
			if (options.AsyncPathProbe)
				PathPatches.PathProbeJobManager.CreateInstance();
			if (options.CachePaths)
				PathPatches.PathCacher.Init();
			if (options.SideScreenOpts) {
				UIPatches.AdditionalDetailsPanelWrapper.Init();
				UIPatches.DetailsPanelWrapper.Init();
				UIPatches.VitalsPanelWrapper.Init();
			}
			if (options.PickupOpts)
				PathPatches.DeferAnimQueueTrigger.CreateInstance();
			if (inst != null) {
				var go = inst.gameObject;
				go.AddOrGet<AsyncJobManager>();
				if (options.BackgroundRoomRebuild)
					go.AddOrGet<GamePatches.BackgroundRoomProber>();
				if (options.ReduceSoundUpdates && !options.DisableSound)
					go.AddOrGet<SoundUpdater>();
				if (options.ParallelInventory)
					UIPatches.BackgroundInventoryUpdater.CreateInstance();
				if (options.MiscOpts)
					go.AddOrGet<GamePatches.AchievementPatches>();
				if (options.RadiationOpts)
					go.AddOrGet<GamePatches.SlicedRadiationGridUpdater>();
				// Requires the AJM to work
				if (options.PickupOpts)
					GamePatches.SolidTransferArmUpdater.CreateInstance();
				if (options.ConduitOpts)
					ConduitPatches.BackgroundConduitUpdater.CreateInstance();
				// If debugging is on, start logging
				if (options.Metrics)
					go.AddOrGet<Metrics.DebugMetrics>();
				inst.StartCoroutine(WaitForCleanLoad());
			}
			ConduitPatches.ConduitFlowVisualizerRenderer.Init();
			if (options.UnstackLights)
				VisualPatches.LightBufferManager.Init();
			VisualPatches.FullScreenDialogPatches.Init();
#if DEBUG
			Metrics.FastTrackProfiler.Begin();
#endif
		}

		/// <summary>
		/// Disables a time-consuming check on whether the save folders could successfully be
		/// written. Only used in the metrics reports anyways.
		/// </summary>
		internal static bool RemoveTestDataLocations(ref string ___saveFolderTestResult) {
			___saveFolderTestResult = "both";
			return false;
		}

		/// <summary>
		/// Skip a time-consuming file write test on load.
		/// </summary>
		internal static bool SkipInitCheck() {
			return false;
		}

		/// <summary>
		/// Waits a few frames as a coroutine, then allows things that require game stability
		/// to run.
		/// </summary>
		private static System.Collections.IEnumerator WaitForCleanLoad() {
			for (int i = 0; i < 3; i++)
				yield return null;
			GameRunning = true;
		}
	}
}
