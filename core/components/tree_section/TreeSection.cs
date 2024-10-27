using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class TreeSection : VBoxContainer
{
    [Signal] public delegate void InterfaceRequestedEventHandler(string headerName, string buttonName, bool isOpen);
    [Signal] public delegate void ToggleOthersOffEventHandler(string headerName);

    [Export]
    public PackedScene subTreeButtonPackedScene;
    [Export]
    public string headerName;
    [Export]
    public string[] subButtons;

    private Button _headerButton;
    private VBoxContainer _subButtonContainer;

    private readonly Dictionary<string, SubTreeButton> _subButtons = [];
    private string _currentButton;

    public override void _ExitTree()
    {
        _headerButton.Toggled -= OnHeaderButtonToggled;
        foreach (SubTreeButton buttonInstance in _subButtonContainer.GetChildren().Cast<SubTreeButton>())
            buttonInstance.Toggled -= OnSubButtonToggled;
    }

    public override void _Ready()
    {
        _headerButton = GetNode<Button>("%HeaderButton");
        _headerButton.Toggled += OnHeaderButtonToggled;
        _subButtonContainer = GetNode<VBoxContainer>("%SubButtonContainer");

        _headerButton.Text = headerName;

        if (subButtons.IsEmpty())
        {
            QueueFree();
            return;
        }

        foreach (string subButtonName in subButtons)
        {
            SubTreeButton buttonInstance = subTreeButtonPackedScene.Instantiate<SubTreeButton>();
            _subButtonContainer.AddChild(buttonInstance);
            buttonInstance.Initialize(subButtonName);
            buttonInstance.Toggled += OnSubButtonToggled;
            _subButtons.TryAdd(subButtonName, buttonInstance);
        }
    }

    public void ToggleFirstOn()
    {
        SubTreeButton button = _subButtonContainer.GetChild<SubTreeButton>(0);
        button.ToggleOn();
        _currentButton = button.ButtonName;
    }

    public void ToggleHeaderOn() => _headerButton.SetPressedNoSignal(true);

    public void ToggleAllSubButtonsOff()
    {
        foreach (KeyValuePair<string, SubTreeButton> entry in _subButtons)
        {
            if (!entry.Value.IsActive) continue;

            entry.Value.ToggleOff();
            EmitSignal(SignalName.InterfaceRequested, headerName, entry.Key, false);
        }
    }

    private bool IsASubButtonOn()
    {
        foreach (SubTreeButton button in _subButtonContainer.GetChildren().Cast<SubTreeButton>())
            if (button.IsActive) return true;
        return false;
    }

    private void OnHeaderButtonToggled(bool toggled)
    {
        if (!toggled)   // Collapsing
        {
            if (IsASubButtonOn())   // Don't collapse if interface is open
            {
                _headerButton.SetPressedNoSignal(true);
                return;
            }
            _subButtonContainer.Hide();
        }
        else    // Opening
        {
            _subButtonContainer.Show();
        }
    }

    private void OnSubButtonToggled(string subButtonName, bool toggled)
    {
        if (!toggled)   // Toggled Off
        {
            _subButtons[subButtonName]?.KeepOn();
        }
        else    // Open Interface
        {
            if (_currentButton != null && _currentButton != subButtonName && _subButtons.TryGetValue(_currentButton, out SubTreeButton currentButton))
            {
                EmitSignal(SignalName.InterfaceRequested, headerName, _currentButton, false);
                currentButton.ToggleOff();
            }

            _currentButton = subButtonName;
            EmitSignal(SignalName.InterfaceRequested, headerName, subButtonName, true);
            EmitSignal(SignalName.ToggleOthersOff, headerName);
        }
    }
}
