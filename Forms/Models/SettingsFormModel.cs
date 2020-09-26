using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

		public List<InstanceModel> InstanceContentResults => DataManager.InstanceContentResults.Values
			.Where(i => (i.IsInDutyFinder && !string.IsNullOrWhiteSpace(i.CurrentLocaleName) ||
						 i.ChnName.StartsWith("随机任务")) && !(i.ChnName.StartsWith("陆行鸟竞赛") && i.ContentUICategory == 0)).Select(i => new InstanceModel()
						 { Name = i.CurrentLocaleName, Id = i.Id })
			.OrderBy(i => i.Id).ToList();

		public class InstanceModel
		{
			public uint Id { get; set; }
			public string Name { get; set; }
			public override string ToString()
			{
				return Name;
			}
		}
		//public class InstanceContentResultModel : InstanceContentResult
		//{
		//	public InstanceContentResultModel(uint id, string name)
		//	{
		//		base.Id = id;
		//		base.ChnName = name;
		//		base.IsInDutyFinder = true;
		//	}

		//	public override string ToString()
		//	{
		//		return $"[{Id}]".PadRight(4) + ChnName;
		//	}
		//}
		// 记得在ViewModel的构造函数中初始化这两个List列表
		// ... InitList()...

		/// <summary>
		/// Reference for the BotBase settings.
		/// Placed here, so we only have to deal with one model.
		/// </summary>
		public BotBase BotBase
		{
			get => BotBase.Instance;

			set
			{
				BotBase.Overwrite(value);
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Reference for the Hotkeys settings.
		/// Placed here, so we only have to deal with one model.
		/// </summary>
		public Hotkeys Hotkeys
		{
			get => Hotkeys.Instance;

			set
			{
				Hotkeys.Overwrite(value);
				OnPropertyChanged();
			}
		}

		public float X => Core.Me == null ? 0 : Core.Me.X;
		public float Y => Core.Me == null ? 0 : Core.Me.Y;
		public float Z => Core.Me == null ? 0 : Core.Me.Z;

		public ICommand SetAnimationSpeedDefault
		{
			get
			{
				return new RelayCommand(s =>
				{
					BotBase.Instance.AnimationSpeed = 1f;
				});
			}
		}

		public ICommand SetAnimationSpeedMaximum
		{
			get
			{
				return new RelayCommand(s =>
				{
					BotBase.Instance.AnimationSpeed = float.MaxValue;
				});
			}
		}

		public ICommand InvertAll
		{
			get
			{
				return new RelayCommand(s =>
				{
					BotBase.Instance.EnableHealInCombat = !BotBase.Instance.EnableHealInCombat;
					BotBase.Instance.EnableHealOutofCombat = !BotBase.Instance.EnableHealOutofCombat;
					BotBase.Instance.EnablePreCombatBuff = !BotBase.Instance.EnablePreCombatBuff;
					BotBase.Instance.EnablePullBuff = !BotBase.Instance.EnablePullBuff;
					BotBase.Instance.EnablePull = !BotBase.Instance.EnablePull;
					BotBase.Instance.EnableCombatBuff = !BotBase.Instance.EnableCombatBuff;
					BotBase.Instance.EnableCombat = !BotBase.Instance.EnableCombat;
					BotBase.Instance.EnableDeath = !BotBase.Instance.EnableDeath;
					BotBase.Instance.EnableRest = !BotBase.Instance.EnableRest;
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
					if (BotBase.Instance.UseFocusOverlay)
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
					if (BotBase.Instance.UseStatusOverlay)
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
						BotBase.Instance.FixedCharacterString = Core.Target.ToString();
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
					BotBase.Instance.FixedCharacterString = string.Empty;
					BotBase.Instance.FixedCharacterType = 0;
				});
			}
		}
	}
}