#if false
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

// This blocks automated notifiers for the first 20 in-game seconds.
// It is meant to avoid false alarms from e.g. smart storage,
// since those send green signal on full, so for detecting not-enough
// they need to be used with a not game, but on game start they initially
// send red, so there's an alarm before they switch to green.
// But this is probably not necessary, I can make 'Storage Refrigerator Thresholds'
// invert the logic (also to match gas/liquid tanks), which should take
// care of this and also false alarms when power goes out.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(LogicAlarm))]
    public class LogicAlarm_Patch : KMonoBehaviour, ISim4000ms
    {
        private static LogicAlarm_Patch instance = null;

        private const float blockedTimeInit = 20;  // 20 seconds

        private static float timeStillBlocked = blockedTimeInit;

        private static Dictionary< LogicAlarm, LogicValueChanged > delayedData = new Dictionary< LogicAlarm, LogicValueChanged >();

        [HarmonyPrefix]
        [HarmonyPatch(nameof(OnLogicValueChanged))]
        public static bool OnLogicValueChanged(object data, LogicAlarm __instance)
        {
            LogicValueChanged logicValueChanged = (LogicValueChanged)data;
            if( timeStillBlocked == 0 )
                return true;
            if (logicValueChanged.portID != LogicAlarm.INPUT_PORT_ID)
                return true;
            delayedData[ __instance ] = logicValueChanged;
            if( instance == null )
            {
                // For some reason merely creating the object does not make the timer
                // work, so temporarily add it as a component somewhere.
                instance = Game.Instance.gameObject.AddComponent< LogicAlarm_Patch >();
            }
            return false;
        }

        public void Sim4000ms(float dt)
        {
            if( timeStillBlocked == 0 )
                return;
            if( timeStillBlocked > dt )
            {
                timeStillBlocked -= dt;
                return;
            }
            timeStillBlocked = 0;
            foreach( KeyValuePair< LogicAlarm, LogicValueChanged > data in delayedData )
                data.Key.OnLogicValueChanged( data.Value );
            delayedData.Clear();
            Object.Destroy( this );
            instance = null;
        }

        public static void Reset()
        {
            // Reset state when finishing game, so that a new load works again.
            instance = null;
            timeStillBlocked = blockedTimeInit;
        }
    }

    [HarmonyPatch(typeof(Game))]
    public class Game_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(DestroyInstances))]
        public static void DestroyInstances()
        {
            LogicAlarm_Patch.Reset();
        }
    }
}
#endif
