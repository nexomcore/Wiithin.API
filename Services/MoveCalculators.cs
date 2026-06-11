namespace WithinAPI.Services;

/// <summary>
/// Pure fitness/nutrition math for the Move Calculators tab. No EF/DI/UI so it unit-tests
/// in isolation and never contends for the running API's locked build output.
///
/// Inputs use the app's lower-token vocabulary (e.g. gender "male"/"female", activity
/// "sedentary".."very_active", goal "lose_fat"..). Every result is an estimate, never
/// medical advice — the calling UI always shows that disclaimer.
/// </summary>
public static class MoveCalculators
{
    public sealed record BmiResult(double Bmi, string Category);
    public sealed record BmrResult(double Bmr, bool IsEstimate);
    public sealed record MacroResult(int Calories, int ProteinGrams, int CarbsGrams, int FatGrams);
    public sealed record BodyFatResult(double Percentage);
    public sealed record HeartRateResult(int MaxHeartRate, int TargetMin, int TargetMax);

    private static double Round(double value, int dp = 1)
    {
        var factor = Math.Pow(10, dp);
        return Math.Round(value * factor, MidpointRounding.AwayFromZero) / factor;
    }

    public static BmiResult Bmi(double heightCm, double weightKg)
    {
        if (heightCm <= 0) throw new ArgumentOutOfRangeException(nameof(heightCm));
        var heightM = heightCm / 100.0;
        var value = Round(weightKg / (heightM * heightM), 1);
        var category = value switch
        {
            < 18.5 => "Underweight",
            < 25 => "Normal",
            < 30 => "Overweight",
            _ => "Obese"
        };
        return new BmiResult(value, category);
    }

    /// <summary>Mifflin-St Jeor. "other"/"prefer_not_to_say" use the male/female average and flag IsEstimate.</summary>
    public static BmrResult Bmr(double weightKg, double heightCm, int age, string? gender)
    {
        var baseValue = 10 * weightKg + 6.25 * heightCm - 5 * age;
        var normalized = (gender ?? "").Trim().ToLowerInvariant();
        return normalized switch
        {
            "male" => new BmrResult(Round(baseValue + 5, 0), false),
            "female" => new BmrResult(Round(baseValue - 161, 0), false),
            _ => new BmrResult(Round(baseValue + (5 - 161) / 2.0, 0), true)
        };
    }

    public static double ActivityFactor(string? activityLevel) => (activityLevel ?? "").Trim().ToLowerInvariant() switch
    {
        "sedentary" => 1.2,
        "light" or "lightly_active" => 1.375,
        "moderate" or "moderately_active" => 1.55,
        "active" or "very_active_lower" => 1.725,
        "very_active" or "athlete" => 1.9,
        _ => 1.2
    };

    public static double Tdee(double bmr, string? activityLevel) => Round(bmr * ActivityFactor(activityLevel), 0);

    /// <summary>Goal-driven macro split over a calorie target. Protein/Carbs = cal/4, Fat = cal/9.</summary>
    public static MacroResult Macros(int calorieTarget, string? goal)
    {
        if (calorieTarget <= 0) throw new ArgumentOutOfRangeException(nameof(calorieTarget));
        var (proteinPct, carbsPct, fatPct) = (goal ?? "").Trim().ToLowerInvariant() switch
        {
            "lose_fat" or "fat_loss" => (0.30, 0.40, 0.30),
            "build_muscle" or "muscle_gain" => (0.25, 0.50, 0.25),
            "body_recomposition" => (0.30, 0.40, 0.30),
            "improve_endurance" or "endurance" => (0.25, 0.55, 0.20),
            "strength" => (0.30, 0.45, 0.25),
            _ => (0.25, 0.45, 0.30) // general_health / maintain / mobility / flexibility
        };

        var protein = (int)Math.Round(calorieTarget * proteinPct / 4.0, MidpointRounding.AwayFromZero);
        var carbs = (int)Math.Round(calorieTarget * carbsPct / 4.0, MidpointRounding.AwayFromZero);
        var fat = (int)Math.Round(calorieTarget * fatPct / 9.0, MidpointRounding.AwayFromZero);
        return new MacroResult(calorieTarget, protein, carbs, fat);
    }

    /// <summary>US Navy method. Hip is required for female estimates; clamped to a sane range.</summary>
    public static BodyFatResult BodyFat(string? gender, double heightCm, double neckCm, double waistCm, double? hipCm)
    {
        var normalized = (gender ?? "").Trim().ToLowerInvariant();
        double value;
        if (normalized == "female")
        {
            var hip = hipCm ?? throw new ArgumentNullException(nameof(hipCm), "Hip measurement is required for female body-fat estimate.");
            value = 163.205 * Math.Log10(waistCm + hip - neckCm) - 97.684 * Math.Log10(heightCm) - 78.387;
        }
        else
        {
            value = 86.010 * Math.Log10(waistCm - neckCm) - 70.041 * Math.Log10(heightCm) + 36.76;
        }

        return new BodyFatResult(Round(Math.Clamp(value, 2, 70), 1));
    }

    /// <summary>Epley 1RM. Reps must be 1-20; reps == 1 returns the entered weight.</summary>
    public static double OneRepMax(double weight, int reps)
    {
        if (reps < 1 || reps > 20) throw new ArgumentOutOfRangeException(nameof(reps), "Reps must be between 1 and 20.");
        return reps == 1 ? Round(weight, 1) : Round(weight * (1 + reps / 30.0), 1);
    }

    /// <summary>Daily water target in millilitres: 35 ml/kg plus an activity bump (300-700 ml).</summary>
    public static double WaterIntakeMl(double weightKg, string? activityLevel)
    {
        var bump = (activityLevel ?? "").Trim().ToLowerInvariant() switch
        {
            "very_active" or "athlete" => 700,
            "active" => 500,
            "moderate" or "moderately_active" => 400,
            "light" or "lightly_active" => 300,
            _ => 0
        };
        return Round(weightKg * 35 + bump, 0);
    }

    /// <summary>Karvonen-free target zone. Intensities are fractions (0.5 = 50%).</summary>
    public static HeartRateResult TargetHeartRate(int age, double intensityMin, double intensityMax)
    {
        if (age <= 0 || age > 120) throw new ArgumentOutOfRangeException(nameof(age));
        var max = 220 - age;
        var lo = Math.Min(intensityMin, intensityMax);
        var hi = Math.Max(intensityMin, intensityMax);
        return new HeartRateResult(max, (int)Math.Round(max * lo), (int)Math.Round(max * hi));
    }
}
