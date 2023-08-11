using HarmonyLib;
using System.Reflection;

// Time sensors (cycle and timer sensor) alternate between both logic states
// even when duration for one of them is zero. Change them to stay in the non-zero
// state in such cases. This makes it possible to e.g. disable timer+notification
// combo by merely setting the active time to 0.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(LogicTimeOfDaySensor))]
    public class LogicTimeOfDaySensor_Patch
    {
        delegate void SetStateDelegate(LogicTimeOfDaySensor sensor, bool on);
        private static SetStateDelegate setState = AccessTools.MethodDelegate<SetStateDelegate>(
            AccessTools.Method( typeof( LogicTimeOfDaySensor ), "SetState" ));

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Sim200ms))]
        public static bool Sim200ms(LogicTimeOfDaySensor __instance, float dt, float ___duration )
        {
            if( !Options.Instance.TimeSensorsStrictZeroInterval )
                return true;
            if( ___duration == 0f )
            {
                setState( __instance, false );
                return false;
            }
            if( ___duration == 1f )
            {
                setState( __instance, true );
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LogicTimerSensor))]
    public class LogicTimerSensor_Patch
    {
        delegate void SetStateDelegate(LogicTimerSensor sensor, bool on);
        private static SetStateDelegate setState = AccessTools.MethodDelegate<SetStateDelegate>(
            AccessTools.Method( typeof( LogicTimerSensor ), "SetState" ));

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Sim33ms))]
        public static bool Sim33ms(LogicTimerSensor __instance, float dt, float ___onDuration, float ___offDuration )
        {
            if( !Options.Instance.TimeSensorsStrictZeroInterval )
                return true;
            if( ___onDuration == 0f )
            {
                setState( __instance, false );
                return false;
            }
            if( ___offDuration == 0f )
            {
                setState( __instance, true );
                return false;
            }
            return true;
        }
    }
}
