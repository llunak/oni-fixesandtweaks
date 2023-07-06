using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;

// The 'Check exposed' diagnostic triggers only depending on the current exposure per cycle
// and it ignores actual exposure of the dupe (i.e. accumulated dose). This means that running past
// high radiation triggers the warning even if the received radiation will be small and thus safe.
// Trigger the diagnostic only if the dupe has already accumualated some radiation, similarly
// to the 'Check sick' diagnostic.
// Additionally, the game code uses LT (less-than) instead of GTE for checking minor exposure,
// so fix this.
// (https://forums.kleientertainment.com/klei-bug-tracker/oni/check-exposed-radiation-diagnostic-has-inverted-condition-r40487/)
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(RadiationDiagnostic))]
    public class RadiationDiagnostic_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(CheckExposure))]
        public static IEnumerable<CodeInstruction> CheckExposure(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found1 = false;
            bool found2 = false;
            bool found3 = false;
            int radiationExposureLoad = -1;
            int minionLoad = -1;
            for( int i = 0; i < codes.Count; ++i )
            {
//                Debug.Log("T:" + i + ":" + codes[i].opcode + "::" + (codes[i].operand != null ? codes[i].operand.ToString() : codes[i].operand));
                if( codes[ i ].opcode == OpCodes.Ldsfld && codes[ i ].operand.ToString().EndsWith( " COMPARE_RECOVERY_IMMEDIATE" )
                    && i + 3 < codes.Count
                    && codes[ i + 1 ].IsLdloc()
                    && codes[ i + 2 ].IsLdloc()
                    && codes[ i + 3 ].opcode == OpCodes.Callvirt
                    && codes[ i + 3 ].operand.ToString() == "Boolean Invoke(Instance, Single)" )
                {
                    radiationExposureLoad = i;
                }
                if( codes[ i ].opcode == OpCodes.Call
                    && codes[ i ].operand.ToString() == "MinionIdentity get_Current()"
                    && i + 2 < codes.Count
                    && codes[ i + 1 ].IsStloc()
                    && codes[ i + 2 ].IsLdloc())
                {
                    minionLoad = i + 2;
                }
                // The function has code:
                // float p = sm.currentExposurePerCycle.Get(sMI);
                // Change to:
                // float p = CheckExposure_Hook1(sm.currentExposurePerCycle.Get(sMI), item);
                if( codes[ i ].opcode == OpCodes.Ldfld
                    && codes[ i ].operand.ToString().EndsWith( " currentExposurePerCycle" )
                    && minionLoad != -1 && i + 3 < codes.Count
                    && codes[ i + 1 ].IsLdloc()
                    && codes[ i + 1 ].operand.ToString().StartsWith( "RadiationMonitor+Instance " )
                    && codes[ i + 2 ].opcode == OpCodes.Callvirt
                    && codes[ i + 2 ].operand.ToString() == "Single Get(Instance)"
                    && codes[ i + 3 ].IsStloc())
                {
                    codes.Insert( i + 3, codes[ minionLoad ].Clone()); // load 'item'
                    codes.Insert( i + 4, CodeInstruction.Call( typeof( RadiationDiagnostic_Patch ), nameof( CheckExposure_Hook1 )));
                    // The original stloc at i+3 will end up here.
                    found1 = true;
                }
                // The function has code:
                // if (RadiationMonitor.COMPARE_LT_MINOR(sMI, p)...)
                // Change to:
                // if (RadiationMonitor.COMPARE_GTE_MINOR(sMI, p)...)
                if( codes[ i ].opcode == OpCodes.Ldsfld && codes[ i ].operand.ToString().EndsWith( " COMPARE_LT_MINOR" ))
                {
                    codes[ i ] = CodeInstruction.LoadField( typeof( RadiationMonitor ), "COMPARE_GTE_MINOR" );
                    found2 = true;
                }
                // The function has code:
                // if (RadiationMonitor.COMPARE_GTE_DEADLY(sMI, p))
                // Change to:
                // if (... && CheckExposure_Hook2(sMI, p2))
                if( codes[ i ].opcode == OpCodes.Ldsfld && codes[ i ].operand.ToString().EndsWith( " COMPARE_GTE_DEADLY" )
                    && radiationExposureLoad != -1 && i + 4 < codes.Count
                    && codes[ i + 1 ].IsLdloc()
                    && codes[ i + 2 ].IsLdloc()
                    && codes[ i + 3 ].opcode == OpCodes.Callvirt
                    && codes[ i + 3 ].operand.ToString() == "Boolean Invoke(Instance, Single)"
                    && codes[ i + 4 ].opcode == OpCodes.Brfalse_S )
                {
                    codes.Insert( i + 5, codes[ radiationExposureLoad + 1 ].Clone()); // load 'RadiationMonitor.Instance'
                    codes.Insert( i + 6, codes[ radiationExposureLoad + 2 ].Clone()); // load 'p2'
                    codes.Insert( i + 7, CodeInstruction.Call( typeof( RadiationDiagnostic_Patch ), nameof( CheckExposure_Hook2 )));
                    codes.Insert( i + 8, codes[ i + 4 ].Clone()); // if false
                    found3 = true;
                }
            }
            if(!found1 || !found2 || !found3)
                Debug.LogWarning("FixesAndTweaks: Failed to patch RadiationDiagnostic.CheckExposure()");
            return codes;
        }

        public static float CheckExposure_Hook1( float radiationExposure, MinionIdentity minion )
        {
            // Apply also the rejuvenation effect of e.g. rad pills, so that duplicants that are actually
            // losing radiation do not trigger the diagnostic. It's already negative, so add it.
            radiationExposure += Db.Get().Attributes.RadiationRecovery.Lookup( minion.gameObject ).GetTotalValue()
                * Constants.SECONDS_PER_CYCLE;
            return Mathf.Max( 0, radiationExposure );
        }

        public static bool CheckExposure_Hook2( RadiationMonitor.Instance sMI, float p2 )
        {
            if( !Options.Instance.ReducedRadiationDiagnostic )
                return true;
            return RadiationMonitor.COMPARE_RECOVERY_IMMEDIATE( sMI, p2 );
        }
    }
}
