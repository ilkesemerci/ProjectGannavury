using Sandbox;
using System;
using System.Linq;

/// <summary>
/// Base zombie NPC component. Holds health, type configuration, and networked state.
/// Handles damage, death, ragdoll, loot drops, and client-side feedback RPCs.
///
/// Attach to a zombie prefab alongside <see cref="ZombieAI"/>, <see cref="NavMeshAgent"/>,
/// and <see cref="SkinnedModelRenderer"/>.
/// </summary>
[Title( "ZombieBase" )]
[Category( "Zombie" )]
[Icon( "skull" )]
public sealed partial class ZombieBase : Component
{
	// ──────────────────────────────────────────────
	//  References
	// ──────────────────────────────────────────────

	[Property] public NavMeshAgent Agent { get; set; }
	[Property] public SkinnedModelRenderer ModelRenderer { get; set; }

	// ──────────────────────────────────────────────
	//  Networked state
	// ──────────────────────────────────────────────

	[Sync( SyncFlags.FromHost )]
	public float Health { get; set; }

	[Sync( SyncFlags.FromHost )]
	public float MaxHealth { get; set; }

	[Sync( SyncFlags.FromHost )]
	public ZombieType ZombieType { get; set; }

	[Sync( SyncFlags.FromHost )]
	public ZombieState CurrentState { get; set; }

	// ──────────────────────────────────────────────
	//  Configuration (derived from ZombieType)
	// ──────────────────────────────────────────────

	public ZombieTypeStats TypeStats { get; private set; }

	/// <summary>
	/// The last attacker that dealt damage to this zombie.
	/// Used by the AI for threat tracking.
	/// </summary>
	public GameObject LastAttacker { get; private set; }

	/// <summary>
	/// Timestamp when this zombie was last damaged.
	/// </summary>
	public float LastDamageTime { get; private set; }

	public bool IsAlive => CurrentState != ZombieState.Dead;

	// ──────────────────────────────────────────────
	//  Lifecycle
	// ──────────────────────────────────────────────

	protected override void OnStart()
	{
		InitializeFromType( ZombieType );
	}

	protected override void OnUpdate()
	{
		if ( ModelRenderer.IsValid() && Agent.IsValid() )
    	{
        	// Continuously update the walk animation speed based on movement
     		ModelRenderer.Set( "move_speed", Agent.Velocity.Length );
    	}
	}

	/// <summary>
	/// Initialises health and movement stats from the zombie type configuration.
	/// Call this after setting <see cref="ZombieType"/> on the host.
	/// </summary>
	public void InitializeFromType( ZombieType type )
	{
		ZombieType = type;
		TypeStats = ZombieTypeConfig.GetConfig( type );

		if ( Networking.IsHost )
		{
			MaxHealth = TypeStats.Health;
			Health = MaxHealth;
			CurrentState = ZombieState.Chasing;
		}

		// Configure navigation agent to match type
		if ( Agent.IsValid() )
		{
			Agent.MaxSpeed = TypeStats.MoveSpeed;
			//Agent.Height = 72f * TypeStats.HeightScale;
		}

		Log.Info( $"[ZombieNPC] Initialized as {type} with {MaxHealth} HP" );
	}

	protected override void OnDestroy()
	{
		// Stop any active navigation
		Agent?.Stop();

		Log.Info( $"[ZombieNPC] Destroyed ({ZombieType})" );
	}

	// ──────────────────────────────────────────────
	//  Damage
	// ──────────────────────────────────────────────

	/// <summary>
	/// Applies damage to the zombie. Must be called on the host.
	/// Armored zombies (Tank) receive 50% reduced damage.
	/// </summary>
	public void TakeDamage( float amount, GameObject attacker )
	{
		if ( !Networking.IsHost )
			return;

		if ( !IsAlive || amount <= 0f )
			return;

		// Armored zombies take reduced damage
		var effectiveDamage = TypeStats.IsArmored ? amount * 0.5f : amount;

		Health = MathF.Max( Health - effectiveDamage, 0f );
		LastAttacker = attacker;
		LastDamageTime = Time.Now;

		Log.Info( $"[ZombieNPC] Took {effectiveDamage:F1} damage, health: {Health:F1}/{MaxHealth:F1}" );

		// Broadcast hit feedback to all clients
		BroadcastPlaySound( "zombie.hit" );

		if ( Health <= 0f )
		{
			Die();
		}
		else
		{
			// Alert the AI about the attacker
			var ai = Components.Get<ZombieAI>();
			ai?.OnDamagedBy( attacker );

			BroadcastPlayAnimation( "flinch" );
		}
	}

	// ──────────────────────────────────────────────
	//  Death
	// ──────────────────────────────────────────────

	/// <summary>
	/// Handles zombie death: stops navigation, enables ragdoll,
	/// drops loot, and schedules destruction.
	/// </summary>
	private void Die()
	{
		if ( !Networking.IsHost )
			return;

		if ( CurrentState == ZombieState.Dead )
			return;

		CurrentState = ZombieState.Dead;

		// Stop all movement
		Agent?.Stop();

		Log.Info( $"[ZombieNPC] {ZombieType} died" );

		// Broadcast death effects to all clients
		BroadcastPlayAnimation( "die" );
		BroadcastPlaySound( "zombie.death" );
		BroadcastEnableRagdoll();

		// Drop loot at current position
		DropLoot();

		// Schedule cleanup after body despawn time
		DestroyAfterDelay( 30f );
	}

	/// <summary>
	/// Drops loot items at the zombie's current position.
	/// Loot table varies by zombie type.
	/// </summary>
	private void DropLoot()
	{
		if ( !Networking.IsHost )
			return;

		var dropPosition = WorldPosition;

		// TODO: integrate with loot table system
		// LootTable.Drop( ZombieType, dropPosition );

		Log.Info( $"[ZombieNPC] Loot dropped at {dropPosition}" );
	}

	/// <summary>
	/// Destroys this zombie's GameObject after a delay.
	/// </summary>
	private async void DestroyAfterDelay( float seconds )
	{
		await Task.DelaySeconds( seconds );

		if ( GameObject.IsValid() )
		{
			GameObject.Destroy();
		}
	}

	// ──────────────────────────────────────────────
	//  RPCs - Client-side feedback
	// ──────────────────────────────────────────────

	/// <summary>
	/// Broadcasts an animation trigger to all clients for visual feedback.
	/// </summary>
	[Rpc.Broadcast]
	public void BroadcastPlayAnimation( string animationName )
	{
		if ( ModelRenderer is null )
			return;

		ModelRenderer.Set( animationName, true );
	}

	/// <summary>
	/// Broadcasts a sound event to all clients.
	/// </summary>
	[Rpc.Broadcast]
	public void BroadcastPlaySound( string soundName )
	{
		Sound.Play( soundName, WorldPosition );
	}

	/// <summary>
	/// Enables ragdoll physics on all clients for death visuals.
	/// </summary>
	[Rpc.Broadcast]
	private void BroadcastEnableRagdoll()
	{
		if ( !ModelRenderer.IsValid() ) return;
		// Disable the skinned model renderer and enable physics body
		var physicsBody = AddComponent<ModelPhysics>();
		physicsBody.Model = ModelRenderer.Model;

		
		var rigidbody = AddComponent<Rigidbody>();

		if ( physicsBody.IsValid() )
		{
			physicsBody.Enabled = true;
			rigidbody.Velocity = Vector3.Forward * 100f;
		}

		// Disable the nav agent visually
		if ( Agent.IsValid() )
		{
			Agent.Enabled = false;
		}

		Log.Info( "[ZombieNPC] Ragdoll enabled" );
	}

	// ──────────────────────────────────────────────
	//  Utility
	// ──────────────────────────────────────────────

	/// <summary>
	/// Returns the closest alive player to this zombie within the given range.
	/// Returns null if none found.
	/// </summary>
	public PlayerMovement FindClosestPlayer( float range )
	{
		var players = Scene.GetAllComponents<PlayerMovement>()
			.Where( p => p.IsAlive );

		PlayerMovement closest = null;
		var closestDistance = range;

		foreach ( var player in players )
		{
			var distance = Vector3.DistanceBetween( WorldPosition, player.WorldPosition );

			if ( distance < closestDistance )
			{
				closestDistance = distance;
				closest = player;
			}
		}

		return closest;
	}
}