using Godot;
using Godot.Collections;

namespace ChestQuest.scenes.scripts;

public partial class ChoicesBox : CanvasLayer
{
    [Signal]
    public delegate void ChoicePressedEventHandler(string choice);
    
    private Container _container;

    private Array<string> _choices;

    public override void _Ready()
    {
        _container = GetNode<Container>("VBoxContainer");

        if (_choices != null)
        {
            foreach (var choice in _choices)
            {
                var button = new Button();
                button.SetText(choice);
                button.Pressed += () => { OnChoiceMade(choice); };
                _container.AddChild(button);
            }
        }
    }

    public void SetChoices(params string[] choices)
    {
        _choices = new Array<string>(choices);
    }

    private void OnChoiceMade(string choice)
    {
        EmitSignal(SignalName.ChoicePressed, choice);
        QueueFree();
    }
}