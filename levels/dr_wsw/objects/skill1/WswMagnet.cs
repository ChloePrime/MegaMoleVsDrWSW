using System;
using ChloePrime.MarioForever.Util;
using Godot;

namespace MegaMoleVsDrWsw;

public partial class WswMagnet : SimpleDanmaku
{
    public int XDirection { get; set; } = -1;
    public int YDirection { get; set; }
    public float TurnDetectRange { get; set; } = 8;


    public override void _EnterTree()
    {
        base._EnterTree();
        _mario = GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) as Node2D;
    }

    public void InitVelocity(float speed)
    {
        Velocity = new Vector2(speed * XDirection, 0);
        Sprite.FlipH = XDirection < 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        var mario = _mario;
        if (_turned || !IsInstanceValid(mario)) return;
        if (Mathf.Abs(mario.GlobalPosition.X - GlobalPosition.X) <= TurnDetectRange)
        {
            TurnAtMario(mario);
        }
    }

    private void TurnAtMario(Node2D mario)
    {
        _turned = true;
        var y = Math.Abs(Velocity.X) * Math.Sign((mario.GlobalPosition.Y - GlobalPosition.Y));
        Velocity = new Vector2(0, y);
        Sprite.Rotation = Sprite.FlipH == (y >= 0) ? -HalfOfPi : HalfOfPi;
    }

    private const float HalfOfPi = (float)(Math.PI / 2);
    private Node2D _mario;
    private bool _turned;
}