using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using Newtonsoft.Json;

public partial class ProjectDataState
{
    private static readonly Texture2D GODOT_ICON = GD.Load<Texture2D>("res://icon.svg");

    private ProjectData _RAM;
    private ProjectData _ROM;
    private bool _isConfigDirty = false;
    private bool _isDirty = false;
    private bool _usingDotNet = false;
    private DateTime _lastEdited;
    private Texture2D _icon = null;
    private string _iconPath;

    public string projectName;

    public Version VersionData => _RAM.version;
    public string VersionStr => _RAM.version.ToString();
    public VersionData.BuildType Build => _RAM.Build;
    public ProjectData.Renderer Renderer => _RAM.renderer;
    public string[] ProjectTags => [.. _RAM.projectTags];
    public string[] SoftwareTags => [.. _RAM.softwareTags];
    public bool HasTags => _RAM.projectTags.Count > 0 || _RAM.softwareTags.Count > 0;
    public bool IsDotNet => _usingDotNet;
    public bool IsGDExt => _RAM.ProjectPathAddtion?.Length > 0;
    public DateTime LastEdited => _lastEdited;
    public Texture2D Icon => _icon;
    public string RootPath => _RAM.RootPath;
    public bool IsFavorited
    {
        get => _RAM.IsFavorited;
        set => _RAM.IsFavorited = value;
    }

    public void CreateProject(string versionStr, string renderer, string folderPath, string gdExtPath, string[] projectTags, string[] softwareTags, VersionData.BuildType build, bool isCSharp)
    {
        _ROM = new ProjectData(versionStr, renderer, folderPath, gdExtPath, false, projectTags, softwareTags);
        _ROM.Build = build;
        _RAM = new ProjectData(versionStr, renderer, folderPath, gdExtPath, false, projectTags, softwareTags);
        _RAM.Build = build;

        _usingDotNet = isCSharp;
        _lastEdited = File.GetLastWriteTime(_ROM.ProjectGodotPath);
        SetProjectIcon("res://icon.svg");
    }

    public bool ContainsTag(string tagName, bool isSoftware)
    {
        if (!HasTags) return false;

        if (isSoftware)
            return _RAM.softwareTags.Contains(tagName);
        return _RAM.projectTags.Contains(tagName);
    }

    public void SetBuild(VersionData.BuildType build)
    {
        _isDirty = true;
        _RAM.Build = build;
    }

    public void SetRenderer(ProjectData.Renderer renderer)
    {
        _isDirty = true;
        _isConfigDirty = true;
        _RAM.renderer = renderer;
    }

    public void SetVersion(Version version)
    {
        _isDirty = true;
        _isConfigDirty = true;
        _RAM.version = version;
    }

    public string GetFullPath(bool prettify = false)
    {
        if (prettify && IsGDExt)
            return _RAM.RootPath + $"/[{_RAM.ProjectPathAddtion}]";
        else
            return _RAM.FullPath;
    }

    public string Write()
    {
        StringWriter sw = new();
        JsonTextWriter writer = new(sw);

        // {
        writer.WriteStartObject();
        // Flags
        WriteEntry(writer, "Favorited", _ROM.IsFavorited);
        WriteEntry(writer, "IsDotNet", _usingDotNet);
        // Version
        WriteEntry(writer, "Version", _ROM.version.ToString());
        // Build
        WriteEntry(writer, "Build", global::VersionData.BuildEnumToString(Build));
        // Renderer
        WriteEntry(writer, "Renderer", _ROM.RendererString);
        // Project Name
        WriteEntry(writer, "Name", projectName);
        // Icon Path
        WriteEntry(writer, "IconPath", _iconPath);
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

    public bool WriteToROM()
    {
        if (!_isConfigDirty)
        {
            if (_isDirty)
            {
                _ROM = new(_RAM);
                _isDirty = false;
            }
            return true;
        }

        GD.Print("Writing ", projectName, " to Config file...");

        _isConfigDirty = false;
        _ROM = new(_RAM);

        //* Write ROM to config file
        ConfigFile config = new();
        if (config.Load(_ROM.ProjectGodotPath) != Error.Ok) return false;
        UpdateConfig(config, _ROM.version.ToString(), _usingDotNet, _ROM.RendererString, [.. _ROM.projectTags]);

        return true;
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

        _ROM = new ProjectData(versionStr, renderer, folderPath,
            configLoadData.Item2, false, projectTags, []);
        _RAM = new ProjectData(versionStr, renderer, folderPath,
            configLoadData.Item2, false, projectTags, []);

        _lastEdited = File.GetLastWriteTime(_ROM.ProjectGodotPath);
        SetProjectIcon(configLoadData.Item1);

        return true;
    }

    public bool LoadCached(ref string cachedData)
    {
        StringReader sr = new(cachedData);
        JsonTextReader reader = new(sr);

        // {
        reader.Read();
        // Flags
        bool favorited = ReadEntry(reader, false);
        _usingDotNet = ReadEntry(reader, false);
        // Version
        string versionStr = ReadEntry(reader, "Unknown");
        // Build
        string buildStr = ReadEntry(reader, "Unknown");
        // Renderer
        string renderer = ReadEntry(reader, "Unknown");
        // Project Name
        projectName = ReadEntry(reader, "Unknown");
        // Icon Path
        _iconPath = ReadEntry(reader, "res://icon.svg");
        // Root Path
        string rootPath = ReadEntry(reader, "");
        // project.godot folder path additions
        string pathAdditions = ReadEntry(reader, "");
        // [Project Tags]
        List<string> projectTags = ReadEntries<string>(reader);
        // [Tool Tags]
        List<string> softwareTags = ReadEntries<string>(reader);
        // }
        reader.Read();


        _ROM = new(versionStr, renderer, rootPath, pathAdditions, favorited, [.. projectTags], [.. softwareTags])
        { Build = global::VersionData.StringToBuildEnum(buildStr) };

        _RAM = new(_ROM);
        _lastEdited = File.GetLastWriteTime(_ROM.ProjectGodotPath);
        SetProjectIcon(_iconPath);

        return true;
    }

    public void UpdateTimeAccessed() => _lastEdited = DateTime.Now;

    private void SetProjectIcon(ConfigFile config)
    {
        string iconPath = config.GetValue("application", "config/icon", "").AsString();
        SetProjectIcon(iconPath);
    }

    private void SetProjectIcon(string iconPath)
    {
        if (iconPath.Length == 0) return;

        if (iconPath == "res://icon.svg")
        {
            _icon = GODOT_ICON;
            _iconPath = "res://icon.svg";
            return;
        }

        string santizedPath = iconPath.Replace("res:/", _ROM.FullPath);
        if (!File.Exists(santizedPath)) return;

        _iconPath = santizedPath;
        Image image = Image.LoadFromFile(santizedPath);
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

    public static void UpdateConfig(ConfigFile file, string versionStr, bool isCSharp, string rendererStr, string[] projectTags)
    {
        List<string> featureData = [];
        featureData.Add(versionStr);
        if (isCSharp) featureData.Add("C#");
        featureData.Add(rendererStr);
        file.SetValue("application", "config/features", featureData.ToArray());
        if (projectTags.Length > 0)
            file.SetValue("application", "config/tags", projectTags);
        else
        {
            if (file.HasSectionKey("application", "config/tags"))
                file.EraseSectionKey("application", "config/tags");
        }
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

    private static T ReadEntry<T>(JsonTextReader reader, T defaultValue)
    {
        if (!reader.Read()) return defaultValue;
        if (reader.TokenType == JsonToken.PropertyName) reader.Read();

        return reader.TokenType switch
        {
            JsonToken.StartObject => defaultValue,
            JsonToken.StartArray => defaultValue,
            _ => (T)reader.Value,
        };
    }

    private static List<T> ReadEntries<T>(JsonTextReader reader)
    {
        List<T> data = [];

        reader.Read();
        if (reader.TokenType == JsonToken.PropertyName) reader.Read();
        if (reader.TokenType != JsonToken.StartArray) return data;

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                case JsonToken.StartArray:
                case JsonToken.PropertyName:
                    continue;
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    return data;
                default:
                    data.Add((T)reader.Value);
                    break;
            }
        }
        return data;
    }

}

