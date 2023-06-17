using HarmonyLib;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/oxylite-not-counted-as-produced-oxygen-causes-insufficient-oxygen-generation-r40908/
// Oxygen generated from oxylite is not counted as produced oxygen, which may lead to the 'Insufficient oxygen generation'
// warning triggering unnecessarily. With oxylite meteors it is reasonable to provide oxygen this way.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(Sublimates))]
    public class Sublimates_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Emit))]
        private static void Emit(Sublimates __instance, float mass)
        {
            if( __instance.info.sublimatedElement == SimHashes.Oxygen )
            {
                ReportManager.Instance.ReportValue(ReportManager.ReportType.OxygenCreated,
                    mass, __instance.gameObject.GetProperName());
            }
        }
    }
}
