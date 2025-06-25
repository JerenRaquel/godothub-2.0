using DataContainer.DatabaseSys.Databases.ProjectDatabase;
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
        _projectLabel.Text = ProjectDatabase.Instance.GenerateProjectMetadataString(in projectName, true);
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
        OSAPI.DeleteFlag deleteFlag = OSAPI.DeleteProject(
            _cachedProjectName,
            _deleteSaveCheckButton.ButtonPressed
        );

        //* Validate Project Deletion
        if ((deleteFlag & OSAPI.DeleteFlag.ERROR_PROJECT_NO_EXISTS) > 0)
            NotifcationManager.Instance.NotifyError(
                $"{_cachedProjectName} root folder could not be found."
            );
        if ((deleteFlag & OSAPI.DeleteFlag.ERROR_PROJECT_FAILED_DB_REMOVAL) > 0)
            NotifcationManager.Instance.NotifyError(
                $"{_cachedProjectName} could not be found in database."
            );
        if ((deleteFlag & OSAPI.DeleteFlag.ERROR_PROJECT_FAILED_TRASH) > 0)
            NotifcationManager.Instance.NotifyError(
                $"Unable to delete {_cachedProjectName}"
            );
        else
            NotifcationManager.Instance.NotifyValid(
                $"{_cachedProjectName} deleted successfully."
            );

        //* Validate Save Folder
        if (_deleteSaveCheckButton.ButtonPressed)
        {
            if ((deleteFlag & OSAPI.DeleteFlag.ERROR_SAVE_NO_EXISTS) > 0)
                NotifcationManager.Instance.NotifyError(
                    $"{_cachedProjectName} save data could not be found."
                );
            if ((deleteFlag & OSAPI.DeleteFlag.ERROR_SAVE_FAILED_TRASH) > 0)
                NotifcationManager.Instance.NotifyError(
                    $"Unable to delete {_cachedProjectName} save data."
                );
            else
                NotifcationManager.Instance.NotifyValid(
                    $"{_cachedProjectName} save data deleted successfully."
                );
        }

        Hide();

        //! Failed -- Couldn't delete project
        if ((deleteFlag & OSAPI.DeleteFlag.ERROR_PROJECT_ANY) > 0) return;

        //* Success -- Both project and save folder deleted
        EmitSignal(SignalName.ProjectDeletedSuccessfully);
    }
}
