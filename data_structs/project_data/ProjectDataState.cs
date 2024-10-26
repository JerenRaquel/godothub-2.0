using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Godot;

public partial class ProjectDataState
{
    private static readonly Texture GODOT_ICON = GD.Load<Texture>("res://icon.svg");

    private ProjectData _RAM;
    private ProjectData _ROM;
    private bool _isDirty = false;
    private DateTime _lastEdited;
    private Texture _icon = null;

    public string Write(string saveLocation)
    {
        if (!_isDirty) return "";

        FileStream saveFile = File.Open(saveLocation, FileMode.OpenOrCreate);

        return "";
    }

    public void LoadUncached(ref Tuple<ConfigFile, string> configLoadData, ref string folderPath)
    {
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
        // GD.Print(_ROM);
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

