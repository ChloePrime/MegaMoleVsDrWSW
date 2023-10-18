using ChloePrime.MarioForever.Player;
using ChloePrime.MarioForever.Util;
using Godot;

namespace ChloePrime.MegaMoleVsWsw;

public partial class MarioStatusMole : MarioStatusBasic
{
    [Export] public PackedScene Projectile { get; set; } = GD.Load<PackedScene>("res://levels/dr_wsw/objects/O_little_rock_mole.tscn");
    [Export] public AudioStream ShootSound { get; set; } = GD.Load<AudioStream>("res://levels/dr_wsw/objects/SE_mole_shoot.ogg");
    
    public override bool Fire(Mario mario)
    {
        base.Fire(mario);
        
        var rock = Projectile.Instantiate<MoleRock>();
        mario.GetPreferredRoot().AddChild(rock);
        rock.GlobalPosition = mario.ToGlobal(new Vector2(0, -16));
        rock.XDirection = mario.CharacterDirection;
        
        ShootSound?.Play();
        return true;
    }
}
