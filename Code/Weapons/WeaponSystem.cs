using Sandbox;
using Sandbox.Audio;
using System;

public sealed class WeaponSystem : Component, IUsable
{
    [Property, Group("Weapon Stats")] public float Damage { get; set; } = 25f;
    [Property, Group("Weapon Stats")] public float FireRate { get; set; } = 0.1f;
    [Property, Group("Weapon Stats")] public int MagSize { get; set; } = 30;
    [Property, Group("Weapon Stats")] public float Range { get; set; } = 5000f;
    [Property, Group("Weapon Stats")] public float ReloadTime { get; set; } = 1350f;
    [Property, Group("Audio")] public SoundEvent EmptyClipSound {get;set;}
    [Property, Group("Audio")] public SoundEvent FireSound {get;set;}
    [Property, Group("Audio")] public SoundEvent ReloadSound {get;set;}
    
    private SkinnedModelRenderer _modelRenderer;

    public int CurrentAmmo { get; private set; }
    public bool IsReloading {get; private set;}
    public bool IsEquipped = false;

    private TimeSince _timeSinceLastShot = 0f; 

    public void OnUse(GameObject user)
    {
        var inventory = user.GetComponent<Inventory>();
        if ( inventory.IsValid() )
        {
            inventory.AddToInventory( GameObject );
        }
        _modelRenderer = user.GetComponentInChildren<SkinnedModelRenderer>();
        IsEquipped = true;
        Log.Info("Used this item!");
    }

    public string GetUseText()
    {
        return "Press E to take weapon";
    }

    protected override void OnStart()
    {
        CurrentAmmo = MagSize; 
        _modelRenderer = GameObject.Root.GetComponentInChildren<SkinnedModelRenderer>();
        //Log.Info($"model renderer on {_modelRenderer.GameObject}");
    }

    protected override void OnUpdate()
    {
        if(GameObject.Tags.Has("knife") && _timeSinceLastShot >= FireRate && Input.Down( "attack1" ) )
        {
            if(_modelRenderer.IsValid()) _modelRenderer.Set("b_attack",true);
            Punch();

            _timeSinceLastShot = 0;
        } 
        if (!GameObject.Tags.Has("knife") &&_timeSinceLastShot >= FireRate && !IsReloading && Input.Down( "attack1" ) && IsEquipped)
        {

			if(_modelRenderer.IsValid()) _modelRenderer.Set("b_attack",true);
            Shoot();
			

			_timeSinceLastShot = 0;
        }

        if (!GameObject.Tags.Has("knife") && Input.Pressed( "reload" ) && !IsReloading && CurrentAmmo < MagSize && IsEquipped)
        {
			if(_modelRenderer.IsValid()) _modelRenderer.Set("b_reload",true);
            Reload();
        }
    }

    public void Shoot()
    {
        if(CurrentAmmo < 1 )
        {
            Sound.Play(EmptyClipSound);
            return;
        }
        else
        {
            Sound.Play(FireSound);
        } 
        CurrentAmmo--;
        Log.Info( $"Shot fired! Ammo left: {CurrentAmmo}/{MagSize}" );

        var startPos = Scene.Camera.WorldPosition; 
        var direction = Scene.Camera.WorldRotation.Forward;
        var endPos = startPos + (direction * Range);

        var trace = Scene.Trace.Ray( startPos, endPos )
            .UseHitboxes()
            .UsePhysicsWorld()
            .IgnoreGameObjectHierarchy( GameObject.Root ) 
            .Run();

        if ( trace.Hit && trace.GameObject.IsValid() )
        {
            var unit = trace.GameObject.Components.Get<ZombieBase>( FindMode.EverythingInSelfAndParent );
            if ( unit.IsValid() )
            {
                unit.TakeDamage( Damage, GameObject.Root );
            }
        }
    }

    public async void Reload()
    {
        IsReloading = true;
        Sound.Play(ReloadSound);
        Log.Info( "Reloading..." );

        // Task.Delay expects milliseconds, so we multiply our seconds by 1000.
        // The 'await' keyword pauses THIS method right here, but lets the rest of the game keep running.
        await Task.Delay( (int)(ReloadTime) );

        if ( !this.IsValid() ) return;

        // Instantly refill for now. You can add an animation delay timer here later!
        CurrentAmmo = MagSize;
        IsReloading = false;
        Log.Info( "Reloaded!" );
    }

    public void Punch()
	{	
		var startPos = Scene.Camera.WorldPosition; 
        var direction = Scene.Camera.WorldRotation.Forward;
        var endPos = startPos + (direction * Range);

        var trace = Scene.Trace.Ray( startPos, endPos )
            .Radius(20f)
            .UseHitboxes()
            .UsePhysicsWorld()
            .IgnoreGameObjectHierarchy( GameObject.Root ) 
            .Run();

        if ( trace.Hit && trace.GameObject.IsValid() )
        {
            var unit = trace.GameObject.Components.Get<ZombieBase>( FindMode.EverythingInSelfAndParent );
            if ( unit.IsValid() )
            {
                unit.TakeDamage( Damage, GameObject.Root );
            }
        }
        Log.Info("knife shoot");

	}
}