using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.props.scripts;

public partial class Bed : Interactable
{
    public override async Task InteractAsync()
    {
        GameManager.Singleton.Pause();

        var dialogBox = DialogBoxScene.Instantiate<DialogBox>();
        dialogBox.SetDialog("Would you like to rest?");
        dialogBox.SetCloseWhenFinished(false);
        GetTree().GetCurrentScene().AddChild(dialogBox);

        await ToSignal(dialogBox, DialogBox.SignalName.DialogFinished);

        var choicesBox = ResourceLoader.Load<PackedScene>("res://scenes/ChoicesBox.tscn").Instantiate<ChoicesBox>();
        choicesBox.SetChoices("Yes", "No");
        GetTree().GetCurrentScene().AddChild(choicesBox);

        var result = await ToSignal(choicesBox, ChoicesBox.SignalName.ChoicePressed);
        var choiceMade = result[0].AsString();
        
        GetTree().GetCurrentScene().RemoveChild(dialogBox);

        if (choiceMade == "Yes")
        {
            var stats = GameManager.Singleton.Player.Stats;
            GameManager.Singleton.Player.CurrentHp = stats.MaxHp;
            
            dialogBox = DialogBoxScene.Instantiate<DialogBox>();
            dialogBox.SetDialog("Health has been restored!");
            GetTree().GetCurrentScene().AddChild(dialogBox);
            await ToSignal(dialogBox, DialogBox.SignalName.DialogClosed);
        }
        
        GameManager.Singleton.Resume();
    }
}