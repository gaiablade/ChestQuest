using ChestQuest.types;

namespace ChestQuest.scenes.scripts;

public partial class Slime : Enemy
{
    public override void OnDefeat()
    {
        base.OnDefeat();
        QueueFree();
    }
}