using ChestQuest.scripts;
using Godot;

namespace ChestQuest.types;

public partial class Map : Node2D
{
    [Export] public TileMapLayer BaseLayer;
    [Export] public Node SpawnPoints;
    [Export(PropertyHint.File)] public string Music;
    
    public override void _Ready()
    {
        GameManager.Singleton.CurrentMap = this;
    }
}