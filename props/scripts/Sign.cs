using System.Linq;
using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;
using Godot.Collections;

namespace ChestQuest.props.scripts;

public partial class Sign : Interactable
{
    [Export] public Array<string> Text;

    public override async Task InteractAsync()
    {
        GameManager.Singleton.Pause();

        var dialogBox = DialogBoxScene.Instantiate<DialogBox>();
        dialogBox.SetDialog(Text.ToArray());
        GetTree().GetCurrentScene().AddChild(dialogBox);
        await ToSignal(dialogBox, DialogBox.SignalName.DialogClosed);
        
        GameManager.Singleton.Resume();
    }
}