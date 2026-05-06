public sealed class SelectionController : Component
{
	// Shared state read by MainHud for the drag-box overlay
	public static bool    IsDragging   { get; private set; }
	public static Vector2 DragStart    { get; private set; }
	public static Vector2 DragCurrent  { get; private set; }

	static readonly List<DotUnit> _selected = new();
	public static IReadOnlyList<DotUnit> Selected => _selected;

	// Manual edge-detection — more reliable than Input.Pressed/Released
	// when the mouse cursor is visible in S&Box.
	bool _leftWasDown;
	bool _rightWasDown;
	bool _mouseWasDown;
	Vector2 _mouseDownPos;

	CameraComponent Cam =>
		Scene.GetAllComponents<CameraComponent>().FirstOrDefault( c => c.IsMainCamera );

	IEnumerable<DotUnit> AllUnits => Scene.GetAllComponents<DotUnit>();

	protected override void OnUpdate()
	{
		Mouse.Visible = true;

		bool leftDown  = Input.Down( "Attack1" );
		bool rightDown = Input.Down( "Attack2" );

		bool leftPressed   = leftDown  && !_leftWasDown;
		bool leftReleased  = !leftDown && _leftWasDown;
		bool rightPressed  = rightDown && !_rightWasDown;

		_leftWasDown  = leftDown;
		_rightWasDown = rightDown;

		HandleLeft( leftPressed, leftReleased );
		HandleRight( rightPressed );
	}

	// ── Left mouse — select ───────────────────────────────────────────────

	void HandleLeft( bool pressed, bool released )
	{
		var mouse = Mouse.Position;

		if ( pressed )
		{
			_mouseWasDown = true;
			_mouseDownPos = mouse;
			IsDragging    = false;
			DragStart     = mouse;
		}

		if ( _mouseWasDown && Input.Down( "Attack1" ) )
		{
			DragCurrent = mouse;
			if ( !IsDragging && (mouse - _mouseDownPos).Length > 8f )
				IsDragging = true;
		}

		if ( released && _mouseWasDown )
		{
			if ( IsDragging )
				BoxSelect( _mouseDownPos, mouse );
			else
				SingleSelect( mouse );

			IsDragging    = false;
			_mouseWasDown = false;
		}
	}

	// ── Right mouse — move command ────────────────────────────────────────

	void HandleRight( bool pressed )
	{
		if ( !pressed ) return;
		if ( _selected.Count == 0 ) return;

		var cam = Cam;
		if ( cam == null ) return;

		var ray = cam.ScreenPixelToRay( Mouse.Position );
		if ( ray.Forward.z >= 0f ) return;

		float t      = -ray.Position.z / ray.Forward.z;
		var   target = (ray.Position + ray.Forward * t).WithZ( 5f );

		int i = 0;
		foreach ( var u in _selected )
			u.MoveTarget = target + Formation( i++, _selected.Count );
	}

	// ── Selection helpers ─────────────────────────────────────────────────

	void SingleSelect( Vector2 screen )
	{
		Deselect();
		var cam = Cam;
		if ( cam == null ) return;

		var tr = Scene.Trace.Ray( cam.ScreenPixelToRay( screen ), 5000f ).Run();
		if ( !tr.Hit ) return;

		var unit = tr.GameObject?.GetComponent<DotUnit>();
		if ( unit == null || unit.TeamId != 0 ) return;

		unit.IsSelected = true;
		_selected.Add( unit );
	}

	void BoxSelect( Vector2 a, Vector2 b )
	{
		Deselect();
		var cam = Cam;
		if ( cam == null ) return;

		float minX = System.Math.Min( a.x, b.x );
		float maxX = System.Math.Max( a.x, b.x );
		float minY = System.Math.Min( a.y, b.y );
		float maxY = System.Math.Max( a.y, b.y );

		foreach ( var unit in AllUnits )
		{
			if ( unit.TeamId != 0 ) continue;

			var sp = WorldToScreen( cam, unit.Transform.Position );
			if ( sp.z < 0 ) continue; // behind camera

			if ( sp.x >= minX && sp.x <= maxX && sp.y >= minY && sp.y <= maxY )
			{
				unit.IsSelected = true;
				_selected.Add( unit );
			}
		}
	}

	// Manual world-to-screen projection using camera transform + FOV.
	static Vector3 WorldToScreen( CameraComponent cam, Vector3 worldPos )
	{
		var rot     = cam.Transform.Rotation;
		var diff    = worldPos - cam.Transform.Position;

		float depth = Vector3.Dot( diff, rot.Forward );
		if ( depth <= 0f ) return new Vector3( 0, 0, -1 );

		float tanHalfFov = (float)System.Math.Tan( cam.FieldOfView * System.Math.PI / 360.0 );
		float aspect     = Screen.Width / (float)Screen.Height;

		float ndcX = Vector3.Dot( diff, rot.Right ) / ( depth * tanHalfFov * aspect );
		float ndcY = Vector3.Dot( diff, rot.Up )    / ( depth * tanHalfFov );

		return new Vector3(
			( ndcX + 1f) * 0.5f * Screen.Width,
			(1f - ndcY) * 0.5f * Screen.Height,
			depth
		);
	}

	static void Deselect()
	{
		foreach ( var u in _selected ) u.IsSelected = false;
		_selected.Clear();
	}

	static Vector3 GroundHit( Ray ray )
	{
		if ( ray.Forward.z >= 0f ) return Vector3.Zero;
		float t = -ray.Position.z / ray.Forward.z;
		return ray.Position + ray.Forward * t;
	}

	static Vector3 Formation( int i, int count )
	{
		if ( count <= 1 ) return Vector3.Zero;
		int cols = (int)System.Math.Ceiling( System.Math.Sqrt( count ) );
		float col = i % cols;
		float row = i / cols;
		return new Vector3( (col - cols * 0.5f + 0.5f) * 22f, row * 22f, 0f );
	}
}
