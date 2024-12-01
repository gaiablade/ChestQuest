using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.scripts;
using ChestQuest.types;

namespace ChestQuest.maps._001_Start_Town.scripts;

public partial class Cutscene1 : MapEvent
{
    private const string SaveKey = "M001-C001-ACTIVE";
    
    private bool _isActive;
    protected override bool IsActive() => _isActive;

    public override void _Ready()
    {
        base._Ready();
        _isActive = SaveManager.Load(SaveKey)?.AsBool() ?? true;
    }

    protected override async Task ProcessEvent()
    {
        _isActive = false;

        GameManager.Singleton.Pause();
        
        var dialogBox = DialogBoxScene.Instantiate<DialogBox>();
        dialogBox.SetDialog("Find and open all 15 chests! Good luck!");
        GetTree().GetCurrentScene().AddChild(dialogBox);
        await ToSignal(dialogBox, DialogBox.SignalName.DialogClosed);
        SaveManager.Save(SaveKey, false);

        GameManager.Singleton.Resume();
    }
}