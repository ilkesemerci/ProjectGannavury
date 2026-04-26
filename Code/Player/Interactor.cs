using Sandbox;

public sealed class Interactor : Component
{
    [Property] public float InteractRange { get; set; }
    [Property] public bool ShowDebugLine { get; set; } = true;


    protected override void OnUpdate()
    {
        var startPos = WorldPosition;
        var forward = WorldRotation.Forward;

        var endPos = startPos + (forward * InteractRange );

        var trace = Scene.Trace.Ray( startPos, endPos )
            .Radius(5f)
            .UsePhysicsWorld()
            .UseHitboxes()
            .IgnoreGameObjectHierarchy( GameObject.Root )
            .WithTag("usable")
            .Run();

        // 3. --- DEBUG DRAWING ---
        if ( ShowDebugLine )
        {
            // Change color based on if we hit something!
            if ( trace.Hit )
                Gizmo.Draw.Color = Color.Green;
            else
                Gizmo.Draw.Color = Color.Red;

            // If we hit something, stop the line at the impact point. 
            // If we missed, draw it all the way to the end of the range.
            var drawEndPos = trace.Hit ? trace.HitPosition : endPos;

            // Draw the line
            Gizmo.Draw.Line( startPos, drawEndPos );
            
            // Optional: Draw a little sphere exactly where the laser hits
            if ( trace.Hit )
            {
                Gizmo.Draw.SolidSphere( trace.HitPosition, 2f );
            }
        }

        if ( trace.Hit && trace.GameObject.IsValid() )
        {
            // Ask the object: Do you or your parents have the IUsable contract?
            var usable = trace.GameObject.Components.Get<IUsable>( FindMode.EverythingInSelfAndParent );
            

            if ( usable != null )
            {
                // TODO later: You could trigger UI here using usable.GetUseText()
                DebugOverlay.Text(trace.GameObject.WorldPosition + Vector3.Up * 10f, usable.GetUseText(),16);

                // If the player presses 'E' (or whatever you bound 'use' to)
                if ( Input.Pressed( "use" ) )
                {
                    // Trigger the object and pass our main player root as the user
                    usable.OnUse( GameObject.Root );
                }
            }
        }
    }
}