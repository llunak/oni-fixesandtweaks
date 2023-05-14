using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using STRINGS;

// The 'Check enough food' diagnostic only checks the planetoid itself, but not landed rockets,
// which means it's possible to have the diagnostic warn about not having enough food even
// though the rocket has enough available (happens e.g. when colonizing a new planetoid).
// If the diagnostic would trigger, check again with all subworlds.
// This only changes the warning, the value shown on the planetoid is unaffected.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(FoodDiagnostic))]
    public class FoodDiagnostic_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CheckEnoughFood))]
        public static void CheckEnoughFood(FoodDiagnostic __instance, ref ColonyDiagnostic.DiagnosticResult __result,
            float ___trackerSampleCountSeconds)
        {
            if( !Options.Instance.CheckEnoughFoodDiagnosticIncludesRockets )
                return;
            if( __result.opinion != ColonyDiagnostic.DiagnosticResult.Opinion.Concern )
                return;
            // Check again, this time include rockets.
            List<MinionIdentity> worldItems = Components.LiveMinionIdentities.GetWorldItems(__instance.worldID);
            bool hasSubWorlds = false;
            float totalAverageValue = __instance.tracker.GetAverageValue(___trackerSampleCountSeconds);
            float totalCurrentValue = __instance.tracker.GetCurrentValue();
            foreach (WorldContainer worldContainer in ClusterManager.Instance.WorldContainers)
            {
                if (worldContainer.ParentWorldId == __instance.worldID && worldContainer.id != __instance.worldID)
                {
                    worldItems.AddRange( Components.LiveMinionIdentities.GetWorldItems(worldContainer.id));
                    Tracker tracker = TrackerTool.Instance.GetWorldTracker<KCalTracker>(worldContainer.id);
                    if (tracker.GetDataTimeLength() < 10f)
                        continue;
                    // The tracker does not count food in restricted buildings, so this is enough.
                    totalAverageValue += tracker.GetAverageValue(___trackerSampleCountSeconds);
                    totalCurrentValue += tracker.GetCurrentValue();
                    hasSubWorlds = true;
                }
            }
            if( !hasSubWorlds )
                return;
            // Calculate again with all worlds. No need to check track.GetDataTimeLength() again.
            int num = 3000;
            if ((float)worldItems.Count * (1000f * (float)num) > totalAverageValue)
            {
                __result.opinion = ColonyDiagnostic.DiagnosticResult.Opinion.Concern;
                float currentValue = totalCurrentValue;
                float f = worldItems.Count * -1000000f;
                string formattedCalories = GameUtil.GetFormattedCalories(currentValue);
                string formattedCalories2 = GameUtil.GetFormattedCalories(Mathf.Abs(f));
                string text = MISC.NOTIFICATIONS.FOODLOW.TOOLTIP;
                text = text.Replace("{0}", formattedCalories);
                text = (__result.Message = text.Replace("{1}", formattedCalories2));
            }
            else
            {
                __result = new ColonyDiagnostic.DiagnosticResult(
                    ColonyDiagnostic.DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
            }
        }
    }
}
