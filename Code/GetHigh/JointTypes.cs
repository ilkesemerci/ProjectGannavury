using Sandbox;

public enum JointType
{
    Gannavuri,
    PurpleHaze,
    AK47,
    OGKush,
    BlueDream
}

public readonly struct JointTypeStats
{
    public float FireRateMultiplier { get; init; }
	public float SpeedMultiplier { get; init; }
	public float DamageMultiplier { get; init; }
	public float Duration { get; init; }
	public float Cooldown { get; init; }
	public bool CanStun { get; init; }
	public bool UsesBloom { get; init; }
	public bool UsesBlur { get; init; }
	public bool UsesMotionBlur { get; init; }
}

public readonly struct WeedEffectData
{
	public float FadeOutDuration { get; init; }
	public float EffectDuration { get; init; }
    
    // Bloom Settings
    public float BloomStrength { get; init; }
    public float BloomThreshold { get; init; }
    public float BloomGamma { get; init; }
	public Color BloomColor { get; init; }

	// Chromatic Aberation Settings ( can be used as a waving effect )
	public float ChromaticScale { get; init; }

	// Film Grain Settings 
	public float FilmGrainIntensity { get; init; } // between 0 and 1
    
    // Blur Settings
    public float BlurSize { get; init; }
    
    // Motion Blur Settings
    public float MotionBlurScale { get; init; } // between 0 and 1
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
		UsesBlur = true
	};

    private static readonly JointTypeStats PurpleHazeStats = new()
	{
		FireRateMultiplier = 1.1f,
		SpeedMultiplier = 1.5f,
		DamageMultiplier = 1f,
		Duration = 20f,
        Cooldown = 30f,
		UsesBloom = true
	};

    private static readonly JointTypeStats AK47Stats = new()
	{
		FireRateMultiplier = 1.6f,
		SpeedMultiplier = 1f,
		DamageMultiplier = 1.3f,
		Duration = 20f,
        Cooldown = 30f
	};

    private static readonly JointTypeStats OGKushStats = new()
	{
		FireRateMultiplier = 0.7f,
		SpeedMultiplier = 0.6f,
		DamageMultiplier = 15f,
		Duration = 20f,
        Cooldown = 30f
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

/// <summary>
/// Provides per-type effects configurations for each <see cref="JointType"/>.
/// </summary>
public static class WeedEffectConfig
{
	private static readonly WeedEffectData GannavuriEffects = new()
	{
		EffectDuration = 10f,
		FadeOutDuration = 2f,
        BlurSize = 1f
	};

	private static readonly WeedEffectData PurpleHazeEffects = new()
	{
		EffectDuration = 10f,
		FadeOutDuration = 2f,
		BloomStrength = 8f,
		BloomThreshold = 1f,
		BloomGamma = 1.7f,
		BloomColor = Color.Magenta

	}; 

	private static readonly WeedEffectData AK47Effects = new()
	{
		
	}; 

	private static readonly WeedEffectData OGKushEffects = new()
	{
		
	}; 

	private static readonly WeedEffectData BlueDreamEffects = new()
	{
		
	}; 
}