using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/disabled-power-transformer-shows-no-wire-status-even-when-it-is-connected-r41231/
// A disabled Power Transformer shows a red 'No Wire' status, even if it is actually connected and only disabled.
namespace FixesAndTweaks
{
    // Track RequireInputs -> PowerTransfomer mapping to avoid GetComponent<> calls.
    [HarmonyPatch(typeof(PowerTransformer))]
    public static class PowerTransformer_Patch
    {
        public static Dictionary< RequireInputs, PowerTransformer > transformers
            = new Dictionary< RequireInputs, PowerTransformer >();

        public static PowerTransformer Get( RequireInputs inputs )
        {
            if( transformers.TryGetValue( inputs, out PowerTransformer transformer ))
                return transformer;
            return null;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(OnSpawn))]
        public static void OnSpawn(PowerTransformer __instance)
        {
            transformers[ __instance.GetComponent< RequireInputs >() ] = __instance;
        }
    }

    // PowerTransformer inherits OnCleanUp(), so patch that one.
    [HarmonyPatch(typeof(Generator))]
    public static class Generator_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(OnCleanUp))]
        public static void OnCleanUp(Generator __instance)
        {
            RequireInputs inputs = __instance.GetComponent< RequireInputs >();
            if( inputs != null )
                PowerTransformer_Patch.transformers.Remove( inputs );
        }
    }

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
            PowerTransformer transformer = PowerTransformer_Patch.Get( instance );
            if( transformer == null )
                return true;
            if( transformer.IsProducingPower())
                return true;
            return false; // do not show the no-wire status, transformer is disabled
        }
    }
}
