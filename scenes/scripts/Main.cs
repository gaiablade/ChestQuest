using System.Threading.Tasks;
using ChestQuest.scripts;
using Godot;

namespace ChestQuest.scenes.scripts;

public partial class Main : Node2D
{
    public static Main Singleton => GameManager.Singleton.MainScene;

    private Overworld _overworldScene;
    public Overworld Overworld => _overworldScene;

    private ChestsCollectedGuiComponent _chestsCollectedGuiComponent;

    public override void _Ready()
    {
        GameManager.Singleton.MainScene = this;
        _overworldScene = GetNode<Overworld>("Overworld");
        _chestsCollectedGuiComponent = GetNode<ChestsCollectedGuiComponent>("CanvasLayer/ChestsCollectedGUIComponent");
    }

    public void ShowChestsCollectedGuiComponent()
    {
        _ = _chestsCollectedGuiComponent.UpdateAndShow();
    }

    public async Task ShowWinConditionMetDialog()
    {
        GameManager.Singleton.Pause();
        
        var dialogBox = ResourceLoader.Load<PackedScene>("res://scenes/DialogBox.tscn").Instantiate<DialogBox>();
        dialogBox.SetDialog($"You found all {GameManager.Singleton.TotalChestCount} map pieces!",
            "After assembling the map, it appears to point to...",
            "The well in the center of the village!");
        GetTree().GetCurrentScene().AddChild(dialogBox);
        await ToSignal(dialogBox, DialogBox.SignalName.DialogClosed);
        
        GameManager.Singleton.Resume();
    }
}