using System.Threading.Tasks;
using ff14bot.Directors;
using ff14bot.Managers;

namespace Kombatant.Interfaces
{
    /// <summary>
    /// This class provides the base methods for the three roles (tank, healer, dps) as those should only be
    /// overridden when absolutely necessary.
    /// </summary>
    // TODO: Switch back to an interface with C# v8.
    internal abstract class LogicExecutor
    {
        /// <summary>
        /// Main executor for the logic.
        /// </summary>
        /// <returns>Did the logic run?</returns>
        // ReSharper disable once UnusedMember.Global
        internal async Task<bool> ExecuteLogic()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Determines whether the Pull behavior should be executed.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldExecutePull()
        {
            return Settings.BotBase.Instance.EnablePull && RoutineManager.Current.PullBehavior != null;
        }

        /// <summary>
        /// Determines whether the PullBuff behavior should be executed.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldExecutePullBuff()
        {
            return Settings.BotBase.Instance.EnablePullBuff && RoutineManager.Current.PullBuffBehavior != null;
        }

        /// <summary>
        /// Determines whether the PreCombatBuff behavior should be executed.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldExecutePreCombatBuff()
        {
	        if (DirectorManager.ActiveDirector is InstanceContentDirector icDirector && icDirector.InstanceEnded)
	        {
		        return false;
	        }

            return Settings.BotBase.Instance.EnablePreCombatBuff && RoutineManager.Current.PreCombatBuffBehavior != null;
        }

        /// <summary>
        /// Determines whether the Rest behavior should be executed.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldExecuteRest()
        {
            return Settings.BotBase.Instance.EnableRest && RoutineManager.Current.RestBehavior != null;
        }

        /// <summary>
        /// Determines whether the Combat behavior should be executed.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldExecuteCombat()
        {
            return Settings.BotBase.Instance.EnableCombat && RoutineManager.Current.CombatBehavior != null;
        }

        /// <summary>
        /// Determines whether the CombatBuff behavior should be executed.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldExecuteCombatBuff()
        {
            return Settings.BotBase.Instance.EnableCombatBuff && RoutineManager.Current.CombatBuffBehavior != null;
        }

        /// <summary>
        /// Determines whether the Heal behavior should be executed.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldExecuteInCombatHeal()
        {
            return Settings.BotBase.Instance.EnableHealInCombat && RoutineManager.Current.HealBehavior != null;
        }

        protected bool ShouldExecuteOutOfCombatHeal()
        {
	        return Settings.BotBase.Instance.EnableHealOutofCombat && RoutineManager.Current.HealBehavior != null;
        }

        /// <summary>
        /// Determines whether the Death behavior should be executed.
        /// </summary>
        /// <returns></returns>
        protected bool ShouldExecuteDeath()
        {
	        return Settings.BotBase.Instance.EnableDeath && RoutineManager.Current.DeathBehavior != null;
        }
    }
}