using Godot;
using System;

public partial class DeletePrompt : WindowBase
{
    private RichTextLabel _projectLabel;
    private CheckButton _deleteSaveCheckButton;

    private string _cachedProjectName;

    public override void _Ready()
    {
        base._Ready();
        _projectLabel = GetNode<RichTextLabel>("%ProjectLabel");
        _deleteSaveCheckButton = GetNode<CheckButton>("%DeleteSave");
    }

    public void Open(string projectName)
    {
        _cachedProjectName = projectName;
        _projectLabel.Text = ProjectCache.Instance.GenerateProjectMetadataString(projectName, true);
        Validate();
        Show();
    }

    protected override bool Validate()
    {
        ForceConfirm();
        return true;
    }

    protected override void OnConfirmPressed()
    {

    }
}
