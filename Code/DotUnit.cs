public sealed class DotUnit : Component
{
	public static readonly Color[] TeamColors =
	{
		new Color( 0.0f,  0.9f,  1.0f ),  // Team 0 (yours): bright cyan
		new Color( 1.0f,  0.35f, 0.1f ),  // Team 1 (enemy): orange
	};

	[Property] public int   TeamId         { get; set; } = 0;
	[Property] public float MaxHealth      { get; set; } = 100f;
	[Property] public float Health         { get; set; } = 100f;
	[Property] public float MoveSpeed      { get; set; } = 80f;
	[Property] public float AttackRange    { get; set; } = 80f;
	[Property] public float AttackDamage   { get; set; } = 15f;
	[Property] public float AttackCooldown { get; set; } = 1.5f;

	public Vector3? MoveTarget { get; set; }

	bool          _isSelected;
	ModelRenderer _renderer;
	float         _attackTimer;
	bool          _dead;

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
		if ( _renderer != null ) _renderer.Enabled = false;
		Health = MaxHealth;
	}

	protected override void OnUpdate()
	{
		if ( _dead ) { GameObject.Destroy(); return; }
		HandleMovement();
		HandleCombat();
	}

	// ── Movement ──────────────────────────────────────────────────────────

	void HandleMovement()
	{
		if ( !MoveTarget.HasValue ) return;

		var flat = MoveTarget.Value.WithZ( Transform.Position.z );
		var diff = flat - Transform.Position;

		if ( diff.Length < 6f ) { MoveTarget = null; return; }

		Transform.Position += diff.Normal * MoveSpeed * Time.Delta;
	}

	// ── Combat ────────────────────────────────────────────────────────────

	void HandleCombat()
	{
		_attackTimer -= Time.Delta;
		if ( _attackTimer > 0f ) return;

		DotUnit target   = null;
		float   bestDist = AttackRange;

		foreach ( var unit in Scene.GetAllComponents<DotUnit>() )
		{
			if ( unit == this || unit.TeamId == TeamId || unit._dead ) continue;
			float d = (unit.Transform.Position - Transform.Position).Length;
			if ( d < bestDist ) { bestDist = d; target = unit; }
		}

		if ( target == null ) return;

		_attackTimer = AttackCooldown;
		target.TakeDamage( AttackDamage );
	}

	// ── Damage & death ────────────────────────────────────────────────────

	public void TakeDamage( float amount )
	{
		if ( _dead ) return;
		Health -= amount;
		if ( Health <= 0f ) _dead = true;
	}
}
