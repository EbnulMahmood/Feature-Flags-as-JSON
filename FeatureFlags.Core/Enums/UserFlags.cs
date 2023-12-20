using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.Core.Enums
{
    public enum UserFlags
    {
        None = 0,

        [Display(Name = "Dark Mode")]
        DarkMode = 10,

        [Display(Name = "Super Admin")]
        SuperAdmin = 20,

        [Display(Name = "Notification Opt-In")]
        NotificationOptIn = 30,

        [Display(Name = "Metered Billing")]
        MeteredBilling = 40,

        [Display(Name = "Rollout Chat")]
        RolloutChat = 50,

        [Display(Name = "Experiment Blue")]
        ExperimentBlue = 60,

        [Display(Name = "Log Verbose")]
        LogVerbose = 70,

        [Display(Name = "New Legal Disclaimer")]
        NewLegalDisclaimer = 80,
    }
}
