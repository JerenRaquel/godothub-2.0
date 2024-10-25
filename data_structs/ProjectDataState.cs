using Godot;

public partial class ProjectDataState
{
    private ProjectData _RAM;
    private ProjectData _ROM;
    private bool _isDirty = false;
    private int _lastEdited;
    private Texture _icon;

    public bool Write()
    {
        return false;
    }
}

