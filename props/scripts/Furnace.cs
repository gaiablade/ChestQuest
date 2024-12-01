using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.scripts;
using ChestQuest.types;

namespace ChestQuest.props.scripts;

public partial class Furnace : Interactable
{
    public override async Task InteractAsync()
    {
        GameManager.Singleton.Pause();
        
        var dialogBox = DialogBoxScene.Instantiate<DialogBox>();
        dialogBox.SetDialog("The furnace flames burn strong... How does it never go out?");
        GetTree().GetCurrentScene().AddChild(dialogBox);
        await ToSignal(dialogBox, DialogBox.SignalName.DialogClosed);
        
        GameManager.Singleton.Resume();
    }
}