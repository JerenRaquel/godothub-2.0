using DataContainer.DatabaseSys.Databases.TagDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public class ProjectStats
    {
        public enum DirtyFlag
        {
            NONE = 0,
            FEATURE_DATA = 0b01,
            TAGS = 0b10,
        }

        private ProjectData _loadedData = null;
        private ProjectData _modifiedData = null;

        public string ProjectName { get; private set; }
        public IconData IconData { get; private set; }
        public bool IsFavorited { get; set; } = false;
        public BuildType BuildType { get; set; }
        public System.DateTime LastEdited { get; private set; }

        public Version VersionData => GetProjectData().version;
        public Renderer Renderer => GetProjectData().renderer;
        public TagKey[] SoftwareTagKeys => GetProjectData().GetSoftwareTags();
        public TagKey[] ProjectTagKeys => GetProjectData().GetProjectTags();
        public bool HasTags => GetProjectData().TagCount > 0;
        public bool UsesDotNet => GetProjectData().usesDotNet;
        public bool UsesGDExt => GetProjectData().UsesGDExt;
        public ProjectPathData PathData => GetProjectData().PathData;

        private ProjectStats() { }

        public ProjectStats(in string projectName, in IconData iconData,
            in Version version, in ProjectPathData pathData, in Renderer renderer,
            in BuildType buildType, in bool isDotNet, in TagKey[] tagKeys)
        {
            ProjectName = projectName;
            IconData = iconData;
            BuildType = buildType;
            _loadedData = new(pathData, version, renderer, isDotNet);
            foreach (TagKey key in tagKeys)
                _loadedData.AddTag(key);
        }

        public void UpdateTimeAccessed() => LastEdited = System.DateTime.Now;

        public DirtyFlag GetConfigDirtyFlag()
        {
            if (_modifiedData == null) return DirtyFlag.NONE;

            DirtyFlag flag = DirtyFlag.NONE;
            if (_loadedData.HasDifferentFeatureData(_modifiedData))
                flag = DirtyFlag.FEATURE_DATA;
            if (_loadedData.HasDifferentTags(_modifiedData))
                flag |= DirtyFlag.TAGS;
            return flag;
        }

        public TagKey[] GetTags(in TagDatabase.TagDatabase.TagFlag flag)
            => GetProjectData().GetTags(flag);

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

        private ProjectData GetProjectData()
        {
            if (_modifiedData != null) return _modifiedData;
            return _loadedData;
        }
    }
}