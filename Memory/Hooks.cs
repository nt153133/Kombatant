using System;
using ff14bot;
using GreyMagic;
using Kombatant.Helpers;
using Kombatant.Settings;

namespace Kombatant.Memory
{
	public class GroundSpeedHook
	{
		public static GroundSpeedHook Instance => _groundSpeedHook ?? (_groundSpeedHook = new GroundSpeedHook());
		private static GroundSpeedHook _groundSpeedHook;
		private readonly IntPtr SpeedMultiplierPtr;
		private readonly IntPtr MinSpeedPtr;
		public bool hooked;

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
				LogHelper.Instance.Log($"[Hook] GroundSpeedHook detour at {detour.ToInt64():X}");
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
				hooked = true;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log(e);
				throw;
				//Kombatant._memoFaliure = true;
			}
		}
	}

	public class CombatReachHook
	{
		public static CombatReachHook Instance => _combatReachHook ?? (_combatReachHook = new CombatReachHook());
		private static CombatReachHook _combatReachHook;
		private readonly IntPtr CombatReachAdjustPtr;
		private readonly IntPtr MyCombatReachPtr;
		public bool hooked;

		public float CombatReachAdjustment
		{
			get => Core.Memory.NoCacheRead<float>(CombatReachAdjustPtr);
			set
			{
				Core.Memory.Write(CombatReachAdjustPtr, value);
				Kombatant.CombatReachAdj = value;
			}
		}

		public float MyCombatReachAdjustment
		{
			get => Core.Memory.NoCacheRead<float>(MyCombatReachPtr);
			set
			{
				Core.Memory.Write(MyCombatReachPtr, value);
				//Kombatant.CombatReachAdj = value;
			}
		}

		private CombatReachHook()
		{
			try
			{
				var hookPtr = Offsets.Instance.CombatReachWriteFunc;
				var detour = Core.Memory.Executor.AllocNear(hookPtr, 0x40, 0x40);

				CombatReachAdjustPtr = detour + 0x38;
				MyCombatReachPtr = detour + 0x3C;
				//Core.Memory.Write(CombatReachTunePtr, 0f);
				LogHelper.Instance.Log($"[Hook] CombatReachHook detour at {detour.ToInt64():X}");
				//LogHelper.Instance.Log($"[Hook] CombatReachAdjustPtr: {CombatReachAdjustPtr.ToInt64():X}");

				var asm = Core.Memory.Asm;
				asm.Clear();
				asm.AddLine("movss xmm0,[rbx+0x000000C0]");
				asm.AddLine($"mov eax, {Core.Me.ObjectId}");

				asm.AddLine("cmp [rbx+0x74], eax");
				asm.AddLine("jne else");

				asm.AddLine($"subss xmm0, [{CombatReachAdjustPtr}]");
				asm.AddLine($"subss xmm0, [{MyCombatReachPtr}]");
				asm.AddLine("test rax, rax");
				asm.AddLine("jmp end");

				asm.AddLine($"else:");
				asm.AddLine($"addss xmm0, [{CombatReachAdjustPtr}]");

				asm.AddLine($"end:");
				asm.AddLine($"ret");
				asm.Inject(detour);

				asm.Clear();
				asm.AddLine($"call {detour}");
				asm.AddLine($"nop");
				asm.AddLine($"nop");
				asm.AddLine($"nop");

				Core.Memory.Patches.Create(hookPtr, asm.Assemble(hookPtr), "CombatReachHook");
				hooked = true;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log(e);
				throw;
				//Kombatant._memoFaliure = true;
			}
		}
	}

	public class FastCastHook
	{
		public static FastCastHook Instance => _fastCastHook ?? (_fastCastHook = new FastCastHook());
		private static FastCastHook _fastCastHook;
		private readonly IntPtr CastingTimePtr1;
		private readonly IntPtr CastingTimePtr2;
		public bool hooked;

		public float CastingTimeAdjustment
		{
			get => Core.Memory.NoCacheRead<float>(CastingTimePtr1) * 100;
			set
			{
				Core.Memory.Write(CastingTimePtr1, value / 100);
				Core.Memory.Write(CastingTimePtr2, value / 100);
			}
		}

		private FastCastHook()
		{
			try
			{
				var hookPtr = Offsets.Instance.CurrentSpellTimeFunc;
				var detour = Core.Memory.Executor.AllocNear(hookPtr, 0x40, 0x40);

				CastingTimePtr1 = detour + 0x38;
				LogHelper.Instance.Log($"[Hook] FastCastHook1 detour at {detour.ToInt64():X}");

				var asm = Core.Memory.Asm;
				asm.Clear();
				asm.AddLine($"mulss xmm0, [{CastingTimePtr1}]");
				asm.AddLine($"movss [{Offsets.Instance.CurrentSpellTimePtr}],xmm0");
				asm.AddLine($"ret");
				asm.Inject(detour);

				asm.Clear();
				asm.AddLine($"call {detour}");
				asm.AddLine($"nop");
				asm.AddLine($"nop");
				asm.AddLine($"nop");

				Core.Memory.Patches.Create(hookPtr, asm.Assemble(hookPtr), "FastCastHook1");
				hooked = true;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log(e);
				throw;
				//Kombatant._memoFaliure = true;
			}

			try
			{
				var hookPtr = Offsets.Instance.MySpellCastTimeWriteFunc;
				var detour = Core.Memory.Executor.AllocNear(hookPtr, 0x40, 0x40);

				CastingTimePtr2 = detour + 0x38;
				LogHelper.Instance.Log($"[Hook] FastCastHook2 detour at {detour.ToInt64():X}");

				var asm = Core.Memory.Asm;
				asm.Clear();
				asm.AddLine($"mulss xmm6,[{CastingTimePtr2}]");
				asm.AddLine($"movss [rdi+0x38],xmm6");
				asm.AddLine($"ret");
				asm.Inject(detour);

				asm.Clear();
				asm.AddLine($"call {detour}");

				Core.Memory.Patches.Create(hookPtr, asm.Assemble(hookPtr), "FastCastHook2");
				hooked = true;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log(e);
				throw;
				//Kombatant._memoFaliure = true;
			}
		}
	}
	public class GcdHook
	{
		public static GcdHook Instance => _gcdHook ?? (_gcdHook = new GcdHook());
		private static GcdHook _gcdHook;
		private readonly IntPtr gcdMultiplierPtr;
		public bool hooked;

		public float GcdAdjustment
		{
			get => Core.Memory.NoCacheRead<float>(gcdMultiplierPtr) * 100;
			set
			{
				Core.Memory.Write(gcdMultiplierPtr, value / 100);
			}
		}

		private GcdHook()
		{

			try
			{
				string gcdhook = "GcdHook";
				var hookPtr = Offsets.Instance.GcdWriteFunc;
				var detour = Core.Memory.Executor.AllocNear(hookPtr, 0x40, 0x40);

				gcdMultiplierPtr = detour + 0x38;
				LogHelper.Instance.Log($"[Hook] {gcdhook} detour at {detour.ToInt64():X}");

				var asm = Core.Memory.Asm;
				asm.Clear();
				asm.AddLine($"mulss xmm6,[{gcdMultiplierPtr}]");
				asm.AddLine($"movss [rax+0x0C],xmm6");
				asm.AddLine($"ret");
				asm.Inject(detour);

				asm.Clear();
				asm.AddLine($"call {detour}");

				Core.Memory.Patches.Create(hookPtr, asm.Assemble(hookPtr), gcdhook);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log(e);
				throw;
				//Kombatant._memoFaliure = true;
			}
		}
	}
}