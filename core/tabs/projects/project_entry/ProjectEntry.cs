using Godot;
using System;

public partial class ProjectEntry : PanelContainer
{
    // Nodes
    private RichTextLabel _projectLabel;
    private Label _pathLabel;
    private Label _dateTimeLabel;
    private Button _tagButton;
    private TextureRect _projectIcon;

    private string _projectName;
    private string _cachedProjectMETAText;

    public override void _Ready()
    {
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
        _pathLabel.Text = "Path: " + ProjectCache.Instance.GetProjectPath(_projectName, true);
        _dateTimeLabel.Text = ProjectCache.Instance.GetLocalTime(_projectName);
        Texture2D texture = ProjectCache.Instance.GetIcon(_projectName);
        if (texture != null) _projectIcon.Texture = texture;
    }

    public bool Contains(string filter)
    {
        string sanitizedFilter = filter.ToLower();

        if (_cachedProjectMETAText.Contains(sanitizedFilter)) return true;

        // TODO: Add tag filtering

        return false;
    }

    private void UpdateProjectLabel()
    {
        string versionStr = ProjectCache.Instance.GetProjectVersion(_projectName);
        // TEMP: Requires the godot version cache
        string buildStr = "TODO";
        string renderStr = ProjectCache.Instance.GetRenderer(_projectName);
        string colorCode = renderStr switch
        {
            "Compatibility" => ColorTheme.Compat,
            "Mobile" => ColorTheme.Mobile,
            "Forward+" => ColorTheme.Forward,
            _ => ColorTheme.Unknown
        };

        string mainTextMETA = _projectName.BBCodeColor(ColorTheme.BaseBlue)
            + $" [ v{versionStr} | ".BBCodeColor(ColorTheme.BaseBlue)
            + buildStr.BBCodeColor(ColorTheme.Stable) + " ] ".BBCodeColor(ColorTheme.BaseBlue)
            + $"[{renderStr}]".BBCodeColor(colorCode);

        if (ProjectCache.Instance.UsesGDExt(_projectName))
            mainTextMETA += " [Uses GDExtension]".BBCodeColor(ColorTheme.HighlightBlue);

        if (ProjectCache.Instance.UsesDotNet(_projectName))
            mainTextMETA += " [Uses .NET]".BBCodeColor(ColorTheme.HighlightBlue);

        _projectLabel.Text = mainTextMETA;
        _cachedProjectMETAText = mainTextMETA.ToLower();

        if (ProjectCache.Instance.HasTags(_projectName))
        {
            _tagButton.Show();
        }
        else
        {
            _tagButton.Hide();
        }
    }
}
