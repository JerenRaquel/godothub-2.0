using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public partial class SettingsCache : Cache
{
    private SettingsData _RAM;
    private SettingsData _ROM;

    public int Count => _RAM.Count;

    #region Singleton Instance
    private static SettingsCache _instance;

    public static SettingsCache Instance => _instance;

    private SettingsCache(string userDirectory) : base(userDirectory + "/SettingsCache.gdhub")
    {
        LoadData();
    }

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
        // TODO: Read from file

        _ROM = new();
        _RAM = new();
        return false;
    }

    public override void WriteData()
    {
        if (!_isDirty) return;

        ForceWrite();
    }

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

        StringWriter sw = new();
        JsonTextWriter writer = new(sw);

        // {
        writer.WriteStartObject();
        using (Dictionary<string, SettingsData.Data>.Enumerator entryEnumerator = _ROM.RawData)
        {
            while (entryEnumerator.MoveNext())
            {
                KeyValuePair<string, SettingsData.Data> entry = entryEnumerator.Current;
                switch (entry.Value.DataType)
                {
                    case SettingsData.Type.BOOL:
                        {
                            WriteEntry<bool>(writer, entry.Key, entry.Value);
                            break;
                        }
                    case SettingsData.Type.INT:
                        {
                            WriteEntry<long>(writer, entry.Key, entry.Value);
                            break;
                        }
                    case SettingsData.Type.STRING_LIST:
                        {
                            WriterEntries<string>(writer, entry.Key, entry.Value);
                            break;
                        }
                    default: break; // Skip
                }
            }
        }
        // }
        writer.WriteEndObject();

        // Write string to file
        file.WriteLine(sw.ToString());

        file.Close();
        _isDirty = false;
    }

    public void AddOrUpdate(string tag, SettingsData.Data data) => _RAM.AddOrUpdate(tag, data);

    public bool Erase(string tag) => _RAM.Erase(tag);

    public SettingsData.Data GetData(string tag) => _RAM.GetData(tag);

}