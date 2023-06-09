using HarmonyLib;
using UnityEngine;

// UI elements such as info cards or tooltips can be sometimes annoying and get in the way,
// for example build tooltip during placing a ceiling light tends to obscure the preview
// of the light area. Change it so that holding down Ctrl temporarily blocks hover text.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(HoverTextDrawer))]
    public class HoverTextDrawer_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SetEnabled))]
        public static void SetEnabled(HoverTextDrawer __instance, ref bool enabled)
        {
            if( !Options.Instance.HoldingCtrlBlocksHoverText )
                return;
            if( Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl ))
                enabled = false;
        }
    }
}
