using Godot;

namespace ChestQuest.types;

public partial class Enemy : CharacterBody2D
{
    [Signal]
    public delegate void DefeatedEventHandler();
    
    public virtual void OnDefeat()
    {
        EmitSignal(SignalName.Defeated);
    }
}