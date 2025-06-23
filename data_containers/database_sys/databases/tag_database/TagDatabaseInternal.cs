using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DataContainer.DatabaseSys.Databases.TagDatabase
{
    /// <summary>
    /// Contains
    ///     - Enums/Constants
    ///     - Overrided Functions
    ///     - Static Functions
    /// </summary>
    public partial class TagDatabase : Database<TagKey, TagData>
    {
        public enum TagFlag
        {
            PROJECT = 0b01,
            SOFTWARE = 0b10,
            ANY = PROJECT | SOFTWARE
        }

        #region Singleton Instance
        private static TagDatabase _instance;

        public static TagDatabase Instance => _instance;

        private TagDatabase(string userDirectory)
            : base(userDirectory, "TagDatabase.gdhub")
                => LoadData();

        public static TagDatabase Initialize(string userDirectory)
        {
            lock (padlock)
            {
                _instance ??= new TagDatabase(userDirectory);
                return _instance;
            }
        }
        #endregion

        public override void ForceWrite()
        {
            StreamWriter file = OpenWritableFile();

            // Check if there's any tags to write
            // This will clear the file
            if (_data.Count == 0)
            {
                file.Close();
                return;
            }

            foreach (KeyValuePair<TagKey, TagData> entry in _data)
            {
                file.WriteLine(entry.Key);
                file.WriteLine(entry.Value.Pack());
            }
            file.Close();
        }

        public override bool LoadData()
        {
            FileData fileData = OpenReadableFile();
            if (!LogReadableFileAccess(in fileData)) return false;

            string key = fileData.file.ReadLine();
            string data = fileData.file.ReadLine();

            while (key != null && data != null)
            {
                LoadDataFromDisk(key, data);
                key = fileData.file.ReadLine();
                data = fileData.file.ReadLine();
            }
            fileData.file.Close();
            return true;
        }

        private void LoadDataFromDisk(in string rawTagKey, in string rawData)
        {
            TagKey? key = TagKey.ParseRawKey(rawTagKey);
            if (key == null) return;

            //? Should I just make the struct a class to avoid the explict cast
            //? it's safe to pass because of the check above... ¯\_(ツ)_/¯
            TagData data = TagData.Unpack(in rawData, (bool)(key?.IsSoftware));
            AddOrUpdate((TagKey)key, in data);
        }

        #region Color Regex
        // https://stackoverflow.com/a/13035186 -- Altered to store Regex for multiple use
        private static bool CheckValidHtmlColor(string inputColor)
        {
            if (HTMLColorRegex().Match(inputColor).Success) return true;
            if (HTMLColorAlphaRegex().Match(inputColor).Success) return true;

            return false;
        }
        //regex from http://stackoverflow.com/a/1636354/2343 -- Altered to remove '#'
        [GeneratedRegex("^(?:[0-9a-fA-F]{3}){1,2}$")]
        private static partial Regex HTMLColorRegex();

        [GeneratedRegex("^(?:[0-9a-fA-F]{4}){1,2}$")]
        private static partial Regex HTMLColorAlphaRegex();

        #endregion
    }
}