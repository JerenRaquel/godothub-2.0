using DataContainer.DatabaseSys.Databases.VersionDatabase;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public partial class ProjectDatabase : Database<string, ProjectStats>
    {
        public void AddProject() { }

        public void DeleteProject(in string projectKey)
        {
            if (!_data.ContainsKey(projectKey)) return;

            _data.Remove(projectKey);
            IsDirty = true;
        }

        public bool UpdateProjectData(in string projectKey, BuildType buildType,
            Renderer renderer, in Version version)
        {
            ProjectStats project = GetProject(projectKey);
            if (project == null) return false;

            SetBuild(in project, in buildType);
            SetRenderer(in project, in renderer);
            SetVersion(in project, in version);
            return true;
        }

        public void SetFavorite(in string projectKey, bool state)
        {
            ProjectStats project = GetProject(projectKey);
            SetFavorite(in project, in state);
        }

        public void SetBuild(in string projectKey, BuildType buildType)
        {
            ProjectStats project = GetProject(projectKey);
            SetBuild(in project, in buildType);
        }

        public string GenerateProjectMetadataString(in string projectKey, bool center = false)
        {
            ProjectStats project = GetProject(projectKey);
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
    }
}