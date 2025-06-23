using System.Collections.Generic;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public class ProjectData
    {
        private List<TagKey> _tags = [];

        public Version version;
        public Renderer renderer;
        public bool usesDotNet;
        public bool isFavorited;
        public BuildType buildType;

        public ProjectPathData PathData { get; private set; }

        public long TagCount => _tags.Count;
        public bool UsesGDExt
            => PathData.GDHubMetaFile != null && PathData.GDHubMetaFile.Length > 0;

        private ProjectData() { }

        public ProjectData(in ProjectPathData pathData, in Version version,
            in BuildType buildType, in Renderer renderer, in bool usesDotNet,
            in bool isFavorited)
        {
            PathData = pathData;
            this.version = version;
            this.buildType = buildType;
            this.renderer = renderer;
            this.usesDotNet = usesDotNet;
            this.isFavorited = isFavorited;
        }

        public TagKey[] GetSoftwareTags()
        {
            if (_tags.Count == 0) return [];

            List<TagKey> keys = [];
            foreach (TagKey key in _tags)
                if (key.IsSoftware) keys.Add(key);
            return [.. keys];
        }

        public TagKey[] GetProjectTags()
        {
            if (_tags.Count == 0) return [];

            List<TagKey> keys = [];
            foreach (TagKey key in _tags)
                if (!key.IsSoftware) keys.Add(key);
            return [.. keys];
        }

        public void AddTag(in TagKey key)
        {
            if (_tags.Contains(key)) return;
            _tags.Add(key);
        }

        public bool RemoveTag(in TagKey key)
        {
            if (!_tags.Contains(key)) return false;
            _tags.Remove(key);
            return true;
        }

        public ProjectData Copy()
        {
            ProjectData copy
                = new(PathData, version, buildType, renderer, usesDotNet, isFavorited);
            foreach (TagKey key in _tags)
                copy.AddTag(key);
            return copy;
        }
    }
}