using Godot;
using System;

public partial class TemplateToolBar : HBoxContainer
{
    [Signal] public delegate void TagAddedEventHandler();
    [Signal] public delegate void TagRemovedEventHandler();
    [Signal] public delegate void FileAddedEventHandler();
    [Signal] public delegate void FileRemovedEventHandler();
    [Signal] public delegate void FolderAddedEventHandler();
    [Signal] public delegate void FolderRemovedEventHandler();
    [Signal] public delegate void FillFolderStateToggledEventHandler();

    public bool FillFolders
    {
        get => _fillFoldersCheckButton.ButtonPressed;
        set => _fillFoldersCheckButton.SetPressedNoSignal(value);
    }

    private Button _addTagButton;
    private Button _removeTagButton;
    private Button _addFileButton;
    private Button _removeFileButton;
    private Button _addFolderButton;
    private Button _removeFolderButton;
    private CheckButton _fillFoldersCheckButton;

    public override void _Ready()
    {
        _addTagButton = GetNode<Button>("%AddTagButton");
        _addTagButton.Pressed += () => EmitSignal(SignalName.TagAdded);

        _removeTagButton = GetNode<Button>("%RemoveTagButton");
        _removeTagButton.Pressed += () => EmitSignal(SignalName.TagRemoved);

        _addFileButton = GetNode<Button>("%AddFileButton");
        _addFileButton.Pressed += () => EmitSignal(SignalName.FileAdded);

        _removeFileButton = GetNode<Button>("%RemoveFileButton");
        _removeFileButton.Pressed += () => EmitSignal(SignalName.FileRemoved);

        _addFolderButton = GetNode<Button>("%AddFolderButton");
        _addFolderButton.Pressed += () => EmitSignal(SignalName.FolderAdded);

        _removeFolderButton = GetNode<Button>("%RemoveFolderButton");
        _removeFolderButton.Pressed += () => EmitSignal(SignalName.FolderRemoved);

        _fillFoldersCheckButton = GetNode<CheckButton>("%FillFoldersCheckButton");
        _fillFoldersCheckButton.Toggled += _ => EmitSignal(SignalName.FillFolderStateToggled);
    }
}
