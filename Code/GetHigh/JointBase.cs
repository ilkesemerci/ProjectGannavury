using System.Runtime.CompilerServices;
using Sandbox;


/// <summary>
/// This component handles the behaviors of a joint when you consume them.
/// </summary>
public sealed partial class JointBase : Component, IUsable
{

	[Sync( SyncFlags.FromHost )]
	public JointType JointType { get; set; }

	[Property] public JointType TestJointType { get; set; }

	public Inventory Inventory;
	public PlayerMovement PlayerMovement;

	public JointTypeStats TypeStats { get; private set; }
	public VisualEffectData EffectData { get; private set; }

	private bool _isFadingOut = false;
    private TimeSince _timeSinceFadeStarted;
	private float _oldSpeed;
	private float _oldRunSpeed;

	private GameObject _player;
	private CameraComponent _cam;

	private Bloom _bloom;
	private Blur _blur;
	private MotionBlur _mBlur;
	private Vignette _vig;
	private ChromaticAberration _chrAb;
	private Pixelate _pix;
	private Sharpen _shrp;
	private FilmGrain _filmGr;
	
	public void OnUse( GameObject user )
	{
		if(!user.IsValid() || !this.IsValid()) return;

		//var cam = user.Children.FirstOrDefault( c => c.Name == "Camera" );
		_cam = user.GetComponentInChildren<CameraComponent>();
		_player = _cam.GameObject;
		Inventory = user.GetComponent<Inventory>();
		PlayerMovement = user.GetComponent<PlayerMovement>();

		user.GetComponent<Inventory>().AddToInventory(GameObject);

	}
    
    public string GetUseText()
	{
		return "Equip Item";
	}

	protected override void OnStart()
	{
		if(TestJointType != JointType.Gannavuri ) JointType = TestJointType;

		InitializeFromType( JointType );
		
	}

	protected override void OnUpdate()
    {
		if(Input.Pressed("smoke")) ApplyWeedEffect();
        DisableWeedEffect();
    }

	private void ApplyWeedEffect()
	{
		switch ( JointType )
		{
			case JointType.Gannavuri:
			GannavuriEffects();
			break;
			case JointType.PurpleHaze:
			PurpleHazeEffects();
			break;
			case JointType.AK47:
			AK47Effects();
			break;
			case JointType.OGKush:
			OGKushEffects();
			break;
			case JointType.BlueDream:
			BlueDreamEffects();
			break;
		}
		Log.Info($"JointType: {JointType}");
	}

	private void DisableWeedEffect()
	{
		switch ( JointType )
		{
			case JointType.Gannavuri:
			DisableGannavuri();
			break;
			case JointType.PurpleHaze:
			DisablePurpleHaze();
			break;
			case JointType.AK47:
			DisableAK47();
			break;
			case JointType.OGKush:
			DisableOGKush();
			break;
			case JointType.BlueDream:
			DisableBlueDream();
			break;
		}
	}

	private async void PurpleHazeEffects()
	{
		_cam.FieldOfView = EffectData.CameraFieldOfView;

		_bloom = _player.Components.GetOrCreate<Bloom>();
		_bloom.Strength = EffectData.BloomStrength;
		_bloom.Threshold = EffectData.BloomThreshold;
		_bloom.Gamma = EffectData.BloomGamma;
		_bloom.Tint = EffectData.BloomTint;
		_mBlur = _player.Components.GetOrCreate<MotionBlur>();
		_mBlur.Scale = EffectData.MotionBlurScale;
		_shrp = _player.Components.GetOrCreate<Sharpen>();
		_shrp.Scale = EffectData.SharpenScale;
		_shrp.TexelSize = EffectData.SharpenTexelSize;
		_vig = _player.Components.GetOrCreate<Vignette>();
		_vig.Color = EffectData.VignetteColor;
		_vig.Intensity = EffectData.VignetteIntensity;
		_vig.Smoothness = EffectData.VignetteSmoothnes;
		_vig.Roundness = EffectData.VignetteRoundness;

		_oldSpeed = PlayerMovement.Speed;
		_oldRunSpeed = PlayerMovement.RunSpeed;

		PlayerMovement.Speed *= TypeStats.SpeedMultiplier;
		PlayerMovement.RunSpeed *= TypeStats.SpeedMultiplier;

		Inventory.ChangeFireRate(TypeStats.FireRateMultiplier);

		_isFadingOut = false;

        // The duration of effect in milliseconds
        await Task.Delay( (int)(EffectData.EffectDuration * 1000) );

        if ( !this.IsValid() ) return;

        // Trigger the fade out loop in OnUpdate
        _isFadingOut = true;
        _timeSinceFadeStarted = 0;
	}
	private void DisablePurpleHaze()
	{
		// Smoothly lerp the effects back to 0 every single frame
        if ( _isFadingOut )
        {
            // Calculate our percentage (0.0 to 1.0) based on time passed
            float fraction = _timeSinceFadeStarted / 3f;
            
            // Clamp it so it doesn't overshoot past 100%
            fraction = MathX.Clamp( fraction, 0f, 1f );

			_cam.FieldOfView = MathX.Lerp( EffectData.CameraFieldOfView , 60f , fraction );

            // Linearly slide the blur size from the max amount down to 0
            _bloom.Strength = MathX.Lerp( EffectData.BloomStrength, 1f, fraction );
			_bloom.Threshold = MathX.Lerp( EffectData.BloomThreshold, 1f, fraction );
			_bloom.Gamma = MathX.Lerp( EffectData.BloomGamma, 2.2f, fraction );

			PlayerMovement.Speed = MathX.Lerp( PlayerMovement.Speed, _oldSpeed, fraction );
			PlayerMovement.RunSpeed = MathX.Lerp( PlayerMovement.RunSpeed, _oldRunSpeed, fraction );
            // Clean up when the fade is 100% complete
            if ( fraction >= 1f )
            {
                _isFadingOut = false;
                _bloom.Destroy(); 
				_shrp.Destroy();
				_vig.Destroy();
				_mBlur.Destroy();
				Inventory.ChangeFireRate(1/TypeStats.FireRateMultiplier);
            }
        }
	}

	private async void GannavuriEffects()
	{
		_blur = _player.Components.GetOrCreate<Blur>();
		_blur.Size = EffectData.BlurSize;
		_chrAb = _player.Components.GetOrCreate<ChromaticAberration>();
		_chrAb.Scale = EffectData.ChromaticScale;
		_filmGr = _player.Components.GetOrCreate<FilmGrain>();
		_filmGr.Intensity = EffectData.FilmGrainIntensity;
		_mBlur = _player.Components.GetOrCreate<MotionBlur>();
		_mBlur.Scale = EffectData.MotionBlurScale;

		_oldSpeed = PlayerMovement.Speed;
		_oldRunSpeed = PlayerMovement.RunSpeed;

		PlayerMovement.Speed *= TypeStats.SpeedMultiplier;
		PlayerMovement.RunSpeed *= TypeStats.SpeedMultiplier;

		Inventory.ChangeFireRate(TypeStats.FireRateMultiplier);
		Inventory.ChangeDamage(TypeStats.DamageMultiplier);

		_isFadingOut = false;
		
		await Task.Delay((int)(EffectData.EffectDuration * 1000));

		if ( !this.IsValid() ) return;

        _isFadingOut = true;
        _timeSinceFadeStarted = 0;
	}
	private void DisableGannavuri()
	{
		 if ( _isFadingOut )
        {
            float fraction = _timeSinceFadeStarted / 3f;
            
            fraction = MathX.Clamp( fraction, 0f, 1f );

			_blur.Size = MathX.Lerp( EffectData.BlurSize, 0f, fraction );
			_chrAb.Scale = MathX.Lerp( EffectData.ChromaticScale, 0f, fraction );
			_filmGr.Intensity = MathX.Lerp( EffectData.FilmGrainIntensity, 0f, fraction );
			_mBlur.Scale = MathX.Lerp( EffectData.MotionBlurScale, 0f, fraction );

			PlayerMovement.Speed = MathX.Lerp( PlayerMovement.Speed, _oldSpeed, fraction );
			PlayerMovement.RunSpeed = MathX.Lerp( PlayerMovement.RunSpeed, _oldRunSpeed, fraction );

            if ( fraction >= 1f )
            {
                _isFadingOut = false;
                _blur.Destroy(); 
				_chrAb.Destroy();
				_filmGr.Destroy();
				_mBlur.Destroy();
				Inventory.ChangeFireRate(1/TypeStats.FireRateMultiplier);
				Inventory.ChangeDamage(1/TypeStats.DamageMultiplier);
            }
        }
	}
	
	private void AK47Effects(){}
	private void DisableAK47(){}

	private void OGKushEffects(){}
	private void DisableOGKush(){}

	private void BlueDreamEffects(){}
	private void DisableBlueDream(){}
	/// <summary>
	/// Initialises stats from the joint type configuration.
	/// Call this after setting <see cref="JointType"/> on the host.
	/// </summary>
	public void InitializeFromType( JointType type )
	{

		JointType = type;
		TypeStats = JointTypeConfig.GetConfig( type );
		EffectData = VisualEffectConfig.GetConfig(type);

	}
}