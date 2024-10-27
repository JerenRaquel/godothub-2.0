using Godot;
using System;
using System.Linq;

public partial class TreeSection : VBoxContainer
{
    [Signal] public delegate void PressedEventHandler(string buttonName);
    [Signal] public delegate void ToggledEventHandler(string headerName);
    [Signal] public delegate void InterfaceRequestedEventHandler(string headerName, string buttonName, bool isOpen);

    private Button _headerButton;
    private VBoxContainer _subButtonContainer;

    [Export]
    public PackedScene subTreeButtonPackedScene;
    [Export]
    public string headerName;
    [Export]
    public string[] subButtons;

    public override void _ExitTree()
    {
        _headerButton.Toggled -= OnHeaderButtonPressed;
        Toggled -= OnHeaderButtonToggled;
        foreach (SubTreeButton buttonInstance in _subButtonContainer.GetChildren().Cast<SubTreeButton>())
            buttonInstance.Toggled -= OnSubButtonToggled;
    }

    public override void _Ready()
    {
        _headerButton = GetNode<Button>("%HeaderButton");
        _headerButton.Toggled += OnHeaderButtonPressed;
        _subButtonContainer = GetNode<VBoxContainer>("%SubButtonContainer");

        _headerButton.Text = headerName;

        Toggled += OnHeaderButtonToggled;

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
        }
    }

    public void ToggleFirstOn()
    {
        _headerButton.SetPressedNoSignal(true);
        _subButtonContainer.GetChild<SubTreeButton>(0).ToggleOn();
    }

    private void ToggleAllSubButtonsOff(string buttonName)
    {
        foreach (SubTreeButton button in _subButtonContainer.GetChildren().Cast<SubTreeButton>())
        {
            if (button.ButtonName == buttonName) continue;
            button.ToggleOff();
            EmitSignal(SignalName.InterfaceRequested, headerName, button.ButtonName, false);
        }
    }

    private bool IsASubButtonOn()
    {
        foreach (SubTreeButton button in _subButtonContainer.GetChildren().Cast<SubTreeButton>())
            if (button.IsActive) return true;
        return false;
    }

    private void OnHeaderButtonToggled(string headerName) => ToggleAllSubButtonsOff("");

    private void OnHeaderButtonPressed(bool toggled)
    {
        if (toggled)
        {
            ToggleAllSubButtonsOff("");
            _subButtonContainer.Show();
        }
        else
        {
            if (IsASubButtonOn())
            {
                _headerButton.SetPressedNoSignal(true);
                return;
            }
            _subButtonContainer.Hide();
        }
        EmitSignal(SignalName.Toggled, headerName);
    }

    private void OnSubButtonToggled(string buttonName, bool state)
    {
        ToggleAllSubButtonsOff(buttonName);
        EmitSignal(SignalName.Pressed, buttonName);
        EmitSignal(SignalName.InterfaceRequested, headerName, buttonName, state);
    }
}
