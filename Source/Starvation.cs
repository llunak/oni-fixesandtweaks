using HarmonyLib;
using Klei.AI;
using System;
using System.Collections.Generic;

// Some chores like going to the toilet and catching breath have a higher priority than eating,
// and at least on the highest hunger difficulty setting it happens quite often that
// at the beginning of the downtime dupes run to the toilet and become starving, which triggers
// a warning, even though those dupes have eating queued right after the toilet and still have
// more than enough calories to make it.
// Since going to eat already disables the starving notification, disable it also if the going
// to eat task is only preceded by these high-priority tasks and the dupe still has enough
// calories.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(CalorieMonitor.Instance))]
    public class CalorieMonitor_Instance_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(IsEating))]
        public static void IsEating(CalorieMonitor.Instance __instance, ref bool __result)
        {
            if( !Options.Instance.ReducedStarvationWarning )
                return;
            if( __result || !__instance.IsStarving())
                return;
            AmountInstance calories = Db.Get().Amounts.Calories.Lookup(__instance.gameObject);
            if( calories.value / calories.GetMax() < 0.20f )
                return; // Less than 800 kcal, do not adjust anything.
            // Chores that may come before eating and can be ignored.
            Func< Chore, bool > isAllowedChore = ( Chore chore ) =>
            {
                return chore.choreType.urge == Db.Get().Urges.Pee
                    || chore.choreType.urge == Db.Get().Urges.RecoverBreath
                    || chore.choreType.urge == Db.Get().Urges.Emote
                    || chore.choreType.urge == Db.Get().Urges.EmoteHighPriority;
            };
            ChoreDriver choreDriver = __instance.master.GetComponent<ChoreDriver>();
            if (choreDriver.HasChore() && !isAllowedChore( choreDriver.GetCurrentChore()))
                return;
            ChoreConsumer choreConsumer = __instance.GetComponent<ChoreConsumer>();
            ChoreConsumer.PreconditionSnapshot lastPreconditionSnapshot = choreConsumer.GetLastPreconditionSnapshot();
            List< Chore.Precondition.Context> chores = new List< Chore.Precondition.Context>();
            if (lastPreconditionSnapshot.doFailedContextsNeedSorting)
            {
                lastPreconditionSnapshot.failedContexts.Sort();
                lastPreconditionSnapshot.doFailedContextsNeedSorting = false;
            }
            chores.AddRange(lastPreconditionSnapshot.failedContexts);
            chores.AddRange(lastPreconditionSnapshot.succeededContexts);
            for( int i = chores.Count - 1; i >= 0; --i )
            {
                if( chores[ i ].chore == null )
                    continue;
                if( chores[ i ].chore.driver != null && chores[ i ].chore.driver != choreConsumer.choreDriver )
                    continue;
                if( isAllowedChore( chores[ i ].chore ))
                    continue;
                // If all chores above eating are allowed ones (or do not apply), make the function
                // claim the dupe is eating.
                if( chores[ i ].chore.choreType.urge == Db.Get().Urges.Eat )
                {
                    __result = true;
                    return;
                }
                // Check IsPotentialSuccess() only after checking the eat chore, as it may return
                // false for that one (because of being "low priority" while going to the toilet or something).
                if( !chores[ i ].IsPotentialSuccess())
                    continue;
                // Found a chore that's not eating and not the allowed one, do not change anything and return.
                return;
            }
        }
    }
}
