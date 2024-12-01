using System;
using System.Threading.Tasks;
using ChestQuest.scripts;
using Godot;

namespace ChestQuest.scenes.scripts;

public partial class ChestsCollectedGuiComponent : ColorRect
{
	private Vector2 _originalPosition;
	
	// Nodes
	private Label _valueLabel;
	
	public override void _Ready()
	{
		_originalPosition = GetPosition();
		_valueLabel = GetNode<Label>("Value Label");
		_valueLabel.SetText(GetValueText());
		
		HideComponent();
	}

	public async Task UpdateAndShow()
	{
		var valueText = GetValueText();

		var tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", _originalPosition, 0.5);
		await ToSignal(tween, Tween.SignalName.Finished);
		await Task.Delay(TimeSpan.FromSeconds(0.5));
		_valueLabel.SetText(valueText);
		await Task.Delay(TimeSpan.FromSeconds(0.5));
		tween = GetTree().CreateTween();
		tween.TweenProperty(this, "position", GetHiddenPosition(), 0.5);
		await ToSignal(tween, Tween.SignalName.Finished);
	}

	private Vector2 GetHiddenPosition()
	{
		return _originalPosition + new Vector2(0, GetSize().Y);
	}

	private void HideComponent()
	{
		SetPosition(GetHiddenPosition());
	}

	private string GetValueText()
	{
		return GameManager.Singleton.OpenedChestsCount + "/" + GameManager.Singleton.TotalChestCount;
	}
}