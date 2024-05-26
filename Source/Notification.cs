using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

// Notifications from the Automated Notifier normally expire after a few seconds, which
// is annoying, as they are usually alarms about something important, and they could be even missed
// (or forgotten). Stop them from expiring, but instead make it possible to dismiss them.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(LogicAlarm))]
    public class LogicAlarm_Patch
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(CreateNotification))]
        public static IEnumerable<CodeInstruction> CreateNotification(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool found = false;
            for( int i = 0; i < codes.Count; ++i )
            {
                // Debug.Log("T:" + i + ":" + codes[i].opcode + "::" + (codes[i].operand != null ? codes[i].operand.ToString() : codes[i].operand));
                // The function has code:
                // new Notification(..., expires: true, ...);
                // I.e. it sets 'expires' to true and 'show_dismiss_button' defaults to false.
                // Change to:
                // new Notification(..., CreateNotification_Hook1(), ..., CreateNotification_Hook2());
                if( codes[ i ].opcode == OpCodes.Ldc_I4_1
                    && i + 8 < codes.Count
                    && codes[ i + 7 ].opcode == OpCodes.Ldc_I4_0
                    && codes[ i + 8 ].opcode == OpCodes.Newobj
                    && codes[ i + 8 ].operand.ToString()
                        == "Void .ctor(String, NotificationType, Func`3, Object, Boolean, Single, ClickCallback, Object, Transform, Boolean, Boolean, Boolean)" )
                {
                    codes[ i ] = new CodeInstruction( OpCodes.Call,
                        typeof( LogicAlarm_Patch ).GetMethod( nameof( CreateNotification_Hook1 ))); // 'expires'
                    codes[ i + 7 ] = new CodeInstruction( OpCodes.Call,
                        typeof( LogicAlarm_Patch ).GetMethod( nameof( CreateNotification_Hook2 ))); // 'show_dismiss_button'
                    found = true;
                    break;
                }
            }
            if(!found)
                Debug.LogWarning("FixesAndTweaks: Failed to patch LogicAlarm.CreateNotification()");
            return codes;
        }

        public static bool CreateNotification_Hook1()
        {
            return !Options.Instance.AutomatedNotifierDoesNotExpire;
        }

        public static bool CreateNotification_Hook2()
        {
            return Options.Instance.AutomatedNotifierDoesNotExpire;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UpdateVisualState))]
        public static void UpdateVisualState( bool ___wasOn, Notification ___notification )
        {
            if( !Options.Instance.AutomatedNotifierRedSignalClears )
                return;
            // 'wasOn' at this point means 'is on'
            if( !___wasOn && ___notification != null )
                ___notification.Clear();
        }

        private static FieldInfo notificationField
            = AccessTools.Field( typeof( LogicAlarm ), "notification" );

        public static void DoOnCleanUp( LogicAlarm logicAlarm )
        {
            // Clear the notification when the building is destroyed.
            Notification notification = (Notification) notificationField.GetValue( logicAlarm );
            if( notification != null )
                notification.Clear();
        }
    }

    // LogicAlarm has no OnCleanUp to patch, so it must be done in the base class.
    [HarmonyPatch(typeof(KMonoBehaviour))]
    public class KMonoBehaviour_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(OnCleanUp))]
        public static void OnCleanUp( KMonoBehaviour __instance )
        {
            if( __instance is LogicAlarm logicAlarm )
            {
                LogicAlarm_Patch.DoOnCleanUp( logicAlarm );
            }
        }
    }
}
