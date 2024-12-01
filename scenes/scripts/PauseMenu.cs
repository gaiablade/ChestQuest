using System.Threading.Tasks;
using ChestQuest.scripts;
using Godot;

namespace ChestQuest.scenes.scripts;

public partial class PauseMenu : CanvasLayer
{
	private ColorRect _colorRect;
	private Label _hpLabel;
	private Label _attackLabel;
	private Label _defenseLabel;
	private Label _speedLabel;
	private Label _chestsLabel;
	private AudioStreamPlayer _sfxPlayer;

	private AudioStream _openSfx;
	private AudioStream _closeSfx;
	
	public override void _Ready()
	{
		_colorRect = GetNode<ColorRect>("ColorRect");
		_colorRect.SetModulate(new Color(1, 1, 1, 0));

		_hpLabel = GetNode<Label>("%Hp Label");
		_attackLabel = GetNode<Label>("%Attack Label");
		_defenseLabel = GetNode<Label>("%Defense Label");
		_speedLabel = GetNode<Label>("%Speed Label");
		_chestsLabel = GetNode<Label>("%Chests Label");

		_sfxPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		_openSfx = ResourceLoader.Load<AudioStream>("res://assets/packs/100_rpg_ui_sfx/091_Pause_03.wav");
		_closeSfx = ResourceLoader.Load<AudioStream>("res://assets/packs/100_rpg_ui_sfx/097_Unpause_03.wav");

		var player = GameManager.Singleton.Player;
		var stats = player?.Stats;
		if (player != null && stats != null)
		{
			_hpLabel.SetText($"HP: {player.CurrentHp}/{stats.MaxHp}");
			_attackLabel.SetText($"Attack: {stats.Attack}");
			_defenseLabel.SetText($"Defense: {stats.Defense}");
			_speedLabel.SetText($"Speed: {stats.Speed}");
		}
		
		_chestsLabel.SetText($"Chests: {GameManager.Singleton.OpenedChestsCount}/{GameManager.Singleton.TotalChestCount}");

		_sfxPlayer.SetStream(_openSfx);
		_sfxPlayer.Play();

		var tween = GetTree().CreateTween();
		tween.TweenProperty(_colorRect, "modulate:a", 1.0, 0.2);
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Pause"))
		{
			_ = Close();
		}
	}

	private async Task Close()
	{
		_sfxPlayer.SetStream(_closeSfx);
		_sfxPlayer.Play();
		var tween = GetTree().CreateTween();
		tween.TweenProperty(_colorRect, "modulate:a", 0, 0.2);
		await ToSignal(tween, Tween.SignalName.Finished);
		QueueFree();
	}
}