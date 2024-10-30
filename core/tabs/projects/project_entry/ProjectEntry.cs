using Godot;

public partial class ProjectEntry : PanelContainer
{
    [Signal] public delegate void LaunchRequestedEventHandler(string projectName);
    [Signal] public delegate void ToggledEventHandler(string projectName, bool state);

    // Nodes
    private Timer _timer;
    private Button _mainButton;
    private RichTextLabel _projectLabel;
    private Label _pathLabel;
    private Label _dateTimeLabel;
    private Button _tagButton;
    private TextureRect _projectIcon;

    private string _projectName;
    private string _cachedProjectMETAText;

    public override void _Ready()
    {
        _timer = GetNode<Timer>("%Timer");
        _mainButton = GetNode<Button>("%MainButton");
        _mainButton.Toggled += OnMainToggled;
        _projectLabel = GetNode<RichTextLabel>("%ProjectLabel");
        _pathLabel = GetNode<Label>("%PathLabel");
        _dateTimeLabel = GetNode<Label>("%DateTimeLabel");
        _tagButton = GetNode<Button>("%TagButton");
        _projectIcon = GetNode<TextureRect>("%ProjectIcon");
        _tagButton.Hide();
    }

    public void Initialize(string projectName)
    {
        _projectName = projectName;
        UpdateProjectLabel();
        UpdatePath();
        _dateTimeLabel.Text = ProjectCache.Instance.GetLocalTime(_projectName);
        Texture2D texture = ProjectCache.Instance.GetIcon(_projectName);
        if (texture != null) _projectIcon.Texture = texture;
    }

    public void UpdatePath()
    {
        string projectPath = ProjectCache.Instance.GetProjectPath(_projectName, true);
        if (SettingsCache.Instance.GetData("Application/Config/abs_proj_path/BOOL"))
        {
            //? Is there a better way than hard coding this?
            string[] paths = SettingsCache.Instance.GetData("Project Settings/Paths/project_paths/STRING_LIST");
            foreach (string path in paths)
            {
                if (projectPath.Contains(path))
                {
                    _pathLabel.Text = "Path: " + projectPath.Replace(path, "[HIDDEN]");
                    return;
                }
            }
        }
        _pathLabel.Text = "Path: " + projectPath;
    }

    public bool Contains(string filter)
    {
        string sanitizedFilter = filter.ToLower();

        if (_cachedProjectMETAText.Contains(sanitizedFilter)) return true;

        // TODO: Add tag filtering

        return false;
    }

    public void ToggleOff()
    {
        _timer.Stop();
        _mainButton.SetPressedNoSignal(false);
    }

    public void UpdateProjectLabel()
    {
        string mainTextMETA = ProjectCache.Instance.GenerateProjectMetadataString(_projectName);
        _projectLabel.Text = mainTextMETA;
        _cachedProjectMETAText = mainTextMETA.ToLower();

        if (ProjectCache.Instance.HasTags(_projectName))
            _tagButton.Show();
        else
            _tagButton.Hide();
    }

    private void OnMainToggled(bool toggled_on)
    {
        if (_timer.TimeLeft > 0.0)
        {
            _timer.Stop();
            EmitSignal(SignalName.LaunchRequested, _projectName);
            //* Must come after launch signal -- Hence the two lines of toggle emitting
            EmitSignal(SignalName.Toggled, _projectName, toggled_on);
            return;
        }
        EmitSignal(SignalName.Toggled, _projectName, toggled_on);
        _timer.Start();
    }
}
