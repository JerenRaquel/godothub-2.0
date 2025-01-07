using Godot;
using System;

public partial class DeletePrompt : WindowBase
{
    [Signal] public delegate void ProjectDeletedSuccessfullyEventHandler();

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
        Tuple<bool, bool> deleteStates = OSAPI.DeleteProject(_cachedProjectName, _deleteSaveCheckButton.ButtonPressed);
        bool projectDeletedState = deleteStates.Item1;
        bool projectSaveDeletedState = deleteStates.Item2;

        // Main Project Folder
        if (projectDeletedState)
            NotifcationManager.Instance.NotifyValid($"Project: {_cachedProjectName} deleted successfully.");
        else
            NotifcationManager.Instance.NotifyError($"Unable to delete project: {_cachedProjectName}");

        // Project's Save Folder
        if (_deleteSaveCheckButton.ButtonPressed)
        {
            if (projectSaveDeletedState)
                NotifcationManager.Instance.NotifyValid($"Project: {_cachedProjectName} save data deleted successfully.");
            else
                NotifcationManager.Instance.NotifyError($"Unable to delete project's save data.: {_cachedProjectName}");
        }

        Hide();

        // Failed -- Couldn't delete project
        if (!projectDeletedState) return;

        // Success -- Don't delete save folder
        if (!_deleteSaveCheckButton.ButtonPressed)
        {
            EmitSignal(SignalName.ProjectDeletedSuccessfully);
            return;
        }

        // Success -- Both project and save folder deleted
        EmitSignal(SignalName.ProjectDeletedSuccessfully);
    }
}
