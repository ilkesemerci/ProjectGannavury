using Sandbox;


/// <summary>
/// This component handles the behaviors of a joint when you consume them.
/// </summary>
public sealed partial class JointBase : Component, IUsable, IJoint
{

	[Sync( SyncFlags.FromHost )]
	public JointType JointType { get; set; }

	public JointTypeStats TypeStats { get; private set; }

	private GameObject _cam;
	private bool _isFadingOut = false;
    private TimeSince _timeSinceFadeStarted;
	private Bloom _bloom;
	
	public void OnUse( GameObject user )
	{
		if(!user.IsValid() || !this.IsValid()) return;

		//var cam = user.Children.FirstOrDefault( c => c.Name == "Camera" );
		_cam = user.GetComponentInChildren<CameraComponent>().GameObject;

		user.GetComponent<Inventory>().AddToInventory(GameObject);

	}
    
    public string GetUseText()
	{
		return "Equip Item";
	}

	public async void ApplyWeedEffect()
	{
		switch ( JointType )
		{
			case JointType.Gannavuri:
			break;
			case JointType.PurpleHaze:
			break;
			case JointType.AK47:
			break;
			case JointType.OGKush:
			break;
			case JointType.BlueDream:
			break;
		}
        _bloom = _cam.Components.GetOrCreate<Bloom>();

        _bloom.Strength = 8f;
		_bloom.Threshold= 1f;
		_bloom.Gamma = 1.7f;
		_bloom.Tint = Color.Magenta;
        _isFadingOut = false;

        // Wait for the duration (Converted to milliseconds!)
        await Task.Delay( (int)(5 * 1000) );

        // Safety check: Did the player die or drop the item during those 20 seconds?
        if ( !this.IsValid() || !_bloom.IsValid() ) return;

        // Trigger the fade out loop in OnUpdate
        _isFadingOut = true;
        _timeSinceFadeStarted = 0;
	}

	protected override void OnStart()
	{
		InitializeFromType( JointType );
	}

	protected override void OnUpdate()
    {
		if(Input.Pressed("smoke")) ApplyWeedEffect();
        // Smoothly lerp the blur back to 0 every single frame
        if ( _isFadingOut && _bloom.IsValid() )
        {
            // Calculate our percentage (0.0 to 1.0) based on time passed
            float fraction = _timeSinceFadeStarted / 3f;
            
            // Clamp it so it doesn't overshoot past 100%
            fraction = MathX.Clamp( fraction, 0f, 1f );

            // Linearly slide the blur size from the max amount down to 0
            _bloom.Strength = MathX.Lerp( 8f, 1f, fraction );
			_bloom.Threshold = MathX.Lerp( 1f, 1f, fraction );
			_bloom.Gamma = MathX.Lerp( 1.7f, 2.2f, fraction );
            // Clean up when the fade is 100% complete
            if ( fraction >= 1f )
            {
                _isFadingOut = false;
                _bloom.Destroy(); 
            }
        }
    }

	/// <summary>
	/// Initialises stats from the joint type configuration.
	/// Call this after setting <see cref="JointType"/> on the host.
	/// </summary>
	public void InitializeFromType( JointType type )
	{

		JointType = type;
		TypeStats = JointTypeConfig.GetConfig( type );

	}
}