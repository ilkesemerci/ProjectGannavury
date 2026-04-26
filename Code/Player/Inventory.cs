using Sandbox;

public sealed class Inventory : Component
{
	[Property] public List<GameObject> Backpack {get;set;} = new List<GameObject>();

	[Property] public GameObject WeaponSocket {get;set;}
	[Property] public SkinnedModelRenderer ModelRenderer {get;set;}

	private int _currentIndex = 0;
    private int _oldestJoint = 0;
	private List<GameObject> _weaponSlots = new();
    private List<GameObject> _jointSlots = new();
    private int _maxWeapons = 2;
    private int _maxJoints = 3;

	protected override void OnStart()
	{
		SpawnAllItems();
	}

	protected override void OnFixedUpdate()
	{
		 if ( _weaponSlots.Count == 0 ) return;

        // Mouse Wheel Swapping
        var scroll = Input.MouseWheel.y;
        if ( scroll > 0 ) SwitchItem( 1 );
        else if ( scroll < 0 ) SwitchItem( -1 );

        // Number Key Swapping
        if ( Input.Pressed( "Slot1" ) && _weaponSlots[0].IsValid()) EquipItem( 0 );
        if ( Input.Pressed( "Slot2" )  && _weaponSlots[1].IsValid()) EquipItem( 1 );

        ChangeHoldType();
	}

	private void SpawnAllItems()
	{
		if ( !WeaponSocket.IsValid() )
        {
            Log.Warning( "WeaponManager: No hold point assigned!" );
            return;
        }

        foreach ( var prefab in Backpack )
        {
            if ( !prefab.IsValid() ) continue;

            // 1. Spawn a copy of the prefab into the world
            var newItem = prefab.Clone();

            // 2. Parent it to the hold point
            newItem.Parent = WeaponSocket;

            // 3. Reset its position and rotation so it snaps perfectly into the hand/camera
            newItem.LocalPosition = Vector3.Zero;

            // 4. Disable it by default so they don't overlap
            newItem.Enabled = false;

            // 5. Add to our active tracking list
            _weaponSlots.Add( newItem );
        }

        // Equip the first weapon in the list once everything is spawned
        if ( _weaponSlots.Count > 0 )
        {
            EquipItem( 0 );
            Log.Info("Equipped on spawn!");
        }
	}

	public void AddToInventory(GameObject newItem)
	{
        if (!newItem.IsValid()) return;

        // Check for Booster Type
        if (newItem.Tags.Has("joint") )
        {
            AddJoint(newItem);
        }
        else if(_weaponSlots.Count < _maxWeapons )
        {
            var collider = newItem.GetComponent<BoxCollider>();
            if ( collider.IsValid() && collider.Enabled ) collider.Enabled = false;     

            newItem.Parent = WeaponSocket;
            newItem.LocalPosition = Vector3.Zero;

            if ( _weaponSlots.Count > 0 ) newItem.LocalRotation = _weaponSlots[0].LocalRotation;
            else newItem.LocalRotation = Rotation.From( 0f, 0f, -90f );

            newItem.Enabled = false;
            _weaponSlots.Add(newItem);

            if ( _weaponSlots.Count == 1 ) EquipItem( 0 );

            Log.Info($"Item: {newItem} is newItem to the backpack!");
        }
        else
        {
            _weaponSlots[_currentIndex] = newItem;
        } 
	}

    private void AddWeapon(GameObject weapon )
    {
        if(!weapon.IsValid()) return;

        if(_weaponSlots.Count < _maxWeapons )
        {
            var collider = weapon.GetComponent<BoxCollider>();
            if ( collider.IsValid() && collider.Enabled ) collider.Enabled = false;     

            weapon.Parent = WeaponSocket;
            weapon.LocalPosition = Vector3.Zero;

            if ( _weaponSlots.Count > 0 ) weapon.LocalRotation = _weaponSlots[0].LocalRotation;
            else weapon.LocalRotation = Rotation.From( 0f, 0f, -90f );

            weapon.Enabled = false;
            _weaponSlots.Add(weapon);

            if ( _weaponSlots.Count == 1 ) EquipItem( 0 );

            Log.Info($"Item: {weapon} is newItem to the backpack!");
        }
        else
        {
            Drop(_weaponSlots[_currentIndex]);

            var collider = weapon.GetComponent<BoxCollider>();
            if ( collider.IsValid() && collider.Enabled ) collider.Enabled = false;     

            weapon.Parent = WeaponSocket;
            weapon.LocalPosition = Vector3.Zero;

            if ( _weaponSlots.Count > 0 ) weapon.LocalRotation =
                 _weaponSlots[(_currentIndex+1) % _maxWeapons ].LocalRotation;
       
            else weapon.LocalRotation = Rotation.From( 0f, 0f, -90f );

            _weaponSlots[_currentIndex] = weapon;

            Log.Info($"Item: {weapon} is newItem to the backpack!");
        } 
    }

    private void AddJoint(GameObject jo )
    {
        if(!jo.IsValid()) return;

        if(_jointSlots.Count >= _maxJoints )
        {
            Drop(_jointSlots[_oldestJoint]);
            _jointSlots[_oldestJoint] = jo;
            _oldestJoint = (_oldestJoint + 1) % 3;
        }
        else
        {
            _jointSlots.Add(jo);
        }
    }

    private void Drop(GameObject obj )
    {
        obj.Clone().SetParent(Scene);
        obj.Destroy();
    }

	private void SwitchItem(int direction)
	{
        if(_weaponSlots.Count == 1) return;
        Log.Info($"Index before switching: {_currentIndex}");
		_currentIndex += direction;
        
        if ( _currentIndex >= _weaponSlots.Count ) _currentIndex = 0;
        else if ( _currentIndex < 0 ) _currentIndex = _weaponSlots.Count - 1;
        ModelRenderer.Set("b_deploy",true);
        
        EquipItem( _currentIndex );
        Log.Info($"Index after switching: {_currentIndex}");
	}

    public GameTags ItemTag()
    {
        return  _weaponSlots[_currentIndex].Tags; 
    }

	private void EquipItem( int index )
    {
        _currentIndex = index;
        _weaponSlots[_currentIndex].LocalPosition = Vector3.Zero;
        _weaponSlots[_currentIndex].LocalRotation = Rotation.Identity;
        _weaponSlots[_currentIndex].LocalRotation = Rotation.From(0f,0f,-90f);
        if ( _weaponSlots[_currentIndex].Tags.Has( "knife" ) )
        {
            _weaponSlots[_currentIndex].LocalRotation = Rotation.From(0f,-90f,45f);
            _weaponSlots[_currentIndex].LocalPosition = new Vector3(25,0,-1);
        }
        if ( _weaponSlots[_currentIndex].Tags.Has( "joint" ) )
        {
            _weaponSlots[_currentIndex].LocalRotation = Rotation.From(90f,90f,0f);
            _weaponSlots[_currentIndex].LocalPosition = new Vector3(-3f,5f,-0.5f);
        }

        for ( int i = 0; i < _weaponSlots.Count; i++ )
        {
            if ( !_weaponSlots[i].IsValid() ) continue;
            
            // This turns the GameObject ON if it's the selected index, and OFF if it isn't
            _weaponSlots[i].Enabled = ( i == index );
            //Log.Info($"Item {_weaponSlots[i]} enable status: {(i==index)}");
        }
        
        //Log.Info($"Equipped item: {_weaponSlots[_currentIndex]}, enable status: {_weaponSlots[_currentIndex].Enabled}");
        //Log.Info($"Current item tags: {ItemTag().ToString()}");
    }

	public WeaponSystem EquippedItem()
	{
		Log.Info($"{_weaponSlots[_currentIndex]} equipped");
		return _weaponSlots[_currentIndex].GetComponent<WeaponSystem>();
	}

    public void ChangeHoldType()
	{
        if ( !ModelRenderer.IsValid() ) return;

        if(_weaponSlots.Count == 0 )
        {
            ModelRenderer.Set("holdtype",0);
        }

        var tags = ItemTag();
        if ( tags == null ) return;

        foreach(var tag in ItemTag() )
        {
            if(tag.ToString() == "rifle") ModelRenderer.Set("holdtype",2);
            else if(tag.ToString() == "pistol") ModelRenderer.Set("holdtype",1);
            else if(tag.ToString() == "joint") ModelRenderer.Set("holdtype",0);
            else if(tag.ToString() == "knife") ModelRenderer.Set("holdtype",5);
        }
	}
}
