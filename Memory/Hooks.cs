using System;
using ff14bot;
using GreyMagic;
using Kombatant.Helpers;

namespace Kombatant.Memory
{
	public class GroundSpeedHook
	{
		public static GroundSpeedHook Instance => _groundSpeedHook ?? (_groundSpeedHook = new GroundSpeedHook());
		private static GroundSpeedHook _groundSpeedHook;
		private readonly IntPtr SpeedMultiplierPtr;
		private readonly IntPtr MinSpeedPtr;

		public float SpeedMultiplier
		{
			get => Core.Memory.NoCacheRead<float>(SpeedMultiplierPtr);
			set => Core.Memory.Write(SpeedMultiplierPtr, value);
		}

		public float GroundMinimumSpeed
		{
			get => Core.Memory.NoCacheRead<float>(MinSpeedPtr);
			set => Core.Memory.Write(MinSpeedPtr, value);
		}

		private GroundSpeedHook()
		{
			try
			{
				var hookPtr = Offsets.Instance.GroundSpeedWriteFunc;
				var detour = Core.Memory.Executor.AllocNear(hookPtr, 0x40, 0x40);

				SpeedMultiplierPtr = detour + 0x38;
				MinSpeedPtr = detour + 0x3C;
				Core.Memory.Write(SpeedMultiplierPtr, 1f);
				Core.Memory.Write(MinSpeedPtr, 0f);
				LogHelper.Instance.Log($"[Hook] GroundSpeedHook allocated at {detour.ToInt64():X}");
				//LogHelper.Instance.Log($"[Hook] SpeedMultiPtr: {SpeedMultiplierPtr.ToInt64():X}");
				//LogHelper.Instance.Log($"[Hook] MinimumSpeedPtr: {MinSpeedPtr.ToInt64():X}");

				var asm = Core.Memory.Asm;
				asm.Clear();
				asm.AddLine($"comiss xmm6,[{MinSpeedPtr}]");
				asm.AddLine($"ja skip");
				asm.AddLine($"movss xmm6,[{MinSpeedPtr}]");
				asm.AddLine($"skip: mulss xmm6,[{SpeedMultiplierPtr}]");
				asm.AddLine("movss [rbx+0x44],xmm6");
				asm.AddLine("test rax,rax");
				asm.AddLine($"ret");
				asm.Inject(detour);

				asm.Clear();
				asm.AddLine($"call {detour}");

				Core.Memory.Patches.Create(hookPtr, asm.Assemble(hookPtr), "GroundSpeedHook");
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log(e);
			}
		}
	}

	public class CombatReachHook
	{
		public static CombatReachHook Instance => _combatReachHook ?? (_combatReachHook = new CombatReachHook());
		private static CombatReachHook _combatReachHook;
		private readonly IntPtr CombatReachAdjustPtr;

		public float CombatReachAdjustment
		{
			get => Core.Memory.NoCacheRead<float>(CombatReachAdjustPtr);
			set
			{
				Core.Memory.Write(CombatReachAdjustPtr, value);
				Kombatant.CombatReachAdj = value;
			}
		}

		private CombatReachHook()
		{
			try
			{
				var hookPtr = Offsets.Instance.CombatReachWriteFunc;
				var detour = Core.Memory.Executor.AllocNear(hookPtr, 0x40, 0x40);

				CombatReachAdjustPtr = detour + 0x38;
				//Core.Memory.Write(CombatReachTunePtr, 0f);
				LogHelper.Instance.Log($"[Hook] CombatReachHook allocated at {detour.ToInt64():X}");
				//LogHelper.Instance.Log($"[Hook] CombatReachAdjustPtr: {CombatReachAdjustPtr.ToInt64():X}");

				var asm = Core.Memory.Asm;
				asm.Clear();
				asm.AddLine("movss xmm0,[rbx+0x000000C0]");
				asm.AddLine($"addss xmm0,[{CombatReachAdjustPtr}]");
				asm.AddLine($"ret");
				asm.Inject(detour);

				asm.Clear();
				asm.AddLine($"call {detour}");
				asm.AddLine($"nop");
				asm.AddLine($"nop");
				asm.AddLine($"nop");

				Core.Memory.Patches.Create(hookPtr, asm.Assemble(hookPtr), "CombatReachHook");
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log(e);
			}
		}
	}
}