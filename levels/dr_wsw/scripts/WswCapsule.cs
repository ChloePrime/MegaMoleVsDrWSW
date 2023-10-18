using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ChloePrime.MarioForever.Enemy;
using ChloePrime.MarioForever.Level;
using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.UI;
using ChloePrime.MarioForever.Util;
using Godot;
using MegaMoleVsDrWsw;
using MixelTools.Util.Extensions;

namespace ChloePrime.MegaMoleVsWsw;

public partial class WswCapsule : Node2D
{
    public enum Phase
    {
        Deactivated,
        Temp,
        Intro,
        BattlePhase1,
        MoveToWaitPointOf2,
        BattlePhase2,
    }
    
    [Export] public AudioStream Music { get; set; } = GD.Load<AudioStream>("res://levels/dr_wsw/objects/BGM_wsw_capsule.ogg");
    [Export] public Color BossBar1Color1;
    [Export] public Color BossBar1Color2;
    [Export] public Color BossBar2Color1;
    [Export] public Color BossBar2Color2;

    private static readonly float Skill0DanmakuSpeed = Units.Speed.CtfToGd(8);
    private static readonly float Skill0DanmakuSpeedEnhanced = Units.Speed.CtfToGd(10);
    private static readonly float Skill0DanmakuSpread = Mathf.DegToRad(6);
    private static readonly PackedScene Skill0DanmakuPrefab;
    private static readonly AudioStream Skill0DanmakuSound;
    private static readonly float Skill1MagnetSpeed = Units.Speed.CtfToGd(6);
    private static readonly float Skill1MagnetSpeedEnhanced = Units.Speed.CtfToGd(8);
    private static readonly PackedScene Skill1MagnetPrefab;
    private static readonly AudioStream Skill1MagnetSound;
    
    private static readonly PackedScene MediumExplosion;

    static WswCapsule()
    {
        NodeEx.Load(out Skill0DanmakuPrefab, "res://levels/dr_wsw/objects/skill0/O_wsw_skill0_danmaku.tscn");
        NodeEx.Load(out Skill0DanmakuSound, "res://levels/dr_wsw/objects/skill0/SE_megaman_enemy_shoot.wav");
        NodeEx.Load(out Skill1MagnetPrefab, "res://levels/dr_wsw/objects/skill1/O_wsw_skill1_magnet.tscn");
        NodeEx.Load(out Skill1MagnetSound, "res://levels/dr_wsw/objects/skill1/SE_gear.ogg");
        NodeEx.Load(out MediumExplosion, "res://objects/effect/O_explosion_m_megaman_with_sound.tscn");
    }

    [Signal] public delegate void BattlePhase1StartedEventHandler();
    [Signal] public delegate void BattlePhase2StartedEventHandler();

    public const float IntroEndX = 544;
    public const float IntroSpeed = 300;

    public int XDirection => _dir;
    public Phase CurrentPhase => _phase;
    public bool Enhanced => _core.NpcData.HitPoint / _core.NpcData.MaxHitPoint < 0.50001;
    public AnimationPlayer AnimationPlayer => _animation;

    public void OnPlayAreaChanger2BodyEntered(Node2D body)
    {
        if (_activated || body is not Mario) return;
        Activate();
    }

    public void StartAddingHp()
    {
        if (_hpBar is not { } bar) return;
        bar.Visible = true;
        bar.Color1 = _phase <= Phase.BattlePhase1 ? BossBar1Color1 : BossBar2Color1;
        bar.Color2 = _phase <= Phase.BattlePhase1 ? BossBar1Color2 : BossBar2Color2;
        bar.Value = 0;
        bar.AddHpAnimated(32);
    }
    
    public void MoveTowards(Vector2 target, double duration, Tween.TransitionType transition, Tween.EaseType ease, [MaybeNull] Action callback)
    {
        var tween = CreateTrackedTween();
        tween.TweenProperty(this, Node2D.PropertyName.GlobalPosition.ToString(), target, duration).SetTrans(transition).SetEase(ease);
        if (callback is not null)
        {
            tween.TweenCallback(Callable.From(callback));
        }
    }
    
    public void MoveTowardsX(float x, double duration, Tween.TransitionType transition, Tween.EaseType ease, [MaybeNull] Action callback)
    {
        MoveTowards(Position with { X = x }, duration, transition, ease, callback);
    }
    
    public void MoveTowardsY(float y, double duration, Tween.TransitionType transition, Tween.EaseType ease, [MaybeNull] Action callback)
    {
        MoveTowards(Position with { Y = y }, duration, transition, ease, callback);
    }

    public void MoveTowardsXOut(float x, double duration, Action callback)
    {
        MoveTowardsX(x, duration, Tween.TransitionType.Cubic, Tween.EaseType.In, callback);
    }

    public void MoveTowardsXIn(float x, double duration, Action callback)
    {
        MoveTowardsX(x, duration, Tween.TransitionType.Cubic, Tween.EaseType.Out, callback);
    }

    public void MoveTowardsX(float x, double duration = 1, Tween.EaseType ease = Tween.EaseType.Out)
    {
        MoveTowardsX(x, duration, Tween.TransitionType.Cubic, ease, null);
    }

    public void StartBattlePhase1()
    {
        if (GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is Mario mario)
        {
            mario.ControlIgnored = false;
        }
        _phase = Phase.BattlePhase1;
        EmitSignal(SignalName.BattlePhase1Started);
    }
    
    public void StartBattlePhase2()
    {
        if (GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is Mario mario)
        {
            mario.ControlIgnored = false;
        }
        _phase = Phase.BattlePhase2;
        EmitSignal(SignalName.BattlePhase2Started);
    }

    public void Activate()
    {
        if (_activated) return;
        _activated = true;
        _phase = Phase.Intro;

        BackgroundMusic.Music = Music;
        if (GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is Mario mario)
        {
            mario.ControlIgnored = true;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetNode(out _animation, "Animation Player");
        this.GetNode(out _frontMuzzle, "Front Muzzle");
        this.GetNode(out _core, "Enemy Core");
        _hpBar = (this.GetLevelManager() as LevelFrame)?.Hud.MegaManBossHpBar;

        if (BackgroundMusic.Music == Music)
        {
            BackgroundMusic.Music = null;
        }
    }

    public override void _Process(double deltaD)
    {
        base._Process(deltaD);
        var delta = (float)deltaD;
        switch (_phase)
        {
            case Phase.Deactivated:
            case Phase.Temp:
                break;
            case Phase.Intro:
            {
                if (Position.X > IntroEndX)
                {
                    Position = Position with { X = Mathf.MoveToward(Position.X, IntroEndX, IntroSpeed * delta) };
                }
                else
                {
                    _phase = Phase.Temp;
                    _animation.Play("intro");
                }
                break;
            }
            case Phase.BattlePhase1:
                ProcessBattlePhase1();
                break;
        }
    }

    private void ProcessBattlePhase1()
    {
        if (_skill == -1)
        {
            _skill = GD.RandRange(0, 1);
            _skillPhase = 0;
            _skillCount++;
        }
        switch (_skill)
        {
            case 0:
                // 发射弹幕
                ProcessSKill0();
                break;
            case 1:
                // 发射磁铁
                ProcessSKill1();
                break;
        }
    }

    private Tween CreateTrackedTween()
    {
        var tween = CreateTween();
        _tweens.Add(tween);
        tween.Finished += () => _tweens.Remove(tween);
        return tween;
    }

    #region 技能0: 狙击弹

    private void ProcessSkillCommonIntro(float? customY = null)
    {
        switch (_skillPhase)
        {
            case 0:
            {
                RandomizeDirection();
                TeleportToScreenOutside();
                if (customY is { } y)
                {
                    Position = Position with { Y = y };
                }
                _skillPhase = 1;
                MoveTowardsX(
                    GetCommonSkillPosX(this.GetFrame()), 0.5,
                    Tween.TransitionType.Cubic, Tween.EaseType.Out,
                    () => _skillPhase = 2
                );
                break;
            }
            case 2:
                _skillPhase = 3;
                SkillWaitFor(0.5F);
                break;
        }
    }

    private void ProcessSKill0()
    {
        switch (_skillPhase)
        {
            case 0:
            case 2:
                ProcessSkillCommonIntro();
                break;
            case 4:
                var enhanced = Enhanced;
                var delay = enhanced ? 0.5F : 0.75F;
                var count = enhanced ? 8 : 6;
                const float delay2 = 1F;

                void Shoot() => ShootDanmaku(enhanced);
                Shoot();
                var tween = CreateTrackedTween().SetLoops(count - 1);
                tween.TweenCallback(Callable.From(Shoot)).SetDelay(delay);
                
                _skillPhase = 99;
                SkillWaitFor(delay * (count - 1) + delay2);
                break;
            default:
                ProcessSkillCommonOutro();
                break;
        }
    }

    private void ProcessSkillCommonOutro()
    {
        switch (_skillPhase)
        {
            case 100:
                _dir *= -1;
                _skillPhase = 101;
                MoveTowardsXOut(GetOutScreenWaitPosX(this.GetFrame()), 1, () => _skillPhase++);
                _dir *= -1;
                break;
            case 102:
                _skillPhase = 103;
                SkillWaitFor(0.5F);
                break;
            case 104:
                _skill = -1;
                break;
        }
    }

    private void ShootDanmaku(bool enhanced)
    {
        ShootDanmaku(1, 0, enhanced);
    }

    private void ShootDanmakuSilently(bool enhanced)
    {
        ShootDanmaku(1, 0, enhanced, true);
    }

    private void ShootDanmaku(int count, float spread, bool enhanced, bool silent = false)
    {
        if (GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is not Mario mario) return;

        var look = (mario.GlobalPosition - new Vector2(0, 16) - _frontMuzzle.GlobalPosition).Angle();
        var parent = this.GetPreferredRoot();

        var dir = look - spread * (count - 1) * 0.5F;
        for (int i = 0; i < count; i++)
        {
            var danmaku = Skill0DanmakuPrefab.Instantiate<SimpleDanmaku>();
            parent.AddChild(danmaku);
            danmaku.GlobalPosition = _frontMuzzle.GlobalPosition;
            danmaku.Velocity =
                Vector2.Right.Rotated(dir + (float)GD.RandRange(-Skill0DanmakuSpread, Skill0DanmakuSpread)) *
                (enhanced ? Skill0DanmakuSpeedEnhanced : Skill0DanmakuSpeed);
            dir += spread;
        }

        if (!silent)
        {
            Skill0DanmakuSound?.Play();
        }
    }

    #endregion

    #region 技能1: 磁铁攻击

    private void ProcessSKill1()
    {
        switch (_skillPhase)
        {
            case 0:
                ProcessSkillCommonIntro(this.GetFrame().GetCenter().Y + 96);
                break;
            case 2:
                ProcessSkillCommonIntro();
                break;
            case 4:
                var enhanced = Enhanced;
                var count = enhanced ? 5 : 4;
                var delay = enhanced ? 1.5F : 2;
                var delay2 =  enhanced ? 1F : 2F;

                void Shoot()
                {
                    if (GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is not Node2D mario) return;
                    var marioPos = ToLocal(mario.GlobalPosition);
                    if (marioPos.X > -64 && marioPos.Y > 32)
                    {
                        ShootDanmaku(5, Mathf.DegToRad(15), enhanced);
                    }
                    else
                    {
                        ShootMagnet(enhanced);
                    }
                }

                var i = 0;
                void MoveAndShoot()
                {
                    i++;
                    var isLast = i == count;
                    var frame = this.GetFrame();
                    var frameCenter = frame.GetCenter();
                    var targetY = GlobalPosition.Y < frameCenter.Y 
                        ? (isLast ? GlobalPosition.Y + 96 : frameCenter.Y + (float)GD.RandRange(64.0, 96))
                        : (isLast ? GlobalPosition.Y - 96 : frameCenter.Y - (float)GD.RandRange(96.0, 192));
                    MoveTowardsY(
                        targetY,
                        isLast ? delay2 : delay,
                        Tween.TransitionType.Cubic, Tween.EaseType.InOut,
                        isLast ? null : Shoot
                    );
                }

                Shoot();
                MoveAndShoot();
                var tween = CreateTrackedTween().SetLoops(count - 1);
                tween.TweenCallback(Callable.From(MoveAndShoot)).SetDelay(delay);
                
                SkillWaitFor((count - 1) * delay + delay2);
                _skillPhase = 99;
                break;
            default:
                ProcessSkillCommonOutro();
                break;
        }
    }

    private void ShootMagnet(bool enhanced)
    {
        if (GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is not Mario mario) return;

        var parent = this.GetPreferredRoot();
        var magnet = Skill1MagnetPrefab.Instantiate<WswMagnet>();
        parent.AddChild(magnet);
        magnet.GlobalPosition = _frontMuzzle.GlobalPosition;
        magnet.XDirection = -_dir;
        magnet.InitVelocity(enhanced ? Skill1MagnetSpeedEnhanced : Skill1MagnetSpeed);

        Skill1MagnetSound?.Play();
    }

    #endregion

    #region 一阶段转二阶段

    public void BattleBreak()
    {
        foreach (var t in _tweens)
        {
            t.Stop();
            t.Dispose();
        }
        _tweens.Clear();
        
        if (GetTree().GetFirstNodeInGroup(MaFo.Groups.Player) is Mario mario)
        {
            mario.ControlIgnored = true;
        }
        
        _phase = Phase.MoveToWaitPointOf2;
        _skill = -1;
        var frame = this.GetFrame();
        var moveTarget = frame.GetCenter() + new Vector2(224 * _dir, 0);
        // 移动至 P2 初始点
        const double moveDuration = 1.5;
        var movementTween = CreateTween();
        movementTween.TweenProperty(this, Node2D.PropertyName.GlobalPosition.ToString(), moveTarget, moveDuration);
        movementTween.TweenCallback(Callable.From(OnPhase1To2MoveEnd));
        // 爆炸特效
        const int explosionTimes = 6;
        CreateMediumExplosion();
        CreateTween().SetLoops(explosionTimes - 1)
            .TweenCallback(Callable.From(CreateMediumExplosion)).SetDelay(moveDuration / explosionTimes);
    }
    
    private void CreateMediumExplosion()
    {
        var explosion = MediumExplosion.Instantiate<Node2D>();
        explosion.GlobalPosition = GlobalPosition + new Vector2(
            (float)GD.RandRange(-96.0, 96.0),
            (float)GD.RandRange(-16.0, 32.0)
        );
        this.GetPreferredRoot().AddChild(explosion);
    }

    private void OnPhase1To2MoveEnd()
    {
        _core.NpcData.HitPoint = _core.NpcData.MaxHitPoint = 1600;
        _animation.Play("1to2");
    }

    #endregion

    private void RandomizeDirection()
    {
        if (_skillCount == 1)
        {
            _dir = 1;
        }
        else
        {
            _dir = (int)(GD.Randi() & 2) - 1;
        }
        Scale = Scale with { X = _dir };
    }

    private void TeleportToScreenOutside()
    {
        var frame = this.GetFrame();
        var x = GetOutScreenWaitPosX(frame);
        var y = frame.GetCenter().Y + (float)GD.RandRange(0, 64F);
        Position = new Vector2(x, y);
    }

    private float GetOutScreenWaitPosX(in Rect2 frame)
    {
        return _dir < 0 ? frame.Position.X - 192 : frame.End.X + 192;
    }

    private float GetCommonSkillPosX(in Rect2 frame)
    {
        return frame.GetCenter().X + 224 * _dir;
    }

    private void SkillWaitFor(double delay)
    {
        GetTree().CreateTimer(delay).Timeout += () => _skillPhase++;
    }

    private HashSet<Tween> _tweens = new();
    private MegaManHpBar _hpBar;
    private AnimationPlayer _animation;
    private EnemyCore _core;
    private Node2D _frontMuzzle;
    private bool _activated;
    private Phase _phase = Phase.Deactivated;
    private int _skill = -1;
    private int _skillPhase;
    private int _dir;
    private int _skillCount;
}