using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ff14bot;
using ff14bot.Managers;
using ff14bot.Objects;
using Kombatant.Memory;

namespace Kombatant.Managers
{
	public static class TargetOffsets
	{
		public const int CurrentTarget = 0x80;
		public const int MouseOverTarget = 0xD0;
		public const int FocusTarget = 0xF8;
		public const int PreviousTarget = 0x110;
	}

	public static class TargetManager
	{
		public static GameObject CurrentTarget
		{
			get => GetGameObject(Offsets.Instance.TargetManager + TargetOffsets.CurrentTarget);
			//set => SetTarget(value.Pointer, TargetOffsets.CurrentTarget);
		}

		public static GameObject FocusTarget
		{
			get => GetGameObject(Offsets.Instance.TargetManager + TargetOffsets.FocusTarget);
			//set => SetTarget(value.Pointer, TargetOffsets.FocusTarget);
		}

		public static GameObject MouseOverTarget
		{
			get => GetGameObject(Offsets.Instance.TargetManager + TargetOffsets.MouseOverTarget);
		}

		public static GameObject PreviousTarget
		{
			get => GetGameObject(Offsets.Instance.TargetManager + TargetOffsets.PreviousTarget);
		}

		public static void ClearCurrentTarget() => SetTarget(IntPtr.Zero, TargetOffsets.CurrentTarget);
		public static void ClearFocusTarget() => SetTarget(IntPtr.Zero, TargetOffsets.FocusTarget);

		public static void Focus(this GameObject o)
		{
			if (o is null) return;
			SetTarget(o.Pointer, TargetOffsets.FocusTarget);
		}

		#region private

		private static void SetTarget(IntPtr actorAddress, int offset)
		{
			if (Offsets.Instance.TargetManager == IntPtr.Zero) return;
			Core.Memory.Write(Offsets.Instance.TargetManager + offset, actorAddress);
		}
		private static GameObject GetGameObject(IntPtr ptr)
		{
			IntPtr intPtr = Core.Memory.Read<IntPtr>(ptr);
			if (intPtr == IntPtr.Zero)
			{
				return null;
			}

			return GameObjectManager.GameObjects.FirstOrDefault(gameObject => gameObject.Pointer == intPtr);
		}

		#endregion
	}
}
