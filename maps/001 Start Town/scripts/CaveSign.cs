using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.scripts;
using ChestQuest.types;

namespace ChestQuest.maps._001_Start_Town.scripts;

public partial class CaveSign : Interactable
{
    public override async Task InteractAsync()
    {
        GameManager.Singleton.Pause();
        
        var dialogBox = DialogBoxScene.Instantiate<DialogBox>();
        dialogBox.SetDialog("Riverflow Cave\nBeware of monsters!");
        GetTree().GetCurrentScene().AddChild(dialogBox);
        await ToSignal(dialogBox, DialogBox.SignalName.DialogClosed);

        GameManager.Singleton.Resume();
    }
}