using Sandbox;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Dynamic zombie spawner that manages a local population of zombies around
/// its position. Respects player proximity constraints, global zombie limits,
/// and per-spawner capacity. Despawns zombies that drift too far from all players.
///
/// Place these in the scene at locations where zombies should appear. Runs host-only.
/// </summary>

[Title( "Zombie Spawner" )]
[Category( "Zombie" )]
[Icon( "group_add" )]
public sealed partial class ZombieSpawner : Component
{
	// ──────────────────────────────────────────────
	//  Configuration
	// ──────────────────────────────────────────────

	/// <summary>Radius around the spawner in which zombies can appear.</summary>
	[Property, Group( "Spawn" )] public float SpawnRadius { get; set; } = 400f;

	/// <summary>Minimum distance from any player before a zombie can spawn. Prevents pop-in.</summary>
	[Property, Group( "Spawn" )] public float MinSpawnDistance { get; set; } = 200f;

	/// <summary>Maximum number of active zombies this spawner manages at once.</summary>
	[Property, Group( "Spawn" )] public int MaxZombies { get; set; } = 8;

	/// <summary>Seconds between spawn attempts.</summary>
	[Property, Group( "Spawn" )] public float SpawnInterval { get; set; } = 10f;

	/// <summary>Default zombie type for this spawner.</summary>
	[Property, Group( "Spawn" )] public ZombieType ZombieType { get; set; } = ZombieType.Default;

	/// <summary>Prefab to clone when spawning a zombie.</summary>
	[Property, Group( "Spawn" )] public GameObject ZombiePrefab { get; set; }


	/// <summary>
	/// Maximum distance from any player for this spawner to be active.
	/// If all players are further than this, the spawner pauses.
	/// </summary>
	[Property, Group( "Activation" )] public float ActivationRange { get; set; } = 1500f;

	// ──────────────────────────────────────────────
	//  Internal state
	// ──────────────────────────────────────────────

	private readonly List<GameObject> _spawnedZombies = new();
	private float _spawnTimer;

	/// <summary>Read-only view of currently managed zombie GameObjects.</summary>
	public IReadOnlyList<GameObject> SpawnedZombies => _spawnedZombies;

	// ──────────────────────────────────────────────
	//  Lifecycle
	// ──────────────────────────────────────────────

	protected override void OnFixedUpdate()
	{
		if ( !Networking.IsHost )
			return;

		CleanupDeadReferences();

		if ( !IsAnyPlayerInActivationRange() )
			return;

		_spawnTimer += Time.Delta;

		if ( _spawnTimer >= SpawnInterval )
		{
			_spawnTimer = 0f;
			TrySpawnZombie();
		}
	}

	protected override void OnDestroy()
	{
		// Clean up all managed zombies when the spawner is destroyed
		foreach ( var zombie in _spawnedZombies )
		{
			if ( zombie.IsValid() )
			{
				zombie.Destroy();
			}
		}

		_spawnedZombies.Clear();
	}

	// ──────────────────────────────────────────────
	//  Spawning
	// ──────────────────────────────────────────────

	/// <summary>
	/// Attempts to spawn a new zombie if all conditions are met:
	/// under max capacity, under global limit, valid spawn position found.
	/// </summary>
	private void TrySpawnZombie()
	{
		// Check per-spawner capacity
		if ( _spawnedZombies.Count >= MaxZombies )
			return;

		// Check global zombie limit
		var globalZombieCount = Scene.GetAllComponents<ZombieBase>().Count();
		var globalMax = (int)(GameConfig.MaxZombies * GameConfig.ZombieCountMultiplier);

		if ( globalZombieCount >= globalMax )
			return;

		// Find a valid spawn position
		var spawnPosition = FindValidSpawnPosition();

		if ( !spawnPosition.HasValue )
			return;

		SpawnZombieAt( spawnPosition.Value, ZombieType );
	}

	/// <summary>
	/// Spawns a zombie of the specified type at the given world position.
	/// Returns the spawned GameObject, or null if the prefab is missing.
	/// </summary>
	public GameObject SpawnZombieAt( Vector3 position, ZombieType type )
	{
		if ( !Networking.IsHost )
			return null;

		if ( ZombiePrefab is null )
		{
			Log.Warning( "[ZombieSpawner] ZombiePrefab is not assigned!" );
			return null;
		}

		var zombieGo = ZombiePrefab.Clone( new Transform( position ) );
		zombieGo.BreakFromPrefab();
		zombieGo.Name = $"Zombie ({type})";

		// Initialize the NPC component with the correct type
		var npc = zombieGo.Components.Get<ZombieBase>();

		if ( npc is not null )
		{
			npc.InitializeFromType( type );
		}

		zombieGo.NetworkSpawn();

		_spawnedZombies.Add( zombieGo );

		Log.Info( $"[ZombieSpawner] Spawned {type} at {position} (total: {_spawnedZombies.Count}/{MaxZombies})" );

		return zombieGo;
	}

	/// <summary>
	/// Tries to find a spawn position within <see cref="SpawnRadius"/> that is
	/// at least <see cref="MinSpawnDistance"/> away from all players.
	/// Uses the NavMesh for a walkable position. Returns null if no valid position found.
	/// </summary>
	private Vector3? FindValidSpawnPosition()
	{
		const int maxAttempts = 10;

		for ( var i = 0; i < maxAttempts; i++ )
		{
			var randomPoint = Scene.NavMesh.GetRandomPoint( WorldPosition, SpawnRadius );

			if ( !randomPoint.HasValue )
				continue;

			var candidatePosition = randomPoint.Value;

			// Ensure it is not too close to any player
			if ( IsPositionTooCloseToPlayer( candidatePosition ) )
				continue;

			return candidatePosition;
		}

		return null;
	}

	// ──────────────────────────────────────────────
	//  Despawning
	// ──────────────────────────────────────────────

	/// <summary>
	/// Removes entries from the tracked list for zombies that have been destroyed
	/// externally (e.g., killed by a player).
	/// </summary>
	private void CleanupDeadReferences()
	{
		_spawnedZombies.RemoveAll( z => !z.IsValid() );
	}

	// ──────────────────────────────────────────────
	//  Utility
	// ──────────────────────────────────────────────

	/// <summary>
	/// Checks whether any alive player is within <see cref="ActivationRange"/>
	/// of this spawner. The spawner pauses spawning if no player is near.
	/// </summary>
	private bool IsAnyPlayerInActivationRange()
	{
		var players = Scene.GetAllComponents<PlayerMovement>()
			.Where( p => p.IsAlive );

		foreach ( var player in players )
		{
			var distance = Vector3.DistanceBetween( WorldPosition, player.WorldPosition );

			if ( distance <= ActivationRange )
				return true;
		}

		return false;
	}

	/// <summary>
	/// Returns true if the candidate position is closer than <see cref="MinSpawnDistance"/>
	/// to any alive player.
	/// </summary>
	private bool IsPositionTooCloseToPlayer( Vector3 position )
	{
		var players = Scene.GetAllComponents<PlayerMovement>()
			.Where( p => p.IsAlive );

		foreach ( var player in players )
		{
			var distance = Vector3.DistanceBetween( position, player.WorldPosition );

			if ( distance < MinSpawnDistance )
				return true;
		}

		return false;
	}
}