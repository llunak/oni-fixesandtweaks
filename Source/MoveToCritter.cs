using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/move-to-for-critters-is-a-store-chore-should-be-ranching-r43765/
// The chore type for 'Move To' for critters is Store, but it requires a rancher to perform it.
// If the rancher has lower priority for Store than for Ranching (and possibly others), which
// is the reasonable setup, then the task will take a long time to perform, until the rancher
// finally gets to doing stores.
// Changes the Chore type of such tasks to Ranching, the same as for wrangling.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(MovePickupableChore))]
    public class MovePickupableChore_Patch
    {
        [HarmonyPrepare]
        public static bool Prepare()
        {
            // It's been fixed for u51-596100u.
            return KleiVersion.ChangeList < 596100u;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[] { typeof(IStateMachineTarget), typeof(GameObject), typeof(Action<Chore>) })]
        public static IEnumerable<CodeInstruction> ctor(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;
            for( int i = 0; i < codes.Count; ++i )
            {
//                Debug.Log("T:" + i + ":" + codes[i].opcode + "::" + (codes[i].operand != null ? codes[i].operand.ToString() : codes[i].operand));
            }
            // The function has code, at the very beginning:
            // .. : base(Db.Get().ChoreTypes.Fetch, ...
            // Change to:
            // .. : base(ctor_Hook(Db.Get().ChoreTypes.Fetch, pickupable), ...
            if( codes[ 0 ].opcode == OpCodes.Ldarg_0 // load 'this'
                && 4 < codes.Count
                && codes[ 1 ].opcode == OpCodes.Call
                && codes[ 1 ].operand.ToString() == "Db Get()"
                && codes[ 2 ].opcode == OpCodes.Ldfld
                && codes[ 2 ].operand.ToString() == "Database.ChoreTypes ChoreTypes"
                && codes[ 3 ].opcode == OpCodes.Ldfld
                && codes[ 3 ].operand.ToString() == "ChoreType Fetch" )
            {
                codes.Insert( 4, new CodeInstruction( OpCodes.Ldarg_2 )); // load 'pickupable'
                codes.Insert( 5, CodeInstruction.Call( typeof( MovePickupableChore_Patch ), nameof( ctor_Hook )));
                found = true;
            }
            if(!found)
                Debug.LogWarning("FixesAndTweaks: Failed to patch MovePickupableChore ctor()");
            return codes;
        }

        public static ChoreType ctor_Hook( ChoreType choreType, GameObject pickupable )
        {
            if((bool)pickupable.GetComponent<CreatureBrain>())
                return Db.Get().ChoreTypes.Capture;
            return choreType;
        }
    }
}
