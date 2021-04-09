//!CompilerOption:Optimize:On
using System;
using System.Linq;
using Clio.Common;
using Clio.Utilities;
using ff14bot;
using ff14bot.CharacterManagement;
using ff14bot.Enums;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using Kombatant.Enums;
using Kombatant.Settings;

namespace Kombatant.Extensions
{
	/// <summary>
	/// Extensions for the GameObject class.
	/// </summary>
	internal static class GameObjectExtension
	{
		/// <summary>
		/// <para>Counts the number of nearby enemies.</para>
		/// <para>The radius is fixed to 5.5f, this will work fine for abilities with a radius of 5f as well as 8f.</para>
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static int NearbyEnemyCount(this GameObject obj)
		{
			return GameObjectManager.GetObjectsOfType<BattleCharacter>(true)
				.Count(g => g.CheckAliveAndValid() && g.Distance2D(obj.Location) - g.CombatReach < 5.5f &&
				            ((g.StatusFlags & StatusFlags.Hostile) != 0 || g.CanAttack));
		}

		/// <summary>
		/// Checks whether the given gameobject is a battle character.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static bool IsBattleCharacter(this GameObject obj)
		{
			return obj is BattleCharacter;
		}

		/// <summary>
		/// Checks whether the given gameobject is a known boss monster.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static bool IsBoss(this GameObject obj)
		{
			return Constants.GameObject.DungeonBosses.Contains(obj.NpcId);
		}

		/// <summary>
		/// Checks whether the given gameobject is a character.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static bool IsCharacter(this GameObject obj)
		{
			return obj is Character;
		}

		/// <summary>
		/// Checks whether a given gameobject could be considered an enemy.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static bool IsEnemy(this GameObject obj)
		{
			if (!obj.CheckAliveAndValid())
				return false;

			var character = obj.GetCharacter();
			return character.CanAttack /*|| character.StatusFlags == StatusFlags.Hostile*/;
		}

		/// <summary>
		/// Checks whether the given gameobject is a striking dummy.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static bool IsStrikingDummy(this GameObject obj)
		{
			return Constants.GameObject.StrikingDummy == obj.NpcId;
		}

		/// <summary>
		/// Checks whether a given gameobject is in pull range.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static bool IsInPullRange(this GameObject obj)
		{
			return obj.Distance2D() <= RoutineManager.Current.PullRange;
		}

		/// <summary>
		/// Checks whether a given gameobject is valid and alive.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static bool CheckAliveAndValid(this GameObject obj)
		{
			return obj is Character character && character.IsValid && !character.IsMe && character.IsVisible &&
				   character.IsAlive && character.IsTargetable;
		}

		/// <summary>
		/// Gets the given GameObject as a BattleCharacter object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static BattleCharacter GetBattleCharacter(this GameObject obj)
		{
			if (obj.IsBattleCharacter())
				return obj as BattleCharacter;

			return null;
		}

		/// <summary>
		/// Gets the given GameObject as a Character object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		internal static Character GetCharacter(this GameObject obj)
		{
			if (obj.IsCharacter())
				return obj as Character;

			return null;
		}

		/// <summary>
		/// Checks if the given gameobject is looking at a given target location
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="targetLocation"></param>
		/// <returns></returns>
		internal static bool LookingAt(this GameObject obj, Vector3 targetLocation)
		{
			float single = Math.Abs
				(MathEx.NormalizeRadian
					(obj.Heading - MathEx.NormalizeRadian
						 (MathHelper.CalculateHeading(obj.Location, targetLocation) + (float)Math.PI)
					)
				);

			if (single > Math.PI)
				single = Math.Abs(single - (float)(Math.PI * 2));

			return single < (float)(Math.PI / 4);
		}

		internal static int EnemyBeingTargetedCount(this GameObject o, bool onlyCountPartMember = false)
		{

			if (onlyCountPartMember)
			{
				return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(i =>
					!i.IsEnemy() && i.Type == GameObjectType.Pc && i.IsInMyParty() && i.TargetGameObject == o);
			}
			else
			{
				return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(i =>
					!i.IsEnemy() && i.Type == GameObjectType.Pc && i.TargetGameObject == o);
			}

		}

		internal static int AllyBeingTargetedCount(this GameObject o)
		{
			return GameObjectManager.GetObjectsOfType<BattleCharacter>().Count(i =>
				i.IsEnemy() && i.Type == GameObjectType.Pc && i.TargetGameObject == o);
		}

		public static float CombatDistance(this GameObject target)
		{
			if (target == null) return 0;
			var increment = BotBase.Instance.EnableCombatReachIncrement ? BotBase.Instance.CombatReachIncrement : 0;
			var targetCR = target.CombatReach;

			if (WorldManager.InPvP && target is BattleCharacter b && !b.HasAura(1420)) targetCR = 0.5f;

			//return Core.Me.Distance2D(target) - Core.Me.CombatReach - targetCR - increment;
			return target.Distance2D() - 0.5f - targetCR - increment;
		}

		public static float Distance2DSqr(this GameObject target)
		{
			float num = target.X - Core.Me.X;
			float num2 = target.Z - Core.Me.Z;
			return num * num + num2 * num2;
		}
		public static float Distance2DSqr(this GameObject target, ref Vector3 Location)
		{
			float num = target.X - Location.X;
			float num2 = target.Z - Location.Z;
			return num * num + num2 * num2;
		}
		public static float Distance2DSqr(this GameObject target, GameObject gameObject)
		{
			float num = target.X - gameObject.X;
			float num2 = target.Z - gameObject.Z;
			return num * num + num2 * num2;
		}

		public static bool MountedOnMachina(this GameObject unit)
		{
			return unit.HiddenGorgeType() == Enums.HiddenGorgeType.WarMachina;
		}

		public static HiddenGorgeType HiddenGorgeType(this GameObject unit)
		{
			if (unit is null) return Enums.HiddenGorgeType.undefined;

			switch (unit.NpcId)
			{
				case 6857:
				case 6858:
					return Enums.HiddenGorgeType.Core;
				case 6859:
				case 6860:
				case 6861:
				case 6862:
					return Enums.HiddenGorgeType.Tower;
				case 6869:
				case 6870:
				case 6871:
				case 6872:
					return Enums.HiddenGorgeType.Mammet;
				case 7889:
				case 7890:
					return Enums.HiddenGorgeType.GobTank;
				case 7891:
				case 7892:
				case 7906:
					return Enums.HiddenGorgeType.GobMercenary;
				case 9031:
					return Enums.HiddenGorgeType.CeruleumTank;
				case 9032:
				case 9033:
				case 9040:
				case 9041:
					return Enums.HiddenGorgeType.Cannon;
				default:
					if (unit is BattleCharacter c && c.HasAura(1420)) return Enums.HiddenGorgeType.WarMachina;
					if (unit.Type == GameObjectType.Pc) return Enums.HiddenGorgeType.Player;
					return Enums.HiddenGorgeType.undefined;
			}
		}

		public static bool IsHiddenGorgeCoreOrTower(this GameObject c)
		{
			return new HiddenGorgeType[] { Enums.HiddenGorgeType.Tower, Enums.HiddenGorgeType.Core }.Contains(c.HiddenGorgeType());
		}
	}
}