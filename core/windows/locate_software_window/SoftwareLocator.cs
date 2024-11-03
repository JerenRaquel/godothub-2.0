using Godot;
using System;
using System.IO;

public partial class SoftwareLocator : WindowBase
{
    [Signal] public delegate void SoftwareLocatedEventHandler(string key);

    private VBoxContainer _container;
    private LineEdit _tagLineEdit;
    private ColorPickerButton _colorPickerButton;
    private LineEdit _pathLineEdit;
    private Button _locateButton;
    private LineEdit _argsLineEdit;

    public override void _Ready()
    {
        _container = GetNode<VBoxContainer>("%ContentContainer");
        _tagLineEdit = GetNode<LineEdit>("%TagLineEdit");
        _tagLineEdit.TextChanged += OnLineEdited;
        _colorPickerButton = GetNode<ColorPickerButton>("%ColorPickerButton");
        _colorPickerButton.ColorChanged += OnColorPicked;
        _pathLineEdit = GetNode<LineEdit>("%PathLineEdit");
        _pathLineEdit.TextChanged += OnLineEdited;
        _locateButton = GetNode<Button>("%LocateButton");
        _locateButton.Pressed += OnLocatePressed;
        _argsLineEdit = GetNode<LineEdit>("%ArgsLineEdit");

        base._Ready();
        Validate();
    }

    protected override bool Validate()
    {
        if (_tagLineEdit.Text.Length == 0)
        {
            DisplayError("Tag name can't be empty...");
            return false;
        }

        if (TagCache.Instance.HasSoftwareTag(_tagLineEdit.Text))
        {
            DisplayError("Tag name already exists...");
            return false;
        }

        if (_pathLineEdit.Text.Length == 0)
        {
            DisplayError("Software path can't be empty...");
            return false;
        }

        // TODO: Revisit this with actual file testing
        if (!File.Exists(_pathLineEdit.Text))
        {
            DisplayError("Software path is not an executable file...");
            return false;
        }

        DisplayMessage("Software data valid.");
        return true;
    }

    protected override void ClearWindowData()
    {
        base.ClearWindowData();
        _tagLineEdit.Text = "";
        _pathLineEdit.Text = "";
        _argsLineEdit.Text = "";
        _colorPickerButton.Color = new(1, 1, 1, 1);
    }

    protected override void OnConfirmPressed()
    {
        TagCache.Instance.AddOrUpdateSoftwareTag(
            _tagLineEdit.Text, new(
                _colorPickerButton.Color.ToHtml(),
                _pathLineEdit.Text,
                _argsLineEdit.Text
            )
        );

        EmitSignal(SignalName.SoftwareLocated, _tagLineEdit.Text);
        Hide();
    }

    private void OnLineEdited(string _) => Validate();

    private void OnLocatePressed()
    {
        FileDialogManager.Instance.DataCompiled += OnFileLocated;
        FileDialogManager.Instance.Open("Locate Software Executable", FileDialog.FileModeEnum.OpenFile, ["*.exe"]);
    }

    private void OnFileLocated(string path)
    {
        FileDialogManager.Instance.DataCompiled -= OnFileLocated;
        if (path.Length == 0) return;

        _pathLineEdit.Text = path;
        Validate();
    }

    private void OnColorPicked(Color _) => Validate();
}
