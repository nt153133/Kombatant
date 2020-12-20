using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Directors;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing;
using Kombatant.Enums;
using Kombatant.Extensions;
using Kombatant.Helpers;
using Kombatant.Interfaces;
using Kombatant.Settings;
using static Kombatant.Settings.BotBase;
using Action = Kombatant.Constants.Action;
using GameObject = ff14bot.Objects.GameObject;

namespace Kombatant.Logic
{
    /// <summary>
    /// Logic for autonomous movement functions.
    /// </summary>
    /// <inheritdoc cref="M:Komabatant.Interfaces.LogicExecutor"/>
    internal class Movement : LogicExecutor
    {
        #region Singleton

        private static Movement _movement;
        private bool _isFighting;
        internal static Movement Instance => _movement ?? (_movement = new Movement());

        #endregion

        //private const float MaxDistance = 3.5f;
        //private const float MoveToDistance = 3f;

        /// <summary>
        /// Main task executor for the Movement logic.
        /// </summary>
        /// <returns>Returns <c>true</c> if any action was executed, otherwise <c>false</c>.</returns>
        internal new async Task<bool> ExecuteLogic()
        {
            if (BotBase.Instance.IsPaused)
                return await Task.FromResult(false);

            var result = false;

            if (ShouldExecuteAutoMovement())
            {
                if (AvoidanceManager.IsRunningOutOfAvoid)
                    return await Task.FromResult(true);

                switch (BotBase.Instance.FollowMode)
                {
                    case FollowMode.None:
                        break;

                    case FollowMode.PartyLeader:
                        result = await FollowPartyLeader();
                        break;

                    case FollowMode.FixedCharacter:
                        result = await FollowFixedCharacter();
                        break;

                    case FollowMode.Tank:
                        result = await FollowTank();
                        break;

                    case FollowMode.TargetedCharacter:
                        result = await FollowTarget();
                        break;
                }
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Determines whether or not the botbase is allowed to execute movement/follow logics.
        /// </summary>
        /// <returns></returns>
        private bool ShouldExecuteAutoMovement()
        {
	        if (!BotBase.Instance.EnableFollowing) return false;
	        if (DirectorManager.ActiveDirector is InstanceContentDirector instanceContentDirector && instanceContentDirector.InstanceEnded) return false;
	        return !PauseFollowBecauseBoss();

	        // Pause following because player is fighting a known boss?
        }

        /// <summary>
        /// Core mechanics for the follow logic that are the same across all currently implemented FollowModes.
        /// </summary>
        /// <param name="characterToFollow"></param>
        /// <returns></returns>
        private async Task<bool> PerformFollowLogic(BattleCharacter characterToFollow)
        {
            // Sprint when the leader sprints
            if (PerformAutoSprint(characterToFollow))
                return await Task.FromResult(true);

            // Automatically mount/dismount
            if (await PerformMountDismount(characterToFollow))
                return await Task.FromResult(true);

            //// Can't do mount stuff when I am under attack, you silly carbuncle!
            //if (Core.Me.InCombat && !Core.Me.IsMounted)
            //    return await Task.FromResult(false);

            if (await PerformFlightTakeOff(characterToFollow))
                return await Task.FromResult(true);

            // Party leader too far away, auto move closer to them.
            if (characterToFollow.Distance2D() > BotBase.Instance.FollowDistance + 0.5f)
                if (await PerformNavigation(characterToFollow))
                    return await Task.FromResult(true);

            Navigator.PlayerMover.MoveStop();

            return await Task.FromResult(false);
        }

        /// <summary>
        /// Follow Mode: Follow Fixed Character
        /// </summary>
        /// <returns></returns>
        private async Task<bool> FollowFixedCharacter()
        {
            if (string.IsNullOrWhiteSpace(BotBase.Instance.FixedCharacterName)) return await Task.FromResult(false);

            var following = GameObjectManager.GetObjectByObjectId(BotBase.Instance.FixedCharacterId) 
                            ?? GameObjectManager.GameObjects.FirstOrDefault(obj => obj.Name == BotBase.Instance.FixedCharacterName && obj.Type == BotBase.Instance.FixedCharacterType) 
                            ?? GameObjectManager.GameObjects.FirstOrDefault(obj => obj.Name == BotBase.Instance.FixedCharacterName);

            // Character not found?
            if (following.GetBattleCharacter() == null)
                return await Task.FromResult(false);

            return await PerformFollowLogic(following.GetBattleCharacter());
        }

        /// <summary>
        /// Follow Mode: Follow Party Leader
        /// </summary>
        /// <returns></returns>
        private async Task<bool> FollowPartyLeader()
        {
            if (!Core.Me.IsInMyParty())
                return await Task.FromResult(false);

            if (Core.Me.IsPartyLeader())
                return await Task.FromResult(false);

            if (!PartyManager.PartyLeader.IsInObjectManager)
                return await Task.FromResult(false);

            var partyLeader = PartyManager.PartyLeader.BattleCharacter;

            return await PerformFollowLogic(partyLeader);
        }

        /// <summary>
        /// Follow Mode: Follow Tank
        /// </summary>
        /// <returns></returns>
        private async Task<bool> FollowTank()
        {
            if (!Core.Me.IsInMyParty())
                return await Task.FromResult(false);

            var tankToFollow = PartyManager.VisibleMembers
                .FirstOrDefault(member => member.IsTank())?.BattleCharacter;

            if (tankToFollow == null)
                return await Task.FromResult(false);

            return await PerformFollowLogic(tankToFollow);
        }

        /// <summary>
        /// Follow Mode: Follow Target
        /// </summary>
        /// <returns></returns>
        private async Task<bool> FollowTarget()
        {
            var target = Core.Target.GetBattleCharacter();
            if (target == null)
                return await Task.FromResult(false);

            return await PerformFollowLogic(target);
        }

        /// <summary>
        /// Determines if the character we are watching has started flying
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task<bool> PerformFlightTakeOff(BattleCharacter obj)
        {
            if (!MovementManager.IsFlying && (obj.IsMounted && obj.Location.IsOverGround(5) || obj.Location.Y > Core.Me.Location.Y + 5))
            {
                await CommonTasks.TakeOff();
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        /// <summary>
        /// Tries to navigate to a given GameObject with the selected navigation mode.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private async Task<bool> PerformNavigation(GameObject obj)
        {
            if (!BotBase.Instance.UseNavGraph)
            {
                Navigator.PlayerMover.MoveTowards(obj.Location);
                return await Task.FromResult(true);
            }

            if (BotBase.Instance.UseNavGraph)
            {
                if (!MovementManager.IsFlying && !MovementManager.IsDiving)
                    await CommonBehaviors.MoveAndStop(
                        r => obj.Location, r => BotBase.Instance.FollowDistance, true,
                        "Following selected target")
                        .ExecuteCoroutine();
                else
                    Flightor.MoveTo(obj.Location);

                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        /// <summary>
        /// Performs auto sprint if necessary.
        /// </summary>
        /// <param name="characterToWatch"></param>
        /// <returns></returns>
        private bool PerformAutoSprint(BattleCharacter characterToWatch)
        {
            if (characterToWatch == null) return false;
            if (characterToWatch.HasAura(Constants.Aura.Sprint) && ActionManager.IsSprintReady)
            {
                LogHelper.Instance.Log("[{0}] Sprinting...", CallStackHelper.Instance.GetCaller());
                ActionManager.Sprint();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Performs automatic mounting/dismounting.
        /// </summary>
        /// <param name="characterToWatch"></param>
        /// <returns></returns>
        private async Task<bool> PerformMountDismount(BattleCharacter characterToWatch)
        {
            if (characterToWatch.IsMounted != Core.Me.IsMounted || characterToWatch.IsCasting && characterToWatch.CastingSpellId == Action.Mount)
            {
                if (!Core.Me.InCombat)
                {
                    // Mount up!
                    if (characterToWatch.IsMounted || characterToWatch.CastingSpellId == Action.Mount)
                    {
                        LogHelper.Instance.Log(
                            "[{0}] Mounting...",
                            CallStackHelper.Instance.GetCaller());
                        await CommonTasks.MountUp();
                        return await Task.FromResult(true);
                    }
                }

                // Dismount - but only when close to the leader!
                if (characterToWatch.Distance2D() <= BotBase.Instance.FollowDistance + 0.5f)
                {
                    LogHelper.Instance.Log(
                        "[{2}] Dismounting, {0} <= {1}...",
                        characterToWatch.Distance2D(), BotBase.Instance.FollowDistance + 0.5f, CallStackHelper.Instance.GetCaller());
                    await CommonTasks.StopAndDismount();
                    return await Task.FromResult(true);
                }
            }

            return await Task.FromResult(false);
        }
        /// <summary>
        /// Determines whether or not the user wants to disable avoidance during bossfights.
        /// </summary>
        /// <returns></returns>
        private bool PauseFollowBecauseBoss()
        {
	        if (BotBase.Instance.PauseFollowingOnBosses &&
	            GameObjectManager.Attackers.Any(attacker => attacker.IsBoss()))
	        {
		        // Fight started
		        if (!_isFighting)
		        {
			        var bossMonster = GameObjectManager.Attackers.FirstOrDefault(attacker => attacker.IsBoss());
			        var toastMsg = string.Format("Fighting {0}!\tFollow will PAUSE", bossMonster?.Name);
			        LogHelper.Instance.Log("Fighting known boss. Follow will temporarily PAUSE.");
			        OverlayHelper.Instance.AddToast(toastMsg, Colors.Coral, Colors.Chocolate, new TimeSpan(0, 0, 0, 5));
		        }

		        _isFighting = true;
		        return true;
	        }

	        // Fight ended
	        if (_isFighting)
	        {
		        LogHelper.Instance.Log("Fight is over. Follow will RESUME.");
		        OverlayHelper.Instance.AddToast("Fight is over. Follow will RESUME.", Colors.LimeGreen, Colors.DarkGreen, new TimeSpan(0, 0, 0, 5));
		        _isFighting = false;
	        }

	        return false;
        }
    }
}