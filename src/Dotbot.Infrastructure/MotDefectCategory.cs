using System.ComponentModel.DataAnnotations;

namespace Dotbot.Infrastructure;

public enum MotDefectCategory
{
    [Display(Name = "DANGEROUS")] DANGEROUS = 0,
    [Display(Name = "MAJOR")] MAJOR = 1,
    [Display(Name = "MINOR")] MINOR = 2,
    [Display(Name = "FAIL")] FAIL = 3,
    [Display(Name = "ADVISORY")] ADVISORY = 4,

    [Display(Name = "NON SPECIFIC")] NONSPECIFIC = 5,

    [Display(Name = "SYSTEM GENERATED")] SYSTEMGENERATED = 6,

    [Display(Name = "USER ENTERED")] USERENTERED = 7,

    [Display(Name = "PASS AFTER RECTIFICATION")]
    PRS = 8
}