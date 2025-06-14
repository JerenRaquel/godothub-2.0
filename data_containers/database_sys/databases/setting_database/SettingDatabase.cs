namespace DataContainer.DatabaseSys.Databases.SettingDatabase
{
    public partial class SettingDatabase : Database<SettingsTag, SettingsData>
    {
        public void AddOrUpdate(in SettingsTag tag, in SettingsData data)
        {
            if (_data.ContainsKey(tag)) return;
            _data[tag] = data;
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
        }

        public bool Erase(in SettingsTag tag)
        {
            if (!_data.ContainsKey(tag)) return false;
            _data.Remove(tag);
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