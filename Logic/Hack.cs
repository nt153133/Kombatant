using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using Kombatant.Interfaces;
using Kombatant.Memory;
using Kombatant.Settings;

namespace Kombatant.Logic
{
	/// <summary>
	/// Logic for interactions with the combat routine.
	/// </summary>
	/// <inheritdoc cref="M:Komabatant.Interfaces.LogicExecutor"/>
	// ReSharper disable once InconsistentNaming
	internal class Hack : LogicExecutor
	{
		#region Singleton

		private static Hack _combatLogic;
		internal static Hack Instance => _combatLogic ?? (_combatLogic = new Hack());

		#endregion

		/// <summary>
		/// Main task executor for the Hack logic.
		/// </summary>
		/// <returns>Returns <c>true</c> if any action was executed, otherwise <c>false</c>.</returns>
		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		internal new async Task<bool> ExecuteLogic()
		{
			// Do not execute this logic if the botbase is paused.

			if (BotBase.Instance.EnableMovementSpeedHack && !Settings.BotBase.Instance.IsPaused)
			{
				GroundSpeedHook.Instance.SpeedMultiplier = BotBase.Instance.GroundSpeedMultiplier;
				GroundSpeedHook.Instance.GroundMinimumSpeed = BotBase.Instance.MinGroundSpeed;
			}
			else
			{
				GroundSpeedHook.Instance.SpeedMultiplier = 1;
				GroundSpeedHook.Instance.GroundMinimumSpeed = 0;
			}

			if (BotBase.Instance.EnableCombatReachIncrement && !Settings.BotBase.Instance.IsPaused)
			{
				CombatReachHook.Instance.CombatReachAdjustment = BotBase.Instance.CombatReachIncrement;
			}
			else
			{
				CombatReachHook.Instance.CombatReachAdjustment = 0;
			}

			if (Settings.BotBase.Instance.IsPaused)
				return false;

			if (BotBase.Instance.EnableAnimationLockHack && Memory.Offsets.Instance.AnimationLockTimer != IntPtr.Zero)
			{
				if (BotBase.Instance.AnimationLockMaxDelay == 0)
				{
					Core.Memory.Write(Memory.Offsets.Instance.AnimationLockTimer, 0f);
				}
				else if (Core.Memory.NoCacheRead<float>(Memory.Offsets.Instance.AnimationLockTimer) > BotBase.Instance.AnimationLockMaxDelay / 1000f)
				{
					Core.Memory.Write(Memory.Offsets.Instance.AnimationLockTimer, BotBase.Instance.AnimationLockMaxDelay / 1000f);
				}
			}

			if (BotBase.Instance.EnableAnimationSpeedHack)
			{
				Core.Memory.Write(Core.Me.Pointer + 0xCD4, BotBase.Instance.AnimationSpeed);
				Core.Memory.Write(Core.Me.Pointer + 0xCD8, BotBase.Instance.AnimationSpeed);
			}

			if (BotBase.Instance.RemoveMovementLock)
			{
				Core.Memory.Write(Offsets.Instance.Conditions, 0);
			}

			return false;
		}
	}
}
