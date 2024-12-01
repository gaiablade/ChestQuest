using Godot;

namespace ChestQuest.types;

public abstract partial class ChestTrigger : Node
{
    public abstract void OnChestOpened();
}