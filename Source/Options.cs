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
        [Option("Faster Horizontal Scrolling",
            "Makes horizontal scrolling in views such as the 'Consumables' one faster.")]
        [JsonProperty]
        public bool FasterHorizontalScrolling { get; set; } = true;

        [Option("Faster Vertical Scrolling",
            "Makes vertical scrolling in views such as the 'Consumables' one faster.")]
        [JsonProperty]
        public bool FasterVerticalScrolling { get; set; } = true;

        [Option("Holding Ctrl Blocks Hover Text",
            "Hover text such as the info cards or tooltips are not shown while the Ctrl key is pressed.")]
        [JsonProperty]
        public bool HoldingCtrlBlocksHoverText { get; set; } = true;

        [Option("Reduced Starvation Warning",
            "If a duplicant is visiting a toilet or catching breath before eating"
            +" and still has at least 800 kcal, the 'Starvation' warning is not shown.")]
        [JsonProperty]
        public bool ReducedStarvationWarning { get; set; } = true;

        [Option("Block Attribute Increase Notification",
            "Disables the 'Attribute increase' notifications.")]
        [JsonProperty]
        public bool BlockAttributeIncreaseNotification { get; set; } = true;

        [Option("Block Cycle Report Ready Notification",
            "Disables the 'Cycle X report ready' notifications.")]
        [JsonProperty]
        public bool BlockCycleReportReadyNotification { get; set; } = true;

        [Option("Block Schedule Notifications",
            "Disables schedule notifications such as 'Default schedule: BathTime!'.\n"
            + "Only applies to text notifications, sound is not affected.")]
        [JsonProperty]
        public bool BlockScheduleNotification { get; set; } = true;

        [Option("Block Critter Starvation Notification Expiration",
            "The 'Critter Starvation' notification will be shown as long at it applies.")]
        [JsonProperty]
        public bool BlockCritterStarvationNotificationExpiration { get; set; } = true;

        [Option("Reduced Radiation Diagnostic",
            "If a duplicant is exposed to strong radiation but has not yet received"
            + "a significant amount of radiation, the 'Check exposed' diagnostic is suppressed.")]
        [JsonProperty]
        [RequireDLC(DlcManager.EXPANSION1_ID)]
        public bool ReducedRadiationDiagnostic { get; set; } = true;

        [Option("Radiation Diagnostic Dose Threshold",
            "The minimal radiation dose received for the 'Check exposed' diagnostic to trigger.\n"
            + "The value of 0 leaves the game default threshold, which is half of the dose"
            + " at which minor radiation sickness starts.")]
        [JsonProperty]
        [RequireDLC(DlcManager.EXPANSION1_ID)]
        public int RadiationDiagnosticDoseThreshold { get; set; } = 0;

        [Option("Planted Diagnostic Only If Farms",
            "'Check farms are planted' diagnostic triggers only if there are farm plots.")]
        [JsonProperty]
        public bool PlantedDiagnosticOnlyIfFarms { get; set; } = true;

        [Option("Block Has Farms Diagnostic",
            "Disable 'Check colony has farms' diagnostic.")]
        [JsonProperty]
        public bool BlockHasFarmsDiagnostic { get; set; } = true;

        [Option("Block Power Use Change Diagnostic",
            "Disable 'Check power use change' diagnostic.")]
        [JsonProperty]
        public bool BlockPowerChangeDiagnostic { get; set; } = true;

        [Option("Check Enough Food Diagnostic Includes Rockets",
            "Check also food available in rockets for 'Check enough food' diagnostic.")]
        [JsonProperty]
        public bool CheckEnoughFoodDiagnosticIncludesRockets { get; set; } = true;

        [Option("Check Enough Food Diagnostic Concern Cycles",
            "Make food diagnostic show a (yellow) concern warning if food left is less than this many cycles.")]
        [JsonProperty]
        public float CheckEnoughFoodDiagnosticConcernCycles { get; set; } = 3; // vanilla value

        [Option("Check Enough Food Diagnostic Warning Cycles",
            "Make food diagnostic show a (red) warning if food left is less than this many cycles.")]
        [JsonProperty]
        public float CheckEnoughFoodDiagnosticWarningCycles { get; set; } = 1;

        [Option("Time Sensors Strict Zero Interval",
            "Time Sensor and Cycle Sensor do not send signal for zero interval.")]
        [JsonProperty]
        public bool TimeSensorsStrictZeroInterval { get; set; } = true;

        public override string ToString()
        {
            return $"FixesAndTweaks.Options[fasterhorizontalscrolling={FasterHorizontalScrolling},"
                + $"fasterverticalscrolling={FasterVerticalScrolling},"
                + $"holdingctrlblockshovertext={HoldingCtrlBlocksHoverText},"
                + $"reducedstarvationwarning={ReducedStarvationWarning},"
                + $"blockattributeincreasenotification={BlockAttributeIncreaseNotification},"
                + $"blockcyclereportreadynotification={BlockCycleReportReadyNotification},"
                + $"blockschedulenotification={BlockScheduleNotification},"
                + $"blockcritterstarvationnotificationexpiration={BlockCritterStarvationNotificationExpiration},"
                + $"reducedradiationdiagnostic={ReducedRadiationDiagnostic},"
                + $"radiationdiagnosticdosethreshold={RadiationDiagnosticDoseThreshold},"
                + $"planteddiagnosticonlyiffarms={PlantedDiagnosticOnlyIfFarms},"
                + $"blockhasfarmsdiagnostic={BlockHasFarmsDiagnostic},"
                + $"blockpowerchangediagnostic={BlockPowerChangeDiagnostic},"
                + $"checkenoughfooddiagnosticincludesrockets={CheckEnoughFoodDiagnosticIncludesRockets},"
                + $"checkenoughfooddiagnosticconcerncycles={CheckEnoughFoodDiagnosticConcernCycles},"
                + $"checkenoughfooddiagnosticwarningcycles={CheckEnoughFoodDiagnosticWarningCycles},"
                + $"timesensorsstrictzerointerval={TimeSensorsStrictZeroInterval}]";
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
