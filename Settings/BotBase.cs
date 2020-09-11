﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using Kombatant.Constants;
using Kombatant.Enums;
using Kombatant.Extensions;
using Kombatant.Helpers;
using Kombatant.Settings.Models;
using Newtonsoft.Json;

namespace Kombatant.Settings
{
	/// <summary>
	/// Settings class for the main botbase configuration.
	/// </summary>
	public class BotBase : JsonSettings, INotifyPropertyChanged
	{
		private static BotBase _botBase;
		public static BotBase Instance => _botBase ?? (_botBase = new BotBase("Settings"));

		// ReSharper disable once MemberCanBePrivate.Global
		public BotBase(string filename) : base(Path.Combine(CharacterSettingsDirectory, "Kombatant", filename + ".json")) { }

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Reloads the settings.
		/// </summary>
		public static void Reload()
		{
			_botBase = new BotBase("Settings");
		}

		/// <summary>
		/// Overwrites the current Instance with the given new version.
		/// </summary>
		/// <param name="settings"></param>
		public static void Overwrite(BotBase settings)
		{
			_botBase = settings;
		}

		/// <summary>
		/// Notifies the user interface that we changed a property.
		/// </summary>
		/// <param name="propertyName"></param>
		[Annotations.NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			Save();
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public string MyName => ff14bot.Core.Me.Name;

		#region --- Debugging Stuff

		#region Botbase Settings Window

		private bool _useWinFormsSettings;

		[DefaultValue(false)]
		[Description("Use the Windows Forms based Botbase Settings dialogue.")]
		[JsonProperty("UseWinFormsSettings")]
		public bool UseWinFormsSettings
		{
			get => _useWinFormsSettings;
			set
			{
				_useWinFormsSettings = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#endregion

		#region --- Convenience

		#region Auto Accept Duty Finder

		private bool _autoAcceptDutyFinder;

		[DefaultValue(false)]
		[Description("Automatically accept Duty Finder when it pops")]
		[JsonProperty("AutoAcceptDutyFinder")]
		public bool AutoAcceptDutyFinder
		{
			get => _autoAcceptDutyFinder;
			set
			{
				_autoAcceptDutyFinder = value;
				OnPropertyChanged();
			}
		}

		private bool _autoRegisterDuties;
		[DefaultValue(false)]
		public bool AutoRegisterDuties
		{
			get => _autoRegisterDuties;
			set
			{
				_autoRegisterDuties = value;
				OnPropertyChanged();
			}
		}

		private string _dutyIDsToRegister;
		[DefaultValue("")]
		[Description("Duty IDs you need to auto register, use comma to separate. up to 5.")]
		public string DutyIDsToRegister
		{
			get => _dutyIDsToRegister;
			set
			{
				_dutyIDsToRegister = value;
				OnPropertyChanged();
			}
		}

		private bool _AutoLeaveDuty;

		[DefaultValue(false)]
		public bool AutoLeaveDuty
		{
			get => _AutoLeaveDuty;
			set
			{
				_AutoLeaveDuty = value;
				OnPropertyChanged();
			}
		}


		#endregion

		#region Auto Accept Quests

		private bool _autoAcceptQuests;

		[DefaultValue(false)]
		[Description("Automatically accept quests")]
		[JsonProperty("AutoAcceptQuests")]
		public bool AutoAcceptQuests
		{
			get => _autoAcceptQuests;
			set
			{
				_autoAcceptQuests = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Advance Dialogue

		private bool _autoAdvanceDialogue;

		[DefaultValue(true)]
		[Description("Automatically advances dialogue")]
		[JsonProperty("AutoAdvanceDialogue")]
		public bool AutoAdvanceDialogue
		{
			get => _autoAdvanceDialogue;
			set
			{
				_autoAdvanceDialogue = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Handover Request Items

		private bool _autoHandoverRequestItems;

		[DefaultValue(false)]
		public bool AutoHandoverRequestItems
		{
			get => _autoHandoverRequestItems;
			set
			{
				_autoHandoverRequestItems = value;
				OnPropertyChanged();
			}
		}

		private bool _handoverHqIfnoNQ;

		[DefaultValue(false)]
		public bool HandoverHqIfnoNQ
		{
			get => _handoverHqIfnoNQ;
			set
			{
				_handoverHqIfnoNQ = value;
				OnPropertyChanged();
			}
		}
		#endregion

		#region AutoAcceptRevive

		private bool _autoAcceptRevive;

		[DefaultValue(false)]
		public bool AutoAcceptRevive
		{
			get => _autoAcceptRevive;
			set
			{
				_autoAcceptRevive = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region _autoAcceptTeleport

		private bool _autoAcceptTeleport;

		[DefaultValue(false)]
		[JsonProperty("AutoAcceptTeleport")]
		public bool AutoAcceptTeleport
		{
			get => _autoAcceptTeleport;
			set
			{
				_autoAcceptTeleport = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region _autoSelectYes

		private bool _autoSelectYes;

		[DefaultValue(false)]
		[JsonProperty("AutoSelectYes")]
		public bool AutoSelectYes
		{
			get => _autoSelectYes;
			set
			{
				_autoSelectYes = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region AutoTrade

		private bool _autoTrade;

		[DefaultValue(false)]
		[JsonProperty("AutoTrade")]
		public bool AutoTrade
		{
			get => _autoTrade;
			set
			{
				_autoTrade = value;
				OnPropertyChanged();
			}
		}

		private bool _autoTradeFriendOnly;

		[DefaultValue(true)]
		[JsonProperty("AutoTradeFriendOnly")]
		public bool AutoTradeFriendOnly
		{
			get => _autoTradeFriendOnly;
			set
			{
				_autoTradeFriendOnly = value;
				OnPropertyChanged();
			}
		}
		#endregion

		#region Auto Emote - Emote Command

		private string _autoEmoteCommand;

		[DefaultValue(@"")]
		[Description("Emote to keep up")]
		[JsonProperty("AutoEmoteCommand")]
		public string AutoEmoteCommand
		{
			get => _autoEmoteCommand;
			set
			{
				_autoEmoteCommand = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Emote - Interval

		private double _autoEmoteInterval;

		[DefaultValue(0)]
		[Description("Seconds between renewing the emote")]
		[JsonProperty("AutoEmoteInterval")]
		public double AutoEmoteInterval
		{
			get => _autoEmoteInterval;
			set
			{
				_autoEmoteInterval = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto End ACT Encounters

		private bool _autoEndActEncounters;

		[DefaultValue(false)]
		[Description("Automatically end ACT encounters")]
		[JsonProperty("AutoEndACTEncounters")]
		public bool AutoEndActEncounters
		{
			get => _autoEndActEncounters;
			set
			{
				_autoEndActEncounters = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Face Target

		private bool _autoFaceTarget;

		[DefaultValue(true)]
		[Description("Automatically face current target")]
		[JsonProperty("AutoFaceTarget")]
		public bool AutoFaceTarget
		{
			get => _autoFaceTarget;
			set
			{
				_autoFaceTarget = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Skip Cutscenes

		private bool _autoSkipCutscenes;

		[DefaultValue(true)]
		[Description("Automatically skip cutscenes")]
		[JsonProperty("AutoSkipCutscenes")]
		public bool AutoSkipCutscenes
		{
			get => _autoSkipCutscenes;
			set
			{
				_autoSkipCutscenes = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Sprint

		private bool _autoSprint;

		[DefaultValue(false)]
		[Description("Automatically sprint when out of combat")]
		[JsonProperty("AutoSprint")]
		public bool AutoSprint
		{
			get => _autoSprint;
			set
			{
				_autoSprint = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Sync Fate

		private bool _autoSyncFate;

		[DefaultValue(false)]
		[Description("Automatically sync to FATE when targeting a FATE mob")]
		[JsonProperty("AutoSyncFate")]
		public bool AutoSyncFate
		{
			get => _autoSyncFate;
			set
			{
				_autoSyncFate = value;
				OnPropertyChanged();
			}
		}

		#region Duty Finder - Notify when Duty is ready

		private bool _dutyFinderNotify;

		[DefaultValue(false)]
		[Description("Notify when the Duty Finder is ready")]
		[JsonProperty("DutyFinderNotify")]
		public bool DutyFinderNotify
		{
			get => _dutyFinderNotify;
			set
			{
				_dutyFinderNotify = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Duty Finder - Wait Time Before Accept

		private int _dutyFinderWaitTime;

		[DefaultValue(10)]
		[Description("Seconds before accepting a Duty Finder")]
		[JsonProperty("DutyFinderWaitTime")]
		public int DutyFinderWaitTime
		{
			get => _dutyFinderWaitTime;
			set
			{
				_dutyFinderWaitTime = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#endregion

		#region Mechanic Warnings

		private bool _enableMechanicWarnings;

		[DefaultValue(false)]
		[Description("Automatically take control from the player during certain attacks")]
		[JsonProperty("MechanicWarnings")]
		public bool MechanicWarnings
		{
			get => _enableMechanicWarnings;
			set
			{
				_enableMechanicWarnings = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Is Autonomous

		private bool _enableAutonomy;

		[DefaultValue(false)]
		[Description("Advertises the botbase as autonomous which will enable several automatic features but will in turn strip the user of some control.")]
		[JsonProperty("IsAutononomous")]
		public bool IsAutonomous
		{
			get => _enableAutonomy;
			set
			{
				_enableAutonomy = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#endregion

		#region --- Combat Behaviors

		#region CombatBuff

		private bool _enableCombatBuff;

		[DefaultValue(true)]
		[Description("Enable executing the CombatBuff behavior")]
		[JsonProperty("EnableCombatBuff")]
		public bool EnableCombatBuff
		{
			get => _enableCombatBuff;
			set
			{
				_enableCombatBuff = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Combat

		private bool _enableCombat;

		[DefaultValue(true)]
		[Description("Enable executing the Combat behavior")]
		[JsonProperty("EnableCombat")]
		public bool EnableCombat
		{
			get => _enableCombat;
			set
			{
				_enableCombat = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Heal

		private bool _enableHeal;

		[DefaultValue(true)]
		[Description("Enable executing the heal behavior")]
		[JsonProperty("EnableHeal")]
		public bool EnableHeal
		{
			get => _enableHeal;
			set
			{
				_enableHeal = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Paused

		private bool _isPaused;

		[DefaultValue(false)]
		[Description("Pause/unpause combat assist")]
		[JsonProperty("Paused")]
		public bool IsPaused
		{
			get => _isPaused;
			set
			{
				_isPaused = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region PreCombatBuff

		private bool _enablePreCombatBuff;

		[DefaultValue(true)]
		[Description("Enable executing the PreCombatBuff behavior")]
		[JsonProperty("EnablePreCombatBuff")]
		public bool EnablePreCombatBuff
		{
			get => _enablePreCombatBuff;
			set
			{
				_enablePreCombatBuff = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region PullBuff

		private bool _enablePullBuff;

		[DefaultValue(true)]
		[Description("Enable executing the PullBuff behavior")]
		[JsonProperty("EnablePullBuff")]
		public bool EnablePullBuff
		{
			get => _enablePullBuff;
			set
			{
				_enablePullBuff = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Pull

		private bool _enablePull;

		[DefaultValue(true)]
		[Description("Enable executing the pull behavior")]
		[JsonProperty("EnablePull")]
		public bool EnablePull
		{
			get => _enablePull;
			set
			{
				_enablePull = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Rest

		private bool _enableRest;

		[DefaultValue(true)]
		[Description("Enable executing the Rest behavior")]
		[JsonProperty("EnableRest")]
		public bool EnableRest
		{
			get => _enableRest;
			set
			{
				_enableRest = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Smart Pull

		private bool _enableSmartPull;

		[DefaultValue(true)]
		[Description("Enable party-aware pulling of mobs")]
		[JsonProperty("EnableSmartPull")]
		public bool EnableSmartPull
		{
			get => _enableSmartPull;
			set
			{
				_enableSmartPull = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#endregion

		#region --- Movement

		#region Auto Move - Follow Mode Selection

		private FollowMode _followMode;

		[DefaultValue(FollowMode.None)]
		[Description("Determines how RebornBuddy follows in auto movement")]
		[JsonProperty("FollowMode")]
		public FollowMode FollowMode
		{
			get => _followMode;
			set
			{
				_followMode = value;
				OnPropertyChanged();
			}
		}

		//private FollowMode _previousFollowMode;

		//[DefaultValue(FollowMode.PartyLeader)]
		//[JsonProperty("PreviousFollowMode")]
		//public FollowMode PreviousFollowMode
		//{
		//	get => _previousFollowMode;
		//	set
		//	{
		//		_previousFollowMode = value;
		//		OnPropertyChanged();
		//	}
		//}

		private bool _enableFollowing;

		[DefaultValue(false)]
		[JsonProperty("EnableFollowing")]
		public bool EnableFollowing
		{
			get => _enableFollowing;
			set
			{
				_enableFollowing = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Move - Follow Distance

		private int _followDistance;

		[DefaultValue(2)]
		[JsonProperty("FollowDistance")]
		public int FollowDistance
		{
			get => _followDistance;
			set
			{
				_followDistance = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Move - Follow This Character

		private string _fixedCharacterName;

		[Description("Name of the character to follow on FixedCharacter follow mode")]
		[JsonProperty("FixedCharacterName")]
		[DefaultValue("")]
		public string FixedCharacterName
		{
			get => _fixedCharacterName;
			set
			{
				_fixedCharacterName = value;
				OnPropertyChanged();
			}
		}

		private string _fixedCharacterString;

		[DefaultValue("")]
		public string FixedCharacterString
		{
			get => _fixedCharacterString;
			set
			{
				_fixedCharacterString = value;
				OnPropertyChanged();
			}
		}

		private GameObjectType _fixedCharacterType;
		public GameObjectType FixedCharacterType
		{
			get => _fixedCharacterType;
			set
			{
				_fixedCharacterType = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Move - Navigation Mode

		private WaypointGenerationMode _waypointGenerationMode;

		[Description("Determines how RebornBuddy handles navigation")]
		[JsonProperty("WaypointGenerationMode")]
		[DefaultValue(WaypointGenerationMode.Offmesh)]
		public WaypointGenerationMode WaypointGenerationMode
		{
			get => _waypointGenerationMode;
			set
			{
				_waypointGenerationMode = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Avoidance - Pause on Bosses

		private bool _pauseAvoidanceOnBosses;

		[DefaultValue(false)]
		[Description("Automatically pause avoidance on bosses")]
		[JsonProperty("PauseAvoidanceOnBosses")]
		public bool PauseAvoidanceOnBosses
		{
			get => _pauseAvoidanceOnBosses;
			set
			{
				_pauseAvoidanceOnBosses = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Enable Avoidance

		private bool _enableAvoidance;

		[DefaultValue(false)]
		[Description("Enable avoidance")]
		[JsonProperty("EnableAvoidance")]
		public bool EnableAvoidance
		{
			get => _enableAvoidance;
			set
			{
				_enableAvoidance = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#endregion

		#region --- Targeting

		#region Allow switch target

		private bool _enableAutoTargetSwitch;

		[DefaultValue(false)]
		[Description("Enable switching targets while the old target is still alive")]
		[JsonProperty("AutoTargetSwitch")]
		public bool AutoTargetSwitch
		{
			get => _enableAutoTargetSwitch;
			set
			{
				_enableAutoTargetSwitch = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto Target

		private bool _enableAutoTarget;

		[DefaultValue(false)]
		[Description("Enable auto target")]
		[JsonProperty("AutoTarget")]
		public bool AutoTarget
		{
			get => _enableAutoTarget;
			set
			{
				_enableAutoTarget = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region FATE Filter

		private bool _enableFateFilter;

		[DefaultValue(false)]
		[Description("Enable FATE target filter")]
		[JsonProperty("FateTargetFilter")]
		public bool FateTargetFilter
		{
			get => _enableFateFilter;
			set
			{
				_enableFateFilter = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Los Check

		private bool _enableLosCheck;

		[DefaultValue(false)]
		[Description("Target Los Check")]
		[JsonProperty("TargetLosCheck")]
		public bool EnableLosCheck
		{
			get => _enableLosCheck;
			set
			{
				_enableLosCheck = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Auto deselect

		private bool _autoDeSelectTarget;

		[DefaultValue(false)]
		[Description("_autoDeSelectTarget")]
		[JsonProperty("AutoDeSelectTarget")]
		public bool AutoDeSelectTarget
		{
			get => _autoDeSelectTarget;
			set
			{
				_autoDeSelectTarget = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Mark Target

		private bool _markTarget;

		[DefaultValue(false)]
		[JsonProperty("MarkTarget")]
		public bool MarkTarget
		{
			get => _markTarget;
			set
			{
				_markTarget = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Target Aquisition Max Distance

		private int _targetSelectionMaxDistance;

		[DefaultValue(0)]
		[Description("If set to a value other than 0, this value will determine the maximum target scan distance")]
		[JsonProperty("TargetScanMaxDistance")]
		public int TargetScanMaxDistance
		{
			get => _targetSelectionMaxDistance;
			set
			{
				_targetSelectionMaxDistance = value;
				OnPropertyChanged();
			}
		}

		private bool _manuallyTargetDistance;

		[DefaultValue(true)]
		[JsonProperty("_manuallyTargetDistance")]
		public bool ManuallyTargetDistance
		{
			get => _manuallyTargetDistance;
			set
			{
				_manuallyTargetDistance = value;
				OnPropertyChanged();
			}
		}
		#endregion

		#region Target Scan Rate

		private int _targetScanRate;

		[DefaultValue(1000)]
		[JsonProperty("TargetScanRate")]
		public int TargetScanRate
		{
			get => _targetScanRate;
			set
			{
				_targetScanRate = value;
				OnPropertyChanged();
			}
		}

		#endregion


		#region Target Mode

		private TargetingMode _targetingMode;

		[DefaultValue(TargetingMode.None)]
		[Description("Automatically choose new target")]
		[JsonProperty("TargetingMode")]
		public TargetingMode AutoTargetingMode
		{
			get => _targetingMode;
			set
			{
				_targetingMode = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Target Whitelist

		private FullyObservableCollection<TargetObject> _targetWhitelist;

		[Description("List of whitelisted targets")]
		[JsonProperty("TargetWhitelist")]
		public FullyObservableCollection<TargetObject> TargetWhitelist
		{
			get
			{
				return _targetWhitelist ?? (_targetWhitelist = new FullyObservableCollection<TargetObject>());
			}
			set
			{
				_targetWhitelist = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Focus Overlay

		private bool _useFocusOverlay;

		[DefaultValue(false)]
		[JsonProperty("UseFocusOverlay")]
		public bool UseFocusOverlay
		{
			get => _useFocusOverlay;
			set
			{
				_useFocusOverlay = value;
				OnPropertyChanged();
			}
		}

		private bool _focusOverlayEnableClickThrough;

		[DefaultValue(false)]
		[JsonProperty("FocusOverlayEnableClickThrough")]
		public bool FocusOverlayEnableClickThrough
		{
			get => _focusOverlayEnableClickThrough;
			set
			{
				_focusOverlayEnableClickThrough = value;
				OnPropertyChanged();
			}
		}

		private double _focusOverlayX;

		[DefaultValue(50)]
		[JsonProperty("FocusOverlayX")]
		public double FocusOverlayX
		{
			get => _focusOverlayX;
			set
			{
				_focusOverlayX = value;
				OnPropertyChanged();
			}
		}

		private double _focusOverlayY;

		[DefaultValue(50)]
		[JsonProperty("FocusOverlayY")]
		public double FocusOverlayY
		{
			get => _focusOverlayY;
			set
			{
				_focusOverlayY = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Status Overlay

		private bool _useStatusOverlay;

		[DefaultValue(false)]
		[JsonProperty("UseStatusOverlay")]
		public bool UseStatusOverlay
		{
			get => _useStatusOverlay;
			set
			{
				_useStatusOverlay = value;
				OnPropertyChanged();
			}
		}

		private double _StatusOverlayX;

		[DefaultValue(50)]
		[JsonProperty("StatusOverlayX")]
		public double StatusOverlayX
		{
			get => _StatusOverlayX;
			set
			{
				_StatusOverlayX = value;
				OnPropertyChanged();
			}
		}

		private double _StatusOverlayY;

		[DefaultValue(120)]
		[JsonProperty("StatusOverlayY")]
		public double StatusOverlayY
		{
			get => _StatusOverlayY;
			set
			{
				_StatusOverlayY = value;
				OnPropertyChanged();
			}
		}

		#endregion



		#endregion
	}
}