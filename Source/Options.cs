using PeterHan.PLib.Options;
using Newtonsoft.Json;

namespace FixesAndTweaks
{
    [JsonObject(MemberSerialization.OptIn)]
    [ModInfo("https://github.com/llunak/oni-fixesandtweaks")]
    [ConfigFile(SharedConfigLocation: true)]
    public sealed class Options : SingletonOptions< Options >
    {
        [Option("Faster Horizontal Scrolling", "Makes horizontal scrolling in views such as the 'Consumables' one faster.")]
        [JsonProperty]
        public bool FasterHorizontalScrolling { get; set; } = true;

        [Option("Reduced Starvation Warning", "If a duplicant is visiting a toilet or catching breath before eating"
            +" and still has at least 800 kcal, the 'Starvation' warning is not shown.")]
        [JsonProperty]
        public bool ReducedStarvationWarning { get; set; } = true;

        [Option("Planted Diagnostic Only If Farms", "'Check farms are planted' diagnostic triggers only if there are farm plots.")]
        [JsonProperty]
        public bool PlantedDiagnosticOnlyIfFarms { get; set; } = true;

        [Option("Block Has Farms Diagnostic", "Disable 'Check colony has farms' diagnostic.")]
        [JsonProperty]
        public bool BlockHasFarmsDiagnostic { get; set; } = true;

        public override string ToString()
        {
            return string.Format("DeliveryTemperatureLimit.Options[fasterhorizontalscrolling={0},reducedstarvationwarning={1},"
                + "planteddiagnosticonlyiffarms={2}, blockhasfarmsdiagnostic={3}]",
                FasterHorizontalScrolling, ReducedStarvationWarning, PlantedDiagnosticOnlyIfFarms, BlockHasFarmsDiagnostic);
        }
    }
}