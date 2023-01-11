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

		public readonly IntPtr AgentNotificationVTable;
		public readonly IntPtr AgentMvpVTable;

		public readonly IntPtr LootFunc;
		public readonly IntPtr LootsAddr;
		public readonly IntPtr TraderTradeStage;
		public readonly IntPtr TargetManager;




		Offsets()
		{
			using (Core.Memory.AcquireFrame())
			{
				InitializeValue(ref AgentMvpVTable, nameof(AgentMvpVTable), "48 8D 05 ? ? ? ? 48 89 03 33 C0 48 89 43 28 89 43 30 48 8B C3 48 83 C4 ? 5B C3 CC CC CC CC CC CC 40 53 Add 3 TraceRelative");
				InitializeValue(ref AgentMvpVTable, nameof(AgentMvpVTable), "48 8D 05 ? ? ? ? 48 89 03 33 C0 48 89 43 ? 48 89 43 ? 48 8B C3 48 83 C4 ? 5B C3 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 40 53 Add 3 TraceRelative");
				InitializeValue(ref AgentNotificationVTable, nameof(AgentNotificationVTable), "48 8D 05 ? ? ? ? 48 8B D9 48 89 01 E8 ? ? ? ? 48 8B CB E8 ? ? ? ? 48 8B CB 48 83 C4 ? 5B E9 ? ? ? ? CC CC CC CC Add 3 TraceRelative");
				InitializeValue(ref LootFunc, nameof(LootFunc), "E8 ? ? ? ? EB 4A 48 8D 4F 10 Add 1 TraceRelative");
				InitializeValue(ref LootsAddr, nameof(LootsAddr), "48 8D 0D ? ? ? ? E8 ? ? ? ? 89 44 24 60 Add 3 TraceRelative");
				InitializeValue(ref TraderTradeStage, nameof(TraderTradeStage), "83 3D ? ? ? ? ? 7F ? Add 2 TraceRelative Add 5");
				InitializeValue(ref TargetManager, nameof(TargetManager), "48 8B 05 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? FF 50 ?? 48 85 DB Add 3 TraceRelative");

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
