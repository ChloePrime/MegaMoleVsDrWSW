using ChloePrime.MarioForever.Player;
using Godot;

namespace ChloePrime.MarioForever.Level;

[GlobalClass]
public partial class PlayAreaChanger : Area2D
{
    [Export] public Rect2 PlayArea { get; set; } = new(0, 0, 640, 480);

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Mario) return;
        if (GetViewport().GetCamera2D() is MaFoCamera camera)
        {
            camera.Border = PlayArea;
        }
    }
}