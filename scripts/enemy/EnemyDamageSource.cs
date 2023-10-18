﻿using System;
using System.Collections.Generic;
using System.Linq;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using DotNext.Collections.Generic;
using Godot;
using MixelTools.Util.Extensions;

namespace ChloePrime.MarioForever.Enemy;

/**
 * 怪物伤害马里奥的判定
 */
[GlobalClass]
public partial class EnemyDamageSource : Area2D
{
    [Export] public EnemyHurtDetector Detector { get; private set; }
    public EnemyCore Core => Detector.Core;

    public void HaltFor(TimeSpan duration)
    {
        HaltFor((float)duration.TotalSeconds);
    }

    public void HaltFor(float seconds)
    {
        _protection = seconds;
    }

    public bool IsHalted => _protection > 0;
    
    public override void _Ready()
    {
        base._Ready();
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
        // BodyEntered += OnBodyEntered;
        // BodyExited += OnBodyExited;
        Detector ??= GetParent()?.Children().OfType<EnemyHurtDetector>().FirstOrNone().Or(null);
        if (Detector is { } detector)
        {
            detector.Stomped += OnStomped;
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!IsInstanceValid(Detector))
        {
            return;
        }
        if (Core.NpcData.Friendly || Detector.Root is GravityObjectBase { ReallyEnabled: false })
        {
            return;
        }
        // 被抱起来以后不会伤害到马里奥
        if (Core.Root is IGrabbable grabbable && IGrabbable.IsGrabbedByPlayer(grabbable))
        {
            HaltFor(TimeSpan.FromSeconds(0.2));
            return;
        }
        if (_protection > 0)
        {
            _protection -= (float)delta;
            return;
        }
        
        var filteredOverlaps = Detector.Stompable ? _overlaps.Where(m => !m.WillStomp(Detector.Root)) : _overlaps;
        filteredOverlaps.ForEach(HurtMario);
    }

    public virtual void HurtMario(Mario mario)
    {
        var (lo, hi) = Core.GetDamage();
        mario.Hurt(new DamageEvent
        {
            DamageTypes = DamageType.Enemy,
            DamageLo = lo,
            DamageHi = hi,
            DirectSource = Detector.Root,
            TrueSource = Detector.Root,
        });
    }

    private float _protection;

    private void OnStomped()
    {
        HaltFor(TimeSpan.FromSeconds(0.2));
    }

    private void OnAreaEntered(Area2D other)
    {
        if (other is MarioHurtZone hurtZone)
        {
            OnBodyEntered(hurtZone.Root);
        }
    }
    
    private void OnAreaExited(Area2D other)
    {
        if (other is MarioHurtZone hurtZone)
        {
            OnBodyExited(hurtZone.Root);
        }
    }

    private void OnBodyEntered(Node2D other)
    {
        if (other is not Mario mario)
        {
            return;
        }

        _overlaps.Add(mario);
    }

    private void OnBodyExited(Node2D other)
    {
        if (other is not Mario mario)
        {
            return;
        }

        _overlaps.Remove(mario);
    }

    protected override void Dispose(bool disposing)
    {
        _overlaps.Clear();
        base.Dispose(disposing);
    }

    private readonly HashSet<Mario> _overlaps = new();
}