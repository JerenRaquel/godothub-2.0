using Godot;
using System;

public partial class TreeDisplay : VBoxContainer
{
    [Signal] public delegate void FileAddedEventHandler();
    [Signal] public delegate void FileRemovedEventHandler();
    [Signal] public delegate void FolderAddedEventHandler();
    [Signal] public delegate void FolderRemovedEventHandler();
    [Signal] public delegate void FillFolderStateToggledEventHandler();

    public bool FillFolders
    {
        get => _fillFolderCheckButton.ButtonPressed;
        set => _fillFolderCheckButton.SetPressedNoSignal(value);
    }

    private Button _addFileButton;
    private Button _removeFileButton;
    private Button _addFolderButton;
    private Button _removeFolderButton;
    private CheckButton _fillFolderCheckButton;
    private TemplateDisplay _templateDisplay;

    public override void _Ready()
    {
        _addFileButton = GetNode<Button>("%AddFileButton");
        _addFileButton.Pressed += () => EmitSignal(SignalName.FileAdded);

        _removeFileButton = GetNode<Button>("%RemoveFileButton");
        _removeFileButton.Pressed += () => EmitSignal(SignalName.FileRemoved);

        _addFolderButton = GetNode<Button>("%AddFolderButton");
        _addFolderButton.Pressed += () => EmitSignal(SignalName.FolderAdded);

        _removeFolderButton = GetNode<Button>("%RemoveFolderButton");
        _removeFolderButton.Pressed += () => EmitSignal(SignalName.FolderRemoved);

        _fillFolderCheckButton = GetNode<CheckButton>("%FillFolderCheckButton");
        _fillFolderCheckButton.Toggled += _ => EmitSignal(SignalName.FillFolderStateToggled);

        _templateDisplay = GetNode<TemplateDisplay>("%TemplateDisplay");
    }

    public void Build(string activeTemplate) => _templateDisplay.Build(activeTemplate);
}
