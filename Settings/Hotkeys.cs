using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Input;
using ff14bot.Helpers;
using Kombatant.Annotations;
using Kombatant.Settings.Models;
using Newtonsoft.Json;

namespace Kombatant.Settings
{
    /// <summary>
    /// Settings class for the botbase hotkey configuration.
    /// </summary>
    public class Hotkeys : JsonSettings, INotifyPropertyChanged
    {
        private static Hotkeys _hotkeys;
        public static Hotkeys Instance => _hotkeys ?? (_hotkeys = new Hotkeys("Hotkeys"));

        // ReSharper disable once MemberCanBePrivate.Global
        public Hotkeys(string filename) : base(
            Path.Combine(CharacterSettingsDirectory, "Kombatant", filename + ".json"))
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null, bool save = false)
        {
            if(save)
                Save();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Reloads the settings.
        /// </summary>
        public static void Reload()
        {
            Instance.Save();
            _hotkeys = new Hotkeys("Hotkeys");
        }

        /// <summary>
        /// Overwrites the current Instance with the given new version.
        /// </summary>
        /// <param name="settings"></param>
        public static void Overwrite(Hotkeys settings)
        {
            _hotkeys = settings;
        }

        #region --- Dynamic Hotkeys

        private HashSet<DynamicHotkey> _dynamicHotkeys;

        [Description("List of dynamic hotkeys")]
        [JsonProperty("DynamicHotkeys")]
        public HashSet<DynamicHotkey> DynamicHotkeys
        {
            get
            {
                if(_dynamicHotkeys == null)
                    _dynamicHotkeys = new HashSet<DynamicHotkey>();

                return _dynamicHotkeys;
            }

            set
            {
                if (_dynamicHotkeys.Equals(value))
                    return;

                _dynamicHotkeys = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region --- Pause/Unpause

        private Keys _pauseKey;
        private ModifierKeys _pauseModifier;

        [DefaultValue(Keys.F)]
        [Description("Hotkey to pause/unpause Kombatant")]
        [JsonProperty("PauseKey")]
        public Keys PauseKey
        {
            get => _pauseKey;
            set
            {
                if(!_pauseKey.Equals(value))
                {
                    _pauseKey = value;
                    OnPropertyChanged();
                }
            }
        }

        [DefaultValue(ModifierKeys.Control)]
        [Description("Modifier for the Pause/Unpause hotkey")]
        [JsonProperty("PauseKeyModifier")]
        public ModifierKeys PauseKeyModifier
        {
            get => _pauseModifier;
            set
            {
                if(!_pauseModifier.Equals(value))
                {
                    _pauseModifier = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enablePauseKey;
        [DefaultValue(false)]
        [JsonProperty("EnablePauseKey")]
        public bool EnablePauseKey
        {
            get => _enablePauseKey;
            set
            {
                if (!_enablePauseKey.Equals(value))
                {
                    _enablePauseKey = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region --- Enable/Disable Autonomous Mode

        private Keys _autonomousKey;
        private ModifierKeys _autonomousModifier;

        [DefaultValue(Keys.T)]
        [Description("Hotkey to toggle autonomous mode")]
        [JsonProperty("ToggleAutonomousKey")]
        public Keys ToggleAutonomousKey
        {
            get => _autonomousKey;
            set
            {
                if(!_autonomousKey.Equals(value))
                {
                    _autonomousKey = value;
                    OnPropertyChanged();
                }
            }
        }

        [DefaultValue(ModifierKeys.Control)]
        [Description("Modifier for the autonomous toggle")]
        [JsonProperty("ToggleAutonomousModifierKey")]
        public ModifierKeys ToggleAutonomousModifierKey
        {
            get => _autonomousModifier;
            set
            {
                if(!_autonomousModifier.Equals(value))
                {
                    _autonomousModifier = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enableAutonomousKey;
        [DefaultValue(false)]
        [JsonProperty("EnableAutonomousKey")]
        public bool EnableAutonomousKey
        {
            get => _enableAutonomousKey;
            set
            {
                if (!_enableAutonomousKey.Equals(value))
                {
                    _enableAutonomousKey = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region --- Enable/Disable Auto Face

        private Keys _autofaceKey;
        private ModifierKeys _autofaceModifier;

        [DefaultValue(Keys.V)]
        [Description("Hotkey to toggle auto face")]
        [JsonProperty("AutoFaceToggleKey")]
        public Keys AutoFaceToggleKey
        {
            get => _autofaceKey;
            set
            {
                if(!_autofaceKey.Equals(value))
                {
                    _autofaceKey = value;
                    OnPropertyChanged();
                }
            }
        }

        [DefaultValue(ModifierKeys.Control)]
        [Description("Modifier for the auto face hotkey")]
        [JsonProperty("AutoFaceToggleModifierKey")]
        public ModifierKeys AutoFaceToggleModifierKey
        {
            get => _autofaceModifier;
            set
            {
                if(!_autofaceModifier.Equals(value))
                {
                    _autofaceModifier = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enableAutoFaceKey;
        [DefaultValue(false)]
        [JsonProperty("EnableAutoFaceKey")]
        public bool EnableAutoFaceKey
        {
            get => _enableAutoFaceKey;
            set
            {
                if (!_enableAutoFaceKey.Equals(value))
                {
                    _enableAutoFaceKey = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region --- Enable/Disable Auto Targeting

        private Keys _autotargetKey;
        private ModifierKeys _autotargetModifier;

        [DefaultValue(Keys.G)]
        [Description("Hotkey to toggle auto target selection")]
        [JsonProperty("AutoTargetToggleKey")]
        public Keys AutoTargetToggleKey
        {
            get => _autotargetKey;
            set
            {
                if(!_autotargetKey.Equals(value))
                {
                    _autotargetKey = value;
                    OnPropertyChanged();
                }
            }
        }

        [DefaultValue(ModifierKeys.Control)]
        [Description("Modifier for the auto target selection hotkey")]
        [JsonProperty("AutoTargetToggleModifierKey")]
        public ModifierKeys AutoTargetToggleModifierKey
        {
            get => _autotargetModifier;
            set
            {
                if(!_autotargetModifier.Equals(value))
                {
                    _autotargetModifier = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enableAutoTargetKey;
        [DefaultValue(false)]
        [JsonProperty("EnableAutoTargetKey")]
        public bool EnableAutoTargetKey
        {
            get => _enableAutoTargetKey;
            set
            {
                if (!_enableAutoTargetKey.Equals(value))
                {
                    _enableAutoTargetKey = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region --- Enable/Disable Avoidance

        private Keys _avoidanceKey;
        private ModifierKeys _avoidanceModifier;

        [DefaultValue(Keys.T)]
        [Description("Hotkey to toggle avoidance")]
        [JsonProperty("AvoidanceToggleKey")]
        public Keys AvoidanceToggleKey
        {
            get => _avoidanceKey;
            set
            {
                if (!_avoidanceKey.Equals(value))
                {
                    _avoidanceKey = value;
                    OnPropertyChanged();
                }
            }
        }

        [DefaultValue(ModifierKeys.Shift)]
        [Description("Modifier for the avoidance toggle hotkey")]
        [JsonProperty("AvoidanceToggleModifierKey")]
        public ModifierKeys AvoidanceToggleModifierKey
        {
            get => _avoidanceModifier;
            set
            {
                if (!_avoidanceModifier.Equals(value))
                {
                    _avoidanceModifier = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enableAvoidanceKey;
        [DefaultValue(false)]
        [JsonProperty("EnableAvoidanceKey")]
        public bool EnableAvoidanceKey
        {
            get => _enableAvoidanceKey;
            set
            {
                if (!_enableAvoidanceKey.Equals(value))
                {
                    _enableAvoidanceKey = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region --- Enable/Disable Following

        private Keys _followingKey;
        private ModifierKeys _followingModifier;

        [DefaultValue(Keys.T)]
        [JsonProperty("FollowingToggleKey")]
        public Keys FollowingToggleKey
        {
            get => _followingKey;
            set
            {
                if (!_followingKey.Equals(value))
                {
                    _followingKey = value;
                    OnPropertyChanged();
                }
            }
        }

        [DefaultValue(ModifierKeys.Shift)]
        [JsonProperty("FollowingToggleModifierKey")]
        public ModifierKeys FollowingToggleModifierKey
        {
            get => _followingModifier;
            set
            {
                if (!_followingModifier.Equals(value))
                {
                    _followingModifier = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enableFollowingKey;
        [DefaultValue(false)]
        [JsonProperty("EnableFollowingKey")]
        public bool EnableFollowingKey
        {
            get => _enableFollowingKey;
            set
            {
                if (!_enableFollowingKey.Equals(value))
                {
                    _enableFollowingKey = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

    }
}