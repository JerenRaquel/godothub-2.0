using Godot;
using System;

public partial class ProjectSidePanel : MarginContainer
{
    private Button _openButton;
    private Button _openWithOutToolsButton;
    private Button _runButton;
    private Button _changeVersionButton;
    private Button _openFolderButton;
    private Button _renameButton;
    private Button _cloneButton;
    private Button _deleteButton;
    private VBoxContainer _quickToolRoot;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _openButton = GetNode<Button>("%OpenButton");
        _openWithOutToolsButton = GetNode<Button>("%OpenWOToolsButton");
        _runButton = GetNode<Button>("%RunButton");
        _changeVersionButton = GetNode<Button>("%ChangeVerButton");
        _openFolderButton = GetNode<Button>("%OpenFolderButton");
        _renameButton = GetNode<Button>("%RenameButton");
        _cloneButton = GetNode<Button>("%CloneButton");
        _deleteButton = GetNode<Button>("%DeleteButton");
        _quickToolRoot = GetNode<VBoxContainer>("%QuickToolRoot");

        _openButton.Disabled = true;
        _openWithOutToolsButton.Disabled = true;
        _runButton.Disabled = true;
        _changeVersionButton.Disabled = true;
        _openFolderButton.Disabled = true;
        _renameButton.Disabled = true;
        _cloneButton.Disabled = true;
        _deleteButton.Disabled = true;
        _quickToolRoot.Hide();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
