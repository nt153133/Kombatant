using System;
using System.Linq;
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
using Kombatant.Forms;
using Kombatant.Forms.Models;
using Kombatant.Logic;
using Kombatant.Helpers;
using TreeSharp;
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
        #region Botbase Metadata

        public override string Name => Resources.Localization.Name_Kombatant;
        public override bool IsAutonomous => Settings.BotBase.Instance.IsAutonomous;
        public override bool RequiresProfile => false;
        public override bool WantButton => true;
        public override PulseFlags PulseFlags => Settings.Fleeting.Instance.BotBasePulseFlags;

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
                        Title = "Kombatant",
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
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
                        Logging.Write(Resources.Localization.Msg_ReloadedSettings);
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

            // Less than 30 TPS are a no-no due to the design of this botbase. At least warn the user of this!
            if (TreeRoot.TicksPerSecond < 30)
            {
                LogHelper.Instance.Log(Resources.Localization.Msg_Not30Tps_Log);
                OverlayHelper.Instance.AddToast(Resources.Localization.Msg_Not30Tps, Colors.Red, Colors.DarkRed, TimeSpan.FromSeconds(10));
            }

            if (Settings.BotBase.Instance.UseFocusOverlay)
            {
                OverlayManager.StartFocusOverlay();
            }
        }

        private static bool sidestepStatus;
        public override void Initialize()
        {
            if (Settings.BotBase.Instance.UseStatusOverlay)
            {
                OverlayManager.StartStatusOverlay();
            }
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

            // Stop Overlays
            OverlayManager.StopFocusOverlay();
            OverlayManager.StatusOverlay.Update(StatusOverlayUiComponent.RunningStatus.Stopped);

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
            if (CommonBehaviors.IsLoading) return await Task.FromResult(false);

            // Execute duty commence logic
            if (await CommenceDuty.Instance.ExecuteLogic())
                return await Task.FromResult(true);

            // Execute mechanics logic (gaze attacks et al)
            if (await Mechanics.Instance.ExecuteLogic())
                return await Task.FromResult(true);

            // Execute convenience logic (auto sprint etc.)
            if (await Convenience.Instance.ExecuteLogic())
                return await Task.FromResult(true);

            // Execute target logic
            if (await Target.Instance.ExecuteLogic())
                return await Task.FromResult(true);

            // Execute avoidance logic
            if (await Avoidance.Instance.ExecuteLogic())
                return await Task.FromResult(true);

            // Execute auto movement
            if (await Movement.Instance.ExecuteLogic())
                return await Task.FromResult(true);

            // Execute tank specific logic
            if (await Tank.Instance.ExecuteLogic())
                return await Task.FromResult(true);

            // Execute healer specific logic
            if (await Healer.Instance.ExecuteLogic())
                return await Task.FromResult(true);

            // Execute DPS specific logic
            if (await DPS.Instance.ExecuteLogic())
                return await Task.FromResult(true);
            return await Task.FromResult(false);
        }
    }
}