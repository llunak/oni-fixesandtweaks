using HarmonyLib;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/game-crashes-with-a-space-scanner-near-the-edge-of-a-map-r41476/
// SkyVisibilityInfo will count cells for Space Scanner near an edge even if technically they are on another planetoid,
// leading to IndexOutOfRangeException in SpaceScannerNetworkManager.CalcWorldNetworkQuality().
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(SkyVisibilityInfo))]
    public class SkyVisibilityInfo_Patch
    {
        // IsVisible() doesn't have access to the origin cell, so save it from the functions that call IsVisible().
        private static int originCellId;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(IsVisible))]
        public static void IsVisible( ref bool __result, int cellId )
        {
            if( __result )
            {
                WorldContainer world = ClusterManager.Instance.GetWorld(Grid.WorldIdx[cellId]);
                WorldContainer originWorld = ClusterManager.Instance.GetWorld(Grid.WorldIdx[originCellId]);
                if( world != originWorld )
                    __result = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ScanAndCollectVisibleCellsTo))]
        private static void ScanAndCollectVisibleCellsTo(int originCellId)
        {
            SkyVisibilityInfo_Patch.originCellId = originCellId;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ScanAndGetVisibleCellCount))]
        private static void ScanAndGetVisibleCellCount(int originCellId)
        {
            SkyVisibilityInfo_Patch.originCellId = originCellId;
        }
    }
}
