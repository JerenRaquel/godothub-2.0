using System;
using System.IO;
using XMLSystem;

namespace DataContainer.DatabaseSys.Databases.TemplateDatabase
{
    public partial class TemplateDatabase : Database<string, TemplateData>
    {
        private readonly string _ASSET_DIRECTORY;
        private readonly string _ASSET_METADATA_FILE_PATH;
        private readonly string _TEMPLATE_DIRECTORY;

        #region Singleton Instance
        private static TemplateDatabase _instance;

        public static TemplateDatabase Instance => _instance;

        private TemplateDatabase(string userDirectory) : base(userDirectory, "/TemplateDatabase.gdhub")
        {
            _ASSET_DIRECTORY = userDirectory + "template_assets";
            _ASSET_METADATA_FILE_PATH = _ASSET_DIRECTORY + "/metadata.gdhub";
            _TEMPLATE_DIRECTORY = userDirectory + "templates";
            LoadData();
        }

        public static TemplateDatabase Initialize(string userDirectory)
        {
            lock (padlock)
            {
                _instance ??= new TemplateDatabase(userDirectory);
                return _instance;
            }
        }
        #endregion

        //* We don't need to save anything, it's all read from a folder
        public override void ForceWrite() { }

        public override bool LoadData()
        {
            // if (!CheckForDefaultFiles())
            //     InitializeDefaultTemplates();

            // ReadAssetFile();

            if (Directory.Exists(_TEMPLATE_DIRECTORY))
            {
                string[] files = Directory.GetFiles(_TEMPLATE_DIRECTORY);
                foreach (string filePath in files)
                {
                    TemplateData data = ReadTemplateFile(filePath);
                    Console.WriteLine(data);    // TEMP
                }
            }

            return true;
        }

        private TemplateData ReadTemplateFile(in string path)
        {
            if (!path.EndsWith(".gdhub")) return null;

            string santizedPath = OSAPI.SanitizePath(path);
            string fileName = OSAPI.ParseFileName(santizedPath);
            XMLReader.ParseResults results = XMLReader.ReadFile(in santizedPath);
            if (results.errorFlag != XMLReader.ErrorFlag.OK)
            {
                // TODO: Replace with notification system
                switch (results.errorFlag)
                {
                    case XMLReader.ErrorFlag.INVALID_FILE_PATH:
                        Console.WriteLine($"{fileName} has an invalid path @ {santizedPath}");
                        break;
                    case XMLReader.ErrorFlag.INVALID_STRUCTURE:
                        Console.WriteLine($"{fileName} is either corrupted or not XML.");
                        break;
                }
                return null;
            }

            TemplateData template
                = TemplateData.CreateFromXMLNode(results.root, in fileName);
            if (template == null) return null;

            _data.Add(template.Name, template);
            return template;
        }
    }
}