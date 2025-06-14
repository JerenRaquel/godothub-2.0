namespace DataContainer.DatabaseSys.Databases.SettingDatabase
{
    public partial class SettingsDatabase : Database<SettingsTag, SettingsData>
    {
        //? May want to re-think this at some point
        #region Constant SettingsTags
        public readonly static SettingsTag PROJECT_PATH_TAG_KEY = new("Project Settings/Paths/project_paths/STRING_LIST");
        public readonly static SettingsTag PROJECT_RENDERING = new("Project Settings/Defaults/rendering_device/LONG");
        public readonly static SettingsTag PROJECT_NAMING = new("Project Settings/Defaults/naming_scheme/LONG");
        public readonly static SettingsTag PROJECT_LAUNCH = new("Project Settings/Defaults/launch_behavior/LONG");

        public readonly static SettingsTag APPLICATION_ABS_PROJ_PATH = new("Application/Config/abs_proj_path/BOOL");
        public readonly static SettingsTag APPLICATION_HUB_BEHAVIOR = new("Application/Config/HUB_behavior/LONG");
        #endregion

        public SettingsTag[] Keys => [.. _data.Keys];

        public void AddOrUpdate(in SettingsTag tag, in SettingsData data)
        {
            _data[tag] = data;
            IsDirty = true;
        }

        public void AddEntryToDataList(in SettingsTag tag, in string entry)
        {
            if (!_data.ContainsKey(tag)) return;
            if (!_data[tag].IsArray) return;

            string[] data = GetData(tag);
            string[] newData = new string[data.Length + 1];
            for (int i = 0; i < data.Length; i++)
                newData[i] = data[i];

            newData[^1] = entry;
            _data[tag] = newData;
            IsDirty = true;
        }

        public void RemoveEntryFromDataList(in SettingsTag tag, in string entry)
        {
            if (!_data.ContainsKey(tag)) return;
            if (!_data[tag].IsArray) return;

            string[] data = GetData(tag);
            string[] newData = new string[data.Length - 1];
            for (int i = 0; i < data.Length; i++)
                if (data[i] != entry)
                    newData[i] = data[i];

            _data[tag] = newData;
            IsDirty = true;
        }

        public bool Erase(in SettingsTag tag)
        {
            if (!_data.ContainsKey(tag)) return false;

            _data.Remove(tag);
            IsDirty = true;
            return true;
        }

        public SettingsData GetData(in SettingsTag tag)
        {
            if (!_data.TryGetValue(tag, out SettingsData data)) return null;
            return data;
        }

        public SettingsData GetDataOrSetDefault(in SettingsTag tag, in SettingsData defaultValue = null)
        {
            SettingsData data = GetData(tag);
            if (data == null)
            {
                AddOrUpdate(tag, defaultValue);
                return defaultValue;
            }
            return data;
        }
    }
}