using DataContainer.DatabaseSys.Databases.TagDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public class ProjectStats
    {
        private ProjectData _loadedData = null;
        private ProjectData _modifiedData = null;

        public string ProjectName { get; private set; }
        public IconData IconData { get; private set; }
        public System.DateTime LastEdited { get; private set; }

        public Version VersionData => GetProjectData().version;
        public BuildType BuildType => GetProjectData().buildType;
        public Renderer Renderer => GetProjectData().renderer;
        public TagKey[] SoftwareTagKeys => GetProjectData().GetSoftwareTags();
        public TagKey[] ProjectTagKeys => GetProjectData().GetProjectTags();
        public bool HasTags => GetProjectData().TagCount > 0;
        public bool UsesDotNet => GetProjectData().usesDotNet;
        public bool UsesGDExt => GetProjectData().UsesGDExt;
        public ProjectPathData PathData => GetProjectData().PathData;
        public bool IsFavorited => GetProjectData().isFavorited;

        private ProjectStats() { }

        public ProjectStats(in string projectName, in IconData iconData,
            in Version version, in ProjectPathData pathData, in Renderer renderer,
            in BuildType buildType, in bool isDotNet, in TagKey[] tagKeys)
        {
            ProjectName = projectName;
            IconData = iconData;
            _loadedData = new(pathData, version, buildType, renderer, isDotNet, false);
            foreach (TagKey key in tagKeys)
                _loadedData.AddTag(key);
        }

        public void UpdateTimeAccessed() => LastEdited = System.DateTime.Now;

        public TagKey[] GetTags(in TagDatabase.TagDatabase.TagFlag flag)
            => GetProjectData().GetTags(flag);

        public void SetBuild(in BuildType buildType)
        {
            _modifiedData ??= _loadedData.Copy();
            _modifiedData.buildType = buildType;
        }

        public void SetRenderer(in Renderer renderer)
        {
            _modifiedData ??= _loadedData.Copy();
            _modifiedData.renderer = renderer;
        }

        public void SetVersion(in Version version)
        {
            _modifiedData ??= _loadedData.Copy();
            _modifiedData.version = version;
        }

        public void SetFavorite(in bool state)
        {
            _modifiedData ??= _loadedData.Copy();
            _modifiedData.isFavorited = state;
        }

        private ProjectData GetProjectData()
        {
            if (_modifiedData != null) return _modifiedData;
            return _loadedData;
        }
    }
}