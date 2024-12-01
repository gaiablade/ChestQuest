using System.Threading.Tasks;
using Godot;

namespace ChestQuest.types;

public abstract partial class Interactable : Node
{
    protected PackedScene DialogBoxScene;

    public override void _Ready()
    {
        DialogBoxScene = ResourceLoader.Load<PackedScene>("res://scenes/DialogBox.tscn");
    }

    public virtual void Interact()
    {
    }

    public virtual Task InteractAsync()
    {
        return Task.CompletedTask;
    }
}