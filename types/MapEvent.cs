using System.Threading.Tasks;
using Godot;

namespace ChestQuest.types;

public abstract partial class MapEvent : Node
{
    protected PackedScene DialogBoxScene;
    
    protected abstract bool IsActive();
    protected abstract Task ProcessEvent();

    public override void _Ready()
    {
        DialogBoxScene = ResourceLoader.Load<PackedScene>("res://scenes/DialogBox.tscn");
    }

    public override void _Process(double delta)
    {
        if (IsActive())
        {
            ProcessEvent();
        }
    }
}