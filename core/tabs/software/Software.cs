using Godot;
using System;

public partial class Software : TabBase
{
    [Export] private PackedScene entryScene;

    private Button _locateButton;
    private Button _editButton;
    private Button _launchButton;
    private LineEdit _filterLineEdit;
    private CheckBox _orderCheckBox;
    private Button _deleteButton;

    public override void _Ready()
    {
        _locateButton = GetNode<Button>("%LocateButton");
        _editButton = GetNode<Button>("%EditButton");
        _launchButton = GetNode<Button>("%LaunchButton");
        _filterLineEdit = GetNode<LineEdit>("%FilterLineEdit");
        _orderCheckBox = GetNode<CheckBox>("%OrderCheckBox");
        _deleteButton = GetNode<Button>("%DeleteButton");

        ToggleEntryButtons(true);
    }

    private void ToggleEntryButtons(bool disabled)
    {
        _editButton.Disabled = disabled;
        _launchButton.Disabled = disabled;
        _deleteButton.Disabled = disabled;
    }
}
