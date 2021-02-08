using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Clio.Utilities;
using ff14bot;
using ff14bot.Enums;
using ff14bot.Managers;
using GreyMagic;
using Kombatant.Helpers;

namespace Kombatant.Memory
{
	class Offsets
	{
		#region Singleton

		private static Offsets _offsets;
		internal static Offsets Instance => _offsets ?? (_offsets = new Offsets());

		#endregion

		public readonly int AgentNotificationId;
		public readonly int AgentMvpId;
		//public readonly int _BRcXRW4UKMinIJUVnAue5gFVjSA;

		public readonly IntPtr AnimationLockTimer;
		public readonly IntPtr MarkingParam1;
		public readonly IntPtr MarkingFunc;
		public readonly IntPtr AgentNotificationVTable;
		public readonly IntPtr AgentMvpVTable;
		public readonly IntPtr GroundSpeedWriteFunc;
		public readonly IntPtr CombatReachWriteFunc;
		public readonly IntPtr Conditions;
		public readonly IntPtr KnockbackFunc;
		public readonly IntPtr LootFunc;
		public readonly IntPtr LootsAddr;



		Offsets()
		{
			using (Core.Memory.AcquireFrame())
			{
				InitializeValue(ref AgentMvpVTable, nameof(AgentMvpVTable), "48 8D 05 ? ? ? ? 48 89 03 33 C0 48 89 43 28 89 43 30 48 8B C3 48 83 C4 ? 5B C3 CC CC CC CC CC CC 40 53 Add 3 TraceRelative");
				InitializeValue(ref AgentNotificationVTable, nameof(AgentNotificationVTable), "48 8D 05 ? ? ? ? 48 8B D9 48 89 01 E8 ? ? ? ? 48 8B CB E8 ? ? ? ? 48 8B CB 48 83 C4 ? 5B E9 ? ? ? ? CC CC CC CC Add 3 TraceRelative");
				InitializeValue(ref AnimationLockTimer, nameof(AnimationLockTimer), "48 8D 0D ? ? ? ? E8 ? ? ? ? 8B F8 8B CF Add 3 TraceRelative Add 8");
				InitializeValue(ref MarkingFunc, nameof(MarkingFunc), "48 89 5C 24 10 48 89 6C 24 18 57 48 83 EC ? 8D 42 FF");
				InitializeValue(ref MarkingParam1, nameof(MarkingParam1), "48 8D 0D ? ? ? ? E8 ? ? ? ? 84 C0 74 ? 45 32 C9 Add 3 TraceRelative");
				InitializeValue(ref GroundSpeedWriteFunc, nameof(GroundSpeedWriteFunc), "F3 0F 11 73 44 0F 28 74 24 40");
				InitializeValue(ref CombatReachWriteFunc, nameof(CombatReachWriteFunc), "F3 0F 10 83 C0 00 00 00 48 83 C4 ?");
				InitializeValue(ref Conditions, nameof(Conditions), "48 8D 0D ? ? ? ? 45 33 C0 41 8D 51 69 Add 3 TraceRelative");
				InitializeValue(ref KnockbackFunc, nameof(KnockbackFunc), "E8 ? ? ? ? 48 8B 9C 24 A0 00 00 00 4C 8D 9C 24 90 00 00 00");
				InitializeValue(ref LootFunc, nameof(LootFunc), "E8 ? ? ? ? EB 4A 48 8D 4F 10 Add 1 TraceRelative");
				InitializeValue(ref LootsAddr, nameof(LootsAddr), "48 8D 0D ? ? ? ? E8 ? ? ? ? 89 44 24 60 Add 3 TraceRelative");

				AgentNotificationId = AgentModule.FindAgentIdByVtable(AgentNotificationVTable);
				AgentMvpId = AgentModule.FindAgentIdByVtable(AgentMvpVTable);
			}
		}

		private static void InitializeValue(ref IntPtr value, string name, string pattern, int offset = 0)
		{
			PatternFinder patternFinder = new PatternFinder(Core.Memory);

			try
			{
				value = patternFinder.Find(pattern) + offset;
				LogHelper.Instance.Log($"[Offset] Found {name} at {value.ToInt64():X}.");
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"[Offset] {name} not found. ");
			}
		}
	}
}
