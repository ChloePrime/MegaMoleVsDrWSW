﻿using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

[GlobalClass]
[Icon("res://resources/enemies/AT_goomba_photo.tres")]
public partial class GoombaPhoto : GravityObjectBase, ICorpse
{
    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _sprite, NpSprite);
    }

    private static readonly NodePath NpSprite = "Sprite";
    private Sprite2D _sprite;
}