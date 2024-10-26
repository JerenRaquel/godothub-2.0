using Godot;
using System;

public partial class Projects : PanelContainer
{
    private Button _newButton;
    private Button _importButton;
    private Button _scanButton;
    private CheckBox _checkBox;

    public override void _Ready()
    {
        _newButton = GetNode<Button>("%NewButton");
        _importButton = GetNode<Button>("%ImportButton");
        _scanButton = GetNode<Button>("%ScanButton");
        _checkBox = GetNode<CheckBox>("%CheckBox");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
