using Sandbox;


/// <summary>
/// This component handles the behaviors of a joint when you consume them.
/// </summary>
public sealed partial class JointBase : Component, IUsable, IJoint
{

	[Sync( SyncFlags.FromHost )]
	public JointType JointType { get; set; }

	public JointTypeStats TypeStats { get; private set; }
	
	public void OnUse( GameObject user )
	{
		
	}
    
    public string GetUseText()
	{
		return "";
	}

	public void ApplyWeedEffect()
	{
		
	}

	protected override void OnStart()
	{
		InitializeFromType( JointType );
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

	public void ApplyWeedEffects()
	{
		
	}
}