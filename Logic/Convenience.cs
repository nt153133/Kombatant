using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
//using DungeonMaster.Managers;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using ff14bot.RemoteAgents;
using ff14bot.RemoteWindows;
using Kombatant.Extensions;
using Kombatant.Forms;
using Kombatant.Helpers;
using Kombatant.Interfaces;
using Kombatant.Resources;
using Kombatant.Settings;
using static Kombatant.Forms.StatusOverlayUiComponent;
using Color = System.Drawing.Color;
using GameObject = Kombatant.Constants.GameObject;

namespace Kombatant.Logic
{
    /// <summary>
    /// Logic for convenience functions like auto-sprint, skip cutscenes et al.
    /// </summary>
    /// <inheritdoc cref="M:Komabatant.Interfaces.LogicExecutor"/>
    internal class Convenience : LogicExecutor
    {
        #region Singleton

        private static Convenience _convenience;
        internal static Convenience Instance => _convenience ?? (_convenience = new Convenience());

        #endregion

        internal static RunningStatus CurrentStatus => BotManager.Current.Name == "Kombatant" && TreeRoot.IsRunning ? BotBase.Instance.IsPaused ? RunningStatus.Paused : RunningStatus.Running : RunningStatus.Stopped;
        private const string AutoEndActWaitName = @"Convenience.AutoEndActEncounters";
        private const string AutoEmoteWaitName = @"Convenience.AutoEmote";

        /// <summary>
        /// Loop store to determine whether the player/the party was previously in a fight or not.
        /// </summary>
        private bool _wasFighting;

        /// <summary>
        /// Main task executor for the convenience logic.
        /// </summary>
        /// <returns>Returns <c>true</c> if any action was executed, otherwise <c>false</c>.</returns>
        internal new async Task<bool> ExecuteLogic()
        {
            if (BotBase.Instance.UseStatusOverlay) OverlayManager.StatusOverlay.Update(CurrentStatus);

            // Do not execute this logic if the botbase is paused
            if (BotBase.Instance.IsPaused)
                return await Task.FromResult(false);

            if (BotBase.Instance.AutoAcceptRevive)
                if (ExecuteAutoAcceptRaise())
                    return await Task.FromResult(true);

            //if (BotBase.Instance.AutoAcceptTeleport)
            //    if (ExecuteAutoAcceptTeleport())
            //        return await Task.FromResult(true);

            // Auto end ACT encounters. Place before CanFight check to ensure it fires regardless!
            if (ShouldExecuteAutoEndActEncounters())
                ExecuteAutoEndActEncounters();

            // We can't do anything if we are dead...
            if (Core.Me.IsDead)
                return await Task.FromResult(false);

            if (BotBase.Instance.AutoSelectYes)
                if (ExecuteAutoSelectYes())
                    return await Task.FromResult(true);

            // Automatically keep up an emote
            if (ShouldExecuteAutoEmote())
                ExecuteAutoEmote();

            // Auto skip cutscenes
            if (BotBase.Instance.AutoSkipCutscenes && QuestLogManager.InCutscene)
                if (ExecuteSkipCutscene())
                    return await Task.FromResult(true);

            // Auto advance dialogue
            if (BotBase.Instance.AutoAdvanceDialogue)
                if (ExecuteAutoAdvanceDialogue())
                    return await Task.FromResult(true);

            // Auto accept/complete quests
            if (BotBase.Instance.AutoAcceptQuests)
                if (ExecuteAutoAcceptQuests())
                    return await Task.FromResult(true);

            // Auto sprint
            if (BotBase.Instance.AutoSprint)
                if (ExecuteAutoSprint())
                    return await Task.FromResult(true);

            // Auto sync FATE
            if (BotBase.Instance.AutoSyncFate)
                if (ExecuteAutoSyncFate())
                    return await Task.FromResult(true);

            if (BotBase.Instance.AutoTrade)
                if (ExecuteAutoTrade())
                    return await Task.FromResult(true);

            //if (BotBase.Instance.AutoAcceptRevive)
            //    if (ExecuteAutoLeaveDuty())
            //        return await Task.FromResult(true);
            if (BotBase.Instance.AutoHandoverRequestItems)
                return await ExecuteAutoHandoverRequestItems();


            return await Task.FromResult(false);
        }

        /// <summary>
        /// Determines whether the botbase should perform auto-emotes.
        /// </summary>
        /// <returns></returns>
        private bool ShouldExecuteAutoEmote()
        {
            return BotBase.Instance.AutoEmoteInterval > 0 &&
                   !string.IsNullOrEmpty(BotBase.Instance.AutoEmoteCommand.Trim());
        }

        /// <summary>
        /// Determines whether the botbase should automatically try to end ACT encounters.
        /// </summary>
        /// <returns></returns>
        private bool ShouldExecuteAutoEndActEncounters()
        {
            return BotBase.Instance.AutoEndActEncounters;
        }

        /// <summary>
        /// Auto accepts/completes quests.
        /// </summary>
        /// <returns></returns>
        private bool ExecuteAutoAcceptQuests()
        {
            // Auto accept quest
            if (JournalAccept.IsOpen)
            {
                JournalAccept.Accept();
                return true;
            }

            // Auto complete quest
            if (JournalResult.IsOpen)
            {
                //if (!JournalResult.ButtonClickable)
                //    JournalResult.SelectSlot(0);

                if (JournalResult.ButtonClickable)
                {
                    JournalResult.Complete();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Will automatically advance dialogue when possible.
        /// </summary>
        /// <returns></returns>
        private bool ExecuteAutoAdvanceDialogue()
        {
            // Auto progress text box
            if (Talk.DialogOpen)
            {
                Talk.Next();
                return true;
            }

            return false;
        }

        private bool ExecuteAutoLeaveDuty()
        {
            if (DutyManager.InInstance && DutyManager.CanLeaveActiveDuty)
            {
                var quitpoint = GameObjectManager.GetObjectByName("退出点");
                if (quitpoint != null && quitpoint.IsValid && quitpoint.IsVisible && quitpoint.IsTargetable && quitpoint.InLineOfSight())
                {
                    if (!GameObjectManager.GetObjectsOfType<Treasure>(true).Any(i=>i.IsValid && i.IsVisible && i.IsTargetable) && RaptureAtkUnitManager.GetWindowByName("_NotificationLoot") == null)
                    {
                        DutyManager.LeaveActiveDuty();
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Will automatically trigger an emote every x seconds.
        /// </summary>
        /// <returns></returns>
        private void ExecuteAutoEmote()
        {
            if(!WaitHelper.Instance.HasWait(AutoEmoteWaitName))
                WaitHelper.Instance.AddWait(AutoEmoteWaitName, new TimeSpan(0, 0, 0, BotBase.Instance.AutoEmoteInterval));

            if (WaitHelper.Instance.IsFinished(AutoEmoteWaitName))
            {
                ChatManager.SendChat(BotBase.Instance.AutoEmoteCommand);
                WaitHelper.Instance.AddWait(AutoEmoteWaitName, new TimeSpan(0, 0, 0, BotBase.Instance.AutoEmoteInterval));
            }
        }

        /// <summary>
        /// Will automatically end encounters in ACT when no enemies are nearby for a given amount of time.
        /// </summary>
        /// <returns></returns>
        private void ExecuteAutoEndActEncounters()
        {
            if(!WaitHelper.Instance.HasWait(AutoEndActWaitName))
                WaitHelper.Instance.AddWait(AutoEndActWaitName, new TimeSpan(0, 0, 0, 10));

            // Are we brawling?
            if (Core.Me.InCombat || PartyManager.VisibleMembers.Any(member => member.BattleCharacter.InCombat))
            {
                _wasFighting = true;
                WaitHelper.Instance.ResetWait(AutoEndActWaitName);
                return;
            }

            // Are we lazy?
            if(WaitHelper.Instance.IsFinished(AutoEndActWaitName))
            {
                // Only send the command when ACT is running, obviously...
                if(IsActRunning() && _wasFighting)
                {
                    _wasFighting = false;
                    ChatManager.SendChat(@"/echo end");
                }
            }
        }

        /// <summary>
        /// Will automatically sprint whenever available.
        /// </summary>
        /// <returns></returns>
        private bool ExecuteAutoSprint()
        {
            if(WorldManager.InPvP/* && DutyManager.InInstance*/)
            {
                return false;
            }

            if (!Core.Me.InCombat && !GameObjectManager.Attackers.Any() && MovementManager.IsMoving)
            {
                //if (ActionManager.CanCast(18942, null) && !Core.Me.HasAura("Sprint"))
                //{
                //    ActionManager.DoAction(18942, null);
                //    return true;
                //}
                if (ActionManager.IsSprintReady/* && !Core.Me.HasAura("Bolt")*/)
                {
                    ActionManager.Sprint();
                    return true;
                }

            }

            return false;
        }

        /// <summary>
        /// Will automatically sync to a FATE if selected mob is part of a FATE.
        /// </summary>
        /// <returns></returns>
        private bool ExecuteAutoSyncFate()
        {
            // Because RebornBuddy can be whimsical with it's Core.Target...
            var target = Core.Target as BattleCharacter;

            // We have a target and it is part of a FATE.
            if (target != null && target.IsCharacter() && target.IsFate)
            {
                var fate = FateManager.GetFateById(target.FateId);

                // Sync us down, Scotty!
                if(fate != null && fate.Within2D(Core.Me.Location) && Core.Me.ClassLevel > fate.MaxLevel)
                {
                    LogHelper.Instance.Log(Localization.Msg_AutoSyncFate, fate.Name);
                    Core.Me.SyncToFate();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Will skip cutscenes by trying to exit out of them or advance the dialogue.
        /// </summary>
        /// <returns></returns>
        private bool ExecuteSkipCutscene()
        {
            // Try to skip the cutscene
            AgentCutScene.Instance.PromptSkip();

            if (AgentCutScene.Instance.CanSkip && SelectString.IsOpen)
            {
                SelectString.ClickSlot(0);
                return true;
            }

            // If that is not an option, at least try to forward it as fast as possible...
            if (Talk.DialogOpen)
            {
                Talk.Next();
                return true;
            }

            return false;
        }

        private async Task<bool> ExecuteAutoHandoverRequestItems()
        {
            if (RaptureAtkUnitManager.GetWindowByName("ShopExchangeItemDialog") != null)
            {
                RaptureAtkUnitManager.GetWindowByName("ShopExchangeItemDialog").SendAction(1, 3, 0);
                LogHelper.Instance.Log("Click ShopExchangeItemDialog Yes");
                return true;
            }

            if (Request.IsOpen)
            {
                try
                {
                    if (await CommonTasks.HandOverRequestedItems(BotBase.Instance.HandoverHqIfnoNQ))
                    {
                        Request.HandOver();
                        LogHelper.Instance.Log("Handing over request items...");
                        return true;
                    }
                }
                catch 
                {
                    LogHelper.Instance.Log("We don't have the required amount of a requested item.");
                    return false;
                }
            }

            return false;
        }

        private bool ExecuteAutoAcceptRaise()
        {
            if (ClientGameUiRevive.ReviveState == ReviveState.Reviving)
            {
                ClientGameUiRevive.Revive();
                return true;
            }

            return false;
        }

        private bool ExecuteAutoAcceptTeleport()
        {
            if (SelectYesno.IsOpen &&
                (RaptureAtkUnitManager.GetWindowByName("SelectYesno").FindLabel(2).Text.Contains("传送邀请") ||
                 RaptureAtkUnitManager.GetWindowByName("SelectYesno").FindLabel(2).Text.StartsWith("确定要花费") && RaptureAtkUnitManager.GetWindowByName("SelectYesno").FindLabel(2).Text.Contains("传送到")))
            {
                SelectYesno.Yes();
                return true;
            }

            return false;
        }

        private bool ExecuteAutoSelectYes()
        {
            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
                return true;
            }

            return false;
        }

        private static bool TradeOpen => RaptureAtkUnitManager.GetWindowByName("Trade") != null;
        private static bool ContextMenuOpened => RaptureAtkUnitManager.GetWindowByName("ContextMenu") != null;
        private static bool HasValidTradeTarget => Core.Me.HasTarget && Core.Target is Character c && !c.IsMe && c.Type == GameObjectType.Pc && 
                                                   (!BotBase.Instance.AutoTradeFriendOnly || (c.StatusFlags & StatusFlags.Friend) != 0) && 
                                                   !DutyManager.InInstance && c.IsWithinInteractRange;

        private bool ExecuteAutoTrade()
        {
            if (Request.IsOpen && Request.HandOverButtonClickable)
            {
                Request.HandOver();
                return true;
            }

            if (TradeOpen)
            {
                if (HasValidTradeTarget)
                {
                    if (InputNumeric.IsOpen)
                    {
                        InputNumeric.Ok((uint) InputNumeric.Field.MaxValue);
                        return true;
                    }

                    if (ContextMenuOpened)
                    {
                        RaptureAtkUnitManager.GetWindowByName("ContextMenu").SendAction(3, 3, 0, 3, 0, 3, 1);
                        return true;
                    }

                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                        return true;
                    }
                }
                else
                {
                    if (Trade.TradeStage == 3)
                    {
                        RaptureAtkUnitManager.GetWindowByName("Trade").SendAction(1, 3, 0);
                        return true;
                    }

                    if (SelectYesno.IsOpen)
                    {
                        SelectYesno.Yes();
                        return true;
                    }
                }
            }
            else
            {
                if (HasValidTradeTarget && ContextMenuOpened)
                {
                    RaptureAtkUnitManager.GetWindowByName("ContextMenu").SendAction(3, 3, 0, 3, 2, 4, 0);
                    return true;
                }
            }

            return false;

            //if (DungeonMaster.Managers.LootManager.IsVisible && LootManager.AvailableItems.Any(i => i.RollState == RollState.NoLoot))
        //{
        //    foreach (var availableItem in LootManager.AvailableItems)
        //    {
        //        Logging.Write(LogLevel.Normal, System.Windows.Media.Color.FromRgb(40,200,220), "放弃了：" );
        //        Log(availableItem.Index + " " + availableItem.Item.ChnName + "State: " + availableItem.RollState + "ID: " + availableItem.RawId);
        //    }
        //    LootManager.AvailableItems.Where(i => i.RollState == RollState.NoLoot).ForEach(i => i.Roll(RollOptions.Pass));
        //    return true;
        //}
        //while (RaptureAtkUnitManager.GetWindowByName("_NotificationLoot") != null)
        //{
        //    await Coroutine.ExternalTask(ExecuteAutoAbandonLoot());
        //    AgentModule.GetAgentInterfaceById(35).Toggle();
        //    await Coroutine.Wait(5000, () => RaptureAtkUnitManager.GetWindowByName("NeedGreed") != null);
        //    RaptureAtkUnitManager.GetWindowByName("NeedGreed").SendAction(2, 3, 2, 0, 0);
        //    await Coroutine.Wait(5000, () => SelectYesno.IsOpen && RaptureAtkUnitManager.GetWindowByName("SelectYesno").FindLabel(2).Text.Contains("确定要放弃"));
        //    if (SelectYesno.IsOpen)
        //    {
        //        SelectYesno.Yes();
        //    }
        //    await Coroutine.Sleep(1000);
        //    RaptureAtkUnitManager.GetWindowByName("NeedGreed").SendAction(1, 3, uint.MaxValue);
        //    await Coroutine.Wait(5000, () => RaptureAtkUnitManager.GetWindowByName("NeedGreed") == null);
        //}
        }

        /// <summary>
        /// Checks whether the Advanced Combat Tracker process can be found in the running task list.
        /// </summary>
        /// <returns></returns>
        private bool IsActRunning()
        {
            return Process.GetProcessesByName(@"Advanced Combat Tracker").Any();
        }
    }
}