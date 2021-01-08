using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Documents;
using Buddy.Coroutines;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Directors;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.NeoProfiles;
using ff14bot.Objects;
using ff14bot.RemoteWindows;
using Kombatant.Constants;
using Kombatant.Extensions;
using Kombatant.Helpers;
using Kombatant.Interfaces;
using Kombatant.Settings;
using TreeSharp;
using Action = TreeSharp.Action;

namespace Kombatant.Logic
{
	/// <summary>
	/// Logic for automatically commencing duties.
	/// </summary>
	/// <inheritdoc cref="M:Komabatant.Interfaces.LogicExecutor"/>
	internal class CommenceDuty : LogicExecutor
	{
		#region Singleton

		private static CommenceDuty _commenceDuty;
		internal static CommenceDuty Instance => _commenceDuty ?? (_commenceDuty = new CommenceDuty());

		#endregion

		private IEnumerable<DictionaryEntry> _psycheList;
		private IEnumerable<string> _audioFiles;
		private readonly Random _random = new Random();

		/// <summary>
		/// Constructor for CommenceDuty.
		/// </summary>
		private CommenceDuty()
		{
			PopulatePsyches();
			PopulateSounds();
		}

		/// <summary>
		/// Main task executor for the Commence Duty logic.
		/// </summary>
		/// <returns>Returns <c>true</c> if any action was executed, otherwise <c>false</c>.</returns>
		internal new async Task<bool> ExecuteLogic()
		{
			// Do not execute this logic if the botbase is paused
			if (BotBase.Instance.IsPaused)
				return await Task.FromResult(false);

			if (ShouldVoteMvp())
			{
				if (await VoteMvp().ExecuteCoroutine())
					return await Task.FromResult(true);
			}

			if (ShouldLeaveDuty())
			{
				LogHelper.Instance.Log("Leaving Duty...");
				DutyManager.LeaveActiveDuty();
				return await Task.FromResult(true);
			}

			if (ShouldRegisterDuties())
			{
				try
				{
					DutyManager.Queue(new InstanceContentResult
					{
						Id = BotBase.Instance.DutyToRegister.Id,
						IsInDutyFinder = true,
						ChnName = BotBase.Instance.DutyToRegister.Name,
						EngName = BotBase.Instance.DutyToRegister.Name
					});
					LogHelper.Instance.Log($"Queued duty {BotBase.Instance.DutyToRegister.Name}");
				}
				catch (ArgumentException e)
				{
					LogHelper.Instance.Log(e.Message);
				}
				catch (NullReferenceException e)
				{
					LogHelper.Instance.Log("Please select a duty to register!");
				}
				//catch (NullReferenceException e)
				//{
				//	LogHelper.Instance.Log();
				//}
				return await Task.FromResult(true);
			}

			// Play Duty notification sound
			if (ShouldPlayDutyReadySound())
			{
				ShowLogNotification();
				PlayNotificationSound();
				return await Task.FromResult(true);
			}

			// Auto accept Duty Finder
			if (ShouldAcceptDutyFinder())
			{
				LogHelper.Instance.Log(Localization.Localization.Msg_DutyConfirm);
				ContentsFinderConfirm.Commence();
				WaitHelper.Instance.RemoveWait(@"CommenceDuty.DutyNotificationSound");
				return await Task.FromResult(true);
			}

			return await Task.FromResult(false);
		}

		//private InstanceContentResult[] DutiesToRegister
		//{
		//	get
		//	{
		//		if (string.IsNullOrWhiteSpace(BotBase.Instance.DutyIDsToRegister))
		//		{
		//			return new InstanceContentResult[] { };
		//		}
		//		try
		//		{
		//			string[] idStrings = BotBase.Instance.DutyIDsToRegister.Split(',');
		//			uint[] idsUints = new uint[idStrings.Length];
		//			for (int i = 0; i < idStrings.Length; i++)
		//			{
		//				idsUints[i] = uint.Parse(idStrings[i]);
		//			}

		//			try
		//			{
		//				var duties = DataManager.InstanceContentResults.Values
		//					.Where(i => i.IsInDutyFinder && idsUints.Contains(i.Id)).ToArray();
		//				if (duties.Length != idsUints.Length)
		//				{
		//					throw new Exception();
		//				}

		//				return duties;
		//			}
		//			catch (Exception e)
		//			{
		//				LogHelper.Instance.Log("任务搜索器中没有你要申请的全部副本。");
		//				//LogHelper.Instance.Log(e);
		//			}
		//		}
		//		catch (Exception e)
		//		{
		//			LogHelper.Instance.Log("输入副本ID的格式有误。");
		//			//LogHelper.Instance.Log(e);
		//		}

		//		return new InstanceContentResult[] { };
		//	}
		//}

		private bool ShouldLeaveDuty()
		{
			if (!BotBase.Instance.AutoLeaveDuty)
				return false;
			if (!DutyManager.InInstance)
				return false;
			if (!DutyManager.CanLeaveActiveDuty)
				return false;
			if (DirectorManager.ActiveDirector is InstanceContentDirector icDirector && icDirector.InstanceEnded)
			{
				return BotBase.Instance.SecondsToAutoLeaveDuty == 0 || WaitHelper.Instance.IsDoneWaiting(
					@"CommenceDuty.AutoLeaveDuty", new TimeSpan(0, 0, BotBase.Instance.SecondsToAutoLeaveDuty));
			}

			return false;
		}

		private bool ShouldRegisterDuties()
		{
			if (!Core.IsInGame) return false;
			if (CommonBehaviors.IsLoading) return false;
			if (!BotBase.Instance.AutoRegisterDuties) return false;
			if (DutyManager.QueueState != QueueState.None) return false;
			return WaitHelper.Instance.IsDoneWaiting(@"CommenceDuty.AutoRegister", new TimeSpan(0, 0, 5), true);
		}

		/// <summary>
		/// Determines whether we should play a sassy sound file when the Duty Finder pops.
		/// </summary>
		/// <returns></returns>
		private bool ShouldPlayDutyReadySound()
		{
			//if (!BotBase.Instance.AutoAcceptDutyFinder)
			//	return false;
			if (!BotBase.Instance.DutyFinderNotify)
				return false;
			if (ContentsFinderReady.IsOpen)
				return false;
			return ContentsFinderConfirm.IsOpen && WaitHelper.Instance.IsDoneWaiting(@"CommenceDuty.DutyNotificationSound", TimeSpan.FromSeconds(45), true);
		}

		/// <summary>
		/// Determines whether a duty should automatically be commended.
		/// </summary>
		/// <returns></returns>
		private bool ShouldAcceptDutyFinder()
		{
			if (!BotBase.Instance.AutoAcceptDutyFinder) return false;
			if (ContentsFinderReady.IsOpen) return false;
			if (Core.Me.IsDead) return false;
			if (!ContentsFinderConfirm.IsOpen) return false;
			return WaitHelper.Instance.IsDoneWaiting(@"CommenceDuty.AutoAcceptDutyFinder", TimeSpan.FromSeconds(BotBase.Instance.DutyFinderWaitTime));
		}

		/// <summary>
		/// Populates the list of available /psyches.
		/// </summary>
		private void PopulatePsyches()
		{
			ResourceSet resourceSet = Localization.Localization.ResourceManager
				.GetResourceSet(CultureInfo.CurrentCulture, true, true);

			_psycheList = resourceSet.Cast<DictionaryEntry>()
				.Where(psyche => psyche.Key.ToString().StartsWith(@"Msg_DutyPsyche"));
		}

		/// <summary>
		/// Populates the list of possible notification sound files.
		/// </summary>
		private void PopulateSounds()
		{
			_audioFiles = Directory.EnumerateFiles(Path.Combine(BotManager.BotBaseDirectory, @"Kombatant", @"Resources", @"Audio"), @"*.wav");
		}

		/// <summary>
		/// Prints an entry into RebornBuddy's log indicating that the Duty is ready.
		/// </summary>
		private void ShowLogNotification()
		{
			// ReSharper disable once RedundantAssignment
			var psyche = _psycheList.ElementAt(_random.Next(_psycheList.Count())).Value.ToString();
			LogHelper.Instance.Log($@"{Localization.Localization.Msg_DutyReady} {psyche}");
		}

		/// <summary>
		/// Plays one of the available notification sounds.
		/// No kekeke though, because it scares the carbuncle.
		/// </summary>
		private void PlayNotificationSound()
		{
			if (_audioFiles.Any())
				new SoundPlayer(_audioFiles.ElementAt(_random.Next(_audioFiles.Count()))).Play();
		}

		bool ShouldVoteMvp()
		{
			if (!BotBase.Instance.AutoVoteMvp) return false;
			if (PartyManager.NumMembers == 1) return false;
			if (Memory.Offsets.Instance.AgentMvpId == 0 || Memory.Offsets.Instance.AgentNotificationId == 0) return false;
			return DirectorManager.ActiveDirector is InstanceContentDirector icDirector && icDirector.InstanceEnded;
		}

		private uint VoteWho
		{
			get
			{
				switch (PartyManager.NumMembers)
				{
					case 4:
						{
							if (Core.Me.IsTank()) return 0;
							if (Core.Me.IsHealer()) return 0;
							if (Core.Me.IsMeleeDps() || Core.Me.IsRangedDps()) return 2;
							break;
						}
					case 8:
						{
							if (Core.Me.IsTank()) return 0;
							if (Core.Me.IsHealer()) return 2;
							if (Core.Me.IsMeleeDps() || Core.Me.IsRangedDps()) return (uint)new Random().Next(3, 6);
							break;
						}
				}

				return (uint)new Random().Next(0, (int)PartyManager.NumMembers);
			}
		}

		private Composite VoteMvp()
		{
			Composite c = new Sequence(
				//new Action(context => LogHelper.Instance.Log($"Starting sequence! waiting for _NotificationIcMvp open")),
				new ActionRunCoroutine(o => Coroutine.Wait(500, () => RaptureAtkUnitManager.GetWindowByName("_NotificationIcMvp") != null)),
				//new Action(context => LogHelper.Instance.Log($" _NotificationIcMvp opened")),
				new Action(context =>
				{
					//LogHelper.Instance.Log($"Toggling agent {RaptureAtkUnitManager.GetWindowByName("_NotificationIcMvp").TryFindAgentInterface()}...");
					//RaptureAtkUnitManager.GetWindowByName("_NotificationIcMvp").TryFindAgentInterface().Toggle();
					AgentModule.ToggleAgentInterfaceById(Memory.Offsets.Instance.AgentNotificationId);
					//LogHelper.Instance.Log($"Toggling agent {59}...");
					AgentModule.ToggleAgentInterfaceById(Memory.Offsets.Instance.AgentMvpId);
					//LogHelper.Instance.Log($"Toggling agent {120}...");
				}),
				//new Action(context => LogHelper.Instance.Log("Waiting for VoteMvp to open...")),
				new ActionRunCoroutine(o => Coroutine.Wait(3000, () => RaptureAtkUnitManager.GetWindowByName("VoteMvp") != null)),
				new Action(context => LogHelper.Instance.Log("VoteMvp opened.")),
				new Action(context =>
				{
					RaptureAtkUnitManager.GetWindowByName("VoteMvp").SendAction(2, 3, 0, 3, VoteWho);
					LogHelper.Instance.Log($"Voted player [{VoteWho + 1}]!");
				}));

			return c;

			//if (DirectorManager.ActiveDirector is InstanceContentDirector icDirector && icDirector.InstanceEnded)
			//{
			//	LogHelper.Instance.Log($"Instance ended... Wait for PlayerRecommendation window to appear");
			//	if (await Coroutine.Wait(3000, () => NotificationMvp != null))
			//	{
			//		LogHelper.Instance.Log($"Toggling agent {NotificationMvp.TryFindAgentInterface()}...");
			//		NotificationMvp.TryFindAgentInterface().Toggle();
			//		LogHelper.Instance.Log($"Toggling agent {59}...");
			//		AgentModule.ToggleAgentInterfaceById(59);
			//		LogHelper.Instance.Log($"Toggling agent {120}...");
			//		AgentModule.ToggleAgentInterfaceById(120);
			//		LogHelper.Instance.Log("Waiting -1 for VoteMvp to open...");

			//		await Coroutine.Wait(-1, () => VoteMvp != null);
			//		LogHelper.Instance.Log("VoteMvp opened.");
			//		VoteMvp.SendAction(2, 3, 0, 3, VoteWho);
			//		LogHelper.Instance.Log($"voted player [{VoteWho}]!");
			//		return true;
			//	}
			//}

			//return false;




		}
	}
}