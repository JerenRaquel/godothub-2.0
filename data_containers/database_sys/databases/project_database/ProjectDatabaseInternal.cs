using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;
using Godot;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public partial class ProjectDatabase : Database<string, ProjectData>
    {
        private static readonly int[] VALID_PROJECT_CONFIG_VERSIONS = [5];

        #region Singleton Instance
        private static ProjectDatabase _instance;

        public static ProjectDatabase Instance => _instance;

        private ProjectDatabase(string userDirectory) : base(userDirectory, "/ProjectDatabase.gdhub") => LoadData();

        public static ProjectDatabase Initialize(string userDirectory)
        {
            lock (padlock)
            {
                _instance ??= new ProjectDatabase(userDirectory);
                return _instance;
            }
        }
        #endregion

        #region File Management
        public override void ForceWrite()
        {
            //* If there's no projects, no point to write
            if (_data.Count == 0) return;

            StreamWriter file = OpenWritableFile();
            foreach (KeyValuePair<string, ProjectData> entry in _data)
            {
                ProjectData project = entry.Value;

                //* Update config is possible
                ProjectData.DirtyFlag flag = project.GetConfigDirtyFlag();
                if (flag > 0) UpdateConfig(project, flag);

                //* Write to file
                string jsonData = WriteProjectToJSON(in project);
                file.WriteLine(jsonData);
            }
            file.Close();
        }

        public override bool LoadData()
        {
            FileData fileData = OpenReadableFile();
            if (!LogReadableFileAccess(in fileData)) return false;

            string entry = fileData.file.ReadLine();
            while (entry != null)
            {
                CreateProjectFromFileEntry(entry);
                entry = fileData.file.ReadLine();
            }
            return true;
        }

        public void ScanProjects(in string[] paths)
        {
            //* Cached projects that were favorited or a build was set
            List<string> cachedProjects = [];
            foreach (KeyValuePair<string, ProjectData> entry in _data)
                if (entry.Value.BuildType.Type != BuildType.FlagType.UNKNOWN ||
                    entry.Value.IsFavorited)
                    cachedProjects.Add(entry.Key);

            //* Begin scanning
            Dictionary<string, ProjectData> newProjects = [];
            foreach (string path in paths)
            {
                //* Check if path to folder is valid
                if (!Directory.Exists(path)) continue;

                //* Grab the path to each folder in this directory
                IEnumerable<string> nestedFolderPaths = Directory.EnumerateDirectories(path);
                foreach (string folderPath in nestedFolderPaths)
                {
                    string sanitizedPath = OSAPI.SanitizePath(folderPath);
                    CreateProjectFromConfig(in sanitizedPath, out ProjectData project);
                    if (project == null)
                        continue;   //! Failed to read Config

                    //* Skip Dupes
                    if (newProjects.ContainsKey(project.ProjectName)) continue;

                    //* Check if project was cached
                    if (cachedProjects.Contains(project.ProjectName))
                    {
                        ProjectData oldProject = _data[project.ProjectName];
                        if (oldProject.VersionData == project.VersionData &&
                            oldProject.Renderer == project.Renderer)
                        {
                            project.BuildType = oldProject.BuildType;
                            project.IsFavorited = oldProject.IsFavorited;
                        }
                    }

                    //* Add the project
                    newProjects.Add(project.ProjectName, project);
                }
            }
            //* Transfer the Data
            _data.Clear();
            foreach (KeyValuePair<string, ProjectData> entry in newProjects)
                _data.Add(entry.Key, entry.Value);
        }

        /// <summary>
        /// Used by the import window.
        /// </summary>
        /// <param name="path">Will contain either `.gdhub` or `project.godot`</param>
        /// <returns>ImportError Flag</returns>
        public ImportError ImportProject(string path)
        {
            string santizedPath = StripToRootDirectory(path);
            //* Read project config
            CreateProjectFromConfig(in santizedPath, out ProjectData project);
            if (project == null)
                //! Project Config couldn't be read.
                return ImportError.INVALID_CONFIG_VERSION;

            //* Skip dupes
            if (_data.ContainsKey(project.ProjectName))
                return ImportError.DUPLICATE_ENTRY;

            //* Import Project
            _data.Add(project.ProjectName, project);
            return ImportError.OK;
        }

        private void CreateProjectFromFileEntry(in string rawEntryStr)
        {
            StringReader sr = new(rawEntryStr);
            JsonTextReader reader = new(sr);

            // {
            reader.Read();
            // Flags
            bool favorited = ReadEntry(reader, false);
            bool usingDotNet = ReadEntry(reader, false);
            // Version
            string versionStr = ReadEntry(reader, "Unknown");
            // Build
            string buildStr = ReadEntry(reader, "Unknown");
            // Renderer
            string renderer = ReadEntry(reader, "Unknown");
            renderer = renderer.Replace(" Plus", "+").Replace("GL ", "");
            // Project Name
            string projectName = ReadEntry(reader, "Unknown");
            // Icon Path
            string iconPath = ReadEntry(reader, "res://icon.svg");
            // Root Path
            string rootPath = ReadEntry(reader, "");
            // project.godot folder path additions
            string pathAdditions = ReadEntry(reader, "");
            // [Project Tags]
            List<string> projectTags = ReadEntries<string>(reader);
            // [Tool Tags]
            List<string> softwareTags = ReadEntries<string>(reader);
            // }
            reader.Read();

            if (_data.ContainsKey(projectName)) return;

            TagKey[] tags = new TagKey[projectTags.Count + softwareTags.Count];
            int index = 0;
            foreach (string tag in projectTags)
            {
                tags[index] = new(tag, false);
                index++;
            }
            foreach (string tag in softwareTags)
            {
                tags[index] = new(tag, true);
                index++;
            }

            ProjectData project = new(
                projectName,
                new(iconPath),
                new(versionStr),
                new(rootPath, pathAdditions),
                (Renderer)renderer,
                (BuildType)buildStr,
                usingDotNet,
                tags
            );
            project.UpdateTimeBaseOnConfig();
            project.IsFavorited = favorited;
            _data.Add(projectName, project);
        }

        public static bool CreateNewProjectGodotConfig(in string[] tagNames,
                    in ProjectCreator.ProjectCreationData data, in string path)
        {
            ConfigFile config = new();
            config.SetValue("application", "config/name", data.Name);

            SetConfigFeatureData(
                in config,
                new(data.Version),
                in data.IsCSharp,
                (Renderer)data.Renderer
            );
            SetConfigTags(in config, in tagNames);
            return config.Save(path) == Error.Ok;
        }

        private static void CreateProjectFromConfig(in string path, out ProjectData project)
        {
            System.Tuple<ConfigFile, ProjectPathData> loadData = LocateProjectConfig(in path);

            //* Check if invalid load
            if (loadData == null)
            {
                project = null;
                return;
            }

            //* Check if valid config version
            int configVersion = (int)loadData.Item1.GetValue("", "config_version", -1);
            if (!VALID_PROJECT_CONFIG_VERSIONS.Contains(configVersion))
            {
                //! Invalid Config Version
                project = null;
                return;
            }

            LoadFromConfig(loadData.Item1, loadData.Item2, out ProjectData createdProject);
            project = createdProject;
        }

        private static void LoadFromConfig(in ConfigFile configFile,
            in ProjectPathData pathData, out ProjectData project)
        {
            //* Fetch Project Name
            string projectName = configFile.GetValue(
                "application", "config/name", ""
            ).AsString();
            if (projectName.Length == 0)
            {
                //! Failed to get project name
                project = null;
                return;
            }

            //* Fetch Icon Data
            string iconPath = configFile.GetValue(
                "application", "config/icon", IconData.GODOT_ICON_DEFAULT_PATH
            ).AsString();

            //* Fetch Feature Data
            string[] features = configFile.GetValue(
                "application", "config/tags", System.Array.Empty<string>()
            ).AsStringArray();
            string versionStr = "Unknown";
            string renderer = "Unkwown";
            bool usingDotNet = false;
            if (features.Length == 2)
            {
                versionStr = features[0];
                renderer = features[1];
            }
            else if (features.Length == 3)
            {
                versionStr = features[0];
                renderer = features[2];
                usingDotNet = features[1] == "C#";
            }

            //* Fetch Project Tags
            string[] projectTags = configFile.GetValue(
                "application", "config/tags", System.Array.Empty<string>()
            ).AsStringArray();
            TagKey[] tagKeys = new TagKey[projectTags.Length];
            for (int i = 0; i < projectTags.Length; i++)
                tagKeys[i] = new(projectTags[i], false);

            project = new(
                in projectName,
                new(iconPath),
                new(versionStr),
                in pathData,
                (Renderer)renderer,
                new(BuildType.FlagType.UNKNOWN),
                in usingDotNet,
                tagKeys
            );
            project.UpdateTimeBaseOnConfig();
        }

        private static System.Tuple<ConfigFile, ProjectPathData> LocateProjectConfig(
            in string folderPath)
        {
            ConfigFile config = new();
            string projectPath = folderPath + "/project.godot";

            //* Attempt One -- project.godot is in the root directory
            if (config.Load(projectPath) == Error.Ok)
                return new(config, new(folderPath, ""));

            //* Attempt Two -- Look for GDExt compat/.gdhub meta file
            projectPath = folderPath + ".gdhub";
            if (!File.Exists(projectPath))
                return null; //! Fail to find, no other solutions

            // Check the relative path located in the meta file
            StreamReader sr = new(projectPath);
            string relativeProjectPath = sr.ReadLine();
            sr.Close();
            if (relativeProjectPath == null || relativeProjectPath.Length == 0)
                return null;    //! Invalid meta file -- Missing location to `project.godot`

            // Use the relative path to find `project.godot`
            projectPath = folderPath + "/" + relativeProjectPath + "/project.godot";
            if (File.Exists(projectPath))
                // Check if it can be loaded
                if (config.Load(projectPath) == Error.Ok)
                    return new(config, new(folderPath, relativeProjectPath));

            // TODO: Log Error
            System.Console.WriteLine($"Couldn't locate project.godot using path: {projectPath}");
            return null;
        }

        // TODO: Uncomment code once integration tests are done -- Not updating config rn
        private static bool UpdateConfig(in ProjectData project, in ProjectData.DirtyFlag flag)
        {
            if (flag == ProjectData.DirtyFlag.NONE) return true;

            // ConfigFile configFile = new();
            // if (configFile.Load(project.PathData.ProjectGodotFile) != Error.Ok) return false;

            //* Update Features
            // if ((flag & ProjectData.DirtyFlag.FEATURE_DATA) > 0)
            //     SetConfigFeatureData(
            //         in configFile,
            //         project.VersionData,
            //         project.UsesDotNet,
            //         project.Renderer
            //     );

            //* Set Tags
            if ((flag & ProjectData.DirtyFlag.TAGS) > 0)
            {
                TagKey[] tagKeys
                    = project.GetTags(TagDatabase.TagDatabase.TagFlag.PROJECT);
                StripTags(in tagKeys, out List<string> tags);
                // SetConfigTags(in configFile, [.. tags]);
            }
            return true;
        }

        private static void SetConfigFeatureData(in ConfigFile config,
            in Version version, in bool usesDotNet, in Renderer renderer)
        {
            System.Console.WriteLine("--- Updating Feature Data ---");
            List<string> featureData = [];
            featureData.Add(version.ToString());
            if (usesDotNet) featureData.Add("C#");
            featureData.Add(renderer.ToString());
            config.SetValue("application", "config/features", featureData.ToArray());
        }

        private static void SetConfigTags(in ConfigFile config, in string[] projectTags)
        {
            System.Console.WriteLine("--- Updating Tags ---");
            if (projectTags.Length > 0)
            {
                config.SetValue("application", "config/tags", projectTags);
            }
            else if (config.HasSectionKey("application", "config/tags"))
                config.EraseSectionKey("application", "config/tags");
        }

        private static string WriteProjectToJSON(in ProjectData project)
        {
            StringWriter sw = new();
            JsonTextWriter writer = new(sw);

            // {
            writer.WriteStartObject();
            // Flags
            WriteEntry(writer, "Favorited", project.IsFavorited);
            WriteEntry(writer, "IsDotNet", project.UsesDotNet);
            // Version
            WriteEntry(writer, "Version", project.VersionData.ToString());
            // Build
            WriteEntry(writer, "Build", project.BuildType.ToString());
            // Renderer
            string renderStr = project.Renderer;
            if (project.Renderer.Type == Renderer.FlagType.COMPAT)
                renderStr = "GL " + renderStr;
            else
                renderStr = renderStr.Replace("+", " Plus");
            WriteEntry(writer, "Renderer", renderStr);
            // Project Name
            WriteEntry(writer, "Name", project.ProjectName);
            // Icon Path
            WriteEntry(writer, "IconPath", project.IconData.Path);
            // Root Path
            WriteEntry(writer, "RootPath", project.PathData.RootFolder);
            // project.godot folder path additions
            WriteEntry(writer, "ProjectPathAdditions", project.PathData.ProjectGodotFileRelativePath);
            // [Project Tags]
            StripTags(
                project.GetTags(TagDatabase.TagDatabase.TagFlag.PROJECT),
                out List<string> projectTags
            );
            WriterEntries(writer, "ProjectTags", projectTags);
            // [Tool Tags]
            StripTags(
                project.GetTags(TagDatabase.TagDatabase.TagFlag.SOFTWARE),
                out List<string> softwareTags
            );
            WriterEntries(writer, "ToolTags", softwareTags);
            // }
            writer.WriteEndObject();

            return sw.ToString();
        }

        #endregion

        #region Getters/Setters
        private ProjectData GetProject(in string projectKey)
        {
            if (_data.TryGetValue(projectKey, out ProjectData data)) return data;
            return null;
        }

        private void SetFavorite(in ProjectData project, in bool state)
        {
            if (state == project.IsFavorited) return;

            IsDirty = true;
            project.IsFavorited = state;
        }

        private void SetBuild(in ProjectData project, in BuildType buildType)
        {
            if (buildType == project.BuildType) return;

            IsDirty = true;
            project.BuildType = buildType;
        }

        private void SetRenderer(in ProjectData project, in Renderer renderer)
        {
            if (renderer == project.Renderer) return;

            IsDirty = true;
            project.SetRenderer(renderer);
        }

        private void SetVersion(in ProjectData project, in Version version)
        {
            if (version == project.VersionData) return;

            IsDirty = true;
            project.SetVersion(version);
        }

        #endregion

        private static void StripTags(in TagKey[] tagKeys, out List<string> tags)
        {
            tags = [];
            tags.Capacity = tagKeys.Length;
            foreach (TagKey tagKey in tagKeys)
                tags.Add(tagKey.TagName);
        }

        private static string StripToRootDirectory(string rawPath)
            => OSAPI.SanitizePath(rawPath)
                .Replace("/.gdhub", "")
                .Replace("/project.godot", "");
    }
}