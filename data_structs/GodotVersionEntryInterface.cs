using Godot;

public interface IGodotVersionEntryInterface
{
    public bool HasCSharp();
    public bool HasBuild(VersionData.BuildType type);

    public void ToggleOff();
}