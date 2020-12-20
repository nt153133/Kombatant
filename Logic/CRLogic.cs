using System;
using System.Threading.Tasks;
using Clio.Common;
using Clio.Utilities;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using Kombatant.Extensions;
using Kombatant.Interfaces;
using Kombatant.Settings;

namespace Kombatant.Logic
{
	/// <summary>
	/// Logic for interactions with the combat routine.
	/// </summary>
	/// <inheritdoc cref="M:Komabatant.Interfaces.LogicExecutor"/>
	// ReSharper disable once InconsistentNaming
	internal class CombatLogic : LogicExecutor
	{
		#region Singleton

		private static CombatLogic _combatLogic;
		internal static CombatLogic Instance => _combatLogic ?? (_combatLogic = new CombatLogic());

		#endregion

		/// <summary>
		/// Main task executor for the Combat logic.
		/// </summary>
		/// <returns>Returns <c>true</c> if any action was executed, otherwise <c>false</c>.</returns>
		internal new async Task<bool> ExecuteLogic()
		{
			// Do not execute this logic if the botbase is paused.
			if (Settings.BotBase.Instance.IsPaused)
				return false;

			if (!WorldManager.InPvP)
			{
				if (Core.Me.IsMounted || MovementManager.IsFlying || MovementManager.IsSwimming || MovementManager.IsDiving)
					return false;
			}

			if (Core.Me.IsDead)
			{
				if (ShouldExecuteDeath())
					if (await RoutineManager.Current.DeathBehavior.ExecuteCoroutine())
						return true;
			}
			else
			{
				if (Core.Me.InCombat)
				{
					if (ShouldExecuteInCombatHeal())
						if (await RoutineManager.Current.HealBehavior.ExecuteCoroutine())
							return true;

					if (ShouldExecuteCombatBuff())
						if (await RoutineManager.Current.CombatBuffBehavior.ExecuteCoroutine())
							return true;

					if (ShouldExecuteCombat())
						if (await RoutineManager.Current.CombatBehavior.ExecuteCoroutine())
							return true;
				}
				else
				{
					if (ShouldExecuteOutOfCombatHeal())
						if (await RoutineManager.Current.HealBehavior.ExecuteCoroutine())
							return true;

					if (ShouldExecuteRest())
						if (await RoutineManager.Current.RestBehavior.ExecuteCoroutine())
							return true;

					if (ShouldExecutePreCombatBuff())
						if (await RoutineManager.Current.PreCombatBuffBehavior.ExecuteCoroutine())
							return true;

					if (ShouldExecutePullBuff())
						if (await RoutineManager.Current.PullBuffBehavior.ExecuteCoroutine())
							return true;

					if (ShouldExecutePull())
						if (await RoutineManager.Current.PullBehavior.ExecuteCoroutine())
							return true;
				}
			}


			return false;
		}
	}
}