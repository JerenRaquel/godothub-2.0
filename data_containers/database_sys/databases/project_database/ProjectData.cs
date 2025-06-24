using System.Collections.Generic;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public class ProjectData
    {
        //* These are the 
        private HashSet<TagKey> _projectTags = [];
        private HashSet<TagKey> _softwareTags = [];
        public Version version;
        public Renderer renderer;
        public bool usesDotNet;

        public ProjectPathData PathData { get; private set; }

        public long TagCount => _projectTags.Count + _softwareTags.Count;
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

        public void AddTag(in TagKey tagKey)
        {
            if (tagKey.IsSoftware)
                _softwareTags.Add(tagKey);
            else
                _projectTags.Add(tagKey);
        }

        public bool RemoveTag(in TagKey tagKey)
        {
            if (tagKey.IsSoftware) return _softwareTags.Remove(tagKey);
            return _projectTags.Remove(tagKey);
        }

        public bool HasTag(in TagKey tagKey)
        {
            if (tagKey.IsSoftware) return _softwareTags.Contains(tagKey);
            return _projectTags.Contains(tagKey);
        }

        public TagKey[] GetTags(in TagDatabase.TagDatabase.TagFlag tagFlag)
        {
            if (TagCount == 0) return [];

            switch (tagFlag)
            {
                case TagDatabase.TagDatabase.TagFlag.PROJECT:
                    return [.. _projectTags];

                case TagDatabase.TagDatabase.TagFlag.SOFTWARE:
                    return [.. _softwareTags];

                case TagDatabase.TagDatabase.TagFlag.ANY:
                    List<TagKey> results = [];
                    results.AddRange(_projectTags);
                    results.AddRange(_softwareTags);
                    return [.. results];

                default:
                    return [];
            }
        }

        public ProjectData Copy()
        {
            ProjectData copy = new(PathData, version, renderer, usesDotNet);
            foreach (TagKey key in _projectTags)
                copy.AddTag(key);
            foreach (TagKey key in _softwareTags)
                copy.AddTag(key);
            return copy;
        }

        public bool HasDifferentTags(in ProjectData other)
        {
            if (TagCount != other.TagCount) return true;
            foreach (TagKey tagKey in _projectTags)
                if (!other._projectTags.Contains(tagKey)) return true;
            foreach (TagKey tagKey in _softwareTags)
                if (!other._softwareTags.Contains(tagKey)) return true;
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