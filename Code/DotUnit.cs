public sealed class DotUnit : Component
{
	public static readonly Color[] TeamColors =
	{
		new Color( 0.2f,  0.45f, 1.0f ),  // Team 0: blue
		new Color( 1.0f,  0.2f,  0.2f ),  // Team 1: red
	};

	[Property] public int   TeamId    { get; set; } = 0;
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float Health    { get; set; } = 100f;
	[Property] public float MoveSpeed { get; set; } = 80f;

	public Vector3? MoveTarget { get; set; }

	bool        _isSelected;
	GameObject  _ring;

	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			_isSelected = value;
			if ( _ring != null ) _ring.Enabled = value;
		}
	}

	protected override void OnStart()
	{
		// Flat selection ring — sibling object (not child) so parent scale doesn't affect it
		_ring = Scene.CreateObject();
		_ring.Name    = "SelectionRing";
		_ring.Enabled = false;
		_ring.Transform.Scale = new Vector3( 0.12f, 0.12f, 0.01f );

		var r = _ring.Components.Create<ModelRenderer>();
		r.Model = Model.Load( "models/dev/plane.vmdl" );
		r.Tint  = new Color( 0.25f, 1f, 0.4f, 0.85f );
	}

	protected override void OnUpdate()
	{
		// Keep ring glued under unit
		if ( _ring != null && _ring.Enabled )
			_ring.Transform.Position = Transform.Position.WithZ( 0.3f );

		// Steering toward move target
		if ( !MoveTarget.HasValue ) return;

		var flat = MoveTarget.Value.WithZ( Transform.Position.z );
		var diff = flat - Transform.Position;

		if ( diff.Length < 6f )
		{
			MoveTarget = null;
			return;
		}

		Transform.Position += diff.Normal * MoveSpeed * Time.Delta;
	}

	protected override void OnDestroy()
	{
		_ring?.Destroy();
	}
}
