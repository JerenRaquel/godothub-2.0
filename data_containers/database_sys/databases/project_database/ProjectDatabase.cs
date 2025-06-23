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

        public bool UpdateProjectData(in string projectKey,
            VersionDatabase.VersionDatabase.BuildType buildType,
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

        public void SetBuild(in string projectKey,
            VersionDatabase.VersionDatabase.BuildType buildType)
        {
            ProjectStats project = GetProject(projectKey);
            SetBuild(in project, in buildType);
        }

        public string GenerateProjectMetadataString(in string projectKey, bool center = false)
        {

        }
    }
}