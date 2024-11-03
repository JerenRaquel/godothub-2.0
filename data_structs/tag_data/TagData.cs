using System.Collections.Generic;
using System.Text.RegularExpressions;

public partial class TagData
{
    private Dictionary<string, SoftwareData> _softwareTags = [];
    // { tag : colorCode }
    private Dictionary<string, string> _projectTags = [];

    public Dictionary<string, SoftwareData>.Enumerator RawSoftwareData => _softwareTags.GetEnumerator();
    public long SoftwareCount => _softwareTags.Count;
    public string[] SoftwareTabs => [.. _softwareTags.Keys];
    public Dictionary<string, string>.Enumerator RawProjectData => _projectTags.GetEnumerator();
    public long ProjectCount => _projectTags.Count;
    public string[] ProjectTabs => [.. _projectTags.Keys];

    public TagData() { }
    public TagData(TagData other) => OverwriteWith(other);

    public void AddOrUpdateSoftwareTag(string name, SoftwareData data)
    {
        if (_softwareTags.ContainsKey(name))
        {
            _softwareTags.Add(name, data);
            return;
        }
        _softwareTags[name] = data;
    }

    public void AddOrUpdateProjectTagColor(string name, string colorCode)
    {
        if (_projectTags.ContainsKey(name))
        {
            _projectTags.Add(name, colorCode);
            return;
        }
        _projectTags[name] = colorCode;
    }

    public bool HasSoftwareTag(string tag) => _softwareTags.ContainsKey(tag);

    public string GetColor(bool isSoftware, string name, string defaultValue)
    {
        if (isSoftware)
        {
            if (!_softwareTags.TryGetValue(name, out SoftwareData data)) return defaultValue;
            if (data.IsNull) return defaultValue;
            return data.ColorCode;
        }
        else
        {
            if (!_projectTags.TryGetValue(name, out string colorCode)) return defaultValue;
            return colorCode;
        }
    }

    public string GetRawCommand(string softwareTag, bool full)
    {
        if (!_softwareTags.TryGetValue(softwareTag, out SoftwareData data)) return null;
        if (data.IsNull) return null;

        if (full)
            return data.RAWCommand;
        else
            return data.PrettyRawCommand;
    }

    public CommandParts GetCommandString(string softwareTag, string projectName)
    {
        if (!_softwareTags.TryGetValue(softwareTag, out SoftwareData data)) return new();
        if (data.IsNull) return new();

        if (projectName != null && projectName.Length == 0) projectName = null;

        return SetMacros(data.CommandData, projectName);
    }

    public string GetPath(string softwareTag)
    {
        if (!_softwareTags.TryGetValue(softwareTag, out SoftwareData data)) return null;
        if (data.IsNull) return null;

        return data.CommandData.Command;
    }

    public string GetArgString(string softwareTag)
    {

        if (!_softwareTags.TryGetValue(softwareTag, out SoftwareData data)) return null;
        if (data.IsNull) return null;
        return data.CommandData.ArgString;
    }

    public void LoadData(string tag, string data)
    {
        if (data.Trim().StartsWith('{') && data.Trim().EndsWith('}'))   // Software Tag
            AddOrUpdateSoftwareTag(tag, SoftwareData.FromJSONString(data));
        else if (CheckValidHtmlColor(data))    // Project Tag
            AddOrUpdateProjectTagColor(tag, data);

        // Else -- Skip and ignore; happens on corrupted files
    }

    public void OverwriteWith(TagData other)
    {
        _softwareTags = [];
        _projectTags = [];

        foreach (KeyValuePair<string, SoftwareData> entry in other._softwareTags)
            _softwareTags.Add(entry.Key, entry.Value);

        foreach (KeyValuePair<string, string> entry in other._projectTags)
            _projectTags.Add(entry.Key, entry.Value);
    }

    #region Color Regex
    // https://stackoverflow.com/a/13035186 -- Altered to store Regex for multiple use
    private static bool CheckValidHtmlColor(string inputColor) => HTMLColorRegex().Match(inputColor).Success;

    //regex from http://stackoverflow.com/a/1636354/2343 -- Altered to remove '#'
    [GeneratedRegex("^(?:[0-9a-fA-F]{3}){1,2}$")]
    private static partial Regex HTMLColorRegex();

    #endregion

}