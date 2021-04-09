//!CompilerOption:Optimize:On
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
	public partial class SettingsFormModel : INotifyPropertyChanged
	{
		private static SettingsFormModel _settingsFormModel;
		internal static SettingsFormModel Instance => _settingsFormModel ?? (_settingsFormModel = new SettingsFormModel());

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private SettingsFormModel()
		{

		}
		public string MyName => ff14bot.Core.Me.Name;

		public List<InstanceModel> InstanceContentResults
		{
			get
			{
				IOrderedEnumerable<InstanceContentResult> current = null;
				var guildhests = DataManager.InstanceContentResults.Values
					.Where(i => !string.IsNullOrWhiteSpace(i.CurrentLocaleName))
					.Where(i => new[] { 0 }.Contains(i.ContentUICategory))
					.Where(i=>i.IsInDutyFinder && !i.ChnName.Contains("活动挑战") && !i.ChnName.Contains("陆行鸟竞赛"))
					.OrderBy(res => res.Id);
				var dungeon = DataManager.InstanceContentResults.Values
					.Where(i => !string.IsNullOrWhiteSpace(i.CurrentLocaleName))
					.Where(i => new[] { 6, 7, 30, 31 }.Contains(i.ContentUICategory))
					.OrderBy(res => res.ContentUICategory);
				var pvp = DataManager.InstanceContentResults.Values
					.Where(i => !string.IsNullOrWhiteSpace(i.CurrentLocaleName))
					.Where(i => new[] { 1, 2, 3, 23, 24, 25, 28}.Contains(i.ContentUICategory))
					.Where(i => i.ContentUICategory != 1 || i.Id > 1000000)
					.Where(i => i.ContentUICategory != 2)
					.OrderBy(res => res.ContentUICategory);
				var goldSaucer = DataManager.InstanceContentResults.Values
					.Where(i => !string.IsNullOrWhiteSpace(i.CurrentLocaleName))
					.Where(i => new[] { 4, 5, 26, 27, 29 }.Contains(i.ContentUICategory))
					.Where(i => i.ContentUICategory != 5 || !i.ChnName.StartsWith("第"))
					.OrderBy(res => res.ContentUICategory);
				var roulette = DataManager.InstanceContentResults.Values
					.Where(i => !string.IsNullOrWhiteSpace(i.ChnName))
					.Where(i => new[] { 0, 28 }.Contains(i.ContentUICategory))
					.Where(i => i.ChnName.StartsWith("随机任务"))
					.OrderBy(res => res.ContentUICategory);
				var trial = DataManager.InstanceContentResults.Values
					.Where(i => !string.IsNullOrWhiteSpace(i.ChnName))
					.Where(i => new[] { 8, 9, 19, 20, 21, 22, 32, 33 }.Contains(i.ContentUICategory))
					.OrderBy(res => res.Id);
				var raid = DataManager.InstanceContentResults.Values
					.Where(i => !string.IsNullOrWhiteSpace(i.CurrentLocaleName))
					.Where(i => new[] { 10, 11, 12, 13, 16, 17, 34, 35 }.Contains(i.ContentUICategory))
					.OrderBy(res => res.RequiredClassJobLevel)
					.ThenBy(res => res.ChnName.Length);
				var allianceRaid = DataManager.InstanceContentResults.Values
					.Where(i => !string.IsNullOrWhiteSpace(i.CurrentLocaleName))
					.Where(i => new[] { 14, 15, 18, 36 }.Contains(i.ContentUICategory))
					.OrderBy(res => res.RequiredClassJobLevel);

				switch (BotBase.SelectedInstanceContentType)
				{
					case InstanceContentType.Dungeon:
						current = dungeon;
						break;
					case InstanceContentType.GoldSaucer:
						current = goldSaucer;
						break;
					case InstanceContentType.Roulette:
						current = roulette;
						break;
					case InstanceContentType.Trial:
						current = trial;
						break;
					case InstanceContentType.Raid:
						current = raid;
						break;
					case InstanceContentType.AllianceRaid:
						current = allianceRaid;
						break;
					case InstanceContentType.PVP:
						current = pvp;
						break;
					case InstanceContentType.GuildHests:
						current = guildhests;
						break;
					default:
						throw new ArgumentException();
				}

				var ret = current.Select(i => new InstanceModel {Id = i.Id, Name = i.CurrentLocaleName}).ToList();
				//BotBase.DutyToRegister = ret[0];
				return ret;
			}
		}

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
					BotBase.Instance.AnimationSpeed = 999999;
				});
			}
		}

		public ICommand CheckAll
		{
			get
			{
				return new RelayCommand(s =>
				{
					BotBase.Instance.EnableHealInCombat = true;
					BotBase.Instance.EnableHealOutofCombat = true;
					BotBase.Instance.EnablePreCombatBuff = true;
					BotBase.Instance.EnablePullBuff = true;
					BotBase.Instance.EnablePull = true;
					BotBase.Instance.EnableCombatBuff = true;
					BotBase.Instance.EnableCombat = true;
					BotBase.Instance.EnableDeath = true;
					BotBase.Instance.EnableRest = true;
				});
			}
		}

		public ICommand UnCheckAll
		{
			get
			{
				return new RelayCommand(s =>
				{
					BotBase.Instance.EnableHealInCombat = false;
					BotBase.Instance.EnableHealOutofCombat = false;
					BotBase.Instance.EnablePreCombatBuff = false;
					BotBase.Instance.EnablePullBuff = false;
					BotBase.Instance.EnablePull = false;
					BotBase.Instance.EnableCombatBuff = false;
					BotBase.Instance.EnableCombat = false;
					BotBase.Instance.EnableDeath = false;
					BotBase.Instance.EnableRest = false;
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

					OverlayManager.StatusOverlay.Update();
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