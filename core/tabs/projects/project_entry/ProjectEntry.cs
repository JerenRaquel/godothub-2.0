using System.Collections.Generic;
using DataContainer.DatabaseSys.Databases.ProjectDatabase;
using DataContainer.DatabaseSys.Databases.SettingDatabase;
using DataContainer.DatabaseSys.Databases.TagDatabase;
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
    private Dictionary<TagKey, Tag> _tagInstances = [];

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
        _dateTimeLabel.Text = ProjectDatabase.Instance.GetLocalTime(in _projectName);
        Texture2D texture = ProjectDatabase.Instance.GetIcon(in _projectName);
        if (texture != null) _projectIcon.Texture = texture;
        if (ProjectDatabase.Instance.IsFavorited(_projectName)) _favoriteButton.SetPressedNoSignal(true);

        if (!ProjectDatabase.Instance.HasTags(projectName)) return;

        foreach (TagKey tagKey in ProjectDatabase.Instance.GetTagKeys(in projectName, TagDatabase.TagFlag.ANY))
        {
            string htmlColor = TagDatabase.Instance.GetHTMLColor(tagKey);
            Tag tagInstance = SpawnTag(tagKey.TagName, htmlColor);
            if (_tagInstances.ContainsKey(tagKey)) continue;

            _tagInstances.Add(tagKey, tagInstance);
        }
    }

    public void UpdatePath()
    {
        ProjectPathData? pathData = ProjectDatabase.Instance.GetPathData(in _projectName);
        if (!pathData.HasValue) return; //? What happens on fail? Is there a fail?

        string projectPath = pathData.Value.ProjectGodotFileLocationPretty;
        if (SettingsDatabase.Instance.GetData(SettingsDatabase.APPLICATION_ABS_PROJ_PATH))
        {
            string[] paths = SettingsDatabase.Instance.GetData(SettingsDatabase.PROJECT_PATH_TAG_KEY);
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

        foreach (KeyValuePair<TagKey, Tag> tagEntry in _tagInstances)
        {
            string tagName = tagEntry.Key.TagName;
            if (tagName.Contains(sanitizedFilter, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public void UpdateProjectLabel()
    {
        string mainTextMETA
            = ProjectDatabase.Instance.GenerateProjectMetadataString(_projectName);
        _projectLabel.Text = mainTextMETA;
        _cachedProjectMETAText = mainTextMETA.ToLower();

        if (ProjectDatabase.Instance.HasTags(_projectName))
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
        ProjectDatabase.Instance.SetFavorite(in _projectName, state);
        EmitSignal(SignalName.EntryFavoriteToggled);
    }

    private void OnTagButtonToggled(bool state)
    {
        foreach (KeyValuePair<TagKey, Tag> entry in _tagInstances)
            entry.Value.Displayed = state;

        if (state) _tagButton.Text = "v";
        else _tagButton.Text = "^";
    }
}
