public sealed class RtsCamera : Component
{
	[Property] public float PanSpeed { get; set; } = 400f;
	[Property] public float EdgeScrollMargin { get; set; } = 20f;
	[Property] public float ZoomStep { get; set; } = 50f;
	[Property] public float MinHeight { get; set; } = 120f;
	[Property] public float MaxHeight { get; set; } = 900f;
	[Property] public float MapBounds { get; set; } = 240f;

	protected override void OnStart()
	{
		// Place camera south of centre, high up, then LookAt the map origin.
		// LookAt is axis-convention-proof unlike raw Angles.
		var pos = new Vector3( 0f, -200f, 500f );
		Transform.Position = pos;
		Transform.Rotation = Rotation.LookAt( -pos, Vector3.Up );
	}

	protected override void OnUpdate()
	{
		Mouse.Visible = true;

		var pos = Transform.Position;
		var dt  = Time.Delta;

		// ── Pan ──────────────────────────────────────────────────────────
		float dx = 0f, dy = 0f;

		if ( Input.Down( "Right" ) )    dx += 1f;
		if ( Input.Down( "Left" ) )     dx -= 1f;
		if ( Input.Down( "Forward" ) )  dy += 1f;
		if ( Input.Down( "Backward" ) ) dy -= 1f;

		// Edge scroll (only when mouse is captured by the game window)
		var mouse  = Mouse.Position;
		float sw   = Screen.Width;
		float sh   = Screen.Height;

		if ( mouse.x <= EdgeScrollMargin )        dx -= 1f;
		if ( mouse.x >= sw - EdgeScrollMargin )   dx += 1f;
		if ( mouse.y <= EdgeScrollMargin )        dy += 1f;
		if ( mouse.y >= sh - EdgeScrollMargin )   dy -= 1f;

		var panDir = new Vector3( dx, dy, 0f );
		if ( panDir.LengthSquared > 0.01f )
			pos += panDir.Normal * PanSpeed * dt;

		// ── Zoom (scroll wheel changes height) ────────────────────────
		var scroll = Input.MouseWheel;
		if ( scroll.y != 0f )
			pos.z -= scroll.y * ZoomStep;

		// ── Clamp ─────────────────────────────────────────────────────
		pos.z = MathX.Clamp( pos.z, MinHeight, MaxHeight );
		pos.x = MathX.Clamp( pos.x, -MapBounds, MapBounds );
		pos.y = MathX.Clamp( pos.y, -MapBounds, MapBounds );

		Transform.Position = pos;
	}
}
