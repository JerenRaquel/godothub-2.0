using Godot;
using System;
using System.Collections.Generic;

public partial class TemplateList : VBoxContainer
{
    [Signal] public delegate void GainFocusEventHandler(string templateName);

    private VBoxContainer _templateContainer;

    private Dictionary<string, Button> _buttons = [];
    private Button _currentlyActive = null;

    public string ActiveTemplate => _currentlyActive?.Text;

    public override void _Ready()
    {
        _templateContainer = GetNode<VBoxContainer>("%TemplateContainer");
    }

    public void AddTemplate(string templateName)
    {
        Button buttonInstance = new()
        {
            Name = templateName,
            Text = templateName,
            ToggleMode = true
        };
        _templateContainer.AddChild(buttonInstance);
        _buttons.Add(templateName, buttonInstance);
        buttonInstance.AddThemeColorOverride("font_pressed_color", new(ColorTheme.Compat));
        buttonInstance.Toggled += (bool state) => OnToggled(state, buttonInstance);
        if (_currentlyActive == null)
        {
            _currentlyActive = buttonInstance;
            buttonInstance.SetPressedNoSignal(true);
        }
    }

    private void OnToggled(bool state, Button button)
    {
        if (state)
        {
            _currentlyActive?.SetPressedNoSignal(false);
            _currentlyActive = button;
            EmitSignal(SignalName.GainFocus, button.Text);
        }
        else
        {
            button.SetPressedNoSignal(true);
        }
    }
}
