using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/disabled-power-transformer-shows-no-wire-status-even-when-it-is-connected-r41231/
// A disabled Power Transformer shows a red 'No Wire' status, even if it is actually connected and only disabled.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(RequireInputs))]
    public class RequireInputs_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(CheckRequirements))]
        public static IEnumerable<CodeInstruction> CheckRequirements(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;
            for( int i = 0; i < codes.Count; ++i )
            {
//                Debug.Log("T:" + i + ":" + codes[i].opcode + "::" + (codes[i].operand != null ? codes[i].operand.ToString() : codes[i].operand));
                // The function has code:
                // bool show2 = VisualizeRequirement(Requirements.NoWire) && !isConnected;
                // Change to:
                // bool show2 = VisualizeRequirement(Requirements.NoWire) && CheckRequirements_Hook(this) && !isConnected;
                if( codes[ i ].opcode == OpCodes.Ldarg_0 && i + 3 < codes.Count
                    && codes[ i + 1 ].opcode == OpCodes.Ldc_I4_1 // 1 == Requirements.NoWire
                    && codes[ i + 2 ].opcode == OpCodes.Call
                    && codes[ i + 2 ].operand.ToString().Contains( "Boolean VisualizeRequirement(Requirements)" )
                    && codes[ i + 3 ].opcode == OpCodes.Brfalse_S )
                {
                    codes.Insert( i + 4, codes[ i ].Clone()); // load 'this'
                    codes.Insert( i + 5, new CodeInstruction( OpCodes.Call,
                        typeof( RequireInputs_Patch ).GetMethod( nameof( CheckRequirements_Hook ))));
                    codes.Insert( i + 6, codes[ i + 3 ].Clone()); // copy the Brfalse_S
                    found = true;
                    break;
                }
            }
            if(!found)
                Debug.LogWarning("FixesAndTweaks: Failed to patch RequireInputs.CheckRequirements()");
            return codes;
        }

        public static bool CheckRequirements_Hook( RequireInputs instance )
        {
            PowerTransformer transformer = instance.GetComponent< PowerTransformer >();
            if( transformer == null )
                return true;
            if( transformer.IsProducingPower())
                return false;
            return false; // do not show the no-wire status, transformer is disabled
        }
    }
}
