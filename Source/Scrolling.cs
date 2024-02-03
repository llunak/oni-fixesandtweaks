using HarmonyLib;

// Horizontal and vertical scrolling is way too slow (annoying e.g. in Consumables view).
// Make it faster.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(KScrollRect))]
    public class KScrollRect_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor)]
        public static void ctor(ref float ___verticalScrollInertiaScale,
            ref float ___horizontalScrollInertiaScale)
        {
            if( Options.Instance.FasterVerticalScrolling )
            {
                if( ___verticalScrollInertiaScale < 20 )
                    ___verticalScrollInertiaScale = 20;
            }
            if( Options.Instance.FasterHorizontalScrolling )
            {
                if( ___horizontalScrollInertiaScale < 20 )
                    ___horizontalScrollInertiaScale = 20;
            }
        }
    }
}
