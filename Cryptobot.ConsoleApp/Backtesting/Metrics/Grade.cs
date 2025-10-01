using System.ComponentModel.DataAnnotations;

namespace Cryptobot.ConsoleApp.Backtesting.Metrics;

public enum Grade
{
    [Display(Name = "-")]
    Ungraded,
    [Display(Name = "[A+]")]
    APlus,
    [Display(Name = " [A]")]
    A,
    [Display(Name = " [B]")]
    B,
    [Display(Name = " [C]")]
    C,
    [Display(Name = " [D]")]
    D,
    [Display(Name = " [E]")]
    E,
    [Display(Name = " [F]")]
    F,
}
