using DataContainer;
using DataContainer.DatabaseSys.Databases.ProjectDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;
using Godot;

public partial class BuildPrompt : WindowBase
{
    [Signal] public delegate void BuildUpdatedEventHandler(string projectName);
    [Signal] public delegate void LaunchConfirmedEventHandler(string projectName);
    [Signal] public delegate void GoToVersionRequestedEventHandler();

    private RichTextLabel _projectLabel;
    private VBoxContainer _mainContainer;
    private OptionButton _buildOptionButton;
    private CheckButton _launchOnConfirmCheckButton;
    private Label _errorLabel;
    private Button _goToVersionsButton;

    private string _projectName = null;

    public override void _Ready()
    {
        _projectLabel = GetNode<RichTextLabel>("%ProjectLabel");

        _mainContainer = GetNode<VBoxContainer>("%MainContainer");

        _buildOptionButton = GetNode<OptionButton>("%BuildOptionButton");
        _buildOptionButton.ItemSelected += OnBuildOptionChanged;

        _launchOnConfirmCheckButton = GetNode<CheckButton>("%LaunchOnConfirmCheckButton");

        _errorLabel = GetNode<Label>("%ErrorLabel");

        _goToVersionsButton = GetNode<Button>("%VersionGotoButton");
        _goToVersionsButton.Pressed += OnGoToVersionsPressed;

        base._Ready();
    }

    public void Open(string projectName)
    {
        _projectName = projectName;
        _projectLabel.Text = ProjectDatabase.Instance.GenerateProjectMetadataString(in projectName, true);
        VersionKey versionKey = ProjectDatabase.Instance.GetVersionKey(in projectName);
        if (VersionDatabase.Instance.HasPartialKey(versionKey.PartialKey))
        {
            _mainContainer.Show();
            _errorLabel.Hide();
            BuildType[] types = VersionDatabase.Instance.GetAvaliableBuilds(versionKey.PartialKey);
            foreach (BuildType type in types)
                _buildOptionButton.AddItem(type.ToString());
            _buildOptionButton.Select(0);
            Validate();
        }
        else
        {
            _mainContainer.Hide();
            _errorLabel.Show();
        }
        Show();
    }

    protected override bool Validate()
    {
        BuildType build = (BuildType)_buildOptionButton.GetItemText(_buildOptionButton.Selected);
        if (build.Type == BuildType.FlagType.UNKNOWN)
        {
            DisplayError("HUHH???? HOW??? -- Please report... idk how that happened");
            return false;
        }

        DisplayMessage("Choice Valid");
        return true;
    }

    protected override void ClearWindowData()
    {
        base.ClearWindowData();
        _projectName = null;
        _buildOptionButton.Clear();
    }

    protected override void OnConfirmPressed()
    {
        bool buildSet = false;
        if (Validate())
        {
            BuildType build
                = (BuildType)_buildOptionButton.GetItemText(_buildOptionButton.Selected);
            buildSet = ProjectDatabase.Instance.SetBuild(in _projectName, build);
        }

        string projectName = _projectName;
        ClearWindowData();
        Hide();

        if (buildSet)
        {
            EmitSignal(SignalName.BuildUpdated, projectName);
            if (_launchOnConfirmCheckButton.ButtonPressed)
                EmitSignal(SignalName.LaunchConfirmed, projectName);
        }
        else
            NotifcationManager.Instance.NotifyError("Build was not able to be updated.");
    }

    private void OnBuildOptionChanged(long index) => Validate();

    private void OnGoToVersionsPressed()
    {
        Hide();
        EmitSignal(SignalName.GoToVersionRequested);
    }
}
