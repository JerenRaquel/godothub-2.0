using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ProjectPathInterface : InterfaceBase
{
    [Export] private PackedScene _pathEntry;

    private CenterContainer _buttonHolder;
    private Button _addButton;

    private int _count = 0;
    private ProjectPath _awaitingInstance = null;

    public override void _Ready()
    {
        base._Ready();

        _buttonHolder = GetNode<CenterContainer>("%AddButtonContainer");
        _addButton = GetNode<Button>("%AddButton");
        _addButton.Pressed += OnAddPressed;

        LoadPath("").Disabled = true;
        // TODO: Fetch all loaded paths
    }

    public override string[] GetAllSettingTags() => ["project_paths"];

    public override SettingsData.Data GetData(string settingTag)
    {
        if (settingTag != "project_paths") return new();


        if (_count == 1)
        {
            ProjectPath projPath = LastPathEntry();
            if (projPath.IsValid) return new([projPath.Path]);
            return new();
        }

        List<string> data = [];
        foreach (ProjectPath projectPath in _contentContainer.GetChildren().Cast<ProjectPath>())
        {
            if (!projectPath.IsValid) continue;
            data.Add(projectPath.Path.Replace("\\", "/"));
        }

        return new([.. data]);
    }

    public override void SetData(string settingTag, SettingsData.Data data)
    {
        if (settingTag != "project_paths") return;

        foreach (string path in (string[])data)
            LoadPath(path);
    }


    private ProjectPath LastPathEntry() => _contentContainer.GetChild<ProjectPath>(_count - 1);

    private ProjectPath LoadPath(string path)
    {
        ProjectPath pathInstance = _pathEntry.Instantiate<ProjectPath>();
        _contentContainer.AddChild(pathInstance);
        pathInstance.Path = path;
        pathInstance.TreeExited += OnPathDeleted;
        pathInstance.FileDialogRequested += OnFileDialogRequested;
        _count += 1;

        if (_count == 2)
            _contentContainer.GetChild<ProjectPath>(0).Disabled = false;
        _contentContainer.MoveChild(_buttonHolder, -1);
        return pathInstance;
    }

    private void OnAddPressed()
    {
        ProjectPath lastPathEntry = LastPathEntry();
        if (lastPathEntry == null || lastPathEntry.Path.Length == 0) return;

        LoadPath("");
    }

    private void OnPathDeleted()
    {
        _count--;
        if (_count == 1)
            _contentContainer.GetChild<ProjectPath>(0).Disabled = true;
    }

    private void OnFileDialogRequested(ProjectPath instance)
    {
        _awaitingInstance = instance;
        FileDialogManager.Instance.DataCompiled += OnFileSelected;
        FileDialogManager.Instance.Open("Locate Projects Directory", FileDialog.FileModeEnum.OpenDir, "");
    }

    private void OnFileSelected(string path)
    {
        if (path.Length > 0)
            _awaitingInstance.Path = path;
        _awaitingInstance = null;
        FileDialogManager.Instance.DataCompiled -= OnFileSelected;
    }
}
