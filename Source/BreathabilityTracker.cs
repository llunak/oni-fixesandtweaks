using HarmonyLib;
using System.Reflection;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/false-breathability-diagnostic-warnings-r41463/
// Prevent breathability tracker from keeping a history full of 0% breathability for empty worlds,
// which would cause breathability diagnostic warning when a duplicant enters this world and the average
// calculated is affected by these 0's. This means the tracker won't really track empty worlds, but it
// shouldn't matter for empty worlds.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(BreathabilityTracker))]
    public class BreathabilityTracker_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(UpdateData))]
        [HarmonyPriority(Priority.High)] // E.g. FastTrack replaces the implementation, so we need to go first.
        public static bool UpdateData( BreathabilityTracker __instance )
        {
            if( Components.LiveMinionIdentities.GetWorldItems( __instance.WorldID ).Count == 0 )
                return false;
            return true;
        }
    }
}
