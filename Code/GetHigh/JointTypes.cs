using Sandbox;

public enum JointType
{
    Gannavuri,
    PurpleHaze,
    AK47,
    OGKush,
    BlueDream
}

public enum VisualEffect
{
    Bloom,

}

public readonly struct JointTypeStats
{
    public float FireRateMultiplier { get; init; }
	public float SpeedMultiplier { get; init; }
	public float DamageMultiplier { get; init; }
	public float Duration { get; init; }
	public float Cooldown { get; init; }
	public bool CanStun { get; init; }
}

/// <summary>
/// Provides per-type stat configurations for each <see cref="JointType"/>.
/// All values are returned as immutable structs; nothing is mutated.
/// </summary>
public static class JointTypeConfig
{
    private static readonly JointTypeStats GannavuriStats = new()
	{
		FireRateMultiplier = 1f,
		SpeedMultiplier = 0.8f,
		DamageMultiplier = 1.5f,
		Duration = 20f,
        Cooldown = 30f,
		CanStun = false
	};

    private static readonly JointTypeStats PurpleHazeStats = new()
	{
		FireRateMultiplier = 1.1f,
		SpeedMultiplier = 1.5f,
		DamageMultiplier = 1f,
		Duration = 20f,
        Cooldown = 30f,
		CanStun = false,
	};

    private static readonly JointTypeStats AK47Stats = new()
	{
		FireRateMultiplier = 1.6f,
		SpeedMultiplier = 1f,
		DamageMultiplier = 1.3f,
		Duration = 20f,
        Cooldown = 30f,
		CanStun = false
	};

    private static readonly JointTypeStats OGKushStats = new()
	{
		FireRateMultiplier = 0.7f,
		SpeedMultiplier = 0.6f,
		DamageMultiplier = 15f,
		Duration = 20f,
        Cooldown = 30f,
		CanStun = false
	};

    private static readonly JointTypeStats BlueDreamStats = new()
	{
		FireRateMultiplier = 1f,
		SpeedMultiplier = 1f,
		DamageMultiplier = 1f,
		Duration = 2f,
        Cooldown = 60f,
		CanStun = true
	};

    /// <summary>
	/// Returns the immutable stats configuration for the given <see cref="JointType"/>.
	/// </summary>
	public static JointTypeStats GetConfig( JointType type )
	{
		return type switch
		{
			JointType.Gannavuri => GannavuriStats,
			JointType.PurpleHaze => PurpleHazeStats,
			JointType.AK47 => AK47Stats,
			JointType.BlueDream => BlueDreamStats,
            JointType.OGKush => OGKushStats,
			_ => GannavuriStats
		};
	}
}