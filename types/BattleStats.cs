using Godot;

namespace ChestQuest.types;

[GlobalClass]
public partial class BattleStats : Resource
{
    [Export] public uint MaxHp = 1;
    [Export] public uint Attack = 1;
    [Export] public uint Defense = 1;
    [Export] public uint Speed = 1;
}