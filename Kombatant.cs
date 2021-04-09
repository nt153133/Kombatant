//!CompilerOption:Optimize:On


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Buddy.Overlay.Controls;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Behavior;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Pathing.Service_Navigation;
using GreyMagic;
using Kombatant.Forms;
using Kombatant.Forms.Models;
using Kombatant.Logic;
using Kombatant.Helpers;
using Kombatant.Localization;
using Kombatant.Memory;
using TreeSharp;
using BotBase = Kombatant.Settings.BotBase;
using UserControl = System.Windows.Controls.UserControl;

namespace Kombatant
{
	/// <summary>
	/// <para>Kombatant - An Advanced Open-Source Combat Assist Botbase</para>
	/// <para>For credits and stuff, see README.md</para>
	/// </summary>
	// ReSharper disable once UnusedMember.Global
	public class Kombatant : AsyncBotBase
	{
		//private static Kombatant kombatant;
		//internal static Kombatant Instance => kombatant ?? (kombatant = new Kombatant());
		#region Botbase Metadata

		public override string Name => Localization.Localization.Name_Kombatant;
		public override bool IsAutonomous => Settings.BotBase.Instance.IsAutonomous;
		public override bool RequiresProfile => false;
		public override bool WantButton => true;
		public override PulseFlags PulseFlags => Settings.Fleeting.Instance.BotBasePulseFlags;
		public static float CombatReachAdj { get; internal set; }
		/// <summary>
		/// Required dummy composite. Why, though?
		/// </summary>
		public override Composite Root => new ActionAlwaysFail();

		#endregion

		#region Xaml Dummies

		private Window _window;
		private static readonly object ContentLock = new object();
		private UserControl _windowContent;

		#endregion

		#region Windows Forms Dummies

		private ClassicSettingsForm _classicSettings;

		#endregion

		/// <summary>
		/// Called when someone presses "Botbase Settings" in the main window.
		/// </summary>
		public override void OnButtonPress()
		{
			if (Settings.BotBase.Instance.UseWinFormsSettings)
			{
				if (_classicSettings == null || _classicSettings.IsDisposed)
					_classicSettings = new ClassicSettingsForm();

				_classicSettings.Show();
			}
			else
			{
				if (_window == null)
				{
					_window = new SettingsForm
					{
						DataContext = SettingsFormModel.Instance,
						Content = LoadWindowContent(),
						//Width = LoadWindowContent().Width+5,
						//Height = LoadWindowContent().Height+30,
						Title = $"Kombatant {Core.Me?.Name}",
						WindowStartupLocation = WindowStartupLocation.CenterScreen,
					};

					_window.Loaded += (e, a) =>
					{

					};

					_window.Closed += (e, a) =>
					{
						_window = null;
						_windowContent = null;

						Settings.BotBase.Reload();
						Settings.Hotkeys.Reload();
						Logging.Write(Localization.Localization.Msg_ReloadedSettings);
					};

					try
					{
						_window.Show();
						_window.Focus();
					}
					catch (Exception)
					{
						// ignored
					}
				}
				else
				{
					try
					{
						if (_window.WindowState == WindowState.Minimized)
						{
							_window.WindowState = WindowState.Normal;
						}
						//else
						//{
						//    _window.WindowState = WindowState.Minimized;
						//}
						_window.Focus();
					}
					catch (Exception)
					{
						// ignored
					}
				}
			}
			//if (RoutineManager.Current.Name == "ShinraPVP")
			//{
			//    RoutineManager.Current.OnButtonPress();
			//}
		}

		/// <summary>
		/// Load up the xaml window.
		/// </summary>
		/// <returns></returns>
		private UserControl LoadWindowContent()
		{
			try
			{
				lock (ContentLock)
				{
					_windowContent = WpfHelper.Instance.LoadWindowContent(Resources.Controls.SettingsControl);
					return _windowContent;
				}
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log(@"Exception loading window content! {0}", ex);
			}
			return null;
		}

		private static bool sidestepStatus;

		private static bool _init;
		public Kombatant()
		{
			if (!_init)
			{
				Task.Factory.StartNew(() =>
				{
					init();
					_init = true;
					LogHelper.Instance.Log("Kombatant Initialized.");
				});
			}
		}

		internal static bool _memoFaliure;
		private void init()
		{
			try
			{
				_ = Offsets.Instance;
				_ = GroundSpeedHook.Instance;
				_ = CombatReachHook.Instance;
				_ = FastCastHook.Instance;
				_ = GcdHook.Instance;
				Core.Memory.Patches.Create(Offsets.Instance.KnockbackFunc, Enumerable.Repeat((byte)0x90, 5).ToArray(), "NoKnockbackPatch");
			}
			catch (Exception e)
			{
				_memoFaliure = true;
			}
			LocalizationInitializer.Initalize();
			//Settings.BotBase.Instance.AutoRegisterDuties = false;
		}

		public override void Pulse()
		{

		}
		//static Thread t1 => new Thread(() =>
		//	{
		//		LogHelper.Instance.Log("<AnimationHack> Thread Started.");
		//		while (running)
		//		{
		//			if (BotBase.Instance.EnableAnimationLockHack)
		//			{
		//				Core.Memory.Write(AnimationLockTimer, BotBase.Instance.AnimationLockMaxDelay);
		//			}
		//			if (BotBase.Instance.EnableAnimationSpeedHack)
		//			{
		//				Core.Memory.Write(Core.Me.Pointer + 0xCD4, BotBase.Instance.AnimationSpeed);
		//				Core.Memory.Write(Core.Me.Pointer + 0xCD8, BotBase.Instance.AnimationSpeed);
		//			}

		//			Thread.SpinWait(10);
		//		}
		//		LogHelper.Instance.Log("<AnimationHack> Thread Aborted.");
		//	});


		/// <summary>
		/// Called when the botbase gets started.
		/// </summary>
		public override void Start()
		{
			try { sidestepStatus = PluginManager.Plugins.First(i => i.Plugin.Name == "SideStep").Enabled; }
			catch { }

			// Always start paused
			Settings.BotBase.Instance.IsPaused = false;

			// Set up navigation
			Navigator.PlayerMover = new SlideMover();
			Navigator.NavigationProvider = new ServiceNavigationProvider();

			// Register the hotkeys
			HotkeyHelper.Instance.ReloadHotkeys();

			//// Less than 30 TPS are a no-no due to the design of this botbase. At least warn the user of this!
			//if (TreeRoot.TicksPerSecond < 30)
			//{
			//	LogHelper.Instance.Log(Localization.Localization.Msg_Not30Tps_Log);
			//	OverlayHelper.Instance.AddToast(Localization.Localization.Msg_Not30Tps, Colors.Red, Colors.DarkRed, TimeSpan.FromSeconds(10));
			//}

			if (Settings.BotBase.Instance.UseStatusOverlay) OverlayManager.StartStatusOverlay();
			if (Settings.BotBase.Instance.UseFocusOverlay) OverlayManager.StartFocusOverlay();
		}


		/// <summary>
		/// Called when the botbase gets stopped.
		/// </summary>
		public override void Stop()
		{
			// Destroy the navigator
			Navigator.PlayerMover = new NullMover();
			Navigator.NavigationProvider = new NullProvider();

			// Unregister the hotkeys
			HotkeyHelper.Instance.UnregisterHotkeys();

			try
			{
				Core.Memory.Patches["GroundSpeedHook"].Remove();
				Core.Memory.Patches["CombatReachHook"].Remove();
				Core.Memory.Patches["NoKnockbackPatch"].Remove();
			}
			catch (Exception e)
			{

			}


			// Stop Overlays
			OverlayManager.StopFocusOverlay();
			OverlayManager.StopStatusOverlay();
			//OverlayManager.StatusOverlay.Update(StatusOverlayUiComponent.RunningStatus.Stopped);

			try { PluginManager.Plugins.First(i => i.Plugin.Name == "SideStep").Enabled = sidestepStatus; }
			catch { }
		}

		/// <summary>
		/// Task wrapper for the main botbase logic.
		/// </summary>
		/// <returns></returns>
		public override Task AsyncRoot()
		{
			return KombatantLogic();
		}

		/// <summary>
		/// Main botbase logic task.
		/// </summary>
		/// <returns></returns>
		private async Task<bool> KombatantLogic()
		{
#if DBG
			using (new PerformanceLogger("TotalExecutionTime"))
#endif
			{
#if DBG
				using (new PerformanceLogger("RefreshOverlay"))
#endif
				{
					//invoking UI thread to refresh overlay
					if (BotBase.Instance.UseFocusOverlay)
						OverlayManager.FocusOverlay.Update();
					if (BotBase.Instance.UseStatusOverlay)
						OverlayManager.StatusOverlay.Update();
				}
#if DBG
				using (new PerformanceLogger("Memory"))
#endif
				{
					if (BotBase.Instance.Hackpanel)
					{
						Hack.Instance.ExecuteLogic();
					}
				}

				// Execute Loot logic
				if (await Loot.Instance.ExecuteLogic())
					return await Task.FromResult(true);

#if DBG
				using (new PerformanceLogger("SetTickRate"))
#endif
				{
					TreeRoot.TicksPerSecond = BotBase.Instance.IsPaused ? (byte)BotBase.Instance.PausingTickRate : (byte)BotBase.Instance.RunningTickRate;
				}
#if DBG
				using (new PerformanceLogger("CommenceDuty"))
#endif
				{
					// Execute duty commence logic
					if (await CommenceDuty.Instance.ExecuteLogic())
						return await Task.FromResult(true);
				}

#if DBG
				using (new PerformanceLogger("Mechanics"))
#endif
				{
					// Execute mechanics logic (gaze attacks et al)
					if (await Mechanics.Instance.ExecuteLogic())
						return await Task.FromResult(true);
				}

#if DBG
				using (new PerformanceLogger("Convenience"))
#endif
				{
					// Execute convenience logic (auto sprint etc.)
					if (await Convenience.Instance.ExecuteLogic())
						return await Task.FromResult(true);
				}

#if DBG
				using (new PerformanceLogger("Target"))
#endif
				{
					// Execute target logic
					if (await Target.Instance.ExecuteLogic())
						return await Task.FromResult(true);
				}

#if DBG
				using (new PerformanceLogger("Avoidance"))
#endif
				{
					// Execute avoidance logic
					if (await Avoidance.Instance.ExecuteLogic())
						return await Task.FromResult(true);
				}

#if DBG
				using (new PerformanceLogger("Movement"))
#endif
				{
					// Execute auto movement
					if (await Movement.Instance.ExecuteLogic())
						return await Task.FromResult(true);
				}

#if DBG
				using (new PerformanceLogger("CombatLogic"))
#endif
				{
					//// Execute combat logic
					if (await CombatLogic.Instance.ExecuteLogic())
						return await Task.FromResult(true);
				}

				//// Execute tank specific logic
				//if (await Tank.Instance.ExecuteLogic())
				//	return await Task.FromResult(true);

				//// Execute healer specific logic
				//if (await Healer.Instance.ExecuteLogic())
				//	return await Task.FromResult(true);

				//// Execute DPS specific logic
				//if (await DPS.Instance.ExecuteLogic())
				//	return await Task.FromResult(true);

				return await Task.FromResult(false);
			}
		}
	}
}