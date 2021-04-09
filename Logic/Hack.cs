//!CompilerOption:Optimize:On
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Managers;
using GreyMagic;
using Kombatant.Helpers;
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
		internal new void ExecuteLogic()
		{
			if (BotBase.Instance.PulseDirector)
			{
				PulseFlagHelper.Instance.EnablePulseFlag(PulseFlags.Directors);
			}
			else
			{
				PulseFlagHelper.Instance.DisablePulseFlag(PulseFlags.Directors);
			}

			if (Kombatant._memoFaliure)
			{
				LogHelper.Instance.Log($"memory patching error occurs. please try restart your game.");
				return;
			}
			
			try
			{
				// Do not execute this logic if the botbase is paused.
				if (BotBase.Instance.IsPaused)
				{
					Core.Memory.Patches["FastCastHook1"].Remove();
					Core.Memory.Patches["FastCastHook2"].Remove();
					Core.Memory.Patches["GcdHook"].Remove();
					Core.Memory.Patches["GroundSpeedHook"].Remove();
					Core.Memory.Patches["CombatReachHook"].Remove();
					Core.Memory.Patches["NoKnockbackPatch"].Remove();
					return;
				}

				if (BotBase.Instance.EnableFastCast)
				{
					FastCastHook.Instance.CastingTimeAdjustment = BotBase.Instance.FastCastPercent;
					Core.Memory.Patches["FastCastHook1"].Apply();
					Core.Memory.Patches["FastCastHook2"].Apply();
				}
				else
				{
					Core.Memory.Patches["FastCastHook1"].Remove();
					Core.Memory.Patches["FastCastHook2"].Remove();
				}

				if (BotBase.Instance.EnableReduceGcd)
				{
					GcdHook.Instance.GcdAdjustment = BotBase.Instance.GcdPercent;
					Core.Memory.Patches["GcdHook"].Apply();
				}
				else
				{
					Core.Memory.Patches["GcdHook"].Remove();
				}

				if (BotBase.Instance.EnableMovementSpeedHack)
				{
					GroundSpeedHook.Instance.SpeedMultiplier = BotBase.Instance.GroundSpeedMultiplier;
					GroundSpeedHook.Instance.GroundMinimumSpeed = BotBase.Instance.MinGroundSpeed;
					Core.Memory.Patches["GroundSpeedHook"].Apply();
				}
				else
				{
					Core.Memory.Patches["GroundSpeedHook"].Remove();
				}

				if (BotBase.Instance.EnableCombatReachIncrement)
				{
					CombatReachHook.Instance.CombatReachAdjustment = BotBase.Instance.CombatReachIncrement;
					CombatReachHook.Instance.MyCombatReachAdjustment = BotBase.Instance.MyCombatReachAdjustment;
					Core.Memory.Patches["CombatReachHook"].Apply();
				}
				else
				{
					Core.Memory.Patches["CombatReachHook"].Remove();
				}

				if (BotBase.Instance.NoKnockback)
				{
					Core.Memory.Patches["NoKnockbackPatch"].Apply();
				}
				else
				{
					Core.Memory.Patches["NoKnockbackPatch"].Remove();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"memory patching error occurs. please try restart your game. \r\n{e.Source}\r\n{e.Message}");
			}

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
				Core.Memory.Write(Core.Me.Pointer + 0xD34, BotBase.Instance.AnimationSpeed);
				Core.Memory.Write(Core.Me.Pointer + 0xD38, BotBase.Instance.AnimationSpeed);
			}

			if (BotBase.Instance.RemoveMovementLock)
			{
				Core.Memory.Write(Offsets.Instance.Conditions + 0x1A, 0);
				Core.Memory.Write(Offsets.Instance.Conditions + 0x48, (byte)0);
				Core.Memory.Write(Offsets.Instance.Conditions + 0x57, (byte)0);
			}
		}
	}
}
