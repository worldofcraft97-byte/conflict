public sealed class DotUnit : Component
{
	public static readonly Color[] TeamColors =
	{
		new Color( 0.0f,  0.9f,  1.0f ),  // Team 0 (yours): bright cyan
		new Color( 1.0f,  0.35f, 0.1f ),  // Team 1 (enemy): orange
	};

	[Property] public int   TeamId    { get; set; } = 0;
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float Health    { get; set; } = 100f;
	[Property] public float MoveSpeed { get; set; } = 80f;

	public Vector3? MoveTarget { get; set; }

	bool          _isSelected;
	ModelRenderer _renderer;

	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			_isSelected = value;
			if ( _renderer != null )
				_renderer.Tint = value ? Color.White : TeamColors[TeamId];
		}
	}

	protected override void OnStart()
	{
		_renderer = Components.Get<ModelRenderer>();
	}

	protected override void OnUpdate()
	{
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
}
