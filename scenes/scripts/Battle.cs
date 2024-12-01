using System;
using System.Threading.Tasks;
using ChestQuest.scenes.battle_enemies.scripts;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.scenes.scripts;

public partial class Battle : CanvasLayer
{
    private TaskCompletionSource<int> _battleOptionTcs = new();

    private Enemy _enemy;
    private BattleEnemy _battleEnemy;
    private int _battleOutcome;
    private AudioStream _slashSfx;
    private AudioStream _fleeSfx;
    private AudioStream _deathSfx;

    // Nodes
    private Marker2D _enemyMarker;
    private Node2D _battlePlayer;
    private Label _battleLog;
    private Container _battleOptions;
    private Button _attackButton;
    private Button _itemButton;
    private Button _runButton;
    private ProgressBar _playerHealthBar;
    private ProgressBar _enemyHealthBar;
    private Label _playerHealthLabel;
    private AudioStreamPlayer _sfxPlayer;

    public override void _Ready()
    {
        _enemyMarker = GetNode<Marker2D>("Background/EnemyMarker");
        _battlePlayer = GetNode<Node2D>("Background/BattlePlayer");
        _battleLog = GetNode<Label>("Background/BattleLog");

        _battleOptions = GetNode<Container>("%BattleOptions");
        _attackButton = GetNode<Button>("%AttackButton");
        _itemButton = GetNode<Button>("%ItemButton");
        _runButton = GetNode<Button>("%RunButton");

        _attackButton.Pressed += () => _battleOptionTcs.SetResult(0);
        _itemButton.Pressed += () => _battleOptionTcs.SetResult(1);
        _runButton.Pressed += () => _battleOptionTcs.SetResult(2);

        _playerHealthBar = GetNode<ProgressBar>("%PlayerHealthBar");
        _enemyHealthBar = GetNode<ProgressBar>("%EnemyHealthBar");
        _playerHealthLabel = GetNode<Label>("%PlayerHealthLabel");

        _sfxPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _slashSfx = ResourceLoader.Load<AudioStream>("res://assets/packs/90_RPG_Battle_SFX/19_Slash_01.wav");
        _fleeSfx = ResourceLoader.Load<AudioStream>("res://assets/packs/90_RPG_Battle_SFX/52_Flee_03.wav");
        _deathSfx = ResourceLoader.Load<AudioStream>("res://assets/packs/90_RPG_Battle_SFX/70_Enemy_death_02.wav");

        _playerHealthBar.SetMax(GameManager.Singleton.Player.Stats.MaxHp);
        _playerHealthBar.SetValue(GameManager.Singleton.Player.CurrentHp);
        _playerHealthLabel.SetText(
            $"{GameManager.Singleton.Player.CurrentHp}/{GameManager.Singleton.Player.Stats.MaxHp}");

        OnScreenResize();
        GetTree().GetRoot().SizeChanged += OnScreenResize;
    }

    public async Task<int> StartAndPerformBattle()
    {
        await OnBattleStart();
        await BattleLoop();
        await OnBattleEnd();
        return _battleOutcome;
    }

    private async Task OnBattleStart()
    {
        await GameManager.Singleton.FadeOut(0.3);
        _battleOutcome = -1;
        _battleOptionTcs = new();
        _enemy = GameManager.Singleton.Enemy;
        _battleEnemy = GetBattleEnemy(_enemy);
        _enemyHealthBar.SetMax(_battleEnemy.Stats.MaxHp);
        _enemyHealthBar.SetValue(_battleEnemy.Stats.MaxHp);
        _battleEnemy.SetPosition(_enemyMarker.GetPosition());
        _battleEnemy.SetScale(new Vector2(3, 3));
        _enemyMarker.GetParent().AddChild(_battleEnemy);
        _battleLog.SetText($"A wild {_battleEnemy.EnemyName} appears!");
        _battleOptions.SetVisible(true);
        SetVisible(true);
        await GameManager.Singleton.FadeIn(0.3);
    }

    private async Task BattleLoop()
    {
        while (true)
        {
            var isRunning = await OnPlayerTurn();
            if (isRunning)
            {
                _sfxPlayer.SetStream(_fleeSfx);
                _sfxPlayer.Play();
                _battleOutcome = 0;
                return;
            }

            UpdateHealthBars();

            if (_battleEnemy.CurrentHp == 0)
            {
                _battleOutcome = 1;
                _battleLog.SetText($"Slime has been defeated!");
                await FadeAndRemoveEnemy();
                return;
            }

            await Task.Delay(TimeSpan.FromSeconds(0.8));
            await OnEnemyTurn();

            UpdateHealthBars();

            _battleOptions.SetVisible(true);
        }
    }

    private async Task<bool> OnPlayerTurn()
    {
        var playerStats = GameManager.Singleton.Player.Stats;
        var enemyStats = _battleEnemy.Stats;

        var result = await _battleOptionTcs.Task;
        var choice = (BattleChoice)result;

        _battleOptions.SetVisible(false);

        switch (choice)
        {
            case BattleChoice.Attack:
            {
                var damage = CalculateBasicAttackDamage(playerStats, enemyStats, 0.2);
                _battleEnemy.CurrentHp -= Math.Min(damage, _battleEnemy.CurrentHp);

                _battleLog.SetText($"Player is attacking!");

                // Make player move forward
                var originalPosition = _battlePlayer.GetPosition();
                var tween = GetTree().CreateTween();
                tween.TweenProperty(_battlePlayer, "position:x", originalPosition.X - 25, 0.2);
                await ToSignal(tween, Tween.SignalName.Finished);

                // Player attack animation
                _sfxPlayer.SetStream(_slashSfx);
                _sfxPlayer.Play();
                var slashAnimation = ResourceLoader.Load<PackedScene>("res://attack animations/SlashAnimation.tscn")
                    .Instantiate<AnimatedSprite2D>();
                slashAnimation.SetScale(new Vector2(3, 3));
                slashAnimation.SetPosition(_enemyMarker.GetPosition());
                CallDeferred(Node.MethodName.AddChild, slashAnimation);
                await ToSignal(slashAnimation, AnimatedSprite2D.SignalName.AnimationFinished);
                CallDeferred(Node.MethodName.RemoveChild, slashAnimation);
                _battleLog.SetText($"Slime took {damage} damage!");

                // Make player move back to original position
                tween = GetTree().CreateTween();
                tween.TweenProperty(_battlePlayer, "position:x", originalPosition.X, 0.2);
                await ToSignal(tween, Tween.SignalName.Finished);
                break;
            }
            case BattleChoice.Item:
            {
                _battleLog.SetText($"Player is using an item.");
                break;
            }
            case BattleChoice.Run:
            {
                _battleLog.SetText($"Player is running away!");
                await Task.Delay(TimeSpan.FromSeconds(0.5));
                _enemyMarker.GetParent().CallDeferred(Node.MethodName.RemoveChild, _battleEnemy);
                return true;
            }
        }

        _battleOptionTcs = new TaskCompletionSource<int>();
        return false;
    }

    private async Task OnEnemyTurn()
    {
        var playerStats = GameManager.Singleton.Player.Stats;
        var enemyStats = _battleEnemy.Stats;
        var damage = CalculateBasicAttackDamage(enemyStats, playerStats, 0.2);

        _battleLog.SetText($"Slime is attacking!");

        var enemyPosition = _battleEnemy.GetPosition();
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_battleEnemy, "position:x", enemyPosition.X + 25, 0.2);
        await ToSignal(tween, Tween.SignalName.Finished);

        _sfxPlayer.SetStream(_slashSfx);
        _sfxPlayer.Play();
        var slashAnimation = ResourceLoader.Load<PackedScene>("res://attack animations/SlashAnimation.tscn")
            .Instantiate<AnimatedSprite2D>();
        slashAnimation.SetScale(new Vector2(3, 3));
        slashAnimation.SetPosition(_battlePlayer.GetPosition());
        CallDeferred(Node.MethodName.AddChild, slashAnimation);
        await ToSignal(slashAnimation, AnimatedSprite2D.SignalName.AnimationFinished);
        CallDeferred(Node.MethodName.RemoveChild, slashAnimation);
        _battleLog.SetText($"Player took {damage} damage!");
        GameManager.Singleton.Player.CurrentHp -= Math.Min(GameManager.Singleton.Player.CurrentHp, damage);

        tween = GetTree().CreateTween();
        tween.TweenProperty(_battleEnemy, "position:x", enemyPosition.X, 0.2);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    private async Task OnBattleEnd()
    {
        await GameManager.Singleton.FadeOut(0.3);
        SetVisible(false);
        await GameManager.Singleton.FadeIn(0.3);
    }

    private static BattleEnemy GetBattleEnemy(Enemy enemy)
    {
        var battleSlimeScene = ResourceLoader.Load<PackedScene>("res://scenes/battle enemies/BattleSlime.tscn");
        return battleSlimeScene.Instantiate<BattleSlime>();
    }

    private void UpdateHealthBars()
    {
        _playerHealthBar.SetValue(GameManager.Singleton.Player.CurrentHp);
        _playerHealthLabel.SetText(
            $"{GameManager.Singleton.Player.CurrentHp}/{GameManager.Singleton.Player.Stats.MaxHp}");
        _enemyHealthBar.SetValue(_battleEnemy.CurrentHp);
    }

    private uint CalculateBasicAttackDamage(BattleStats attackerStats, BattleStats defenderStats, double variance)
    {
        GD.Print(
            $"Calculating Basic Attack Damage: Attack = {attackerStats.Attack}, Defense = {defenderStats.Defense}");
        var baseDamage = attackerStats.Attack - defenderStats.Defense;
        var attackVariance = baseDamage * variance;
        var minDamage = baseDamage - attackVariance;
        var rn = Random.Shared.NextDouble();
        return (uint)Math.Round(minDamage + rn * attackVariance * 2);
    }

    private void OnScreenResize()
    {
        // Place enemy 30% from the left, player 30% from the right, both 50% from the top
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var enemyPosition = new Vector2(viewportSize.X * 0.3F, viewportSize.Y * 0.5F);
        var playerPosition = new Vector2(viewportSize.X * 0.7F, viewportSize.Y * 0.5F);

        _enemyMarker.SetPosition(enemyPosition);
        if (_battleEnemy != null)
        {
            _battleEnemy.SetPosition(enemyPosition);
        }
        
        _battlePlayer.SetPosition(playerPosition);
    }

    private async Task FadeAndRemoveEnemy()
    {
        _sfxPlayer.SetStream(_deathSfx);
        _sfxPlayer.Play();
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_battleEnemy, "modulate:a", 0, 1);
        await ToSignal(tween, Tween.SignalName.Finished);
        _battleEnemy.QueueFree();
    }

    private enum BattleChoice
    {
        Attack,
        Item,
        Run
    }
}