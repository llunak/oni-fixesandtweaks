using HarmonyLib;
using UnityEngine;
using System;
using System.Reflection;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/copy-settings-between-more-compatible-buildings-r51119/
// 'Copy Settings' doesn't work between some related buildings, such as ration box and refrigerator.
namespace FixesAndTweaks;

public class CopySettingsPatches
{
    private static readonly Tag TagCritterTransport = TagManager.Create("FixesAndTweaks.CritterTransport");

    private static readonly Tag TagFoodStorageMy = TagManager.Create("FixesAndTweaks.FoodStorage");
    private static Tag TagFoodStorage = null;

    private static readonly Tag TagEmptier = TagManager.Create("FixesAndTweaks.Emptier");
    private static readonly Tag TagEmptierGas = TagManager.Create("FixesAndTweaks.EmptierGas");

    public static void Patch( Harmony harmony )
    {
        Patch( typeof(CritterDropOffConfig), PatchCritterTransport, harmony );
        Patch( typeof(CritterPickUpConfig), PatchCritterTransport, harmony );

        Patch( typeof(RationBoxConfig), PatchFoodStorage, harmony );
        Patch( typeof(RefrigeratorConfig), PatchFoodStorage, harmony );
        // Only after those above, so that TagFoodStorage is set by them.
        Patch( Type.GetType( "Psyko.Freezer.FreezerConfig, Freezer" ), PatchFoodStorageFreezer, harmony );

        Patch( typeof(SolidConduitInboxConfig), PatchStorageLocker, harmony );

        Patch( typeof(BottleEmptierConfig), PatchEmptier, harmony );
        Patch( typeof(BottleEmptierConduitLiquidConfig), PatchEmptier, harmony );

        Patch( typeof(BottleEmptierGasConfig), PatchEmptierGas, harmony );
        Patch( typeof(BottleEmptierConduitGasConfig), PatchEmptierGas, harmony );
    }

    private static void Patch( Type type, Action<GameObject> function, Harmony harmony )
    {
        if( type == null )
            return;
        MethodInfo info = AccessTools.Method( type, "DoPostConfigureComplete");
        if( info != null )
            harmony.Patch( info, postfix: new HarmonyMethod( function.Method ));
        else
            Debug.LogError("FixesAndTweaks: Failed to patch CopySettings for " + type);
    }

    // All of these are postfixes for void DoPostConfigureComplete(GameObject go) in building *Config classes.

    public static void PatchCritterTransport(GameObject go)
    {
        SetupCopySettings(go, TagCritterTransport);
    }

    public static void PatchFoodStorage(GameObject go)
    {
        // Try to set this mod's tag, but if already present, save that for the Freezer mod (so if Klei fixes
        // this at some point, Freezer will use the vanilla tag).
        TagFoodStorage = SetupCopySettings(go, TagFoodStorageMy);
    }

    public static void PatchFoodStorageFreezer(GameObject go)
    {
        // Copy tag from vanilla buildings.
        SetupCopySettings(go, TagFoodStorage);
    }

    public static void PatchStorageLocker(GameObject go)
    {
        SetupCopySettings(go, GameTags.StorageLocker);
    }

    public static void PatchEmptier(GameObject go)
    {
        SetupCopySettings(go, TagEmptier);
    }

    public static void PatchEmptierGas(GameObject go)
    {
        SetupCopySettings(go, TagEmptierGas);
    }

    private static Tag SetupCopySettings(GameObject go, Tag copyTag)
    {
        CopyBuildingSettings settings = go.AddOrGet<CopyBuildingSettings>();
        // Do not overwrite existing copy group, in case Klei fixes this one day.
        if(settings.copyGroupTag == Tag.Invalid)
            settings.copyGroupTag = copyTag;
        return settings.copyGroupTag;
    }
}
