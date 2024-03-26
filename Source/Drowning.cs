using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/slicksters-do-not-float-and-drown-r44058/
// When a liquid crossed the substantial-liquid threshold, something related to pathfinding changes,
// but pathfinding itself is not updated. Check for drowning and make pathfinding dirty if needed.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(DrowningMonitor))]
    public class DrowningMonitor_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CheckDrowning))]
        static void CheckDrowning( DrowningMonitor __instance, bool ___drowning )
        {
            if( !___drowning )
                return;
            if( !__instance.canDrownToDeath )
                return; // The code is used also for lettuce, which cannot move or drown.
            int cell = Grid.PosToCell( __instance.gameObject );
            if( IsPossiblyDrowningChamber( cell ))
                return;
            Pathfinding.Instance.AddDirtyNavGridCell( cell );
        }

        static bool IsPossiblyDrowningChamber( int cell )
        {
            // I'm not sure how big the performance impact of making the pathfinding dirty is,
            // but it can't hurt to avoid the common case of a drowning chamber, in which
            // case the critter cannot escape the drowning anyway.
            CavityInfo cavity = Game.Instance.roomProber.GetCavityForCell( cell );
            if( cavity == null )
                return false;
            if( cavity.numCells > 50 )
                return false;
            for( int x = cavity.minX; x <= cavity.maxX; ++x )
                for( int y = cavity.minY; x <= cavity.maxY; ++x )
                {
                    int xyCell = Grid.XYToCell( x, y );
                    if( !Grid.IsSubstantialLiquid( Grid.XYToCell( x, y )))
                        return false;
                }
            return true;
        }
    }
}
