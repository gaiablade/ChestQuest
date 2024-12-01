using System;
using System.Threading.Tasks;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.scenes.scripts;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 32.0f;
    [Export] public BattleStats Stats;

    public uint CurrentHp;

    // Nodes
    private AnimatedSprite2D _animatedSprite2D;
    private Marker2D _rotationMarker;
    private Area2D _interactionFinder;
    private Sprite2D _interactBalloon;
    private Area2D _entranceFinder;
    private Area2D _enemyFinder;

    private PlayerState _state = PlayerState.Idle;
    private Vector2 _directionVector;
    private string _direction;

    private Enemy _lastEnemy;

    public void Pause() => SetProcessMode(ProcessModeEnum.Disabled);
    public void Resume() => SetProcessMode(ProcessModeEnum.Inherit);

    public override void _Ready()
    {
        CurrentHp = Stats.MaxHp;

        _direction = "Up";

        _animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _animatedSprite2D.Play($"Idle {_direction}");
        _rotationMarker = GetNode<Marker2D>("RotatingMarker");
        _interactionFinder = GetNode<Area2D>("RotatingMarker/InteractionFinder");
        _interactBalloon = GetNode<Sprite2D>("Interact Balloon");
        _entranceFinder = GetNode<Area2D>("EntranceFinder");
        _enemyFinder = GetNode<Area2D>("EnemyFinder");
    }

    public override void _Process(double delta)
    {
        switch (_state)
        {
            case PlayerState.Idle:
                HandleMovement(delta);
                if (_directionVector != Vector2.Zero)
                {
                    SetState(PlayerState.Moving);
                }

                HandleInteracts();

                break;
            case PlayerState.Moving:
                HandleMovement(delta);
                if (_directionVector == Vector2.Zero)
                {
                    SetState(PlayerState.Idle);
                }

                HandleInteracts();
                HandleEntrances();
                HandleEnemyEncounters();

                break;
        }
    }

    private void SetState(PlayerState state)
    {
        _state = state;
        switch (_state)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Moving:
                break;
        }
    }

    private void HandleMovement(double delta)
    {
        _directionVector = GetDirectionVector();
        SetDirection(_directionVector);

        Velocity = _directionVector * Speed;
        MoveAndSlide();
        SetMovementAnimation();
        RotateMarker();
    }

    private Vector2 GetDirectionVector()
    {
        return Input.GetVector("Move Left", "Move Right", "Move Up", "Move Down");
    }


    private void SetDirection(Vector2 directionVector)
    {
        if (directionVector.X < 0)
        {
            _direction = "Left";
        }
        else if (directionVector.X > 0)
        {
            _direction = "Right";
        }

        if (directionVector.Y < 0)
        {
            _direction = "Up";
        }
        else if (directionVector.Y > 0)
        {
            _direction = "Down";
        }
    }

    private void SetMovementAnimation()
    {
        switch (_state)
        {
            case PlayerState.Idle:
            {
                var animation = "Idle " + _direction;
                if (_animatedSprite2D.GetAnimation() != animation)
                {
                    _animatedSprite2D.Play(animation);
                }

                break;
            }
            case PlayerState.Moving:
            {
                var animation = "Move " + _direction;
                if (_animatedSprite2D.GetAnimation() != animation)
                {
                    _animatedSprite2D.Play(animation);
                }

                break;
            }
        }
    }

    private void RotateMarker()
    {
        var rotation = _direction switch
        {
            "Down" => 0,
            "Left" => Mathf.DegToRad(90),
            "Up" => Mathf.DegToRad(180),
            "Right" => Mathf.DegToRad(270)
        };

        _rotationMarker.SetRotation(rotation);
    }

    private void HandleInteracts()
    {
        var overlapping = _interactionFinder.GetOverlappingAreas();
        if (overlapping.Count > 0)
        {
            _interactBalloon.SetVisible(true);
        }
        else
        {
            _interactBalloon.SetVisible(false);
        }

        if (Input.IsActionJustPressed("Interact"))
        {
            foreach (var area in overlapping)
            {
                if (area.GetParent() is Interactable interactable)
                {
                    interactable.Interact();
                    interactable.InteractAsync();
                }
                else
                {
                    GD.Print($"{_interactionFinder.GetName()} scanned an area without an Interact method.");
                }
            }
        }
    }

    private void HandleEntrances()
    {
        var overlapping = _entranceFinder.GetOverlappingAreas();
        foreach (var area in overlapping)
        {
            if (area is Entrance entrance)
            {
                _ = GameManager.Singleton.QueueMapChange(entrance);
            }
        }
    }

    private void HandleEnemyEncounters()
    {
        var overlapping = _enemyFinder.GetOverlappingBodies();
        foreach (var body in overlapping)
        {
            if (body is Enemy enemy)
            {
                _ = GameManager.Singleton.QueueEnemyEncounter(enemy);
                return;
            }
        }
    }
}

public enum PlayerState
{
    Idle,
    Moving
}