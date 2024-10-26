using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Newtonsoft.Json;

public partial class ProjectDataState
{
    private static readonly Texture GODOT_ICON = GD.Load<Texture>("res://icon.svg");

    private ProjectData _RAM;
    private ProjectData _ROM;
    private bool _isDirty = false;
    private bool _isConfigDirty = false;
    private bool _usingDotNet = false;
    private DateTime _lastEdited;
    private Texture _icon = null;

    public string projectName;

    public string VersionStr { get => _RAM.version.ToString(); }
    public ProjectData.Renderer Renderer { get => _RAM.renderer; }
    public bool HasTags { get => _RAM.projectTags.Count > 0 || _RAM.softwareTags.Count > 0; }
    public bool IsDotNet { get => _usingDotNet; }
    public bool IsGDExt { get => _RAM.ProjectPathAddtion?.Length > 0; }

    public string Write()
    {
        StringWriter sw = new();
        JsonTextWriter writer = new(sw);

        // {
        writer.WriteStartObject();
        // Use Flags
        WriteEntry(writer, "Favorited", _ROM.IsFavorited);
        WriteEntry(writer, "IsDotNet", _usingDotNet);
        WriteEntry(writer, "IsGDExt", IsGDExt);
        // Version
        WriteEntry(writer, "Version", _ROM.version.ToString());
        // Renderer
        WriteEntry(writer, "Renderer", _ROM.RendererString);
        // Root Path
        WriteEntry(writer, "RootPath", _ROM.RootPath);
        // project.godot folder path additions
        WriteEntry(writer, "ProjectPathAdditions", _ROM.ProjectPathAddtion);
        // [Project Tags]
        WriterEntries(writer, "ProjectTags", _ROM.projectTags);
        // [Tool Tags]
        WriterEntries(writer, "ToolTags", _ROM.softwareTags);
        // }
        writer.WriteEndObject();

        return sw.ToString();
    }

    public bool LoadUncached(ref Tuple<ConfigFile, string> configLoadData, ref string folderPath)
    {
        projectName = configLoadData.Item1.GetValue(
            "application", "config/name", "Unknown Project"
        ).AsString();
        if (projectName.Length == 0) return false;

        string[] features = configLoadData.Item1.GetValue(
            "application", "config/features", Array.Empty<string>()
        ).AsStringArray();
        var projectTags = configLoadData.Item1.GetValue(
            "application", "config/tags", Array.Empty<string>()
        ).AsStringArray();

        string versionStr = "Unknown";
        string renderer = "Unkwown";
        if (features.Length == 2)
        {
            versionStr = features[0];
            renderer = features[1];
        }
        else if (features.Length == 3)
        {
            versionStr = features[0];
            renderer = features[2];
            _usingDotNet = features[1] == "C#";
        }

        _ROM = new ProjectData(
            versionStr, renderer, folderPath, configLoadData.Item2, false, projectTags,
            Array.Empty<string>()
        );
        _RAM = new ProjectData(
            versionStr, renderer, folderPath, configLoadData.Item2, false, projectTags,
            Array.Empty<string>()
        );
        _lastEdited = File.GetLastAccessTime(_ROM.ProjectGodotPath);
        SetProjectIcon(configLoadData.Item1);

        _isDirty = true;
        return true;
    }

    public void LoadCached()
    {

    }

    private void SetProjectIcon(ConfigFile config)
    {
        string iconPath = config.GetValue("application", "config/icon", "").AsString();
        if (iconPath.Length == 0) return;
        if (!File.Exists(iconPath)) return;

        if (iconPath == "res://icon.svg")
        {
            _icon = GODOT_ICON;
            return;
        }

        Image image = Image.LoadFromFile(iconPath);
        if (image == null)
        {
            _icon = GODOT_ICON;
            return;
        }

        ImageTexture imageTexture = ImageTexture.CreateFromImage(image);
        if (imageTexture == null)
        {
            _icon = GODOT_ICON;
            return;
        }

        _icon = imageTexture;
    }

    private static void WriteEntry<T>(JsonTextWriter writer, string propName, T value)
    {
        writer.WritePropertyName(propName);
        writer.WriteValue(value);
    }

    private static void WriterEntries<T>(JsonTextWriter writer, string propName, List<T> values)
    {
        writer.WritePropertyName(propName);
        // [
        writer.WriteStartArray();
        foreach (T value in values)
        {
            writer.WriteValue(value);
        }
        // ]
        writer.WriteEndArray();
    }
}

