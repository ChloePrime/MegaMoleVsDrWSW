using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Level;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.RPG;
using ChloePrime.MarioForever.UI;
using ChloePrime.MarioForever.Util;
using Godot;
using MixelTools.Util.Extensions;
using static ChloePrime.MegaMoleVsWsw.WswCapsule.Phase;

namespace ChloePrime.MegaMoleVsWsw;

public partial class WswCapsuleHurtDetector : EnemyHurtDetector
{
    public WswCapsule RootWswCapsule { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _customHurtPlayer, NpCustomHurtPlayer);
        RootWswCapsule = (WswCapsule)Core.Root;
        RootWswCapsule.BattlePhase2Started += OnInvulnerableEnds;
        _bossBar = (this.GetLevelManager() as LevelFrame)?.Hud.MegaManBossHpBar;
    }

    public override void StompBy(Node2D stomper)
    {
        if (Stompable && stomper is Mario mario)
        {
            mario.XDirection = -RootWswCapsule.XDirection;
            mario.XSpeed = Units.Speed.CtfMovementToGd(160);
        }
        base.StompBy(stomper);
    }

    public override bool HurtBy(DamageEvent e)
    {
        if (_invulnerable) return false;
        if (RootWswCapsule.CurrentPhase is not BattlePhase1 and not BattlePhase2) return false;
        
        _invulnerable = true;
        Stompable = false;
        return base.HurtBy(e);
    }

    protected override void OnHurt(DamageEvent e)
    {
        base.OnHurt(e);
        if (_bossBar is { } bossBar)
        {
            var npcData = Core.AsNpc.NpcData;
            bossBar.Value = Mathf.FloorToInt(bossBar.Max * npcData.HitPoint / npcData.MaxHitPoint + 1e-5);
        }
    }

    public override void PlayHurtAnimation()
    {
        _customHurtPlayer.Stop();
        _customHurtPlayer.Play(AnimHurt);
    }

    private void OnInvulnerableEnds()
    {
        Stompable = true;
        _invulnerable = false;
    }

    public override bool Kill(DamageEvent e)
    {
        switch (RootWswCapsule.CurrentPhase)
        {
            case BattlePhase1:
                if (_bossBar is { } bar)
                {
                    bar.Value = 0;
                }
                RootWswCapsule.BattleBreak();
                PlayHurtSound(e);
                return true;
            case BattlePhase2:
                RootWswCapsule.CallDeferred(WswCapsule.MethodName.RaiseFireworks);
                return true;
            default:
                return false;
        }
    }

    private static readonly StringName AnimHurt = "hurt";
    private static readonly NodePath NpCustomHurtPlayer = "Custom Hurt Animation";
    private bool _invulnerable;
    private AnimationPlayer _customHurtPlayer;
    private MegaManHpBar _bossBar;
}