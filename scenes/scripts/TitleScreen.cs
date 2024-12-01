using System.Threading.Tasks;
using ChestQuest.scripts;
using Godot;

namespace ChestQuest.scenes.scripts;

public partial class TitleScreen : CanvasLayer
{
	private Button _startButton;
	private Button _quitButton;
	private ColorRect _screenRect;
	
	public override void _Ready()
	{
		_startButton = GetNode<Button>("PanelContainer/ColorRect/VBoxContainer/StartButton");
		_quitButton = GetNode<Button>("PanelContainer/ColorRect/VBoxContainer/QuitButton");
		_screenRect = GetNode<ColorRect>("ColorRect");

		_startButton.Pressed += () => _ = OnStartPressed();
		_quitButton.Pressed += () => _ = OnQuitPressed();
		_screenRect.SetVisible(false);
	}

	private async Task OnStartPressed()
	{
		_startButton.SetDisabled(true);
		_quitButton.SetDisabled(true);
		_screenRect.SetVisible(true);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(_screenRect, "color:a", 1.0, 2);
		await ToSignal(tween, Tween.SignalName.Finished);

		GetTree().ChangeSceneToFile("res://scenes/Main.tscn");
	}

	private async Task OnQuitPressed()
	{
		_startButton.SetDisabled(true);
		_quitButton.SetDisabled(true);
		_screenRect.SetVisible(true);

		var tween = GetTree().CreateTween();
		tween.TweenProperty(_screenRect, "color:a", 1.0, 2);
		await ToSignal(tween, Tween.SignalName.Finished);

		GetTree().Quit();
	}
}