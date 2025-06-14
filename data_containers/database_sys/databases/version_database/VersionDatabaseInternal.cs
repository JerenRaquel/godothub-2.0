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
        public enum BuildType { UNKNOWN, STABLE, RELEASE_CANDIDATE, BETA, DEV }

        public static readonly ReverseComparer reverseComparer = new();
        // https://learn.microsoft.com/en-us/dotnet/api/system.array.sort?view=net-8.0
        public class ReverseComparer : IComparer
        {
            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer.Compare(object x, object y)
            {
                string lhs = (string)(VersionKey)x;
                string rhs = (string)(VersionKey)y;
                return new CaseInsensitiveComparer().Compare(rhs, lhs);
            }
        }

        #region Singleton Instance
        private static VersionDatabase _instance;

        public static VersionDatabase Instance => _instance;

        private VersionDatabase(string userDirectory)
            : base(userDirectory, "VersionCache - Copy.gdhub")
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
            FileData fileData = OpenFile();
            if (fileData.file == null)
            {
                // Replace with Logger
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to load data from {SAVE_LOCATION}.");
                Console.WriteLine($"Error: {fileData.error}.");
                Console.ResetColor();
                return false;
            }

            string key = fileData.file.ReadLine();
            string path = fileData.file.ReadLine();
            while (key != null && path != null)
            {
                Console.WriteLine($"{key} : {path}");

                VersionKey versionKey = (VersionKey)key;
                AddVersion(versionKey, path);

                key = fileData.file.ReadLine();
                path = fileData.file.ReadLine();
            }
            fileData.file.Close();
            return true;
        }

        public override void WriteData()
        {
            if (!IsDirty) return;
            ForceWrite();
        }

        public override void ForceWrite()
        {
            using StreamWriter file = new(SAVE_LOCATION);
            file.WriteLine($"Version: {READABLE_VERSION}");
            foreach (KeyValuePair<VersionKey, string> pair in _data)
            {
                file.WriteLine((string)pair.Key);
                file.WriteLine(pair.Value);
            }
        }

        public static string BuildEnumToString(BuildType type)
        {
            return type switch
            {
                BuildType.STABLE => "Stable",
                BuildType.RELEASE_CANDIDATE => "Release Candidate",
                BuildType.BETA => "Beta",
                BuildType.DEV => "Dev",
                _ => "Unknown"
            };
        }

        public static BuildType StringToBuildEnum(string str)
        {
            return str switch
            {
                "Stable" => BuildType.STABLE,
                "Release Candidate" => BuildType.RELEASE_CANDIDATE,
                "Beta" => BuildType.BETA,
                "Dev" => BuildType.DEV,
                _ => BuildType.UNKNOWN
            };
        }
    }
}