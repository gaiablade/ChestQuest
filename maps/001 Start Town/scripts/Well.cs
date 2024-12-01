using System.Threading.Tasks;
using ChestQuest.scenes.scripts;
using ChestQuest.scripts;
using ChestQuest.types;
using Godot;

namespace ChestQuest.maps._001_Start_Town.scripts;

public partial class Well : Interactable
{
	private bool _enabled;
	private CollisionShape2D _collisionShape2D;
	
	public override void _Ready()
	{
		base._Ready();
		_collisionShape2D = GetNode<CollisionShape2D>("Area2D/CollisionShape2D");
	}

	public override void _Process(double delta)
	{
		if (!_enabled && GameManager.Singleton.OpenedChestsCount >= GameManager.Singleton.TotalChestCount)
		{
			_enabled = true;
			_collisionShape2D.SetDisabled(false);
		}
	}

	public override async Task InteractAsync()
	{
		GameManager.Singleton.Pause();
		
		var dialogBox = DialogBoxScene.Instantiate<DialogBox>();
		dialogBox.SetDialog("You spot a faint glow at the bottom of the well...", "You found the fabled treasure!", "The End.");
		GetTree().GetCurrentScene().AddChild(dialogBox);
		await ToSignal(dialogBox, DialogBox.SignalName.DialogClosed);
		await GameManager.Singleton.FadeOut();
		GetTree().ChangeSceneToFile("res://scenes/ThanksForPlayingScreen.tscn");
	}
}