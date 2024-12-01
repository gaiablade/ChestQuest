using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.maps._001_Start_Town.scripts;

public partial class Book1 : Interactable
{
    public override async Task InteractAsync()
    {
        GameManager.Singleton.Pause();

        var dialogBoxScene = ResourceLoader.Load<PackedScene>("res://scenes/DialogBox.tscn");

        // First, ask if player wants to read the book
        var dialogBox = dialogBoxScene.Instantiate<DialogBox>();
        dialogBox.SetDialog("\"The Treasure of the Clouds\"",
            "Legends exist of a hidden treasure located in this very village.",
            "No living soul knows the true location of the treasure, but a map was once made pointing to its secret hiding place.",
            $"The map was divided into {GameManager.Singleton.TotalChestCount} separate pieces and scattered in chests throughout the surrounding area...");
        GetTree().GetCurrentScene().AddChild(dialogBox);
        await ToSignal(dialogBox, DialogBox.SignalName.DialogClosed);

        GameManager.Singleton.Resume();
    }
}