using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

// The timelapse recording is broken once another planetoid is discovered:
// - Other planetoid are not saved at all (incorrect calculations).
// - Main planetoid picture uses dimensions of all built buildings on the entire world.
// https://forums.kleientertainment.com/klei-bug-tracker/oni/timelapse-broken-with-the-dlc-fixes-r40128/
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(Timelapser))]
    public class Timelapser_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(OnNewDay))]
        public static void OnNewDay(object data, int[] ___timelapseScreenshotCycles, ref bool ___screenshotToday,
             ref List<int> ___worldsToScreenshot)
        {
            int cycle = GameClock.Instance.GetCycle();
            if( cycle == 0 ) // Game start (called during dupe selection).
                return;
            foreach (WorldContainer worldContainer in ClusterManager.Instance.WorldContainers)
            {
                if (!worldContainer.IsDiscovered || worldContainer.IsModuleInterior)
                    continue;
                if (cycle - (int)worldContainer.DiscoveryTimestamp > ___timelapseScreenshotCycles[___timelapseScreenshotCycles.Length - 1])
                {
                    if (((cycle - (int)worldContainer.DiscoveryTimestamp) % 10) == 0)
                    {
                        if(!___worldsToScreenshot.Contains(worldContainer.id))
                        {
                            ___screenshotToday = true;
                            ___worldsToScreenshot.Add(worldContainer.id);
                        }
                    }
                    continue;
                }
                for (int i = 0; i < ___timelapseScreenshotCycles.Length; i++)
                {
                    if (cycle - (int)worldContainer.DiscoveryTimestamp == ___timelapseScreenshotCycles[i])
                    {
                        if(!___worldsToScreenshot.Contains(worldContainer.id))
                        {
                            ___screenshotToday = true;
                            ___worldsToScreenshot.Add(worldContainer.id);
                        }
                    }
                }
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SetPostionAndOrtho))]
        public static IEnumerable<CodeInstruction> SetPostionAndOrtho(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;
            for( int i = 0; i < codes.Count; ++i )
            {
//                Debug.Log("T:" + i + ":" + codes[i].opcode + "::" + (codes[i].operand != null ? codes[i].operand.ToString() : codes[i].operand));
                // The function has code:
                // foreach (BuildingComplete item in Components.BuildingCompletes.Items)
                // {
                //     Vector3 position2 = item.transform.GetPosition();
                // Insert as the first statement in the loop:
                //     if( !SetPostionAndOrtho_Hook( world_id, item ))
                //         continue;
                if( codes[ i ].opcode == OpCodes.Br_S && i + 2 < codes.Count
                    && codes[ i + 1 ].opcode == OpCodes.Ldloca_S
                    && codes[ i + 1 ].operand.ToString().Contains( "Enumerator[BuildingComplete]" )
                    && codes[ i + 2 ].opcode == OpCodes.Call
                    && codes[ i + 2 ].operand.ToString() == "BuildingComplete get_Current()" )
                {
                    codes.Insert( i + 3, new CodeInstruction( OpCodes.Dup )); // duplicate 'item'
                    codes.Insert( i + 4, new CodeInstruction( OpCodes.Ldarg_1 )); // load 'world_id'
                    codes.Insert( i + 5, new CodeInstruction( OpCodes.Call,
                        typeof( Timelapser_Patch ).GetMethod( nameof( SetPostionAndOrtho_Hook ))));
                    codes.Insert( i + 6, codes[ i ].Clone()); // copy the Br_S
                    codes[ i + 6 ].opcode = OpCodes.Brfalse_S;
                    found = true;
                    break;
                }
            }
            if(!found)
                Debug.LogWarning("FixesAndTweaks: Failed to patch Timelapser.SetPostionAndOrtho()");
            return codes;
        }

        public static bool SetPostionAndOrtho_Hook( BuildingComplete item, int world_id )
        {
            // Only count buildings on the same world.
            return item.gameObject.GetMyWorldId() == world_id;
        }
    }
}
