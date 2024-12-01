using System.Collections.Generic;
using Godot;

namespace ChestQuest.scenes.scripts;

public partial class DialogBox : CanvasLayer
{
    [Signal]
    public delegate void DialogClosedEventHandler();

    [Signal]
    public delegate void DialogFinishedEventHandler();

    [Export] public double CharacterPause = 0.08;
    [Export] public double SpacePause = 0.05;
    [Export] public double PunctuationPause = 0.5;
    [Export] public AudioStream CharacterSfx;

    private RichTextLabel _label;
    private MarginContainer _dialogOverIndicator;
    private AudioStreamPlayer _sfxPlayer;
    private string[] _dialogs;
    private uint _dialogIdx;
    private SceneTreeTimer _charTimer;

    private Dialog _currentDialog;
    private bool _closeWhenFinished = true;

    public override void _Ready()
    {
        _label = GetNode<RichTextLabel>("ColorRect/MarginContainer/RichTextLabel");
        _dialogOverIndicator = GetNode<MarginContainer>("ColorRect/MarginContainer/DialogOverIndicator");
        _sfxPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _sfxPlayer.SetStream(CharacterSfx);

        if (_dialogs != null)
        {
            ProgressDialogs(false);
        }
    }

    public override void _Process(double delta)
    {
        if (_charTimer.GetTimeLeft() <= 0)
        {
            ProgressCurrentDialog();
        }

        if (Input.IsActionJustPressed("Interact"))
        {
            if (_currentDialog.CurrentToken < _currentDialog.Tokens.Length - 1)
            {
                _currentDialog.CurrentToken = (uint)_currentDialog.Tokens.Length - 1;
                _currentDialog.VisibleCharacters = (uint)_currentDialog.ProcessedString.Length;
                _label.SetVisibleCharacters((int)_currentDialog.VisibleCharacters);
                CurrentDialogFinished();
            }
            else
            {
                ProgressDialogs();
            }
        }
    }

    public void SetDialog(params string[] dialog)
    {
        _dialogs = dialog;
    }

    public void SetCloseWhenFinished(bool closeWhenFinished)
    {
        _closeWhenFinished = closeWhenFinished;
    }

    private void ProgressDialogs(bool increment = true)
    {
        if (increment)
        {
            _dialogIdx++;
        }

        if (_dialogIdx >= _dialogs.Length && _closeWhenFinished)
        {
            EmitSignal(SignalName.DialogClosed);
            QueueFree();
        }
        else if (_dialogIdx < _dialogs.Length)
        {
            _currentDialog = ProcessDialog(_dialogs[_dialogIdx]);
            _label.SetText(_currentDialog.ProcessedString);
            _label.SetVisibleCharacters((int)_currentDialog.VisibleCharacters);
            SetCharacterTimer(_currentDialog.Tokens[0]);
            _dialogOverIndicator.SetVisible(false);
        }
    }

    private void ProgressCurrentDialog()
    {
        _currentDialog.CurrentToken++;

        if (_currentDialog.CurrentToken < _currentDialog.Tokens.Length)
        {
            var token = _currentDialog.Tokens[_currentDialog.CurrentToken];
            switch (token.Type)
            {
                case DialogTokenType.Char:
                    _currentDialog.VisibleCharacters++;
                    var pitch = new RandomNumberGenerator().RandfRange(0.9F, 1.1F);
                    _sfxPlayer.SetPitchScale(pitch);
                    _sfxPlayer.Play();
                    _label.SetVisibleCharacters((int)_currentDialog.VisibleCharacters);
                    SetCharacterTimer(token);
                    break;
                case DialogTokenType.Wait:
                    SetCharacterTimer(token);
                    break;
            }
        }
        else
        {
            CurrentDialogFinished();
        }
    }

    private void SetCharacterTimer(DialogToken token)
    {
        switch (token.Type)
        {
            case DialogTokenType.Char:
                var duration = token.Character switch
                {
                    ' ' => SpacePause,
                    '.' or '!' or '?' or ',' or ';' or '~' or ':' => PunctuationPause,
                    _ => CharacterPause
                };
                _charTimer = GetTree().CreateTimer(duration);
                break;
            case DialogTokenType.Wait:
                _charTimer = GetTree().CreateTimer(token.WaitTime);
                break;
        }
    }

    private void CurrentDialogFinished()
    {
        if (_dialogIdx < _dialogs.Length - 1 || _closeWhenFinished)
        {
            _dialogOverIndicator.SetVisible(true);
        }
        if (_dialogIdx >= _dialogs.Length - 1)
        {
            EmitSignal(SignalName.DialogFinished);
        }
    }

    private Dialog ProcessDialog(string text)
    {
        GD.Print($"Processing Dialog with Text: {text}");
        
        var dialog = new Dialog
        {
            RawString = text
        };

        var tokens = new List<DialogToken>();

        for (var idx = 0; idx < text.Length; idx++)
        {
            var ch = text[idx];
            if (ch == '/')
            {
                var command = text[idx + 1];
                switch (command)
                {
                    case 'W': // Wait
                        var comb = idx + 3; // Skip idx + 2 (should be '[')
                        var numberString = string.Empty;
                        while (text[comb] != ']')
                        {
                            numberString += text[comb];
                            comb++;
                        }

                        var number = double.Parse(numberString);
                        idx = comb;
                        tokens.Add(new DialogToken
                        {
                            Type = DialogTokenType.Wait,
                            WaitTime = number
                        });
                        break;
                }
            }
            else
            {
                tokens.Add(new DialogToken
                {
                    Type = DialogTokenType.Char,
                    Character = text[idx]
                });
            }
        }

        var processed = string.Empty;
        foreach (var token in tokens)
        {
            if (token.Type == DialogTokenType.Char)
            {
                processed += token.Character;
            }
        }

        dialog.ProcessedString = processed;
        dialog.Tokens = tokens.ToArray();
        dialog.VisibleCharacters = 1;
        dialog.CurrentToken = 0;

        return dialog;
    }

    private class Dialog
    {
        public string RawString;
        public string ProcessedString;
        public DialogToken[] Tokens;
        public uint VisibleCharacters;
        public uint CurrentToken;
    }

    private class DialogToken
    {
        public DialogTokenType Type;
        public char Character;
        public double WaitTime;
    }

    private enum DialogTokenType
    {
        Char,
        Wait
    }
}