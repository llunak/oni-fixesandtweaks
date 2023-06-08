using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using STRINGS;

// The 'Check enough food' diagnostic only checks the planetoid itself, but not landed rockets,
// which means it's possible to have the diagnostic warn about not having enough food even
// though the rocket has enough available (happens e.g. when colonizing a new planetoid).
// Moreover the calculation assumes 3000 kcal per dupe, which is 3 days, but only on normal
// difficulty. Fix all these, also make the amounts configurable, and have two warning levels.
// This only changes the warning, the value shown on the planetoid is unaffected.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(FoodDiagnostic))]
    public class FoodDiagnostic_Patch
    {
        // This is intentionally a postfix, even though it replaces the original code.
        // The reason for this is that the Stock Bug Fix mod also fixes a problem
        // in the code and returns false in its prefix, blocking the original code
        // and other prefixes after it, so whether this would be called would depend
        // on mod order. Since this is a superset of what that mod fixes, always
        // use this code (a prefix cannot block a postfix).
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CheckEnoughFood))]
        public static void CheckEnoughFood(FoodDiagnostic __instance, ref ColonyDiagnostic.DiagnosticResult __result,
            float ___trackerSampleCountSeconds)
        {
            if (__instance.tracker.GetDataTimeLength() < 10f)
                return;
            // Check again, this time include rockets, count per-dupe need properly, and add two configurable thresholds.
            __result = new ColonyDiagnostic.DiagnosticResult(
                ColonyDiagnostic.DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
            List<MinionIdentity> worldItems = new List<MinionIdentity>();
            float totalAverageValue = 0;
            float totalCurrentValue = 0;
            foreach (WorldContainer worldContainer in ClusterManager.Instance.WorldContainers)
            {
                if( worldContainer.id == __instance.worldID
                    || ( Options.Instance.CheckEnoughFoodDiagnosticIncludesRockets
                        && worldContainer.ParentWorldId == __instance.worldID ))
                {
                    worldItems.AddRange( Components.LiveMinionIdentities.GetWorldItems(worldContainer.id));
                    Tracker tracker = TrackerTool.Instance.GetWorldTracker<KCalTracker>(worldContainer.id);
                    if (tracker.GetDataTimeLength() < 10f)
                        continue;
                    // The tracker does not count food in restricted buildings, so this is enough to handle rockets.
                    totalAverageValue += tracker.GetAverageValue(___trackerSampleCountSeconds);
                    totalCurrentValue += tracker.GetCurrentValue();
                }
            }
            float totalCaloriesPerCycle = 0;
            foreach( MinionIdentity minion in worldItems )
            {
                float caloriesPerSecond = Db.Get().Amounts.Calories.Lookup( minion ).GetDelta();
                if( caloriesPerSecond != float.PositiveInfinity ) // not lowest hunger difficulty
                    totalCaloriesPerCycle += caloriesPerSecond * Constants.SECONDS_PER_CYCLE * -1;
            }
            if( totalCaloriesPerCycle == 0 )
                return;
            if( totalAverageValue <= totalCaloriesPerCycle * Options.Instance.CheckEnoughFoodDiagnosticWarningCycles )
                __result.opinion = ColonyDiagnostic.DiagnosticResult.Opinion.Warning;
            else if( totalAverageValue <= totalCaloriesPerCycle * Options.Instance.CheckEnoughFoodDiagnosticConcernCycles )
                __result.opinion = ColonyDiagnostic.DiagnosticResult.Opinion.Concern;
            else
                return;
            float currentValue = totalCurrentValue;
            string formattedCalories = GameUtil.GetFormattedCalories(currentValue);
            string formattedCalories2 = GameUtil.GetFormattedCalories(totalCaloriesPerCycle);
            string text = MISC.NOTIFICATIONS.FOODLOW.TOOLTIP;
            text = text.Replace("{0}", formattedCalories);
            text = (__result.Message = text.Replace("{1}", formattedCalories2));
        }
    }
}
