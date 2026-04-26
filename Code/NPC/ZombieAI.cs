using Sandbox;
using System;
using System.Linq;

/// <summary>
/// AI state machine for zombie NPCs. Drives transitions between Idle, Patrol,
/// Alert, Chase, Attack, and Dead states. All AI logic runs host-only; clients
/// receive state changes via the synced <see cref="ZombieBase.CurrentState"/>.
///
/// Detection is modified by time of day, player stance (crouching vs running),
/// and zombie type configuration.
/// </summary>
[Title( "Zombie AI" )]
[Category( "Zombie" )]
[Icon( "psychology" )]
//[RequireComponent( typeof(ZombieBase) )]
public sealed partial class ZombieAI : Component
{
	// ──────────────────────────────────────────────
	//  References
	// ──────────────────────────────────────────────

	[RequireComponent] public ZombieBase Npc { get; set; }

	// ──────────────────────────────────────────────
	//  Internal state
	// ──────────────────────────────────────────────

	private PlayerMovement _targetPlayer;
	private float _stateTimer;
	private float _idleDuration;
	private float _attackCooldownTimer;

	// ──────────────────────────────────────────────
	//  Lifecycle
	// ──────────────────────────────────────────────

	protected override void OnStart()
	{
		_stateTimer = 0f;
		_idleDuration = Game.Random.Float( 3f, 8f );
		Npc.CurrentState = ZombieState.Chasing;
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		if ( Npc is null || !Npc.IsAlive )
			return;

		_stateTimer += Time.Delta;
		_attackCooldownTimer = MathF.Max( _attackCooldownTimer - Time.Delta, 0f );

		switch ( Npc.CurrentState )
		{
			case ZombieState.Stunned:
				TickStunned();
				break;
			case ZombieState.Chasing:
				TickChasing();
				break;
			case ZombieState.Attacking:
				TickAttacking();
				break;
			case ZombieState.Dead:
				break;
		}
	}
	// ──────────────────────────────────────────────
	//  State: Chasing
	// ──────────────────────────────────────────────

	private void TickChasing()
	{
		// Validate target
		if ( _targetPlayer is null || !_targetPlayer.IsAlive )
		{
			_targetPlayer = Npc.FindClosestPlayer( 2000f );

			if(_targetPlayer is null ) return;
		}

		var distanceToTarget = Vector3.DistanceBetween( WorldPosition, _targetPlayer.WorldPosition );

		// Transition to attack if within range
		if ( distanceToTarget <= Npc.TypeStats.AttackRange )
		{
			TransitionTo( ZombieState.Attacking );
			return;
		}

		// Face the target
		FaceTarget( _targetPlayer.WorldPosition );

		// Move toward target at full speed
		if ( Npc.Agent is not null )
		{
			Npc.Agent.MaxSpeed = Npc.TypeStats.MoveSpeed;
			Npc.Agent.MoveTo( _targetPlayer.WorldPosition );
		}

		if ( Npc.Agent is not null )
    {
        Npc.Agent.MaxSpeed = Npc.TypeStats.MoveSpeed;
        Npc.Agent.MoveTo( _targetPlayer.WorldPosition );

        // Tell the renderer to play the walking animation
        // "move_speed" should match the parameter name in your model's Animgraph
        Npc.ModelRenderer.Set( "move_speed", Npc.Agent.Velocity.Length );
    }
	}

	// ──────────────────────────────────────────────
	//  State: Attacking
	// ──────────────────────────────────────────────

	private void TickAttacking()
	{
		// Validate target
		if ( _targetPlayer is null || !_targetPlayer.IsAlive )
		{
			_targetPlayer = null;
			return;
		}

		var distanceToTarget = Vector3.DistanceBetween( WorldPosition, _targetPlayer.WorldPosition );

		// If target moved out of attack range, go back to chasing
		if ( distanceToTarget > Npc.TypeStats.AttackRange * 1.2f )
		{
			TransitionTo( ZombieState.Chasing );
			return;
		}

		// Face the target
		FaceTarget( _targetPlayer.WorldPosition );

		// Stop moving while attacking
		Npc.Agent?.Stop();

		// Deal damage on cooldown
		if ( _attackCooldownTimer <= 0f )
		{
			DealDamage();
			_attackCooldownTimer = Npc.TypeStats.AttackCooldown;

			Npc.BroadcastPlayAnimation( "attack" );
			Npc.BroadcastPlaySound( "zombie.attack" );
		}
	}

	// ──────────────────────────────────────────────
	//  State: Stunned
	// ──────────────────────────────────────────────

	private async void TickStunned()
	{
		// Validate target
		if ( _targetPlayer is null || !_targetPlayer.IsAlive )
		{
			_targetPlayer = null;
			return;
		}

		// Stop the movements
		Npc.Agent?.Stop();

		// Wait for stun time amount (to be added, static amount just for testing now)
		await Task.Delay(2000);

		// Transition to: chasing if out of range, attacking if within range
		var distanceToTarget = Vector3.DistanceBetween( WorldPosition, _targetPlayer.WorldPosition );

		if ( distanceToTarget > Npc.TypeStats.AttackRange * 1.2f )
		{
			TransitionTo( ZombieState.Chasing );
			return;
		}
		else if ( distanceToTarget <= Npc.TypeStats.AttackRange )
		{
			TransitionTo( ZombieState.Attacking );
			return;
		}
	}

	/// <summary>
	/// Applies damage to the current target player.
	/// </summary>
	private void DealDamage()
	{
		if ( _targetPlayer is null || !_targetPlayer.IsAlive )
			return;

		var damage = Npc.TypeStats.Damage;

		_targetPlayer.TakeDamage( damage );

		Log.Info( $"[ZombieAI] Dealt {damage:F1} damage to {_targetPlayer}" );
	}

	// ──────────────────────────────────────────────
	//  State transitions
	// ──────────────────────────────────────────────

	private void TransitionTo( ZombieState newState )
	{
		if ( Npc.CurrentState == newState )
			return;

		var oldState = Npc.CurrentState;
		Npc.CurrentState = newState;
		_stateTimer = 0f;

		// Per-state entry logic
		switch ( newState )
		{
			case ZombieState.Chasing:
				Npc.BroadcastPlaySound( "zombie.aggro" );
				break;

			case ZombieState.Attacking:
				_attackCooldownTimer = 0f;
				break;
			
			case ZombieState.Stunned:
				break;

			case ZombieState.Dead:
				Npc.Agent?.Stop();
				break;
		}

		Log.Info( $"[ZombieAI] {oldState} -> {newState}" );
	}

	// ──────────────────────────────────────────────
	//  Utility
	// ──────────────────────────────────────────────

	/// <summary>
	/// Rotates the zombie to face a target world position.
	/// </summary>
	private void FaceTarget( Vector3 targetPosition )
	{
		var direction = (targetPosition - WorldPosition).WithZ( 0f );

		if ( direction.Length < 0.1f )
			return;

		var targetRotation = Rotation.LookAt( direction, Vector3.Up );
		WorldRotation = Rotation.Lerp( WorldRotation, targetRotation, Time.Delta * 8f );
	}

	/// <summary>
	/// Called by <see cref="ZombieBase.TakeDamage"/> when this zombie receives damage.
	/// Forces the AI to become aware of the attacker.
	/// </summary>
	public void OnDamagedBy( GameObject attacker )
	{
		if ( attacker is null )
			return;

		// Try to find the SurvivalPlayer on the attacker
		var player = attacker.Components.Get<PlayerMovement>();

		if ( player is null )
			player = attacker.Root.Components.Get<PlayerMovement>();

		if ( player is null || !player.IsAlive )
			return;

		_targetPlayer = player;

	}
}