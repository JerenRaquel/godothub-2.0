using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace DataContainer.DatabaseSys.Databases.VersionDatabase
{
    /// <summary>
    /// Contains
    ///     - Enums/Constants
    ///     - Overrided Functions
    ///     - Static Functions
    /// </summary>
    public partial class VersionDatabase : Database<VersionKey, string>
    {
        #region Compare Class
        public static readonly ReverseComparer reverseComparer = new();
        // https://learn.microsoft.com/en-us/dotnet/api/system.array.sort?view=net-8.0
        public class ReverseComparer : IComparer
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(object x, object y)
            {
                string lhs, rhs;
                if (x is VersionKey)
                {
                    lhs = (string)(VersionKey)x;
                    rhs = (string)(VersionKey)y;
                }
                else
                {
                    lhs = (string)x;
                    rhs = (string)y;
                }
                return new CaseInsensitiveComparer().Compare(rhs, lhs);
            }
        }
        #endregion

        #region Singleton Instance
        private static VersionDatabase _instance;

        public static VersionDatabase Instance => _instance;

        private VersionDatabase(string userDirectory)
            : base(userDirectory, "VersionDatabase.gdhub")
                => LoadData();

        public static VersionDatabase Initialize(string userDirectory)
        {
            lock (padlock)
            {
                _instance ??= new VersionDatabase(userDirectory);
                return _instance;
            }
        }
        #endregion

        public override bool LoadData()
        {
            FileData fileData = OpenReadableFile();
            if (!LogReadableFileAccess(in fileData)) return false;

            string key = fileData.file.ReadLine();
            string path = fileData.file.ReadLine();
            while (key != null && path != null)
            {
                VersionKey versionKey = (VersionKey)key;
                AddVersion(versionKey, path);

                key = fileData.file.ReadLine();
                path = fileData.file.ReadLine();
            }
            fileData.file.Close();
            return true;
        }

        public override void ForceWrite()
        {
            StreamWriter file = OpenWritableFile();
            foreach (KeyValuePair<VersionKey, string> pair in _data)
            {
                file.WriteLine((string)pair.Key);
                file.WriteLine(pair.Value);
            }
            file.Close();
        }
    }
}