using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public partial class SettingsCache : Cache
{
    private SettingsData _RAM;
    private SettingsData _ROM;

    public int Count => _RAM.Count;
    public string[] Keys => _RAM.Keys;

    #region Singleton Instance
    private static SettingsCache _instance;

    public static SettingsCache Instance => _instance;

    private SettingsCache(string userDirectory) : base(userDirectory, "/SettingsCache.gdhub") => LoadData();

    public static SettingsCache Initialize(string userDirectory)
    {
        lock (padlock)
        {
            _instance ??= new SettingsCache(userDirectory);
            return _instance;
        }
    }
    #endregion

    public override bool LoadData()
    {
        _ROM = new();

        bool fileRead = false;
        if (File.Exists(SAVE_LOCATION))
        {
            if (new FileInfo(SAVE_LOCATION).Length > 0)
            {
                using (StreamReader file = new(SAVE_LOCATION))
                {
                    string key = file.ReadLine();
                    string data = file.ReadLine();

                    while (key != null && data != null)
                    {
                        _ROM.LoadData(key, data);
                        key = file.ReadLine();
                        data = file.ReadLine();
                    }
                    fileRead = true;
                }
            }
        }

        _RAM = new(_ROM);
        return fileRead;
    }

    public override void WriteData() => ForceWrite();

    public override void ForceWrite()
    {
        _ROM.OverwriteWith(_RAM);

        using StreamWriter file = new(SAVE_LOCATION, false);

        // No Data to write -- Wipe file
        if (_ROM.Count == 0)
        {
            file.Close();
            return;
        }

        using (Dictionary<string, SettingsData.Data>.Enumerator romEnumerator = _ROM.RawData)
        {
            while (romEnumerator.MoveNext())
            {
                KeyValuePair<string, SettingsData.Data> entry = romEnumerator.Current;
                if (entry.Value.IsNull) continue;

                file.WriteLine(entry.Key);
                file.WriteLine(entry.Value.ToString());
            }
        }
        file.Close();
    }

    public void AddOrUpdate(string key, SettingsData.Data data) => _RAM.AddOrUpdate(key, data);

    public void AddEntryToDataList(string key, string entry) => _RAM.AddEntryToDataList(key, entry);

    public void RemoveEntryFromDataList(string key, string entry) => _RAM.RemoveEntryFromStringList(key, entry);

    public bool Erase(string key) => _RAM.Erase(key);

    public SettingsData.Data GetData(string key) => _RAM.GetData(key);

    public SettingsData.Data GetDataOrSetDefault(string key, SettingsData.Data defaultValue)
    {
        SettingsData.Data data = _RAM.GetData(key);
        if (data.IsNull)
        {
            _RAM.AddOrUpdate(key, defaultValue);
            return defaultValue;
        }
        return data;
    }

}