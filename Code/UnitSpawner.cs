public sealed class UnitSpawner : Component
{
	// Sphere model radius in model-space is ~25 units; at scale 0.18 → ~4.5 world units.
	// GroundZ places the sphere centre just above the Z=0 plane.
	const float UnitScale = 0.18f;
	const float GroundZ   = 5f;

	protected override void OnStart()
	{
		// Team 0 (cyan, yours) — left side of map center
		for ( int i = 0; i < 5; i++ )
			Spawn( 0, new Vector3( Rnd( -160f, -60f ), Rnd( 0f, 80f ), GroundZ ) );

		// Team 1 (orange, enemy) — right side of map center
		for ( int i = 0; i < 5; i++ )
			Spawn( 1, new Vector3( Rnd( 60f, 160f ), Rnd( 0f, 80f ), GroundZ ) );
	}

	void Spawn( int teamId, Vector3 pos )
	{
		var go = Scene.CreateObject();
		go.Name            = $"Unit_T{teamId}";
		go.Transform.Position = pos;
		go.Transform.Scale    = Vector3.One * UnitScale;

		var renderer = go.Components.Create<ModelRenderer>();
		renderer.Model = Model.Load( "models/dev/sphere.vmdl" );
		renderer.Tint  = DotUnit.TeamColors[teamId];

		// Solid collider so Scene.Trace raycasts can detect clicks on units
		var col = go.Components.Create<SphereCollider>();
		col.Radius    = 25f;   // matches sphere model radius in local space
		col.IsTrigger = false;

		var unit       = go.Components.Create<DotUnit>();
		unit.TeamId    = teamId;
	}

	static float Rnd( float min, float max ) =>
		min + System.Random.Shared.NextSingle() * (max - min);
}
