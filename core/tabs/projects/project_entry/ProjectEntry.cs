using System.Collections.Generic;
using Godot;

public partial class ProjectEntry : PanelContainer
{
    [Signal] public delegate void EntryFavoriteToggledEventHandler();

    [Export] private PackedScene _tagPackedScene;

    // Nodes
    private Button _favoriteButton;
    private HFlowContainer _tagContainer;
    private RichTextLabel _projectLabel;
    private Label _pathLabel;
    private Label _dateTimeLabel;
    private Button _tagButton;
    private TextureRect _projectIcon;

    private string _projectName;
    private string _cachedProjectMETAText;
    private Dictionary<string, Tag> _projectTags = [];
    private Dictionary<string, Tag> _softwareTags = [];

    public DoubleClickButton DoubleClickButton { get; private set; }

    public override void _Ready()
    {
        DoubleClickButton = GetNode<DoubleClickButton>("%DoubleClickButton");
        _favoriteButton = GetNode<Button>("%FavoriteButton");
        _favoriteButton.Toggled += OnFavoriteToggled;
        _tagContainer = GetNode<HFlowContainer>("%TagsContainer");
        _projectLabel = GetNode<RichTextLabel>("%ProjectLabel");
        _pathLabel = GetNode<Label>("%PathLabel");
        _dateTimeLabel = GetNode<Label>("%DateTimeLabel");
        _tagButton = GetNode<Button>("%TagButton");
        _tagButton.Toggled += OnTagButtonToggled;
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
        if (ProjectCache.Instance.IsFavorited(_projectName)) _favoriteButton.SetPressedNoSignal(true);

        if (!ProjectCache.Instance.HasTags(projectName)) return;

        foreach (string tag in ProjectCache.Instance.GetProjectTags(projectName))
        {
            Tag tagInstance = SpawnTag(tag, TagCache.Instance.GetColor(false, tag));
            _projectTags.Add(tag, tagInstance);
        }

        foreach (string tag in ProjectCache.Instance.GetSoftwareTags(projectName))
        {
            Tag tagInstance = SpawnTag(tag, TagCache.Instance.GetColor(true, tag));
            _softwareTags.Add(tag, tagInstance);
        }
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

        foreach (KeyValuePair<string, Tag> tagEntry in _projectTags)
            if (tagEntry.Key.ToLower().Contains(sanitizedFilter)) return true;

        foreach (KeyValuePair<string, Tag> tagEntry in _softwareTags)
            if (tagEntry.Key.ToLower().Contains(sanitizedFilter)) return true;

        return false;
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

    private Tag SpawnTag(string tagName, string colorCode)
    {
        Tag tagInstance = _tagPackedScene.Instantiate<Tag>();
        _tagContainer.AddChild(tagInstance);
        tagInstance.SetData(tagName, colorCode);
        return tagInstance;
    }

    private void OnFavoriteToggled(bool state)
    {
        ProjectCache.Instance.ToggleFavorite(_projectName, state);
        EmitSignal(SignalName.EntryFavoriteToggled);
    }

    private void OnTagButtonToggled(bool state)
    {
        foreach (KeyValuePair<string, Tag> entry in _projectTags)
            entry.Value.Displayed = state;

        foreach (KeyValuePair<string, Tag> entry in _softwareTags)
            entry.Value.Displayed = state;

        if (state) _tagButton.Text = "v";
        else _tagButton.Text = "^";
    }
}
