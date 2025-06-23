using DataContainer.DatabaseSys.Databases.VersionDatabase;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public partial class ProjectDatabase : Database<string, ProjectStats>
    {
        #region Singleton Instance
        private static ProjectDatabase _instance;

        public static ProjectDatabase Instance => _instance;

        private ProjectDatabase(string userDirectory) : base(userDirectory, "/ProjectDatabase.gdhub") => LoadData();

        public static ProjectDatabase Initialize(string userDirectory)
        {
            lock (padlock)
            {
                _instance ??= new ProjectDatabase(userDirectory);
                return _instance;
            }
        }
        #endregion

        public override void ForceWrite()
        {
            throw new System.NotImplementedException();
        }

        public override bool LoadData()
        {
            throw new System.NotImplementedException();
        }

        private ProjectStats GetProject(in string projectKey)
        {
            if (_data.TryGetValue(projectKey, out ProjectStats data)) return data;
            return null;
        }

        private void SetFavorite(in ProjectStats project, in bool state)
        {
            if (state == project.IsFavorited) return;

            IsDirty = true;
            project.SetFavorite(state);
        }

        private void SetBuild(in ProjectStats project, in BuildType buildType)
        {
            if (buildType == project.BuildType) return;

            IsDirty = true;
            project.SetBuild(buildType);
        }

        private void SetRenderer(in ProjectStats project, in Renderer renderer)
        {
            if (renderer == project.Renderer) return;

            IsDirty = true;
            project.SetRenderer(renderer);
        }

        private void SetVersion(in ProjectStats project, in Version version)
        {
            if (version == project.VersionData) return;

            IsDirty = true;
            project.SetVersion(version);
        }
    }
}