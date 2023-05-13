using HarmonyLib;
using UnityEngine;

// https://forums.kleientertainment.com/klei-bug-tracker/oni/duplicants-with-empty-oxygen-mask-or-suit-do-not-need-any-oxygen-r40368/
// A duplicant with an oxygen mask or suit that runs out of oxygen will start losing breath,
// but then will start recovering breath, even with the mask/suit on, if there is no oxygen,
// and will not consume any oxygen.
// The game has some code to drop the mask/suit in that case, but it's not used and is broken.
namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(RecoverBreathChore.States))]
    public class RecoverBreathChore_States_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(InitializeStates))]
        public static void InitializeStates(RecoverBreathChore.States.State ___remove_suit, RecoverBreathChore.States.TargetParameter ___recoverer)
        {
            // The game simply goes from remove_suit state directly to recover state without doing anything.
            // There is RemoveSuitIfNecessary, but it's not called, and moreover it's broken (gets Equipment
            // from the wrong place and crashes).
            ___remove_suit.Enter(delegate(RecoverBreathChore.StatesInstance smi)
            {
                // smi.RemoveSuitIfNecessary();
                GameObject gameObject = ___recoverer.Get(smi);
                if (gameObject != null)
                {
                    MinionIdentity minion = gameObject.GetComponent<MinionIdentity>();
                    if(minion != null)
                    {
                        Equipment equipment = minion.assignableProxy.Get().GetComponent<Equipment>();
                        if (equipment != null)
                        {
                            Assignable assignable = equipment.GetAssignable(Db.Get().AssignableSlots.Suit);
                            if (assignable != null)
                            {
                                // First check if the mask/suit is empty and do not unequip it if it's not.
                                // It is possible for dupes to search for a place to recover breath,
                                // run through a mask/suit checkpoint while doing so and then start
                                // recovering breath, which would unassign the functional mask/suit.
                                SuitTank suitTank = assignable.GetComponent<SuitTank>();
                                if( suitTank != null && !suitTank.IsEmpty())
                                    return;
                                assignable.Unassign();
                            }
                        }
                    }
                }
            });
        }
    }
}
