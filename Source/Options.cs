using PeterHan.PLib.Options;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace FixesAndTweaks
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("https://github.com/llunak/oni-fixesandtweaks")]
    [ConfigFile(SharedConfigLocation: true)]
    public sealed class Options : SingletonOptions< Options >, IOptions
    {
        [Option("Faster Horizontal Scrolling", "Makes horizontal scrolling in views such as the 'Consumables' one faster.")]
        [JsonProperty]
        public bool FasterHorizontalScrolling { get; set; } = true;

        [Option("Faster Vertical Scrolling", "Makes vertical scrolling in views such as the 'Consumables' one faster.")]
        [JsonProperty]
        public bool FasterVerticalScrolling { get; set; } = true;

        [Option("Reduced Starvation Warning", "If a duplicant is visiting a toilet or catching breath before eating"
            +" and still has at least 800 kcal, the 'Starvation' warning is not shown.")]
        [JsonProperty]
        public bool ReducedStarvationWarning { get; set; } = true;

        [Option("Reduced Radiation Diagnostic", "If a duplicant is exposed to strong radiation but has not yet received"
            + "a significant amount of radiation, the 'Check exposed' diagnostic is suppressed.")]
        [JsonProperty]
        [RequireDLC(DlcManager.EXPANSION1_ID)]
        public bool ReducedRadiationDiagnostic { get; set; } = true;

        [Option("Planted Diagnostic Only If Farms", "'Check farms are planted' diagnostic triggers only if there are farm plots.")]
        [JsonProperty]
        public bool PlantedDiagnosticOnlyIfFarms { get; set; } = true;

        [Option("Block Has Farms Diagnostic", "Disable 'Check colony has farms' diagnostic.")]
        [JsonProperty]
        public bool BlockHasFarmsDiagnostic { get; set; } = true;

        [Option("Block Power Use Change Diagnostic", "Disable 'Check power use change' diagnostic.")]
        [JsonProperty]
        public bool BlockPowerChangeDiagnostic { get; set; } = true;

        [Option("Check Enough Food Diagnostic Includes Rockets", "Check also food available in rockets for 'Check enough food' diagnostic.")]
        [JsonProperty]
        public bool CheckEnoughFoodDiagnosticIncludesRockets { get; set; } = true;

        [Option("Check Enough Food Diagnostic Concern Cycles", "Make food diagnostic show a (yellow) concern warning if food left is less than this many cycles.")]
        [JsonProperty]
        public float CheckEnoughFoodDiagnosticConcernCycles { get; set; } = 3; // vanilla value

        [Option("Check Enough Food Diagnostic Warning Cycles", "Make food diagnostic show a warning if food left is less than this many cycles.")]
        [JsonProperty]
        public float CheckEnoughFoodDiagnosticWarningCycles { get; set; } = 1;


        public override string ToString()
        {
            return string.Format("FixesAndTweaks.Options[fasterhorizontalscrolling={0},"
                + "fasterverticalscrolling={1},reducedstarvationwarning={2},"
                + "reducedradiationdiagnostic={3},planteddiagnosticonlyiffarms={4}, blockhasfarmsdiagnostic={5},"
                + "blockpowerchangediagnostic={6}, checkenoughfooddiagnosticincludesrockets={7},"
                + "checkenoughfooddiagnosticconcerncycles={8}, checkenoughfooddiagnosticwarningcycles={9}]",
                FasterHorizontalScrolling, FasterVerticalScrolling, ReducedStarvationWarning, ReducedRadiationDiagnostic,
                PlantedDiagnosticOnlyIfFarms, BlockHasFarmsDiagnostic, BlockPowerChangeDiagnostic,
                CheckEnoughFoodDiagnosticIncludesRockets, CheckEnoughFoodDiagnosticConcernCycles,
                CheckEnoughFoodDiagnosticWarningCycles);
        }

        public void OnOptionsChanged()
        {
            // 'this' is the Options instance used by the options dialog, so set up
            // the actual instance used by the mod. MemberwiseClone() is enough to copy non-reference data.
            Instance = (Options) this.MemberwiseClone();
        }

        public IEnumerable<IOptionsEntry> CreateOptions()
        {
            return null;
        }
    }
}
