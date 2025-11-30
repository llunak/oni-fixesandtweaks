using HarmonyLib;
using STRINGS;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/rocket-is-loading-crew-even-if-it-has-no-crew-assigned-r40371/
// It is possible to hit 'Begin Launch Sequence' and get a rocket to status 'Loading Crew...' even if the rocket
// has no crew assigned, thus waiting there forever. Disable the button in that case and provide 'No Crew Assigned' status.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(LaunchButtonSideScreen))]
    public class LaunchButtonSideScreen_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Refresh))]
        public static void Refresh( KButton ___launchButton, LocText ___statusText,
            RocketModuleCluster ___rocketModule, LaunchPad ___selectedPad )
        {
            if( !___launchButton.isInteractable
                || !( ___statusText.text == UI.UISIDESCREENS.LAUNCHPADSIDESCREEN.STATUS.LOADING_CREW
                        || ___statusText.text == UI.UISIDESCREENS.LAUNCHPADSIDESCREEN.STATUS.READY_FOR_LAUNCH ))
            {
                return;
            }
            if( ___rocketModule == null || ___selectedPad == null )
                return;
            PassengerRocketModule component = ___rocketModule.GetComponent<PassengerRocketModule>();
            if( component == null )
                return;
            if( component.GetCrewBoardedFraction().second == 0 )
            {
                ___launchButton.isInteractable = false;
                ___statusText.text = FIXESANDTWEAKS.NO_CREW_ASSIGNED;
                ___launchButton.GetComponentInChildren<LocText>().text
                    = UI.UISIDESCREENS.LAUNCHPADSIDESCREEN.LAUNCH_BUTTON;
                ___launchButton.GetComponentInChildren<ToolTip>().toolTip
                    = UI.UISIDESCREENS.LAUNCHPADSIDESCREEN.LAUNCH_BUTTON_NOT_READY_TOOLTIP;
            }
        }
    }
}
