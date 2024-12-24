﻿/*
 * Copyright 2024 Peter Han
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

using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace PeterHan.ToastControl {
	/// <summary>
	/// The options class for Popup Control.
	/// </summary>
	[ModInfo("https://github.com/peterhaneve/ONIMods", collapse: true)]
	[JsonObject(MemberSerialization = MemberSerialization.OptOut)]
	public sealed class ToastControlOptions {
		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.OPTIONS_CAPTION")]
		[JsonIgnore]
		// Usually OptIn is the right choice, but here there are dozens to serialize and one to
		// not
		public LocText Caption => null;

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DISABLE_MOVING",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DISABLE_MOVING_TOOLTIP")]
		public bool DisableMoving { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.CONTROL_ALL",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.CONTROL_ALL_TOOLTIP")]
		public bool GlobalEnable { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.BUILD_COMPLETE",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.BUILD_COMPLETE",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool BuildingComplete { get; set; }

		[Option("STRINGS.BUILDING.STATUSITEMS.ANGERDAMAGE.NAME",
			"STRINGS.BUILDING.STATUSITEMS.ANGERDAMAGE.TOOLTIP",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamageAnger { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DAMAGE_INPUT",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DAMAGE_INPUT",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamageInput { get; set; }

		[Option("STRINGS.BUILDING.STATUSITEMS.LOGICOVERLOADED.NAME",
			"STRINGS.BUILDING.STATUSITEMS.LOGICOVERLOADED.TOOLTIP",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamageLogic { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DAMAGE_METEOR",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DAMAGE_METEOR",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamageMeteor { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DAMAGE_OTHER",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DAMAGE_OTHER",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamageOther { get; set; }

		[Option("STRINGS.BUILDING.STATUSITEMS.OVERHEATED.NAME",
			"STRINGS.BUILDING.STATUSITEMS.OVERHEATED.TOOLTIP",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamageOverheat { get; set; }

		[Option("STRINGS.BUILDING.STATUSITEMS.OVERLOADED.NAME",
			"STRINGS.BUILDING.STATUSITEMS.OVERLOADED.TOOLTIP",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamageOverload { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DAMAGE_PIPE",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DAMAGE_PIPE",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamagePipe { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DAMAGE_PRESSURE",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DAMAGE_PRESSURE",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamagePressure { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DAMAGE_ROCKET",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DAMAGE_ROCKET",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool DamageRocket { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.GERMS_ADDED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.GERMS_ADDED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool GermsAdded { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.INVALID_CONNECTION",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.INVALID_CONNECTION",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool InvalidConnection { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.RADS_REMOVED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.RADS_REMOVED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.BUILDINGS")]
		public bool RadiationRemoved { get; set; }

		[Option("STRINGS.MISC.NOTIFICATIONS.LEVELUP.NAME",
			"STRINGS.MISC.NOTIFICATIONS.LEVELUP.TOOLTIP",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DUPLICANTS")]
		public bool AttributeIncrease { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.CRITTER_DROPS",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.CRITTER_DROPS",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DUPLICANTS")]
		public bool CritterDrops { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DISEASE_CURED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DISEASE_CURED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DUPLICANTS")]
		public bool DiseaseCure { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DISEASE_INFECTED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DISEASE_INFECTED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DUPLICANTS")]
		public bool DiseaseInfect { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.EFFECT_ADDED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.EFFECT_ADDED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DUPLICANTS")]
		public bool EffectAdded { get; set; }

		[Option("STRINGS.CREATURES.STATUSITEMS.FLEEING.NAME",
			"STRINGS.CREATURES.STATUSITEMS.FLEEING.TOOLTIP",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DUPLICANTS")]
		public bool Fleeing { get; set; }

		[Option("STRINGS.UI.GAMEOBJECTEFFECTS.FORGAVEATTACKER",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.FORGIVENESS",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DUPLICANTS")]
		public bool Forgiveness { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.OVERJOYED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.OVERJOYED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DUPLICANTS")]
		public bool Overjoyed { get; set; }

		[Option("STRINGS.MISC.NOTIFICATIONS.SKILL_POINT_EARNED.NAME",
			"STRINGS.MISC.NOTIFICATIONS.SKILL_POINT_EARNED.TOOLTIP",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.DUPLICANTS")]
		public bool SkillPointEarned { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DELIVERED_DUPLICANT",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DELIVERED_DUPLICANT",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool Delivered { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.DELIVERED_MACHINE",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DELIVERED_MACHINE",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool DeliveredMachine { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.ELEMENT_GAINED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.ELEMENT_GAINED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool ElementDropped { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.ELEMENT_DUG",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.ELEMENT_DUG",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool ElementDug { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.ELEMENT_REMOVED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.ELEMENT_REMOVED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool ElementRemoved { get; set; }

		[Option("STRINGS.MISC.NOTIFICATIONS.FOODROT.NAME",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.FOODROT",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool FoodDecayed { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.ITEM_GAINED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.ITEM_GAINED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool ItemGained { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.PICKEDUP_DUPLICANT",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.PICKEDUP_DUPLICANT",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool PickedUp { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.PICKEDUP_MACHINE",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.PICKEDUP_MACHINE",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool PickedUpMachine { get; set; }

		[Option("STRINGS.UI.TOOLTIPS.NOMATERIAL",
			"STRINGS.UI.TOOLTIPS.SELECTAMATERIAL",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool InsufficientMaterials { get; set; }
		
		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIAL_CHANGED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.MATERIAL_CHANGED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool MaterialChanged { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.RESEARCH_GAINED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.RESEARCH_GAINED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.MATERIALS")]
		public bool ResearchGained { get; set; }

		[Option("STRINGS.UI.TOOLS.CAPTURE.NOT_CAPTURABLE",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.CAPTURE_FAILED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.TOOLS")]
		public bool CannotCapture { get; set; }

		[Option("STRINGS.UI.COPIED_SETTINGS",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.SETTINGS_APPLIED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.TOOLS")]
		public bool CopySettings { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.ELEMENT_MOPPED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.ELEMENT_MOPPED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.TOOLS")]
		public bool ElementMopped { get; set; }

		[Option("STRINGS.UI.SANDBOXTOOLS.CLEARFLOOR.DELETED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.FLOOR_CLEARED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.TOOLS")]
		public bool FloorCleared { get; set; }

		[Option("STRINGS.UI.FRONTEND.TOASTCONTROL.HARVEST_TOGGLED",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.HARVEST_TOGGLED",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.TOOLS")]
		public bool HarvestToggle { get; set; }

		[Option("STRINGS.UI.DEBUG_TOOLS.INVALID_LOCATION",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.DEBUG_LOCATION_INVALID",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.TOOLS")]
		public bool InvalidLocation { get; set; }
		
		[Option("STRINGS.UI.TOOLS.MOP.NOT_ON_FLOOR",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.MOP_NOT_FLOOR",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.TOOLS")]
		public bool MopNotFloor { get; set; }

		[Option("STRINGS.UI.TOOLS.MOP.TOO_MUCH_LIQUID",
			"STRINGS.UI.TOOLTIPS.TOASTCONTROL.MOP_TOO_MUCH",
			"STRINGS.UI.FRONTEND.TOASTCONTROL.TOOLS")]
		public bool MopTooMuch { get; set; }

		public ToastControlOptions() {
			DisableMoving = false;
			GlobalEnable = true;
			AttributeIncrease = true;
			BuildingComplete = true;
			CannotCapture = true;
			CopySettings = true;
			CritterDrops = true;
			DamageAnger = true;
			DamageInput = true;
			DamageLogic = true;
			DamageMeteor = true;
			DamageOther = true;
			DamageOverheat = true;
			DamageOverload = true;
			DamagePipe = true;
			DamagePressure = true;
			DamageRocket = true;
			Delivered = false;
			DeliveredMachine = false;
			DiseaseCure = true;
			DiseaseInfect = true;
			EffectAdded = true;
			ElementDug = false;
			ElementDropped = false;
			ElementMopped = true;
			ElementRemoved = false;
			Fleeing = true;
			FloorCleared = true;
			FoodDecayed = true;
			Forgiveness = true;
			GermsAdded = true;
			HarvestToggle = true;
			InsufficientMaterials = true;
			InvalidConnection = true;
			InvalidLocation = true;
			ItemGained = true;
			Overjoyed = true;
			MaterialChanged = true;
			MopNotFloor = true;
			MopTooMuch = true;
			PickedUp = false;
			PickedUpMachine = false;
			RadiationRemoved = true;
			ResearchGained = true;
			SkillPointEarned = true;
		}
	}
}
