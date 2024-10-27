using Godot;
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
            buttonInstance.Initialize(subButtonName, () => OnSubButtonPressed(subButtonName));
        }
    }

    private void ToggleAllSubButtonsOff(string buttonName)
    {
        foreach (SubTreeButton button in _subButtonContainer.GetChildren().Cast<SubTreeButton>())
        {
            if (button.ButtonName == buttonName) continue;
            button.ToggleOff();
            EmitSignal(SignalName.InterfaceRequested, headerName, buttonName, false);
        }
    }

    private void OnHeaderButtonToggled(string headerName) => ToggleAllSubButtonsOff("");

    private void OnHeaderButtonPressed(bool toggled)
    {
        if (toggled)
            _subButtonContainer.Hide();
        else
        {
            ToggleAllSubButtonsOff("");
            _subButtonContainer.Show();
        }
        EmitSignal(SignalName.Toggled, headerName);
    }

    private void OnSubButtonPressed(string buttonName)
    {
        ToggleAllSubButtonsOff(buttonName);
        EmitSignal(SignalName.Pressed, buttonName);
        EmitSignal(SignalName.InterfaceRequested, headerName, buttonName, true);
    }
}
