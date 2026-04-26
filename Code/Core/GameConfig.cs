using Sandbox;

/// <summary>
/// Server-configurable gameplay settings exposed as ConVars.
/// </summary>
public static class GameConfig
{
	//[ConVar( "sdz_friendly_fire" )]
	//public static bool FriendlyFire { get; set; } = true;

	[ConVar( "sdz_zombie_count_multiplier" )]
	public static float ZombieCountMultiplier { get; set; } = 1f;

	[ConVar( "sdz_max_zombies" )]
	public static int MaxZombies { get; set; } = 200;

	[ConVar( "sdz_starting_items" )]
	public static bool GiveStartingItems { get; set; } = true;

	[ConVar( "sdz_headshot_multiplier" )]
	public static float HeadshotMultiplier { get; set; } = Constants.HeadshotMultiplier;
}