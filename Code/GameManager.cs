public sealed class GameManager : Component
{
	public static GameManager Instance { get; private set; }

	protected override void OnAwake()
	{
		Instance = this;
	}
}
