using Godot;
using System;

public partial class FileDialogManager : FileDialog
{
    #region Singleton Instance
    private static FileDialogManager _instance;
    public static FileDialogManager Instance => _instance;
    #endregion

    [Signal] public delegate void DataCompiledEventHandler(string path);

    public override void _Ready()
    {
        base._Ready();
        if (_instance != null && _instance != this)
        {
            QueueFree();
            return;
        }
        Hide();
        _instance = this;

        VisibilityChanged += OnVisibilityChanged;
        FileSelected += OnSelected;
        DirSelected += OnSelected;
        CloseRequested += OnCancelled;
        Canceled += OnCancelled;
    }

    public void Open(string title, FileModeEnum mode, string fileExt)
    {
        if (mode != FileModeEnum.OpenFile && mode != FileModeEnum.OpenDir) return;

        Title = title;
        FileMode = mode;
        if (mode == FileModeEnum.OpenDir)
            Filters = ["Folder"];
        else if (fileExt.Length > 0)
            Filters = [fileExt];
        else
            Filters = [];

        Show();
        ControlShield.Instance.Show();
    }

    private void OnSelected(string path) => EmitSignal(SignalName.DataCompiled, path);

    private void OnCancelled() => EmitSignal(SignalName.DataCompiled, "");

    private void OnVisibilityChanged()
    {
        if (ControlShield.Instance == null || Visible) return;

        ControlShield.Instance.Hide();
    }
}
