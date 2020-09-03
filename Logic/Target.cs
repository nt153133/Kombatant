using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xaml;
using Clio.Utilities;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Objects;
using Kombatant.Enums;
using Kombatant.Extensions;
using Kombatant.Forms;
using Kombatant.Helpers;
using Kombatant.Interfaces;

namespace Kombatant.Logic
{
    /// <summary>
    /// Logic for automatically targeting enemies.
    /// </summary>
    /// <inheritdoc cref="M:Komabatant.Interfaces.LogicExecutor"/>
    internal class Target : LogicExecutor
    {
        #region Singleton

        private static Target _target;
        internal static Target Instance => _target ?? (_target = new Target());

        #endregion

        /// <summary>
        /// Main task executor for the targeting logic.
        /// </summary>
        /// <returns>Returns <c>true</c> if any action was executed, otherwise <c>false</c>.</returns>
        internal new async Task<bool> ExecuteLogic()
        {
            //invoking UI thread to refresh overlay
            if (Settings.BotBase.Instance.UseFocusOverlay)
                OverlayManager.FocusOverlay.Update();

            // Do not execute this logic if the botbase is paused.
            if (Settings.BotBase.Instance.IsPaused || IsTraveling())
                return await Task.FromResult(false);

            //if (WorldManager.InPvP && !Settings.BotBase.Instance.ManuallyTargetDistance)
            //{
            //    switch (Core.Me.CurrentJob)
            //    {
            //        //case ClassJobType.Paladin:
            //        //    break;
            //        //case ClassJobType.Monk:
            //        //    break;
            //        //case ClassJobType.Warrior:
            //        //    break;
            //        case ClassJobType.Dragoon:
            //            if (Settings.BotBase.Instance.TargetScanMaxDistance == 20) break;
            //            Settings.BotBase.Instance.TargetScanMaxDistance = 20;
            //            break;

            //        case ClassJobType.Bard:
            //            if (Settings.BotBase.Instance.TargetScanMaxDistance == 25) break;
            //            Settings.BotBase.Instance.TargetScanMaxDistance = 25;
            //            break;
            //        //case ClassJobType.WhiteMage:
            //        //    break;
            //        case ClassJobType.BlackMage:
            //            if (Settings.BotBase.Instance.TargetScanMaxDistance == 25) break;
            //            Settings.BotBase.Instance.TargetScanMaxDistance = 25;
            //            break;
            //        case ClassJobType.Summoner:
            //            if (Settings.BotBase.Instance.TargetScanMaxDistance == 25) break;
            //            Settings.BotBase.Instance.TargetScanMaxDistance = 25;
            //            break;
            //        //case ClassJobType.Scholar:
            //        //    break;
            //        case ClassJobType.Ninja:
            //            if (Core.Me.HasAura(1317) || ActionResourceManager.Ninja.HutonTimer == TimeSpan.Zero)
            //            {
            //                if (Settings.BotBase.Instance.TargetScanMaxDistance == 25) break;
            //                Settings.BotBase.Instance.TargetScanMaxDistance = 25;
            //                break;
            //            }
            //            if (Settings.BotBase.Instance.TargetScanMaxDistance == 15) break;
            //            Settings.BotBase.Instance.TargetScanMaxDistance = 15;
            //            break;
            //        case ClassJobType.Machinist:
            //            if (Settings.BotBase.Instance.TargetScanMaxDistance == 25) break;
            //            Settings.BotBase.Instance.TargetScanMaxDistance = 25;
            //            break;
            //        //case ClassJobType.DarkKnight:
            //        //    break;
            //        //case ClassJobType.Astrologian:
            //        //    break;
            //        //case ClassJobType.Samurai:
            //        //    break;
            //        //case ClassJobType.RedMage:
            //        //    break;
            //        //case ClassJobType.BlueMage:
            //        //    break;
            //        //case ClassJobType.Gunbreaker:
            //        //    break;
            //        case ClassJobType.Dancer:
            //            Settings.BotBase.Instance.TargetScanMaxDistance = 25;
            //            break;
            //    }
            //}

            // Auto face target
            if (ShouldExecuteAutoFaceTarget())
            {
                GameSettingsManager.FaceTargetOnAction = Settings.BotBase.Instance.AutoFaceTarget;
                return await Task.FromResult(true);
            }

            // Try to determine current target. Possibly a target chosen by the prior mechanisms.
            var currentTarget = Core.Me.CurrentTarget as BattleCharacter;
            var potentialTarget = null as BattleCharacter;

            // Automatically select a target if we do not have one. Uses one of the many colourful selection modes!
            if (ShouldExecuteAutoTarget())
            {
                switch (Settings.BotBase.Instance.AutoTargetingMode)
                {
                    case TargetingMode.None:
                        break;

                    case TargetingMode.BestAoE:
                        potentialTarget = TargetBestAoeEnemy();
                        break;

                    case TargetingMode.OnlyWhitelisted:
                        potentialTarget = TargetOnlyWhitelistedEnemy();
                        break;

                    case TargetingMode.Nearest:
                        potentialTarget = TargetNearestEnemy();
                        break;

                    case TargetingMode.LowestHealth:
                        potentialTarget = WorldManager.InPvP ? PVPLowestHpEnemy() : TargetLowestHpEnemy();
                        break;

                    case TargetingMode.LowestHealthPercent:
                        potentialTarget = WorldManager.InPvP ? PVPLowestHpPercentEnemy() : TargetLowestHpPercentEnemy();
                        break;

                    case TargetingMode.HighestHealth:
                        potentialTarget = TargetHighestHpEnemy();
                        break;

                    case TargetingMode.HighestHealthPercent:
                        potentialTarget = TargetHighestHpPercentEnemy();
                        break;

                    case TargetingMode.AssistTank:
                        potentialTarget = TargetAssistTank();
                        break;

                    case TargetingMode.AssistLeader:
                        potentialTarget = TargetAssistLeader();
                        break;

                    case TargetingMode.AssistFixedCharacter:
                        potentialTarget = TargetAssistFixedCharacter();
                        break;

                    case TargetingMode.AssistHighestLvl:
                        potentialTarget = TargetAssistHighestLvlCharacter();
                        break;

                    case TargetingMode.MostFocused:
                        potentialTarget = TargetMostFocusedEnemy();
                        break;

                }

                // Player target differs from chosen target or is a FATE mob?
                if (potentialTarget != null && potentialTarget.CheckAliveAndValid() && potentialTarget.InLineOfSight() && currentTarget != potentialTarget)
                {
                    LogHelper.Instance.Log(Resources.Localization.Msg_NewTargetSelected,
                        potentialTarget.Name,
                        $@"0x{potentialTarget.ObjectId:X8}");

                    potentialTarget.Target();
                    if (Settings.BotBase.Instance.MarkTarget)
                    {
                        ChatManager.SendChat(@"/marking bind1 <t>");
                    }
                    return await Task.FromResult(true);
                }
            }

            // No target? Then the following checks would return false anyway...
            if (currentTarget == null)
                return await Task.FromResult(false);

            // Target not in line of sight?
            if (!currentTarget.InLineOfSight())
                return await Task.FromResult(true);

            // Target is invincible?
            if (currentTarget.IsInvincible())
                return await Task.FromResult(true);

            //// Make sure we don't pull stuff as a non-tank when in a party and smart pull is enabled.
            //if (!IsAllowedToFight(currentTarget))
            //    return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        /// <summary>
        /// Determines whether the combat routine is allowed to pull/do combat with the current target.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool IsAllowedToFight(BattleCharacter target)
        {
            // Only limit when we have Smart Pull enabled, are not a tank ourselves and there is a tank nearby!
            if (Settings.BotBase.Instance.EnableSmartPull /*&& !Core.Me.IsTank() && PartyManager.VisibleMembers.Any(member => member.IsTank())*/)
                return /*Core.Me.IsInMyParty() && */target.IsEnemy() && (GameObjectManager.Attackers.Contains(target) || target.HasBeenTaggedByPartyMember());

            return true;
        }

        /// <summary>
        /// Helper method to determine whether a given GameObject is a valid target
        /// for the target strategy "Whitelist only".
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool IsValidWhitelistTarget(GameObject obj)
        {
            return Settings.BotBase.Instance.TargetWhitelist.Any(whiteListEntry => whiteListEntry.NpcId == obj.NpcId) ||
                   GameObjectManager.Attackers.Contains(obj);
        }

        /// <summary>
        /// Is the player swimming, flying, moving or in any other way occupied?
        /// </summary>
        /// <returns></returns>
        private bool IsTraveling()
        {
            return MovementManager.IsSwimming || MovementManager.IsFlying || /*MovementManager.IsMoving*/ Core.Me.IsMounted ||
                   MovementManager.IsOccupied;
        }

        /// <summary>
        /// Determines whether auto facing a given target should be executed.
        /// </summary>
        /// <returns></returns>
        private bool ShouldExecuteAutoFaceTarget()
        {
            return GameSettingsManager.FaceTargetOnAction != Settings.BotBase.Instance.AutoFaceTarget
                   && WaitHelper.Instance.IsDoneWaiting(@"Target.AutoFace");
        }

        /// <summary>
        /// Determines whether we should select a new target, taking into account the current state and settings.
        /// The order in which these conditions are checked is important, so please be careful!
        /// </summary>
        /// <returns></returns>
        private bool ShouldExecuteAutoTarget()
        {
            // Target search disabled
            if (!Settings.BotBase.Instance.AutoTarget || Settings.BotBase.Instance.AutoTargetingMode == TargetingMode.None)
                return false;

            if (!Core.Target.CheckAliveAndValid())
            {
                return true;
            }

            //auto deselect when target is unreachable
            if (Settings.BotBase.Instance.AutoDeSelectTarget && Core.Me.HasTarget && Core.Target.IsBattleCharacter() &&
                (Core.Target.GetBattleCharacter().IsInvincible() ||
                 Core.Target.Distance2D() - Core.Me.CombatReach - Core.Target.CombatReach > Settings.BotBase.Instance.TargetScanMaxDistance ||
                 Settings.BotBase.Instance.EnableLosCheck && !Core.Target.InLineOfSight()))
            {
                Core.Me.ClearTarget();
                return true;
            }

            //don't change target when target has specific aura
            if (Core.Target.IsBattleCharacter() &&
                (Core.Me.CurrentTarget.GetBattleCharacter().HasMyAura(1323) ||
                 Core.Me.CurrentTarget.GetBattleCharacter().HasMyAura(1986)))
            {
                return false;
            }

            // Too soon to select a new target?
            if (!WaitHelper.Instance.IsDoneWaiting(@"Target.AutoTarget", TimeSpan.FromMilliseconds(Settings.BotBase.Instance.TargetScanRate)))
                return false;

            // Does the player allow us to switch targets?
            if (Settings.BotBase.Instance.AutoTargetSwitch)
                return true;

            return !Core.Me.HasTarget;
        }

        /// <summary>
        /// General caller methods for all post-target filters.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private IEnumerable<BattleCharacter> ApplyPostFilters(IEnumerable<BattleCharacter> group)
        {
            var result = group;

            result = PostFilterDistance(result);
            result = PostFilterThreatList(result);
            result = PostFilterInvincible(result);
            result = PostFilterLos(result);
            result = PostFilterFate(result);

            return result;
        }

        private IEnumerable<BattleCharacter> PostFilterThreatList(IEnumerable<BattleCharacter> group)
        {
            if (Settings.BotBase.Instance.EnableSmartPull && !WorldManager.InPvP)
                return group.Where(o => o.IsEnemy() && (GameObjectManager.Attackers.Contains(o) || o.HasBeenTaggedByPartyMember()));

            return group;
        }

        private IEnumerable<BattleCharacter> PostFilterInvincible(IEnumerable<BattleCharacter> group)
        {
            //if (WorldManager.InPvP)
            return group.Where(o => !o.IsInvincible());

            return group;
        }

        private IEnumerable<BattleCharacter> PostFilterLos(IEnumerable<BattleCharacter> group)
        {
            if (Settings.BotBase.Instance.EnableLosCheck)
                return group.Where(o => o.InLineOfSight());

            return group;
        }

        private IEnumerable<BattleCharacter> PostFilterDistance(IEnumerable<BattleCharacter> group)
        {
            if (Settings.BotBase.Instance.TargetScanMaxDistance == 0)
                return group.Where(o => o.IsInPullRange());

            return group.Where(o => o.Distance2D() - o.CombatReach - Core.Me.CombatReach < Settings.BotBase.Instance.TargetScanMaxDistance);
        }

        private IEnumerable<BattleCharacter> PostFilterAutoDistance(IEnumerable<BattleCharacter> group)
        {

            if (WorldManager.InPvP && Core.Me.CurrentJob == ClassJobType.Dragoon)
                return group.Where(o => o.IsInPullRange());

            return group.Where(o => o.Distance2D() - o.CombatReach - Core.Me.CombatReach <= Settings.BotBase.Instance.TargetScanMaxDistance);
        }

        /// <summary>
        /// Applies a filter for FATE-only targets to a group of BattleCharacters.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        private IEnumerable<BattleCharacter> PostFilterFate(IEnumerable<BattleCharacter> group)
        {
            // FATE filter disabled? Return the group as-is.
            if (!Settings.BotBase.Instance.FateTargetFilter)
                return group;

            // Do we have aggro from something else? Prioritize those targets!
            if (GameObjectManager.Attackers.Any(o => o.TargetCharacter == Core.Me))
                return GameObjectManager.Attackers
                    .Where(o => o.TargetCharacter == Core.Me)
                    .OrderBy(o => o.Distance2D());

            // No external attackers, prioritize FATE mobs.
            if (FateManager.WithinFate)
                return group
                    .Where(o => o.IsFate)
                    .OrderBy(o => o.Distance2D());

            // Nothing special? Return group as-is.
            return group;
        }

        /// <summary>
        /// Target selection: Assist fixed character.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetAssistFixedCharacter()
        {
            // No character name set.
            if (string.IsNullOrEmpty(Settings.BotBase.Instance.FixedCharacterName))
                return null;

            var character = GameObjectManager.GetObjectsOfType<BattleCharacter>().FirstOrDefault(c => c.Name == Settings.BotBase.Instance.FixedCharacterName && c.ObjectId == Settings.BotBase.Instance.FixedCharacterId) ??
                            GameObjectManager.GetObjectsOfType<BattleCharacter>().FirstOrDefault(c => c.Name == Settings.BotBase.Instance.FixedCharacterName);

            // Character is not in the vicinity...
            if (character == null || !character.IsValid)
                return null;

            // Character doesn't have a target or it's not what we consider an enemy.
            if (!character.HasTarget ||
                !character.TargetGameObject.IsEnemy())
                return null;

            return character.TargetGameObject as BattleCharacter;
        }


        /// <summary>
        /// Target selection: Assist party leader.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetAssistLeader()
        {
            // Not in party or party leader?
            if (!Core.Me.IsInMyParty() || Core.Me.IsInMyParty() && Core.Me.IsPartyLeader())
                return null;

            // Party leader is not in the vicinity...
            if (!PartyManager.PartyLeader.IsInObjectManager)
                return null;

            // Leader doesn't have a target or it's not what we consider an enemy.
            if (!PartyManager.PartyLeader.BattleCharacter.HasTarget ||
                !PartyManager.PartyLeader.BattleCharacter.TargetGameObject.IsEnemy())
                return null;

            return PartyManager.PartyLeader.BattleCharacter.TargetGameObject as BattleCharacter;
        }

        /// <summary>
        /// Target selection: Assist tank.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetAssistTank()
        {
            // I am not in a party? I am the tank? Or there is no tank in my party?
            if (!Core.Me.IsInMyParty() || Core.Me.IsTank() || !PartyManager.VisibleMembers.Any(member => member.IsTank() && !member.IsMe))
                return null;

            // Find the closest tank in my party and target whatever they are targeting!
            var nearestTank = PartyManager.VisibleMembers
                .Where(member => member.IsTank())
                .OrderBy(member => member.BattleCharacter.Distance2D())
                .FirstOrDefault();

            // No tanksywhirls?
            if (nearestTank == null)
                return null;

            // No targetsie?
            if (!nearestTank.BattleCharacter.HasTarget || !nearestTank.BattleCharacter.TargetGameObject.IsEnemy())
                return null;

            return nearestTank.BattleCharacter.TargetGameObject as BattleCharacter;
        }

        private BattleCharacter TargetAssistHighestLvlCharacter()
        {
            if (!Core.Me.IsInMyParty() ||
                !PartyManager.VisibleMembers.Any(member => member.SyncdLevel > Core.Me.ClassLevel))
                return null;

            var highestLevelChar = PartyManager.VisibleMembers
                .OrderByDescending(member => member.SyncdLevel)
                .ThenByDescending(member => member.Name)
                .FirstOrDefault();

            if (highestLevelChar == null || highestLevelChar.IsMe)
                return null;

            if (!highestLevelChar.BattleCharacter.HasTarget || !highestLevelChar.BattleCharacter.TargetGameObject.IsEnemy())
                return null;

            return highestLevelChar.BattleCharacter.TargetGameObject as BattleCharacter;
        }

        /// <summary>
        /// Target selection: Best AOE target.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetBestAoeEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => !o.IsStrikingDummy() && o.IsEnemy())
                .OrderByDescending(o => o.NearbyEnemyCount())
                .ThenBy(o => o.Distance2D());

            var bestEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return bestEnemy;
        }

        /// <summary>
        /// Target selection: Nearest enemy.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetNearestEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => !o.IsStrikingDummy() && o.IsEnemy())
                .OrderBy(o => o.Distance2D());

            var nearestEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return nearestEnemy;
        }

        /// <summary>
        /// Target selection: Whitelisted enemies only.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetOnlyWhitelistedEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => !o.IsStrikingDummy() && o.IsEnemy() && IsValidWhitelistTarget(o))
                .OrderBy(o => o.Distance2D());

            var validEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return validEnemy;
        }

        /// <summary>
        /// Target selection: Highest HP enemy.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetHighestHpEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => !o.IsStrikingDummy() && o.IsEnemy())
                .OrderByDescending(o => o.CurrentHealth)
                .ThenBy(o => o.Distance2D());

            var strongestEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return strongestEnemy;
        }

        /// <summary>
        /// Target selection: Highest HP percent enemy.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetHighestHpPercentEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => !o.IsStrikingDummy() && o.IsEnemy())
                .OrderByDescending(o => o.CurrentHealthPercent)
                .ThenBy(o => o.Distance2D());

            var strongestEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return strongestEnemy;
        }

        /// <summary>
        /// Target selection: Lowest HP enemy.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetLowestHpEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => !o.IsStrikingDummy() && o.IsEnemy())
                .OrderBy(o => o.CurrentHealth)
                .ThenBy(o => o.Distance2D());

            var weakestEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return weakestEnemy;
        }
        private BattleCharacter PVPLowestHpEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => !o.IsStrikingDummy() && o.IsEnemy())
                .OrderByDescending(o => o.IsCasting && o.TargetGameObject != null &&
                                        (o.TargetGameObject.Type == GameObjectType.EventNpc ||
                                         o.TargetGameObject.Type == GameObjectType.EventObject))
                .ThenByDescending(o => o.HasMyAura(1323))
                .ThenByDescending(o => o.HasAura(1420))
                .ThenByDescending(o => o.IsMounted)
                .ThenBy(o => o.CurrentHealth)
                .ThenBy(o => o.Distance2D());

            var weakestEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return weakestEnemy;
        }

        /// <summary>
        /// Target selection: Lowest HP percent enemy.
        /// </summary>
        /// <returns>Potential BattleCharacter object as the new target or null when no suitable target was found.</returns>
        private BattleCharacter TargetLowestHpPercentEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => !o.IsStrikingDummy() && o.IsEnemy())
                .OrderBy(o => o.CurrentHealthPercent)
                .ThenBy(o => o.Distance2D());

            var weakestEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return weakestEnemy;
        }

        private BattleCharacter PVPLowestHpPercentEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => !o.IsStrikingDummy() && o.IsEnemy())
                .OrderByDescending(o => o.IsCasting && o.TargetGameObject != null &&
                                        (o.TargetGameObject.Type == GameObjectType.EventNpc ||
                                         o.TargetGameObject.Type == GameObjectType.EventObject))
                .ThenByDescending(o => o.HasMyAura(1323))
                .ThenByDescending(o => o.HasAura(1420))
                .ThenByDescending(o => o.IsMounted)
                .ThenBy(o => o.CurrentHealthPercent)
                .ThenBy(o => o.Distance2D());

            var weakestEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return weakestEnemy;
        }

        private BattleCharacter TargetMostFocusedEnemy()
        {
            var potentialTargets = GameObjectManager.GetObjectsOfType<BattleCharacter>()
                .Where(o => o.IsEnemy())
                .OrderByDescending(o => o.BeingTargetedCount())
                .ThenBy(o => o.Distance2D());

            var mostFocusedEnemy = ApplyPostFilters(potentialTargets).FirstOrDefault();

            return mostFocusedEnemy;
        }
    }
}