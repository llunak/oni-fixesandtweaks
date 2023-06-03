using HarmonyLib;
using System.Collections.Generic;
using STRINGS;

namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(PowerUseDiagnostic))]
    public class PowerUseDiagnostic_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CheckPowerChange))]
        public static bool CheckPowerChange( ref ColonyDiagnostic.DiagnosticResult __result )
        {
            if( !Options.Instance.BlockPowerChangeDiagnostic )
                return true;
            __result = new ColonyDiagnostic.DiagnosticResult(
                ColonyDiagnostic.DiagnosticResult.Opinion.Normal, UI.COLONY_DIAGNOSTICS.GENERIC_CRITERIA_PASS);
            return false;
        }
    }
}
