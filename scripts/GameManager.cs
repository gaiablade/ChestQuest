using System;
using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.scripts;

public partial class GameManager : Node
{
    #region Singleton

    private static GameManager _singleton;

    public static GameManager Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = new();
            }

            return _singleton;
        }
    }

    public override void _EnterTree()
    {
        _singleton = this;
    }

    #endregion

    #region Constants

    public uint TotalChestCount = 5;

    #endregion

    public Main MainScene { get; set; }
    public Map CurrentMap { get; set; }
    public Player Player { get; set; }
    public Enemy Enemy { get; set; }
    public SaveManager SaveManager { get; private set; } = new();
    public uint OpenedChestsCount { get; set; }

    public void Pause()
    {
        Player?.SetProcessMode(ProcessModeEnum.Disabled);
    }

    public void Resume()
    {
        Player?.SetProcessMode(ProcessModeEnum.Inherit);
    }

    public async Task QueueMapChange(Entrance entrance)
    {
        await MainScene.Overworld.ChangeMap(entrance);
    }

    public async Task QueueEnemyEncounter(Enemy enemy)
    {
        // Turn player and enemy invisible
        Player.SetVisible(false);
        enemy.SetVisible(false);

        Pause();

        Enemy = enemy;
        var battleScene = MainScene.GetNode<Battle>("Battle");
        var outcome = await battleScene.StartAndPerformBattle();

        Resume();

        // Show player and enemy again
        Player.SetVisible(true);
        enemy.SetVisible(true);

        switch (outcome)
        {
            case 0:
            {
                var collisionShape2d = enemy.GetNode<CollisionShape2D>("CollisionShape2D");
                collisionShape2d.SetDisabled(true);

                var timer = GetTree().CreateTimer(4);
                while (timer.GetTimeLeft() > 0)
                {
                    enemy.SetVisible(!enemy.IsVisible());
                    await Task.Delay(TimeSpan.FromSeconds(0.05));
                }

                enemy.SetVisible(true);
                collisionShape2d.SetDisabled(false);
                break;
            }
            case 1:
            {
                enemy.OnDefeat();
                break;
            }
        }
    }

    public async Task FadeOut(double duration = 1.0)
    {
        var colorRect = MainScene.GetNode<ColorRect>("CanvasLayer/ColorRect");
        var tween = GetTree().CreateTween();
        tween.TweenProperty(colorRect, "self_modulate:a", 1.0, duration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public async Task FadeIn(double duration = 1.0)
    {
        var colorRect = MainScene.GetNode<ColorRect>("CanvasLayer/ColorRect");
        var tween = GetTree().CreateTween();
        tween.TweenProperty(colorRect, "self_modulate:a", 0.0, duration);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public void OnChestOpened()
    {
        OpenedChestsCount++;
        SaveManager.Save("STAT_CHESTS_OPENED", OpenedChestsCount);
        MainScene.ShowChestsCollectedGuiComponent();
        if (OpenedChestsCount >= TotalChestCount)
        {
            _ = MainScene.ShowWinConditionMetDialog();
        }
    }
}