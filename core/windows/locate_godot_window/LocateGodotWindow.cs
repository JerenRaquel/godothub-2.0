using Godot;
using System;

public partial class LocateGodotWindow : WindowBase
{
    [Export] private FileDialog _fileDialog;

    private LineEdit _pathLineEdit;
    private Button _chooseLocationButton;

    public override void _ExitTree()
    {
        base._ExitTree();
        _chooseLocationButton.Pressed -= OnChooseLocationPressed;
        _fileDialog.VisibilityChanged -= OnFileDialogVisibilityChanged;
        _fileDialog.FileSelected -= OnFileDialogFileSelected;
    }

    public override void _Ready()
    {
        base._Ready();

        _fileDialog.VisibilityChanged += OnFileDialogVisibilityChanged;
        _fileDialog.FileSelected += OnFileDialogFileSelected;
        _pathLineEdit = GetNode<LineEdit>("%PathLineEdit");
        _chooseLocationButton = GetNode<Button>("%ChooseLocationButton");
        _chooseLocationButton.Pressed += OnChooseLocationPressed;

        DisplayError("Missing Data");
    }

    private void OnChooseLocationPressed()
    {
        ControlShield.Instance.Show();
        _fileDialog.Show();
    }

    private void OnFileDialogVisibilityChanged()
    {
        if (_fileDialog.Visible || ControlShield.Instance == null) return;

        ControlShield.Instance.Hide();
    }

    private void OnFileDialogFileSelected(string path)
    {
        _pathLineEdit.Text = path;
    }
}
