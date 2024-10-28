using Godot;
using System;

public partial class LocateGodotWindow : WindowBase
{
    private LineEdit _pathLineEdit;
    private Button _chooseLocationButton;

    public override void _ExitTree()
    {
        base._ExitTree();
        _chooseLocationButton.Pressed -= OnChooseLocationPressed;
    }

    public override void _Ready()
    {
        base._Ready();

        _pathLineEdit = GetNode<LineEdit>("%PathLineEdit");
        _chooseLocationButton = GetNode<Button>("%ChooseLocationButton");
        _chooseLocationButton.Pressed += OnChooseLocationPressed;

        DisplayError("Missing Data");
    }

    private void OnChooseLocationPressed()
    {
        FileDialogManager.Instance.DataCompiled += OnFileDialogFileSelected;
        FileDialogManager.Instance.Open("Locate Godot.Exe", FileDialog.FileModeEnum.OpenFile, "*.exe");
    }

    private void OnFileDialogFileSelected(string path)
    {
        if (path.Length > 0)
            _pathLineEdit.Text = path;
        FileDialogManager.Instance.DataCompiled -= OnFileDialogFileSelected;
    }
}
