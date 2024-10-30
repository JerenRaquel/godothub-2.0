using Godot;
using System;
using System.IO;

public partial class LocateGodotWindow : WindowBase
{
    private readonly VersionData.BuildType[] BUILD_MAP = [
        VersionData.BuildType.STABLE,
        VersionData.BuildType.RELEASE_CANDIDATE,
        VersionData.BuildType.BETA,
        VersionData.BuildType.DEV,
    ];

    [Signal] public delegate void VersionLocatedEventHandler(string key);

    private LineEdit _pathLineEdit;
    private Button _chooseLocationButton;
    private OptionButton _versionOptionButton;
    private OptionButton _buildOptionButton;
    private CheckButton _netSupportCheckButton;

    public override void _Ready()
    {
        _pathLineEdit = GetNode<LineEdit>("%PathLineEdit");
        _pathLineEdit.TextChanged += OnPathTextUpdated;
        _chooseLocationButton = GetNode<Button>("%ChooseLocationButton");
        _chooseLocationButton.Pressed += OnChooseLocationPressed;
        _versionOptionButton = GetNode<OptionButton>("%VersionOptionButton");
        _versionOptionButton.ItemSelected += OnOptionUpdated;
        _buildOptionButton = GetNode<OptionButton>("%BuildOptionButton");
        _buildOptionButton.ItemSelected += OnOptionUpdated;
        _netSupportCheckButton = GetNode<CheckButton>("%NetCheckButton");
        _netSupportCheckButton.Toggled += OnToggleUpdated;

        base._Ready();
        Validate();
    }

    protected override bool Validate()
    {
        if (!File.Exists(_pathLineEdit.Text))
        {
            DisplayError("Path is not valid.");
            return false;
        }

        if (VersionCache.Instance.HasPath(_pathLineEdit.Text))
        {
            DisplayError("Path is being used.");
            return false;
        }

        string key = VersionData.GenerateKey(
            new Version(_versionOptionButton.GetItemText(_versionOptionButton.Selected)),
            _netSupportCheckButton.ButtonPressed,
            BUILD_MAP[_buildOptionButton.Selected]
        );
        if (VersionCache.Instance.HasKey(key))
        {
            DisplayError("Version already exists.");
            return false;
        }

        DisplayMessage("Version is valid.");
        return true;
    }

    protected override void ClearWindowData()
    {
        base.ClearWindowData();
        _pathLineEdit.Clear();
    }

    protected override void OnConfirmPressed()
    {
        if (!File.Exists(_pathLineEdit.Text)) return;

        string key = VersionCache.Instance.AddVersion(
            new Version(_versionOptionButton.GetItemText(_versionOptionButton.Selected)),
            _netSupportCheckButton.ButtonPressed,
            BUILD_MAP[_buildOptionButton.Selected],
            _pathLineEdit.Text
        );

        if (key.Length == 0) return;

        EmitSignal(SignalName.VersionLocated, key);
        Hide();
    }

    private void OnChooseLocationPressed()
    {
        FileDialogManager.Instance.DataCompiled += OnFileDialogFileSelected;
        FileDialogManager.Instance.Open("Locate Godot.Exe", FileDialog.FileModeEnum.OpenFile, ["*.exe"]);
    }

    private void OnFileDialogFileSelected(string path)
    {
        if (path.Length > 0)
        {
            _pathLineEdit.Text = path;
            Validate();
        }
        FileDialogManager.Instance.DataCompiled -= OnFileDialogFileSelected;
    }
}
