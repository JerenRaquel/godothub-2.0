using Godot;
using System;

public partial class BuildPrompt : WindowBase
{
    private RichTextLabel _projectLabel;
    private VBoxContainer _mainContainer;
    private OptionButton _buildOptionButton;
    private Label _errorLabel;


    public override void _Ready()
    {
        base._Ready();

        _projectLabel = GetNode<RichTextLabel>("%ProjectLabel");
        _mainContainer = GetNode<VBoxContainer>("%MainContainer");
        _buildOptionButton = GetNode<OptionButton>("%BuildOptionButton");
        _errorLabel = GetNode<Label>("%ErrorLabel");
        Hidden += OnClosed;
    }

    public void Open(string projectName)
    {
        _projectLabel.Text = ProjectCache.Instance.GenerateProjectMetadataString(projectName, true);
        string partialKey = ProjectCache.Instance.ProjectNameToPartialKey(projectName);
        if (VersionCache.Instance.HasPartialKey(partialKey))
        {
            _mainContainer.Show();
            _errorLabel.Hide();
            VersionData.BuildType[] types = VersionCache.Instance.GetAvailableBuilds(partialKey);
            foreach (VersionData.BuildType type in types)
                _buildOptionButton.AddItem(VersionData.BuildEnumToString(type));
            _buildOptionButton.Select(0);
        }
        else
        {
            _mainContainer.Hide();
            _errorLabel.Show();
        }
        Show();
    }

    private void OnClosed() => _buildOptionButton.Clear();
}
