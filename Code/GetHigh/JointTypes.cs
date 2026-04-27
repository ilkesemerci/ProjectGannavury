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
	public float Cooldown { get; init; }
}

public readonly struct VisualEffectData
{
	public float FadeOutDuration { get; init; }
	public float EffectDuration { get; init; }

	// Camera Settings
	public float CameraFieldOfView { get; init; }
    
    // Bloom Settings
    public float BloomStrength { get; init; }
    public float BloomThreshold { get; init; }
    public float BloomGamma { get; init; }
	public Color BloomTint { get; init; }

	// Chromatic Aberation Settings ( can be used as a waving effect )
	public float ChromaticScale { get; init; }

	// Pixelate Settings
	public float PixelateScale { get; init; } // between 0 and 1

	// Sharpen Settings
	public float SharpenScale { get; init; }
	public float SharpenTexelSize { get; init; }

	// Vignette Settings
	public Color VignetteColor { get; init; }
	public float VignetteIntensity { get; init; }
	public float VignetteSmoothnes { get; init; }
	public float VignetteRoundness { get; init; }

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
		FireRateMultiplier = 0.92f,
		SpeedMultiplier = 0.8f,
		DamageMultiplier = 1.3f,
        Cooldown = 30f,
	};

    private static readonly JointTypeStats PurpleHazeStats = new()
	{
		FireRateMultiplier = 0.7f,
		SpeedMultiplier = 1.5f,
		DamageMultiplier = 1f,
        Cooldown = 30f,
	};

    private static readonly JointTypeStats AK47Stats = new()
	{
		FireRateMultiplier = 0.85f,
		SpeedMultiplier = 1f,
		DamageMultiplier = 1.8f,
        Cooldown = 30f
	};

    private static readonly JointTypeStats OGKushStats = new()
	{
		FireRateMultiplier = 1.3f,
		SpeedMultiplier = 0.6f,
		DamageMultiplier = 15f,
        Cooldown = 30f
	};

    private static readonly JointTypeStats BlueDreamStats = new()
	{
		FireRateMultiplier = 1f,
		SpeedMultiplier = 1f,
		DamageMultiplier = 1f,
        Cooldown = 45f,
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
public static class VisualEffectConfig
{
	private static readonly VisualEffectData GannavuriEffects = new()
	{
		EffectDuration = 20f,
		FadeOutDuration = 2f,
		
		BlurSize = 0.06f,
		ChromaticScale = 0.5f,
		FilmGrainIntensity = 0.04f,
		MotionBlurScale = 0.1f

	};

	private static readonly VisualEffectData PurpleHazeEffects = new()
	{
		EffectDuration = 20f,
		FadeOutDuration = 2f,
		CameraFieldOfView = 80f,
		BloomStrength = 2f,
		BloomThreshold = 1.2f,
		BloomGamma = 1.8f,
		BloomTint = Color.Magenta,
		MotionBlurScale = 0.7f,
		SharpenScale = 0.5f,
		SharpenTexelSize = 2f,
		VignetteColor = Color.Magenta,
		VignetteIntensity = 0.3f,
		VignetteSmoothnes = 1f,
		VignetteRoundness = 0.6f

	}; 

	private static readonly VisualEffectData AK47Effects = new()
	{
		
	}; 

	private static readonly VisualEffectData OGKushEffects = new()
	{
		
	}; 

	private static readonly VisualEffectData BlueDreamEffects = new()
	{
		
	}; 

	/// <summary>
	/// Returns the immutable stats configuration for the given <see cref="JointType"/>.
	/// </summary>
	public static VisualEffectData GetConfig( JointType type )
	{
		return type switch
		{
			JointType.Gannavuri => GannavuriEffects,
			JointType.PurpleHaze => PurpleHazeEffects,
			JointType.AK47 => AK47Effects,
			JointType.BlueDream => BlueDreamEffects,
            JointType.OGKush => OGKushEffects,
			_ => GannavuriEffects
		};
	}
}