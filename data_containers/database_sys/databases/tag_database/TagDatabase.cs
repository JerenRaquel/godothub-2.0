using System.Collections.Generic;

namespace DataContainer.DatabaseSys.Databases.TagDatabase
{
    public partial class TagDatabase : Database<TagKey, TagData>
    {
        public void AddOrUpdate(in TagKey key, in TagData data)
        {
            //? Might want to check on file load too?
            if (!CheckValidHtmlColor(data.HTMLColor)) return;
            if (_data.TryAdd(key, data)) return;

            _data[key] = data;
            IsDirty = true;
        }

        public void UpdateFavoriteState(in TagKey key, in bool state)
        {
            if (!_data.TryGetValue(key, out TagData data)) return;
            data.IsFavorited = state;
            IsDirty = true;
        }

        public TagCommand GetCommandData(in TagKey key)
        {
            if (!_data.TryGetValue(key, out TagData data)) return null;
            return data.CommandData;
        }

        public TagCommand GetExecutableCommand(in TagKey key, in string projectName)
        {
            if (!_data.TryGetValue(key, out TagData data)) return null;

            string santizedName = projectName;
            // An empty project name is still null
            if (projectName != null && projectName.Length == 0)
                santizedName = null;
            return MacroHandler.SubsituteMacros(data.CommandData, in santizedName);
        }

        public string GetArgsAsString(in TagKey key)
        {
            if (!_data.TryGetValue(key, out TagData data)) return null;
            return data.CommandData.ArgString;
        }

        public string GetPath(in TagKey key)
        {
            if (!_data.TryGetValue(key, out TagData data)) return null;
            return data.CommandData.Path;
        }

        public string GetHTMLColor(in TagKey key, in string defaultValue = "FFFFFF")
        {
            if (_data.TryGetValue(key, out TagData value))
                return value.HTMLColor;
            return defaultValue;
        }

        public TagKey[] GetFavoritedSoftwareTags()
        {
            List<TagKey> results = [];
            foreach (KeyValuePair<TagKey, TagData> entry in _data)
            {
                if (!entry.Value.IsFavorited) continue;

                results.Add(entry.Key);
            }

            return [.. results];
        }

        public TagKey[] GetSoftwareTags()
        {
            List<TagKey> results = [];
            foreach (KeyValuePair<TagKey, TagData> entry in _data)
            {
                if (!entry.Key.IsSoftware) continue;

                results.Add(entry.Key);
            }

            return [.. results];
        }

        public TagKey[] GetProjectTags()
        {
            List<TagKey> results = [];
            foreach (KeyValuePair<TagKey, TagData> entry in _data)
            {
                if (entry.Key.IsSoftware) continue;

                results.Add(entry.Key);
            }

            return [.. results];
        }

        public bool IsFavorited(in TagKey key)
        {
            if (_data.TryGetValue(key, out TagData value))
                return value.IsFavorited;
            return false;
        }

        public void SetFavorited(in TagKey key, in bool state)
        {
            if (!_data.ContainsKey(key)) return;
            _data[key].IsFavorited = state;
        }

    }
}