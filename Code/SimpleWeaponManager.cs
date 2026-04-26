using Sandbox;
using System.Collections.Generic;

public sealed class SimpleWeaponManager : Component
{
    [Property, Description("Drag the weapons already in your hierarchy into this list")] 
    public List<GameObject> Weapons { get; set; } = new();


    [Property] public GameObject WeaponSocket {get;set;}


    private int _currentIndex = 0;

    protected override void OnStart()
    {
        EquipWeapon( 0 ); // Turn on the first gun, hide the rest
    }

    protected override void OnUpdate()
    {
        if ( Weapons.Count == 0 ) return;

        // Mouse Wheel Swapping
        var scroll = Input.MouseWheel.y;
        if ( scroll > 0 ) SwitchWeapon( 1 );
        else if ( scroll < 0 ) SwitchWeapon( -1 );

        // Number Key Swapping
        if ( Input.Pressed( "Slot1" ) ) EquipWeapon( 0 );
        if ( Input.Pressed( "Slot2" ) ) EquipWeapon( 1 );
    }

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
	}

    private void SwitchWeapon( int direction )
    {
        _currentIndex += direction;
        
        if ( _currentIndex >= Weapons.Count ) _currentIndex = 0;
        else if ( _currentIndex < 0 ) _currentIndex = Weapons.Count - 1;
        
        EquipWeapon( _currentIndex );
    }

    private void EquipWeapon( int index )
    {
        _currentIndex = index;

        for ( int i = 0; i < Weapons.Count; i++ )
        {
            if ( !Weapons[i].IsValid() ) continue;
            
            // This turns the GameObject ON if it's the selected index, and OFF if it isn't
            Weapons[i].Enabled = ( i == index );
        }
    }

}