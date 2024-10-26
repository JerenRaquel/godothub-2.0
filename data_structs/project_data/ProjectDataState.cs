using System;
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
    private DateTime _lastEdited;
    private Texture _icon = null;

    public string projectName;

    public string Write()
    {
        if (!_isDirty && !_isConfigDirty) return "";

        StringWriter sw = new StringWriter();
        JsonTextWriter writer = new JsonTextWriter(sw);

        // {
        writer.WriteStartObject();

        // Favorited
        writer.WritePropertyName("Favorited");
        writer.WriteValue(_ROM.IsFavorited);

        // Version
        writer.WritePropertyName("Version");
        writer.WriteValue(_ROM.version.ToString());

        // Renderer
        writer.WritePropertyName("Renderer");
        writer.WriteValue(_ROM.RendererString);

        // Root Path
        writer.WritePropertyName("RootPath");
        writer.WriteValue(_ROM.RootPath);

        // project.godot folder path additions
        writer.WritePropertyName("ProjectPathAdditions");
        writer.WriteValue(_ROM.ProjectPathAddtion);

        // Project Tags
        writer.WritePropertyName("ProjectTags");
        // [
        writer.WriteStartArray();
        foreach (string tag in _ROM.projectTags)
        {
            writer.WriteValue(tag);
        }
        // ]
        writer.WriteEndArray();

        // Tool Tags
        writer.WritePropertyName("ToolTags");
        // [
        writer.WriteStartArray();
        foreach (string tag in _ROM.softwareTags)
        {
            writer.WriteValue(tag);
        }
        // ]
        writer.WriteEndArray();

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
            "application", "config/features", new string[2] { "Unknown.Unknown", "Unknown" }
        ).AsStringArray();
        var projectTags = configLoadData.Item1.GetValue(
            "application", "config/tags", Array.Empty<string>()
        ).AsStringArray();

        _ROM = new ProjectData(
            features[0], features[1], folderPath, configLoadData.Item2, false, projectTags,
            Array.Empty<string>()
        );
        _RAM = new ProjectData(
            features[0], features[1], folderPath, configLoadData.Item2, false, projectTags,
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
}

