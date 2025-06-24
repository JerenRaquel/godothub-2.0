using System.Collections.Generic;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public class ProjectData
    {
        //* These are the 
        private List<TagKey> _tags = [];
        public Version version;
        public Renderer renderer;
        public bool usesDotNet;

        public ProjectPathData PathData { get; private set; }

        public long TagCount => _tags.Count;
        public bool UsesGDExt
            => PathData.ProjectGodotFile != null && PathData.ProjectGodotFile.Length > 0;

        private ProjectData() { }

        public ProjectData(in ProjectPathData pathData, in Version version,
            in Renderer renderer, in bool usesDotNet)
        {
            PathData = pathData;
            this.version = version;
            this.renderer = renderer;
            this.usesDotNet = usesDotNet;
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

        public TagKey[] GetTags(in TagDatabase.TagDatabase.TagFlag tagFlag)
        {
            if (_tags.Count == 0) return [];

            List<TagKey> results = [];
            foreach (TagKey key in _tags)
            {
                switch (tagFlag)
                {
                    case TagDatabase.TagDatabase.TagFlag.PROJECT:
                        if (key.IsSoftware) continue;
                        results.Add(key);
                        break;

                    case TagDatabase.TagDatabase.TagFlag.SOFTWARE:
                        if (!key.IsSoftware) continue;
                        results.Add(key);
                        break;

                    case TagDatabase.TagDatabase.TagFlag.ANY:
                        results.Add(key);
                        break;

                    default:
                        continue;
                }
            }
            return [.. results];
        }

        public ProjectData Copy()
        {
            ProjectData copy = new(PathData, version, renderer, usesDotNet);
            foreach (TagKey key in _tags)
                copy.AddTag(key);
            return copy;
        }

        public bool HasDifferentTags(in ProjectData other)
        {
            if (_tags.Count != other._tags.Count) return true;
            foreach (TagKey tagKey in _tags)
                if (!other._tags.Contains(tagKey)) return true;
            return false;
        }

        public bool HasDifferentFeatureData(in ProjectData other)
        {
            if (other.usesDotNet != usesDotNet) return true;
            if (other.renderer != renderer) return true;
            if (other.version != version) return true;

            return false;
        }
    }
}