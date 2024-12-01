using System.Threading.Tasks;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.maps._002_Start_Cave.scripts;

public partial class Cave1 : Map
{
    private TileMapLayer _hiddenExit;
    private CollisionShape2D _toSecretRoomCollisionShape;

    public override void _Ready()
    {
        _hiddenExit = GetNode<TileMapLayer>("Layers/Hidden Exit");
        _toSecretRoomCollisionShape = GetNode<CollisionShape2D>("Entrances/ToSecretRoom/Area2D/CollisionShape2D");
        HideExit();
    }

    private void HideExit()
    {
        var modulate = _hiddenExit.GetModulate();
        _hiddenExit.SetModulate(new Color(modulate.R, modulate.G, modulate.B, 0));
        
        _toSecretRoomCollisionShape.SetDisabled(true);
    }

    private void OnSlimeDefeated()
    {
        _ = RevealExit();
    }

    private async Task RevealExit()
    {
        GameManager.Singleton.Pause();
        
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_hiddenExit, "modulate:a", 1, 1);
        await ToSignal(tween, Tween.SignalName.Finished);
        
        _toSecretRoomCollisionShape.SetDisabled(false);
        
        GameManager.Singleton.Resume();
    }
}