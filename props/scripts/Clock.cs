using System;
using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.props.scripts;

public partial class Clock : Interactable
{
    public override async Task InteractAsync()
    {
        GameManager.Singleton.Pause();
        
        var dialogBox = ResourceLoader.Load<PackedScene>("res://scenes/DialogBox.tscn").Instantiate<DialogBox>();
        
        dialogBox.SetDialog(GetClockString());
        GetTree().GetCurrentScene().AddChild(dialogBox);

        await ToSignal(dialogBox, "tree_exiting");

        GameManager.Singleton.Resume();
    }

    private static string GetClockString()
    {
        var timeString = DateTime.Now.ToString("T");
        return $"The time is currently {timeString}";
    }
}