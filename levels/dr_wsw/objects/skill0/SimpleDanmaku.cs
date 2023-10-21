using Godot;

namespace MegaMoleVsDrWsw;

public partial class SimpleDanmaku : Node2D
{
	[Export] public Vector2 Velocity { get; set; }

	public Sprite2D Sprite => _sprite ??= GetNode<Sprite2D>(NpSprite);


	private static readonly NodePath NpSprite = "Sprite";
	private Sprite2D _sprite;

	public override void _Process(double delta)
	{
		base._Process(delta);
		Translate(Velocity * (float)delta);
	}
}
