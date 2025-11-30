#if false
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

//zda se, ze dela warning, kdyz se known seed uz najde, i kdyz jeste neni identifikovane
//mam na to freezer save (jen pred dokoncenim vyzkumu)

// Normally a notification is shown only for seed of yet unknown mutations.
// Optionally show a notification also for new seeds of known mutations.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(PlantSubSpeciesCatalog))]
    public class PlantSubSpeciesCatalog_Patch
    {
        public static bool isDuringLoad = true;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DiscoverSubSpecies))]
        public static void DiscoverSubSpecies( PlantSubSpeciesCatalog __instance,
            PlantSubSpeciesCatalog.SubSpeciesInfo newSubSpeciesInfo, MutantPlant source,
            Dictionary<Tag, List<PlantSubSpeciesCatalog.SubSpeciesInfo>> ___discoveredSubspeciesBySpecies )
        {
            if( !Options.Instance.ShowNotificationForSeedsOfKnownMutations )
                return;
            // This is called also for already existing seeds when loading a game from a save. I can't find
            // a good way to tell if OnSpawn() is called during game loading or during gameplay, and things
            // like App.isLoading or Game.Instance.IsLoading() are all false by the time this is called
            // (because OnSpawn() is called during first frame rendering presumably). So the hack is to
            // use Game.OnSpawn() to mark the start of loading and unpausing of the game as the end of loading.
            if( isDuringLoad )
                return;
            if( newSubSpeciesInfo.IsOriginal )
                return;
            if( !___discoveredSubspeciesBySpecies[newSubSpeciesInfo.speciesID].Contains(newSubSpeciesInfo))
                return;
            Debug.Log("XXX MUT:" + source + ":" + source.IsIdentified);
            Notification notification = new Notification( STRINGS.FIXESANDTWEAKS.NEWMUTANTSEED.NAME, NotificationType.Good,
                NewSubspeciesTooltipCB, newSubSpeciesInfo, expires: !Options.Instance.MutatedSeedNotificationDoesNotExpire,
                0f, null, null, source.transform, show_dismiss_button: Options.Instance.MutatedSeedNotificationDoesNotExpire );
            __instance.gameObject.AddOrGet<Notifier>().Add(notification);
        }

        private static string NewSubspeciesTooltipCB( List<Notification> notifications, object data )
        {
            PlantSubSpeciesCatalog.SubSpeciesInfo subSpeciesInfo = (PlantSubSpeciesCatalog.SubSpeciesInfo)data;
            return STRINGS.FIXESANDTWEAKS.NEWMUTANTSEED.TOOLTIP.Replace("{Plant}",
                subSpeciesInfo.GetNameWithMutations( subSpeciesInfo.speciesID.ProperName(), true, false));
        }


        [HarmonyTranspiler]
        [HarmonyPatch(nameof(DiscoverSubSpecies))]
        public static IEnumerable<CodeInstruction> DiscoverSubSpecies(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;
            for( int i = 0; i < codes.Count; ++i )
            {
                Debug.Log("T:" + i + ":" + codes[i].opcode + "::" + (codes[i].operand != null ? codes[i].operand.ToString() : codes[i].operand));
                // The function has code:
                // new Notification(..., expires: true, ...);
                // I.e. it sets 'expires' to true and 'show_dismiss_button' defaults to false.
                // Change to:
                // new Notification(..., DiscoverSubSpecies_Hook1(), ..., DiscoverSubSpecies_Hook2());
                if( codes[ i ].opcode == OpCodes.Ldc_I4_1
                    && i + 9 < codes.Count
                    && codes[ i + 8 ].opcode == OpCodes.Ldc_I4_0
                    && codes[ i + 9 ].opcode == OpCodes.Newobj
                    && codes[ i + 9 ].operand.ToString()
                        == "Void .ctor(String, NotificationType, Func`3, Object, Boolean, Single, ClickCallback, Object, Transform, Boolean, Boolean, Boolean)" )
                {
                    codes[ i ] = new CodeInstruction( OpCodes.Call,
                        typeof( PlantSubSpeciesCatalog_Patch ).GetMethod( nameof( DiscoverSubSpecies_Hook1 ))); // 'expires'
                    codes[ i + 7 ] = new CodeInstruction( OpCodes.Call,
                        typeof( PlantSubSpeciesCatalog_Patch ).GetMethod( nameof( DiscoverSubSpecies_Hook2 ))); // 'show_dismiss_button'
                    found = true;
                    break;
                }
            }
            if(!found)
                Debug.LogWarning("FixesAndTweaks: Failed to patch PlantSubSpeciesCatalog.DiscoverSubSpecies()");
            return codes;
        }

        public static bool DiscoverSubSpecies_Hook1()
        {
            return !Options.Instance.MutatedSeedNotificationDoesNotExpire;
        }

        public static bool DiscoverSubSpecies_Hook2()
        {
            return Options.Instance.MutatedSeedNotificationDoesNotExpire;
        }
    }

    [HarmonyPatch(typeof(Game))]
    public class Game_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(OnSpawn))]
        public static void OnSpawn()
        {
            PlantSubSpeciesCatalog_Patch.isDuringLoad = true;
        }
    }

    [HarmonyPatch(typeof(SpeedControlScreen))]
    public class SpeedControlScreen_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(OnChanged))]
        public static void OnChanged()
        {
            if( Time.timeScale != 0 )
                PlantSubSpeciesCatalog_Patch.isDuringLoad = false;
        }
    }
}
#endif
