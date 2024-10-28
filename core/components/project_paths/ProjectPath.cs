using Godot;
using System;
using System.IO;

public partial class ProjectPath : HBoxContainer
{
    [Signal] public delegate void FileDialogRequestedEventHandler(ProjectPath instance);

    private Button _changeLocationButton;
    private LineEdit _pathLineEdit;
    private Button _deleteButton;

    public string Path
    {
        get => _pathLineEdit.Text;
        set => _pathLineEdit.Text = value;
    }
    public bool Disabled
    {
        get => _deleteButton.Disabled;
        set => _deleteButton.Disabled = value;
    }
    public bool IsValid => Directory.Exists(_pathLineEdit.Text);

    public override void _ExitTree()
    {
        _changeLocationButton.Pressed -= OnChangeLocationPressed;
        _deleteButton.Pressed -= OnDeletePressed;
    }

    public override void _Ready()
    {
        _changeLocationButton = GetNode<Button>("%ChangeLocationButton");
        _changeLocationButton.Pressed += OnChangeLocationPressed;
        _pathLineEdit = GetNode<LineEdit>("%PathLineEdit");
        _deleteButton = GetNode<Button>("%DeleteButton");
        _deleteButton.Pressed += OnDeletePressed;
    }

    private void OnChangeLocationPressed() => EmitSignal(SignalName.FileDialogRequested, this);

    private void OnDeletePressed()
    {
        // TODO: Alert DB that path was removed.

        QueueFree();
    }
}
