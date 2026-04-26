using Sandbox;

public enum ZombieType
{
    Default,
    Tank,
    Ranger,
    Sprinter
}

public enum ZombieState
{
	Chasing,
	Attacking,
    Stunned,
	Dead,
}

public readonly struct ZombieTypeStats
{
	public float Health { get; init; }
	public float MoveSpeed { get; init; }
	public float Damage { get; init; }
	public float AttackRange { get; init; }
	public float AttackCooldown { get; init; }
	public bool IsArmored { get; init; }
}

/// <summary>
/// Provides per-type stat configurations for each <see cref="ZombieType"/>.
/// All values are returned as immutable structs; nothing is mutated.
/// </summary>
public static class ZombieTypeConfig
{
	private static readonly ZombieTypeStats DefaultStats = new()
	{
		Health = 100f,
		MoveSpeed = 60f,
		Damage = 10f,
		AttackRange = Constants.ZombieAttackRange,
		AttackCooldown = 1.5f,
		IsArmored = false
	};

    private static readonly ZombieTypeStats TankStats = new()
    {
        Health = 200f,
        MoveSpeed = 40f,
        Damage = 25f,
        AttackRange = Constants.ZombieAttackRange * 0.8f,
        AttackCooldown = 2.5f,
        IsArmored = true
    };

    private static readonly ZombieTypeStats RangerStats = new()
    {
        Health = 80f,
        MoveSpeed = 50f,
        Damage = 10f,
        AttackRange = Constants.ZombieAttackRange * 2f,
        AttackCooldown = 2f,
        IsArmored = false
    };

    private static readonly ZombieTypeStats SprinterStats = new()
    {
        Health = 40f,
        MoveSpeed = 85f,
        Damage = 7f,
        AttackRange = Constants.ZombieAttackRange * 0.6f,
        AttackCooldown = 1.5f,
        IsArmored = false
    };

    /// <summary>
	/// Returns the immutable stats configuration for the given <see cref="ZombieType"/>.
	/// </summary>
	public static ZombieTypeStats GetConfig( ZombieType type )
	{
		return type switch
		{
			ZombieType.Default => DefaultStats,
			ZombieType.Tank => TankStats,
			ZombieType.Ranger => RangerStats,
			ZombieType.Sprinter => SprinterStats,
			_ => DefaultStats
		};
	}
}