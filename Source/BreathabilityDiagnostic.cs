using HarmonyLib;
using STRINGS;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/false-breathability-diagnostic-warnings-r41463/
// Avoid breathability diagnostic warnings right after loading the game, when there's not enough
// data yet.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(BreathabilityDiagnostic))]
    public class BreathabilityDiagnostic_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CheckLowBreathability))]
        public static bool CheckLowBreathability( BreathabilityDiagnostic __instance,
            ref ColonyDiagnostic.DiagnosticResult __result )
        {
            if (__instance.tracker.GetDataTimeLength() < 10f)
            {
                __result = new ColonyDiagnostic.DiagnosticResult(
                    ColonyDiagnostic.DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.NO_DATA);
                return false;
            }
            return true;
        }
    }
}
