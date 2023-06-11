using HarmonyLib;
using Klei.AI;

// Notifications 'Attribute increase', 'Cycle X report ready', 'Default schedule: Bathtime!',
// are not very useful, as they happen repeatedly and cannot really be acted upon. Block them.
namespace FixesAndTweaks
{
    // The notifications could be blocked by using transpillers and blocking the calls
    // in the relevant functions, but instead simply set/reset a flag in prefix/postfix
    // and then bail out in the notification function.
    [HarmonyPatch(typeof(NotificationManager))]
    public class NotificationManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(AddNotification))]
        public static bool AddNotification()
        {
            if( AttributeLevel_Patch.inLevelUp && Options.Instance.BlockAttributeIncreaseNotification )
                return false;
            if( ReportManager_Patch.inOnNightTime && Options.Instance.BlockCycleReportReadyNotification )
                return false;
            if( ScheduleManager_Patch.inPlayScheduleAlarm && Options.Instance.BlockScheduleNotification )
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(AttributeLevel))]
    public class AttributeLevel_Patch
    {
        public static bool inLevelUp = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(LevelUp))]
        public static void LevelUp()
        {
            inLevelUp = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(LevelUp))]
        public static void LevelUp2()
        {
            inLevelUp = false;
        }
    }

    [HarmonyPatch(typeof(ReportManager))]
    public class ReportManager_Patch
    {
        public static bool inOnNightTime = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(OnNightTime))]
        public static void OnNightTime()
        {
            inOnNightTime = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(OnNightTime))]
        public static void OnNightTime2()
        {
            inOnNightTime = false;
        }
    }

    [HarmonyPatch(typeof(ScheduleManager))]
    public class ScheduleManager_Patch
    {
        public static bool inPlayScheduleAlarm = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayScheduleAlarm))]
        public static void PlayScheduleAlarm()
        {
            inPlayScheduleAlarm = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayScheduleAlarm))]
        public static void PlayScheduleAlarm2()
        {
            inPlayScheduleAlarm = false;
        }
    }
}
