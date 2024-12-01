using Godot;

namespace ChestQuest.scenes.scripts;

public partial class Entrance : Area2D
{
    [Export(PropertyHint.File)] public string ToMap;
    [Export] public string SpawnPointName;
}