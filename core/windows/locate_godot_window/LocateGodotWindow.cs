using DataContainer.DatabaseSys.Databases.VersionDatabase;
using Godot;
using System;
using System.IO;

public partial class LocateGodotWindow : WindowBase
{
    private readonly string[] VERSIONS = ["4.6", "4.5", "4.4", "4.3", "4.2", "4.1", "4.0"];
    private const int DEFAULT_VERSION = 2;

    private readonly VersionDatabase.BuildType[] BUILD_MAP = [
        VersionDatabase.BuildType.STABLE,
        VersionDatabase.BuildType.RELEASE_CANDIDATE,
        VersionDatabase.BuildType.BETA,
        VersionDatabase.BuildType.DEV,
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
        LoadVersionsIntoButton();
        Validate();
    }

    protected override bool Validate()
    {
        if (!File.Exists(_pathLineEdit.Text))
        {
            DisplayError("Path is not valid.");
            return false;
        }

        if (VersionDatabase.Instance.HasKey(new(_pathLineEdit.Text)))
        {
            DisplayError("Path is being used.");
            return false;
        }

        string selectedVersion = _versionOptionButton.GetItemText(_versionOptionButton.Selected);
        VersionKey key = new(
            new(selectedVersion),
            _netSupportCheckButton.ButtonPressed,
            BUILD_MAP[_buildOptionButton.Selected]
        );
        if (VersionDatabase.Instance.HasKey(key))
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

        VersionKey key = VersionDatabase.Instance.AddVersion(
            new(_versionOptionButton.GetItemText(_versionOptionButton.Selected)),
            _netSupportCheckButton.ButtonPressed,
            BUILD_MAP[_buildOptionButton.Selected],
            _pathLineEdit.Text
        );

        if (key.IsValid) return;

        EmitSignal(SignalName.VersionLocated, key.FullKey);
        Hide();
    }

    private void LoadVersionsIntoButton()
    {
        _versionOptionButton.Clear();
        foreach (string version in VERSIONS)
        {
            _versionOptionButton.AddItem(version);
        }
        _versionOptionButton.Select(DEFAULT_VERSION);
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
