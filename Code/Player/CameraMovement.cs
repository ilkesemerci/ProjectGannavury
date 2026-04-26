using Sandbox;

public sealed class CameraMovement : Component
{
	[Property] public PlayerMovement Player {get;set;}
	[Property] public GameObject Head{get;set;}
	[Property] public float Distance{get;set;} = 0f;

	public float AimDownSightsDistance = 50f;
	private CameraComponent Camera;
	private Vector3 _currentOffset = Vector3.Zero;

	protected override void OnAwake()
	{
		Camera = Components.Get<CameraComponent>();
		Camera.RenderExcludeTags.Remove("worldmodel");
		Camera.RenderExcludeTags.Remove( "viewmodel" );
	}

	protected override void OnUpdate()
	{
		var eyeAngles = Head.WorldRotation.Angles();
		eyeAngles.pitch += Input.MouseDelta.y * 0.1f;
		eyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
		eyeAngles.roll = 0f;
		eyeAngles.pitch = eyeAngles.pitch.Clamp(-89.9f,89.9f);
		Head.WorldRotation = eyeAngles.ToRotation();

		var targetOffset = Vector3.Zero;
		if(Player.IsCrouching) targetOffset += Vector3.Down * 32f; 
		_currentOffset = Vector3.Lerp(_currentOffset,targetOffset,Time.Delta * 10f);

		if ( Camera.IsValid() )
		{
			var camPos = Head.WorldPosition  + _currentOffset;
			var camForward = eyeAngles.ToRotation().Forward;
			var camTrace = Scene.Trace.Ray(camPos,camPos - (camForward * Distance))
				.WithoutTags("player","trigger")
				.Run();

			if ( camTrace.Hit )
			{
				camPos = camTrace.HitPosition + camTrace.Normal;
			}
			else
			{
				camPos = camTrace.EndPosition;
			}

			Camera.WorldPosition = camPos;
			Camera.WorldRotation = eyeAngles.ToRotation();
		}
	}
}
