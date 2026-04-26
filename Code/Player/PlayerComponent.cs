using Sandbox;

public sealed class PlayerComponent : Component
{
	[Property]
	public SkinnedModelRenderer ModelRenderer{get;set;}

	[Property]
	public PlayerController Controller{get;set;}

	[Property]
	public UnitComponent UnitComponent{get;set;}

	[Property]
	public float PunchRange{get;set;}
	
	[Property]
	public float PunchDamage{get;set;}

	[Property]
	public float PunchCooldown{get;set;}

	public TimeUntil NextPunch;
	private ModelPhysics _ragdoll;
	private Vector3 _spawnPosition;
	private TimeUntil _resetPose;

	protected override void OnStart()
	{
		_spawnPosition = WorldPosition;
	}

	protected override void OnFixedUpdate()
	{

		if ( Input.Down( "Attack1" ) && NextPunch )
		{
			Punch();
			NextPunch = PunchCooldown;
		}
		if ( _resetPose )
		{
			ModelRenderer.Set("holdtype",0);
		}
	}

	[Button]
	public void Ragdoll()
	{
		if(!ModelRenderer.IsValid()) return;
		if(_ragdoll.IsValid()) return;

		_ragdoll = AddComponent<ModelPhysics>();
		_ragdoll.Renderer = ModelRenderer;
		_ragdoll.Model = ModelRenderer.Model;

		Controller.UseInputControls = false;

	}

	public void Punch()
	{
		ModelRenderer.Set("holdtype",5);
		ModelRenderer.Set("b_attack",true);
		_resetPose = 3f;
		
		var punchDirection = Controller.EyeAngles.Forward;
		var punchStartPosition = Controller.EyePosition;
		var punchEndPosition = punchStartPosition + punchDirection * PunchRange;

		var punchTrace = Scene.Trace.Ray(punchStartPosition,punchEndPosition)
		.Radius(20f)
		.WithoutTags("player")
		.IgnoreGameObjectHierarchy(GameObject)
		.Run();

		if(!punchTrace.Hit) return;
		if(!punchTrace.GameObject.Components.TryGet<UnitComponent>(out var unit )) return;
		if(unit.Team == UnitComponent.Team) return;

		unit.Damage(PunchDamage);

	}

	[Button]
	public void Unragdoll()
	{
		if(!ModelRenderer.IsValid()) return;
		if(!_ragdoll.IsValid()) return;

		_ragdoll.Destroy();

		Controller.UseInputControls = true;

	}

	public void Respawn()
	{
		Unragdoll();
		UnitComponent.Alive = true;
		UnitComponent.Health = UnitComponent.MaxHealth;
		WorldPosition = _spawnPosition;
	}
}
