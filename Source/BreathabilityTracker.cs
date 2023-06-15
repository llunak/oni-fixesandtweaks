using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

// The breathability tracker used by the 'Check low breathability' diagnostic
// counts as low oxygen (and 50% breathability) even if the duplicant has something
// providing oxygen, such as a suit. This can lead to the diagnostic showing a warning.
// https://forums.kleientertainment.com/klei-bug-tracker/oni/check-low-breathability-diagnostic-warning-triggering-for-dupes-in-suits-r40489/
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(BreathabilityTracker))]
    public class BreathabilityTracker_Patch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            // It's been fixed for u47-561558.
            return KleiVersion.ChangeList < 561558u;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(UpdateData))]
        public static IEnumerable<CodeInstruction> UpdateData(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;
            for( int i = 0; i < codes.Count; ++i )
            {
//                Debug.Log("T:" + i + ":" + codes[i].opcode + "::" + (codes[i].operand != null ? codes[i].operand.ToString() : codes[i].operand));
                // The function has code (two times):
                // if (component.IsLowOxygen())
                // Change to:
                // if (component.IsLowOxygen() && UpdateData_Hook(component))
                if( CodeInstructionExtensions.IsLdloc( codes[ i ] )
                    && codes[ i + 1 ].opcode == OpCodes.Callvirt && codes[ i + 1 ].operand.ToString() == "Boolean IsLowOxygen()"
                    && i + 2 < codes.Count
                    && codes[ i + 2 ].opcode == OpCodes.Brfalse_S )
                {
                    codes.Insert( i + 3, codes[ i ].Clone()); // load 'component'
                    codes.Insert( i + 4, CodeInstruction.Call( typeof( BreathabilityTracker_Patch ), nameof( UpdateData_Hook )));
                    codes.Insert( i + 5, codes[ i + 2 ].Clone()); // if false
                    found = true;
                    // break; do not break, there are several places
                }
            }
            if(!found)
                Debug.LogWarning("FixesAndTweaks: Failed to patch BreathabilityTracker.UpdateData()");
            return codes;
        }

        public static bool UpdateData_Hook( OxygenBreather component )
        {
            if (component.GetGasProvider() is TubeTraveller
                || component.GetGasProvider() is ClusterTelescope.ClusterTelescopeWorkable
                || component.GetGasProvider() is ClusterTelescope.ClusterTelescopeIdentifyMeteorWorkable
                || component.GetGasProvider() is Telescope
                || component.GetGasProvider() is SuitTank )
            {
                // Block counting it as low oxygen when in unbreathable environment but
                // there's something providing it.
                // This possibly should include checking if the gas provider is not empty,
                // but let's say the tracker is for environment, not for objects. And for the case
                // when the gas provider runs out there's the suffocation diagnostic, assuming
                // the dupe does abandon the gas provider first.
                return false;
            }
            return true;
        }
    }
}
