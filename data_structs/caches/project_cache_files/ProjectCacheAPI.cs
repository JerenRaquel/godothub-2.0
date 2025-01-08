using System.IO;
using System.Collections.Generic;
using Godot;

public partial class ProjectCache : Cache
{
    public void AddProject(ProjectCreator.ProjectCreationData data, string path, string templateName, VersionData.BuildType build)
    {
        ProjectDataState project = new();
        TemplateStructure template = TemplateCache.Instance.GetTemplate(templateName);

        project.CreateProject(data.Version, data.Renderer, path, "", template.ProjectTags, template.SoftwareTags, build, data.IsCSharp);
        _projects.Add(data.Name, project);
    }

    public void DeleteProject(string projectName)
    {
        if (!_projects.ContainsKey(projectName)) return;

        _projects.Remove(projectName);
    }

    public void ToggleFavorite(string projectName, bool state)
    {
        ProjectDataState project = GetProject(projectName);
        if (project == null) return;
        project.IsFavorited = state;
    }

    public bool SetBuild(string projectName, VersionData.BuildType build)
    {
        ProjectDataState project = GetProject(projectName);
        if (project == null) return false;

        project.SetBuild(build);
        return true;
    }

    public bool UpdateProjectData(string projectName, VersionData.BuildType build, ProjectData.Renderer renderer, string version)
    {
        ProjectDataState project = GetProject(projectName);
        if (project == null) return false;

        _isDirty = true;

        if (build != project.Build)
            project.SetBuild(build);

        if (renderer != project.Renderer)
            project.SetRenderer(renderer);

        if (version != project.VersionStr)
            project.SetVersion(new(version));

        return true;
    }

    public VersionData.BuildType GetBuild(string projectName) => GetProject(projectName)?.Build ?? VersionData.BuildType.UNKNOWN;

    public string GetProjectVersion(string projectName) => GetProject(projectName)?.VersionStr ?? "Unknown";

    public string GetProjectVersionBuild(string projectName)
    {
        ProjectDataState projectData = GetProject(projectName);
        if (projectData == null) return null;

        string versionBuild = $"v{projectData.VersionStr ?? "Unknown"} [{VersionData.BuildEnumToString(projectData.Build)}]";
        if (projectData.IsDotNet)
            versionBuild += $" [.Net]";

        return versionBuild;
    }

    public string GetLocalTime(string projectName) => GetProject(projectName)?.LastEdited.ToLocalTime().ToString() ?? "Unknown";

    public Texture2D GetIcon(string projectName) => GetProject(projectName)?.Icon ?? null;

    public long GetRawTime(string projectName) => GetProject(projectName)?.LastEdited.Ticks ?? 0;

    public string GetProjectPath(string projectName, bool prettify = false)
    {
        ProjectDataState data = GetProject(projectName);
        if (data == null) return "";

        return data.GetFullPath(prettify);
    }

    public string GetProjectFolder(string projectName) => GetProject(projectName)?.RootPath ?? null;

    public string GetProjectSaveFolder(string projectName)
    {
        if (!_projects.ContainsKey(projectName)) return null;

        string path = OSAPI.DEFAULT_GODOT_USER_ROOT + projectName;
        if (!Directory.Exists(path))
            path = OSAPI.OS_USER_DATA_ROOT + "/" + projectName;

        if (!Directory.Exists(path)) return null;

        return path;
    }

    public string GetRenderer(string projectName)
    {
        ProjectData.Renderer renderer = GetProject(projectName)?.Renderer ?? ProjectData.Renderer.INVALID;
        return RenderEnumToString(renderer);
    }

    public string[] GetProjectTags(string projectName) => GetProject(projectName)?.ProjectTags;

    public string[] GetSoftwareTags(string projectName) => GetProject(projectName)?.SoftwareTags;

    public bool HasBuildSelected(string projectName) => GetProject(projectName)?.Build != VersionData.BuildType.UNKNOWN;

    public bool HasTags(string projectName) => GetProject(projectName)?.HasTags ?? false;

    public bool IsFavorited(string projectName) => GetProject(projectName)?.IsFavorited ?? false;

    public bool UsesDotNet(string projectName) => GetProject(projectName)?.IsDotNet ?? false;

    public bool UsesGDExt(string projectName) => GetProject(projectName)?.IsGDExt ?? false;

    public string[] GetVersions()
    {
        HashSet<string> data = [];
        foreach (KeyValuePair<string, ProjectDataState> entry in _projects)
        {
            string versionStr = entry.Value.VersionStr;
            if (data.Contains(versionStr)) continue;

            data.Add(versionStr);
        }
        return [.. data];
    }

    public void UpdateTimeAccessed(string projectName) => GetProject(projectName)?.UpdateTimeAccessed();

    public string ProjectNameToPartialKey(string projectName)
    {
        ProjectDataState data = GetProject(projectName);
        return VersionData.GeneratePartialKey(data.VersionData, data.IsDotNet);
    }

    public string ProjectNameToKey(string projectName)
    {
        ProjectDataState data = GetProject(projectName);
        if (data == null) return null;
        if (data.Build == VersionData.BuildType.UNKNOWN) return null;

        return VersionData.GenerateKey(data.VersionData, data.IsDotNet, data.Build);
    }

    public string GenerateProjectMetadataString(string projectName, bool center = false)
    {
        ProjectDataState data = GetProject(projectName);
        if (data == null) return "";

        VersionData.BuildType buildType = data.Build;
        string buildStr = VersionData.BuildEnumToString(buildType);
        string versionStr = data.VersionStr ?? "Unknown";
        string renderStr = GetRenderer(projectName);
        string colorCode = renderStr switch
        {
            "Compatibility" => ColorTheme.Compat,
            "Mobile" => ColorTheme.Mobile,
            "Forward+" => ColorTheme.Forward,
            _ => ColorTheme.Unknown
        };

        string mainTextMETA = projectName.BBCodeColor(ColorTheme.BaseBlue)
            + $" [ v{versionStr} | ".BBCodeColor(ColorTheme.BaseBlue)
            + buildStr.BBCodeColor(ColorTheme.GetColorFromBuild(buildType)) + " ] ".BBCodeColor(ColorTheme.BaseBlue)
            + $"[{renderStr}]".BBCodeColor(colorCode);

        if (UsesGDExt(projectName))
            mainTextMETA += " [Uses GDExtension]".BBCodeColor(ColorTheme.HighlightBlue);

        if (UsesDotNet(projectName))
            mainTextMETA += " [Uses .NET]".BBCodeColor(ColorTheme.CSharp);

        if (center)
            return $"[center]{mainTextMETA}[/center]";
        else
            return mainTextMETA;
    }

    private ProjectDataState GetProject(string projectName)
    {
        if (projectName == null || projectName.Length == 0) return null;
        if (!_projects.TryGetValue(projectName, out ProjectDataState value)) return null;

        return value;
    }

    public static string RenderEnumToString(ProjectData.Renderer renderer)
    {
        return renderer switch
        {
            ProjectData.Renderer.COMPAT => "Compatibility",
            ProjectData.Renderer.MOBILE => "Mobile",
            ProjectData.Renderer.FORWARD => "Forward+",
            _ => "Unkwown"
        };
    }

}