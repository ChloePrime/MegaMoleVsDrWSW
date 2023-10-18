using ChloePrime.MarioForever;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MegaMoleVsWsw;

public partial class MoleRock : Area2D
{
    [Export] public float XSpeed { get; set; } = Units.Speed.CtfToGd(8);
    [Export] public float XDirection { get; set; } = 1;

    public override void _Ready()
    {
        base._Ready();
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Translate(new Vector2(XSpeed * XDirection * (float)delta, 0));
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not EnemyHurtDetector ehd) return;
        ehd.HurtBy(new DamageEvent
        {
            DamageTypes = DamageType.Fireball,
            DamageToEnemy = this.GetRule().StompPower,
            DirectSource = this,
        });
        QueueFree();
    }
}