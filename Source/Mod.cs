using HarmonyLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using System.Collections.Generic;

namespace FixesAndTweaks
{
    public class Mod : KMod.UserMod2
    {
        public override void OnLoad( Harmony harmony )
        {
            base.OnLoad( harmony );
            LocString.CreateLocStringKeys( typeof( STRINGS.FIXESANDTWEAKS ));
            PUtil.InitLibrary( false );
            new POptions().RegisterOptions( this, typeof( Options ));
            CreatureCalorieMonitor_Patch.Patch( harmony );
        }

        public override void OnAllModsLoaded(Harmony harmony, IReadOnlyList<KMod.Mod> mods)
        {
            base.OnAllModsLoaded( harmony, mods );
            CopySettingsPatches.Patch( harmony );
        }
    }
}
