using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Buddy.Overlay.Commands;
using ff14bot;
using ff14bot.Managers;
using Kombatant.Annotations;
using Kombatant.Enums;
using Kombatant.Helpers;
using Kombatant.Settings;
using Kombatant.Settings.Models;

namespace Kombatant.Forms.Models
{
    /// <summary>
    /// ViewModel for the settings window.
    /// </summary>
    public class SettingsFormModel : INotifyPropertyChanged
    {
        private static SettingsFormModel _settingsFormModel;
        internal static SettingsFormModel Instance => _settingsFormModel ?? (_settingsFormModel = new SettingsFormModel());

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private SettingsFormModel()
        {

        }

        public bool AutoSelectYesNegate { get { return !Settings.BotBase.Instance.AutoSelectYes; } }

        /// <summary>
        /// Reference for the BotBase settings.
        /// Placed here, so we only have to deal with one model.
        /// </summary>
        public Settings.BotBase BotBase
        {
            get => Settings.BotBase.Instance;

            set
            {
                Settings.BotBase.Overwrite(value);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Reference for the Hotkeys settings.
        /// Placed here, so we only have to deal with one model.
        /// </summary>
        public Settings.Hotkeys Hotkeys
        {
            get => Settings.Hotkeys.Instance;

            set
            {
                Settings.Hotkeys.Overwrite(value);
                OnPropertyChanged();
            }
        }

        public ICommand InvertAll
        {
	        get
	        {
		        return new RelayCommand(s =>
		        {
			        BotBase.Instance.EnableRest = !BotBase.Instance.EnableRest;
			        BotBase.Instance.EnableHeal = !BotBase.Instance.EnableHeal;
			        BotBase.Instance.EnablePreCombatBuff = !BotBase.Instance.EnablePreCombatBuff;
			        BotBase.Instance.EnablePullBuff = !BotBase.Instance.EnablePullBuff;
			        BotBase.Instance.EnablePull = !BotBase.Instance.EnablePull;
			        BotBase.Instance.EnableCombatBuff = !BotBase.Instance.EnableCombatBuff;
			        BotBase.Instance.EnableCombat = !BotBase.Instance.EnableCombat;
		        });
	        }
        }

        /// <summary>
        /// Pops open the combat routine selector.
        /// </summary>
        public ICommand SelectCombatRoutine
        {
            get
            {
                return new RelayCommand(s =>
                {
                    RoutineManager.PreferedRoutine = @"";
                    RoutineManager.PickRoutine();
                });
            }
        }

        /// <summary>
        /// Adds the currently selected target to the targeting whitelist.
        /// </summary>
        public ICommand AddToTargetWhitelist
        {
            get
            {
                return new RelayCommand(s =>
                {
                    var to = new TargetObject(1, "Test");
                    /*
                    if (!Core.Me.HasTarget || BotBase.TargetWhitelist.Any(o => o.NpcId == Core.Target.NpcId) || !Core.Target.IsEnemy())
                        return;
                    */
                    BotBase.TargetWhitelist.Add(to);
                    //LogHelper.Instance.Log($"Adding target {Core.Target.Name} to whitelist...");
                });
            }
        }

        /// <summary>
        /// Removes the currently selected entry from the targeting whitelist.
        /// </summary>
        public ICommand RemoveFromTargetWhitelist
        {
            get
            {
                return new RelayCommand(s =>
                {
                    var to = new TargetObject(Core.Target.NpcId, Core.Target.Name);
                    /*if (!Core.Me.HasTarget || BotBase.TargetWhitelist.All(o => o.NpcId != Core.Target.NpcId) || !Core.Target.IsEnemy())
                        return;*/

                    //LogHelper.Instance.Log($"Removing target {Core.Target.Name} from whitelist...");
                    BotBase.TargetWhitelist.Remove(to);
                });
            }
        }

        public ICommand FocusOverlaySwitch
        {
            get
            {
                return new RelayCommand(s =>
                {
                    if (Settings.BotBase.Instance.UseFocusOverlay)
                    {
                        OverlayManager.StartFocusOverlay();
                    }
                    else
                    {
                        OverlayManager.StopFocusOverlay();
                    }

                    OverlayManager.FocusOverlay.Update();
                });
            }
        }
        public ICommand StatusOverlaySwitch
        {
            get
            {
                return new RelayCommand(s =>
                {
                    if (Settings.BotBase.Instance.UseStatusOverlay)
                    {
                        OverlayManager.StartStatusOverlay();
                    }
                    else
                    {
                        OverlayManager.StopStatusOverlay();
                    }

                    OverlayManager.StatusOverlay.Update(Logic.Convenience.CurrentStatus);
                });
            }
        }

        //public ICommand AutoSelectYesCommand
        //{
        //    get
        //    {
        //        return new RelayCommand(s =>
        //        {
        //            if (Settings.BotBase.Instance.AutoSelectYes)
        //            {
        //                Settings.BotBase.Instance.AutoHandoverRequestItems = true;
        //                Settings.BotBase.Instance.AutoAcceptTeleport = true;
        //            }
        //            else
        //            {
        //                Settings.BotBase.Instance.AutoHandoverRequestItems = false;
        //                Settings.BotBase.Instance.AutoAcceptTeleport = false;
        //            }
        //        });
        //    }
        //}

        public ICommand AutoTargetCommand
        {
            get
            {
                return new RelayCommand(s =>
                {
                    //if (Settings.BotBase.Instance.AutoTarget)
                    //{
                    //    Settings.BotBase.Instance.AutoDeSelectTarget = true;
                    //    Settings.BotBase.Instance.EnableLosCheck = true;
                    //}
                    //else
                    //{
                    //    Settings.BotBase.Instance.AutoDeSelectTarget = false;
                    //    Settings.BotBase.Instance.EnableLosCheck = false;
                    //}
                });
            }
        }

        public ICommand ReloadOverlay
        {
            get
            {
                return new RelayCommand(s =>
                {
                    if (Core.OverlayManager.IsActive)
                    {
                        if (BotBase.UseFocusOverlay)
                        {
                            OverlayManager.StartFocusOverlay();
                        }

                        if (BotBase.UseStatusOverlay)
                        {
                            OverlayManager.StartStatusOverlay();
                        }
                    }
                });
            }
        }

        public ICommand ReloadHotkeys
        {
            get
            {
                return new RelayCommand(s =>
                {
                    HotkeyHelper.Instance.ReloadNonDynamicHotkeys();
                    LogHelper.Instance.Log("Hotkeys reloaded.");
                });
            }
        }

        public ICommand SetCurrentTarget
        {
            get
            {
                return new RelayCommand(s =>
                {
                    if (Core.Me.HasTarget)
                    {
                        BotBase.Instance.FixedCharacterName = Core.Target.Name;
                        BotBase.Instance.FixedCharacterId = Core.Target.ObjectId;
                        BotBase.Instance.FixedCharacterType = Core.Target.Type;
                        //BotBase.FollowMode = FollowMode.FixedCharacter;
                    }
                });
            }
        }

        public ICommand ClearSetTarget
        {
            get
            {
                return new RelayCommand(s =>
                {
                    BotBase.Instance.FixedCharacterName = string.Empty;
                    BotBase.Instance.FixedCharacterId = 0;
                    BotBase.Instance.FixedCharacterType = 0;
                });
            }
        }
    }
}