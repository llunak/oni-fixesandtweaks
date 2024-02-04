using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

// The 'Critter Starvation' notification shows only for a short while and then expires.
// This makes it stay shown as long as it applies (optional, enabled by default).
namespace FixesAndTweaks
{
    public static class CreatureCalorieMonitor_Patch
    {
        public static void Patch( Harmony harmony )
        {
            bool done = false;
            MethodInfo oldMethod;
            MethodInfo newMethod;
            Type nestedClass = typeof(CreatureCalorieMonitor).GetNestedType("<>c", BindingFlags.NonPublic);
            if(nestedClass != null)
            {
                oldMethod = AccessTools.Method(nestedClass, "<InitializeStates>b__9_10");
                newMethod = typeof(CreatureCalorieMonitor_Patch).GetMethod("InitializeStates_delegate");
                if(oldMethod != null)
                {
                    harmony.Patch(oldMethod, transpiler: new HarmonyMethod(newMethod));
                    done = true;
                }
            }
            if(!done)
                Debug.LogWarning("FixesAndTweaks: Failed to find CreatureCalorieMonitor notification for patching");
        }

        public static IEnumerable<CodeInstruction> InitializeStates_delegate(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;
            for( int i = 0; i < codes.Count; ++i )
            {
                // Debug.Log("T:" + i + ":" + codes[i].opcode + "::" + (codes[i].operand != null ? codes[i].operand.ToString() : codes[i].operand));
                // Changes 'expires' parameter from true to the return value of our hook function.
                // Do not match the OpCodes.Newobj call, to possibly keep this working even if the Notification
                // constructor gets additional parameters.
                if(codes[i].opcode == OpCodes.Stsfld
                    && i+3 < codes.Count
                    && codes[i+1].opcode == OpCodes.Ldnull
                    && codes[i+2].opcode == OpCodes.Ldc_I4_1
                    && codes[i+3].opcode == OpCodes.Ldc_R4 && codes[i+3].operand.ToString() == "0")
                {
                    codes[i+2] = new CodeInstruction(OpCodes.Call,
                        typeof(CreatureCalorieMonitor_Patch).GetMethod(nameof(InitializeStates_Hook)));
                    found = true;
                    break;
                }
            }
            if(!found)
                Debug.LogWarning("FixesAndTweaks: Failed to patch CreatureCalorieMonitor notification");
            return codes;
        }

        public static bool InitializeStates_Hook()
        {
            return !Options.Instance.BlockCritterStarvationNotificationExpiration;
        }
    }
}
