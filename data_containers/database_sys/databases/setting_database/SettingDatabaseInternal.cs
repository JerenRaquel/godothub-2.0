using System;
using System.Collections.Generic;
using System.IO;

namespace DataContainer.DatabaseSys.Databases.SettingDatabase
{
    /// <summary>
    /// Contains
    ///     - Enums/Constants
    ///     - Overrided Functions
    ///     - Static Functions
    /// </summary>
    public partial class SettingsDatabase : Database<SettingsTag, SettingsData>
    {
        #region Singleton Instance
        private static SettingsDatabase _instance;

        public static SettingsDatabase Instance => _instance;

        private SettingsDatabase(string userDirectory)
            : base(userDirectory, "SettingsDatabase.gdhub")
                => LoadData();

        public static SettingsDatabase Initialize(string userDirectory)
        {
            lock (padlock)
            {
                _instance ??= new SettingsDatabase(userDirectory);
                return _instance;
            }
        }
        #endregion

        public override void ForceWrite()
        {
            if (_data.Count == 0) return;

            StreamWriter file = OpenWritableFile();
            foreach (KeyValuePair<SettingsTag, SettingsData> entry in _data)
            {
                if (entry.Value == null) continue;

                file.WriteLine(entry.Key);
                file.WriteLine(entry.Value.ToString());
            }
            file.Close();
        }

        public override bool LoadData()
        {
            FileData fileData = OpenReadableFile();
            if (fileData.file == null)
            {
                // TODO: Replace with Logger
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to load data from {SAVE_LOCATION}.");
                Console.WriteLine($"Error: {fileData.error}.");
                Console.ResetColor();
                return false;
            }

            string key = fileData.file.ReadLine();
            string data = fileData.file.ReadLine();
            while (key != null && data != null)
            {
                LoadRawData(key, data);
                key = fileData.file.ReadLine();
                data = fileData.file.ReadLine();
            }
            fileData.file.Close();

            foreach (KeyValuePair<SettingsTag, SettingsData> entry in _data)
                Console.WriteLine($"{entry.Key} : {entry.Value}");

            return true;
        }

        private void LoadRawData(string rawKey, string rawData)
        {
            SettingsTag tag = new(rawKey);
            switch (tag.Type)
            {
                case SettingsData.Type.NULL:
                    return;
                case SettingsData.Type.STRING_LIST:
                    string santized = rawData.Replace("[", "").Replace("]", "");
                    string[] parts = santized.Split(",", StringSplitOptions.RemoveEmptyEntries);
                    AddOrUpdate(tag, new(in parts));
                    return;
                case SettingsData.Type.BOOL:
                    AddOrUpdate(tag, new(rawData == "TRUE"));
                    return;
                case SettingsData.Type.LONG:
                    if (!long.TryParse(rawData, out long value)) return;
                    AddOrUpdate(tag, new(value));
                    return;
            }
        }
    }
}