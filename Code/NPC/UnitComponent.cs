using System;
using System.Diagnostics.CodeAnalysis;
using Sandbox;

public enum TeamType
{
	[Icon("tag_faces")]
	[Description("Players, Turrets and Whatnots")]
	Player,
	[Icon("warning")]
	[Description("Anything green and mean")]
	Enemy
}
public sealed class UnitComponent : Component
{
	/// <summary>
	/// The name displayed for your unit
	/// </summary>
	[Property]
	[Category("Info")]
	public string Name {get; set;}
	/// <summary>
	/// TeamTyoe
	/// </summary>
	[Property]
	[Category("Info")]
	public TeamType Team {get; set;}

	[Property]
	[Category("Health")]
	public float MaxHealth {get;set;}= 100f;

	[Property]
	[Category("Components")]
	public SkinnedModelRenderer ModelRenderer {get; set;}

	/// <summary>
	/// How much health is regenerated per second
	/// </summary>
	[Property]
	[Category("Health")]
	public float HealthRegeneration {get;set;} = 5f;

	private float _health;

	public bool Alive = true;

	private TimeUntil _nextRegen;
	private ModelPhysics _ragdoll;
	private Vector3 _spawnPosition;
	public float Health
	{
		get
		{
			return _health;
		}
		set
		{
			UpdateHealth(value);
		}
	}
	protected override void OnFixedUpdate()
	{
		if ( _nextRegen )
		{
			Heal(HealthRegeneration);
			_nextRegen = 5f;
		}
	}
	protected override void OnUpdate()
	{
		if(!Alive) return;
		if( !ModelRenderer.IsValid()) return;
		
		if(Team == TeamType.Enemy )
		{
			var remappedHealth = MathX.Remap(Health,0f,MaxHealth,0f,100f);
			var currentHealth = ModelRenderer.GetFloat("health");
			var lerpedHealth = MathX.Lerp(currentHealth,remappedHealth, Time.Delta*2f);
			ModelRenderer.Set("health",lerpedHealth);
		}

		DebugOverlay.Text(WorldPosition + Vector3.Up * 80f, $"{Name} [{Health}/{MaxHealth}]");

	}

	protected override void OnStart()
	{
		_health = MaxHealth;
		_spawnPosition = WorldPosition;

	}

	[Button]
	[Category("Health")]
	public void HurtDebug()
	{
		Damage(10f);
	}

	[Button]
	[Category("Health")]
	public void HealDebug()
	{
		Heal(10f);
	}

	[Button]
	[Category("Health")]
	public void HurtLotDebug()
	{
		Damage(30f);
	}

	/// <summary>
	/// Positive = hurt, Negative = heal
	/// </summary>
	/// <param name="damage"></param>
	public void Damage(float damage )
	{
		if(!Alive) return;
		Health -= damage;

		if(damage >= 0f )
		{
			_nextRegen = 5f;
		}
	}

	public void Heal(float heal )
	{
		if(!Alive) return;
		Health += heal;

	}

	private void UpdateHealth(float newHealth)
	{
		var difference = newHealth - Health;
		_health = float.Clamp(newHealth,0f,MaxHealth);

		if( !ModelRenderer.IsValid()) return;

		if(difference < 0 )
		{
			var remappedDamage = MathX.Remap(-difference,0f,MaxHealth,0f,100f);
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

	private void DeathAnimation()
	{
		ModelRenderer.Set("dead",true);
	}

	public async void Kill()
	{
		Alive = false;

		DeathAnimation();
		Ragdoll();

		await Task.DelaySeconds(3f);

		Respawn();

		var playerComponent = GetComponent<PlayerComponent>();
		if ( playerComponent.IsValid() )
		{
			playerComponent.Ragdoll();
		}

		await Task.DelaySeconds(3f);

	}

	[Button]
	public void Ragdoll()
	{
		if(!ModelRenderer.IsValid()) return;
		if(_ragdoll.IsValid()) return;

		_ragdoll = AddComponent<ModelPhysics>();
		_ragdoll.Renderer = ModelRenderer;
		_ragdoll.Model = ModelRenderer.Model;

	}

	[Button]
	public void Unragdoll()
	{
		if(!ModelRenderer.IsValid()) return;
		if(!_ragdoll.IsValid()) return;

		_ragdoll.Destroy();

	}

	public void Respawn()
	{
		Alive = true;
		Health = MaxHealth;
		Unragdoll();
		WorldPosition = _spawnPosition;
	}
}
