using System.Runtime.Serialization;
using Sandbox;

public sealed class NavNPC : Component
{
	[Property] public GameObject Target {get;set;}
	[Property] public NavMeshAgent Agent {get;set;}

	private SkinnedModelRenderer _modelRenderer;

	protected override void OnAwake()
	{
		_modelRenderer = GameObject.GetComponentInChildren<SkinnedModelRenderer>();
	}

	protected override void OnUpdate()
	{
		Walk();
	}

	private void Walk()
	{
		Agent.MoveTo(Target.WorldPosition);
		_modelRenderer.Set("b_walk",true);
	}
}
