using System;
using Sandbox;
using Sandbox.Citizen;
using Sandbox.Services;

public sealed class PlayerMovement : Component
{
	[Property] public float Health {get;set;} = 100f;
	[Property] public float MaxHealth {get;set;} = 100f;
	[Property] public float RegenAmount {get;set;} = 5f;
	[Property] public int NextRegenTime {get;set;} = 5;
	[Property] public float Armor {get;set;} = 0f;
	[Property] public float MaxArmor {get;set;} = 100f;
	[Property] public int Coins {get;set;} = 0;
	public TimeSince TimeAlive {get;set;} = 0f;
	public bool IsAlive = true;

	[Property] 
	public List<string> Inventory {get;set;} = new List<string>
	{
		"weapon_pistol"
	};
	public int ActiveSlot = 0;
	public int Slots => 9;

	[Property] public SkinnedModelRenderer ModelRenderer {get;set;}
	[Property] public CameraMovement Camera {get;set;}

	[Property, Group("Movement")] public float GroundControl {get;set;} = 4f;
	[Property, Group("Movement")] public float AirControl {get;set;} = 0.1f;
	[Property, Group("Movement")] public float MaxForce {get;set;} = 50f;
	[Property, Group("Movement")] public float Speed {get;set;} = 160f;
	[Property, Group("Movement")] public float RunSpeed {get;set;} = 290f;
	[Property, Group("Movement")] public float CrouchSpeed {get;set;} = 90f;
	[Property, Group("Movement")] public float JumpForce {get;set;} = 400f;

	[Property] public GameObject Head {get;set;}
	[Property] public GameObject Body {get;set;}
	[Property] public Inventory Item {get;set;}

	[Property] public float PunchRange{get;set;}
	
	[Property] public float PunchDamage{get;set;}

	[Property] public float PunchCooldown{get;set;}

	public TimeUntil NextPunch;
	private TimeUntil _resetPose;
	private TimeUntil _nextRegen = 0;

	public Vector3 WishVelocity = Vector3.Zero;
	public bool IsCrouching = false;
	public bool IsSprinting = false;

	private CharacterController _characterController;
	private CitizenAnimationHelper _animationHelper;
	private ModelPhysics _ragdoll;

	protected override void OnStart()
	{
		Health = MaxHealth;
		Armor = 0f;
		IsAlive = true;
	}
	protected override void OnAwake()
	{
		_characterController = Components.Get<CharacterController>();
		_animationHelper = Components.Get<CitizenAnimationHelper>();
	}

	protected override void OnUpdate()
	{
		if(ModelRenderer.IsValid())
        {
            var remappedHealth = MathX.Remap(Health,0f,MaxHealth,0f,100f);
		    var currentHealth = ModelRenderer.GetFloat("health");
		    var lerpedHealth = MathX.Lerp(currentHealth,remappedHealth, Time.Delta*2f);
		    ModelRenderer.Set("health",lerpedHealth);
        }

		Regen();

        DebugOverlay.Text(WorldPosition + Vector3.Up * 80f, $"Health: [{Health}/{MaxHealth}]");
        if ( Health <= 0  ) return;


		UpdateCrouch();
		IsSprinting = Input.Down("Run");
		
		if(Input.Pressed("Jump")) Jump();

		if(Input.MouseWheel.y != 0 )
		{
			ActiveSlot = (ActiveSlot + Math.Sign(Input.MouseWheel.y)) % Slots;
		}

		RotateBody();
		UpdateAnimations();

	}

	protected override void OnFixedUpdate()
	{
		BuildWishVelocity();
		Move();

		if ( Input.Down( "Melee" ) && NextPunch )
		{
			Punch();
			NextPunch = PunchCooldown;
		}
		if ( _resetPose && Item.IsValid() )
		{
			Item.ChangeHoldType();

		}
	}

	private void BuildWishVelocity()
	{
		WishVelocity = 0;
		
		var rot = Head.WorldRotation;
		if(Input.Down("Forward")) WishVelocity += rot.Forward;
		if(Input.Down("Backward")) WishVelocity += rot.Backward;
		if(Input.Down("Left")) WishVelocity += rot.Left;
		if(Input.Down("Right")) WishVelocity += rot.Right;

		WishVelocity = WishVelocity.WithZ(0);
		if(!WishVelocity.IsNearZeroLength) WishVelocity = WishVelocity.Normal;

		if(IsCrouching) WishVelocity *= CrouchSpeed;
		else if(IsSprinting) WishVelocity *= RunSpeed;
		else WishVelocity *= Speed;
	}

	private void Move()
	{
		var gravity = Scene.PhysicsWorld.Gravity;

		if ( _characterController.IsOnGround )
		{
			_characterController.Velocity = _characterController.Velocity.WithZ(0);
			_characterController.Accelerate(WishVelocity);
			_characterController.ApplyFriction(GroundControl);

		}
		else
		{
			_characterController.Velocity += gravity * Time.Delta * 0.5f;
			_characterController.Accelerate(WishVelocity.ClampLength(MaxForce));
			_characterController.ApplyFriction(AirControl);

		}

		_characterController.Move();

		if ( !_characterController.IsOnGround )
		{
			_characterController.Velocity += gravity * Time.Delta * 0.5f;
		}
		else
		{
			_characterController.Velocity = _characterController.Velocity.WithZ(0);
		}
	}

	private void RotateBody()
	{
		if(!Body.IsValid()) return;

		var targetAngle = new Angles(0 , Head.WorldRotation.Yaw() , 0 ).ToRotation();

		float rotateDifference = Body.WorldRotation.Distance(targetAngle);

		if(rotateDifference > 50f || _characterController.Velocity.Length > 10f )
		{
			Body.WorldRotation = Rotation.Lerp( Body.WorldRotation , targetAngle , Time.Delta * 2f);
		}
	}

	private void Jump()
	{
		if(!_characterController.IsOnGround) return;

		_characterController.Punch(Vector3.Up * JumpForce);
		//if ( !Camera.IsFirstPerson )
		//{
		//	_animationHelper?.TriggerJump();
		//}
		//else
		//{
		//}
		
	}

	private void Regen()
	{
		if(Health > 0 && Health < MaxHealth && _nextRegen <= 0 )
		{
			Health += RegenAmount;
			_nextRegen = NextRegenTime;
		}
	}

	private void UpdateAnimations()
	{
		if(!_animationHelper.IsValid()) return;

		_animationHelper.WithWishVelocity(WishVelocity);
		_animationHelper.WithVelocity(_characterController.Velocity);
		_animationHelper.AimAngle = Head.WorldRotation;
		_animationHelper.IsGrounded = _characterController.IsOnGround;
		_animationHelper.WithLook(Head.WorldRotation.Forward,1f,0.75f,0.5f);
		_animationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		_animationHelper.DuckLevel = IsCrouching ? 1f : 0f;
		
	}

	private void UpdateCrouch()
	{

		if(!_characterController.IsValid()) return;

		bool wantsToCrouch = Input.Down("Crouch");

		if(wantsToCrouch && !IsCrouching )
		{
			IsCrouching = true;
			_characterController.Height /= 2;
		}
		else if(!wantsToCrouch && IsCrouching )
		{
			var upTrace = Scene.Trace.Ray(Head.WorldPosition, Head.WorldPosition + (Vector3.Up * 30f))
				.Radius(7f)
				.WithoutTags("player","trigger")
				.Run();
			if ( !upTrace.Hit )
			{
				IsCrouching = false;
				_characterController.Height *= 2;
			}
		}
	}

	public void Punch()
	{
		ModelRenderer.Set("holdtype",5);
		ModelRenderer.Set("b_attack",true);
		_resetPose = 3f;
		
		var punchDirection = Head.WorldRotation.Forward;
		var punchStartPosition = Head.WorldPosition;
		var punchEndPosition = punchStartPosition + punchDirection * PunchRange;

		var punchTrace = Scene.Trace.Ray(punchStartPosition,punchEndPosition)
		.Radius(20f)
		.WithoutTags("player")
		.IgnoreGameObjectHierarchy(GameObject)
		.Run();

		if(!punchTrace.Hit) return;
		if(!punchTrace.GameObject.Components.TryGet<UnitComponent>(out var unit )) return;

		unit.Damage(PunchDamage);

	}

	public void TakeDamage( float amount )
    {
        // If we have the armored stat, maybe we take half damage!
        if ( Armor > 1 ) amount *= 0.5f;


        var health = Health - amount;

        var difference = amount - Health;
		Health = float.Clamp(health,0f,MaxHealth);

		if( !ModelRenderer.IsValid()) return;

		if(difference < 0 )
		{
			var remappedDamage = MathX.Remap(-difference,0f,MaxHealth,0f,100f);
			_characterController.Punch(Vector3.Up * 1f);
			DamageAnimation(remappedDamage);
		}
		

		if(Health <= 0 )
		{
			Kill();
		}
    }

    private async void DamageAnimation(float damage )
	{
		ModelRenderer.LocalScale *= 1.1f;
		ModelRenderer.Tint = Color.Red;

		await Task.DelaySeconds(0.15f);

		ModelRenderer.GameObject.LocalScale /= 1.1f;
		ModelRenderer.Tint = Color.White;
	}

    public async void Kill()
	{
		if(IsAlive == false) return;
		
		IsAlive = false;

		_characterController.Enabled = false;
		Ragdoll();

		await Task.DelaySeconds(3f);

		GameObject.Destroy();

	}

    public void Ragdoll()
	{
		if(!ModelRenderer.IsValid()) return;
		if(_ragdoll.IsValid()) return;

		_ragdoll = AddComponent<ModelPhysics>();
		_ragdoll.Renderer = ModelRenderer;
		_ragdoll.Model = ModelRenderer.Model;

	}
}
