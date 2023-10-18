﻿using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
public partial class GoombaHurtDetector : EnemyHurtDetector
{
    [Export]
    public PackedScene StompedCorpse { get; set; } = GD.Load<PackedScene>("res://resources/enemies/goomba_photo.tscn");

    public override Node2D CreateCorpse(DamageEvent e)
    {
        return e.DamageTypes == DamageType.Stomp ? StompedCorpse.Instantiate<Node2D>() : base.CreateCorpse(e);
    }
}