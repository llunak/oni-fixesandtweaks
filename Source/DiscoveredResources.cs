#if false
using HarmonyLib;
using System.Collections.Generic;

namespace FixesAndTweaks
{
    [HarmonyPatch(typeof(DiscoveredResources))]
    public class DiscoveredResources_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(FilterDisabledContent))]
        public static void FilterDisabledContent(HashSet<Tag> ___Discovered,
            Dictionary<Tag, HashSet<Tag>> ___DiscoveredCategories)
        {
            HashSet<Tag> hashSet = new HashSet<Tag>();
            hashSet.Add(SimHashes.Magma.CreateTag());
            hashSet.Add(SimHashes.MoltenIron.CreateTag());
            hashSet.Add(SimHashes.MoltenGlass.CreateTag());
            hashSet.Add(SimHashes.LiquidSulfur.CreateTag());
            foreach (Tag item2 in hashSet)
                ___Discovered.Remove(item2);
            foreach (KeyValuePair<Tag, HashSet<Tag>> discoveredCategory in ___DiscoveredCategories)
                foreach (Tag item3 in hashSet)
                    if (discoveredCategory.Value.Contains(item3))
                        discoveredCategory.Value.Remove(item3);
        }
    }
}
#endif
