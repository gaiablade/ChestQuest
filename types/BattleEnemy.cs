using Godot;

namespace ChestQuest.types;

public partial class BattleEnemy : Node2D
{
    [Export] public AnimatedSprite2D Sprite;
    [Export] public string EnemyName;
    [Export] public BattleStats Stats;

    public uint CurrentHp;

    public override void _Ready()
    {
        CurrentHp = Stats.MaxHp;
    }
}