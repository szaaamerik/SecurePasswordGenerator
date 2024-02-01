using System.Windows.Media;

namespace SecurePasswordGenerator.Models;

public readonly struct SecurePassword
{
    public string Password { get; init; }
    public int Length { get; init; }
    public StrengthLevels StrengthLevel { get; init; }
    public Brush StrengthColor { get; init; }
    
    public enum StrengthLevels
    {
        Low,
        MediumLow,
        Medium,
        MediumHigh,
        High
    }

    public static Brush GetStrengthColorFromLevel(StrengthLevels strengthLevel)
    {
        return strengthLevel switch
        {
            StrengthLevels.Low => Brushes.Red,
            StrengthLevels.MediumLow => Brushes.OrangeRed,
            StrengthLevels.Medium => Brushes.Orange,
            StrengthLevels.MediumHigh => Brushes.Green,
            StrengthLevels.High => Brushes.Lime,
            _ => throw new ArgumentOutOfRangeException(nameof(strengthLevel), strengthLevel, null)
        };
    }
}