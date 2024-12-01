using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.props.scripts;

[Tool]
public partial class Chest : Interactable
{
    [Export] public AudioStream OpenSfx;
    [Export] public string SaveId = "M001-CH001";
    
    private AnimatedSprite2D _animatedSprite2D;
    private bool _qOpened;
    private bool _qOpenFinished;
    private ChestState _state = ChestState.Closed;
    private ChestTrigger _trigger;

    public override void _Ready()
    {
        _animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animatedSprite2D.AnimationFinished += OnAnimationFinished;
        
        var isOpened = SaveManager.Load(SaveId)?.AsBool() ?? false;
        if (isOpened)
        {
            SetState(ChestState.OpenedBefore);
            return;
        }
        
        var children = GetChildren();
        foreach (var child in children)
        {
            if (child is ChestTrigger trigger)
            {
                GD.Print($"{GetName()} Chest found Chest Trigger {trigger.GetName()}");
                _trigger = trigger;
                break; // Chest can only have one trigger
            }
        }
    }

    public override void _Process(double delta)
    {
        switch (_state)
        {
            case ChestState.Closed:
                if (_qOpened)
                {
                    SetState(ChestState.Opening);
                    _qOpened = false;
                }

                break;
            case ChestState.Opening:
                if (_qOpenFinished)
                {
                    SetState(ChestState.Opened);
                }

                break;
            case ChestState.Opened:
                break;
        }
    }

    private void SetState(ChestState state)
    {
        _state = state;
        switch (_state)
        {
            case ChestState.Closed:
                _animatedSprite2D.Play("default");
                break;
            case ChestState.Opening:
                GameManager.Singleton.MainScene.Overworld.SfxPlayer.SetStream(OpenSfx);
                GameManager.Singleton.MainScene.Overworld.SfxPlayer.Play();
                _animatedSprite2D.Play("Opening");
                break;
            case ChestState.Opened:
                _animatedSprite2D.Play("Opened");
                _trigger?.OnChestOpened();
                GameManager.Singleton.OnChestOpened();
                SaveManager.Save(SaveId, true);
                break;
            case ChestState.OpenedBefore:
                _animatedSprite2D.Play("Opened");
                break;
        }
    }

    public override void Interact()
    {
        _qOpened = true;
    }

    #region Handlers

    private void OnAnimationFinished()
    {
        if (_animatedSprite2D.GetAnimation() == "Opening")
        {
            _qOpenFinished = true;
        }
    }

    #endregion

    private enum ChestState
    {
        Closed,
        Opening,
        Opened,
        OpenedBefore
    }
}