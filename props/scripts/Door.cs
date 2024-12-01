using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.props.scripts;

public partial class Door : Interactable
{
    [Export(PropertyHint.File)] public string ToMap;
    [Export] public string SpawnPointName;
    
    public override async Task InteractAsync()
    {
        GameManager.Singleton.Pause();
        await GameManager.Singleton.QueueMapChange(new Entrance
        {
            ToMap = ToMap,
            SpawnPointName = SpawnPointName
        });
        GameManager.Singleton.Resume();
    }
}