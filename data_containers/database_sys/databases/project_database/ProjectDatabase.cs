using System.Collections.Generic;
using System.IO;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;
using Godot;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public partial class ProjectDatabase : Database<string, ProjectData>
    {
        #region Manipulation
        // TODO: Refactor `ProjectCreator.ProjectCreationData`
        public string AddProject(in ProjectCreator.ProjectCreationData data,
            in string path, in string templateKey, in BuildType buildType)
        {
            // TODO: Replace with new fetch method -- Need Tag List
            //. TemplateStructure template = TemplateCache.Instance.GetTemplate(templateKey);

            //* Create new project
            ProjectData project = new(
                in data.Name,
                new(),
                new(data.Version),
                new(path, "", ""),
                (Renderer)data.Renderer,
                buildType,
                data.IsCSharp,
                []  // TODO: Replace with something like `template.GetTags()`
            );

            //* Add project entry and return the name as the key
            _data.Add(data.Name, project);
            IsDirty = true;
            return data.Name;
        }

        public void DeleteProject(in string projectKey)
        {
            if (!_data.ContainsKey(projectKey)) return;

            _data.Remove(projectKey);
            IsDirty = true;
        }

        public bool UpdateProjectData(in string projectKey, BuildType buildType,
            Renderer renderer, in Version version)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return false;

            SetBuild(in project, in buildType);
            SetRenderer(in project, in renderer);
            SetVersion(in project, in version);
            return true;
        }

        public void UpdateTimeAccessed(in string projectKey)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return;

            project.UpdateTimeAccessed();
        }

        #endregion

        #region Has/Contains/Is
        public bool HasTags(in string projectKey)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return false;
            return project.HasTags;
        }

        public bool HasTag(in string projectKey, in TagKey tagKey)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return false;
            return project.HasTag(tagKey);
        }

        public bool IsFavorited(in string projectKey)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return false;
            return project.IsFavorited;
        }

        #endregion

        #region Getters
        public Texture2D GetIcon(in string projectKey)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return null;
            return project.IconData.Icon;
        }

        public Renderer? GetRenderer(in string projectKey)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return null;
            return project.Renderer;
        }

        public VersionKey GetVersionKey(in string projectKey)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return null;
            return new(project.VersionData, project.UsesDotNet, project.BuildType);
        }

        public TagKey[] GetTagKeys(in string projectKey, TagDatabase.TagDatabase.TagFlag flag)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return [];
            return project.GetTags(flag);
        }

        public ProjectPathData? GetPathData(in string projectKey)
        {
            ProjectData project = GetProject(in projectKey);
            if (project == null) return null;

            return project.PathData;
        }

        public string GetProjectUserDirectory(in string projectKey)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return null;

            string path = OSAPI.DEFAULT_GODOT_USER_ROOT + project.ProjectName;
            if (!Directory.Exists(path))
                path = OSAPI.OS_USER_DATA_ROOT + "/" + project.ProjectName;

            if (!Directory.Exists(path)) return null;
            return path;
        }

        public string[] GetAllUsedVersionsAsStr()
        {
            HashSet<string> results = [];
            foreach (KeyValuePair<string, ProjectData> entry in _data)
            {
                string versionStr = entry.Value.VersionData.ToString();
                results.Add(versionStr);
            }
            return [.. results];
        }
        #endregion

        #region Setters
        public void SetFavorite(in string projectKey, bool state)
        {
            ProjectData project = GetProject(projectKey);
            SetFavorite(in project, in state);
        }

        public void SetBuild(in string projectKey, BuildType buildType)
        {
            ProjectData project = GetProject(projectKey);
            SetBuild(in project, in buildType);
        }

        #endregion

        public string GenerateProjectMetadataString(in string projectKey, bool center = false)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return null;

            string metaStr = project.ProjectName.BBCodeColor(ColorTheme.BaseBlue)
                + $" [ v{project.VersionData} | ".BBCodeColor(ColorTheme.BaseBlue)
                + ((string)project.BuildType).BBCodeColor(project.BuildType.HTMLColor)
                + " ] ".BBCodeColor(ColorTheme.BaseBlue)
                + $"{project.Renderer}".BBCodeColor(project.Renderer.HTMLColor);

            if (project.UsesGDExt)
                metaStr += " [Uses GDExtension]".BBCodeColor(ColorTheme.HighlightBlue);

            if (project.UsesDotNet)
                metaStr += " [Uses .NET]".BBCodeColor(ColorTheme.CSharp);

            if (center)
                return $"[center]{metaStr}[/center]";
            else
                return metaStr;
        }

        public string GenerateSimpleProjectMetadataString(in string projectKey)
        {
            ProjectData project = GetProject(projectKey);
            if (project == null) return null;

            string result = $"v{project.VersionData} [{project.BuildType}]";
            if (project.UsesDotNet) result += " [.NET]";
            return result;
        }
    }
}