using System.Collections.Generic;
using System.IO;
using DataContainer.DatabaseSys.Databases.TagDatabase;
using DataContainer.DatabaseSys.Databases.VersionDatabase;
using Godot;
using Newtonsoft.Json;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public partial class ProjectDatabase : Database<string, ProjectStats>
    {
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
            foreach (KeyValuePair<string, ProjectStats> entry in _data)
            {
                ProjectStats project = entry.Value;

                //* Update config is possible
                ProjectStats.DirtyFlag flag = project.GetConfigDirtyFlag();
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
            renderer = renderer.Replace(" Plus", "+");
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

            ProjectStats project = new(
                projectName,
                new(iconPath),
                new(versionStr),
                new(rootPath, pathAdditions, ""),   //? Does the last param need to be saved?
                (Renderer)renderer,
                (BuildType)buildStr,
                usingDotNet,
                tags
            );
            _data.Add(projectName, project);
        }

        // TODO: Uncomment code once integration tests are done -- Not updating config rn
        private static bool UpdateConfig(in ProjectStats project, in ProjectStats.DirtyFlag flag)
        {
            if (flag == ProjectStats.DirtyFlag.NONE) return true;

            // ConfigFile configFile = new();
            // if (configFile.Load(project.PathData.ProjectGodotFile) != Error.Ok) return false;

            //* Update Features
            if ((flag & ProjectStats.DirtyFlag.FEATURE_DATA) > 0)
            {
                System.Console.WriteLine("--- Updating Feature Data ---");
                // List<string> featureData = [];
                // featureData.Add(project.VersionData.ToString());
                // if (project.UsesDotNet) featureData.Add("C#");
                // featureData.Add(project.Renderer.ToString());
                // configFile.SetValue("application", "config/features", featureData.ToArray());
            }

            //* Set Tags
            if ((flag & ProjectStats.DirtyFlag.TAGS) > 0)
            {
                System.Console.WriteLine("--- Updating Tags ---");
                // TagKey[] tagKeys = project.GetTags(TagDatabase.TagDatabase.TagFlag.PROJECT);
                // if (tagKeys.Length > 0)
                // {
                //     StripTags(in tagKeys, out List<string> strippedTags);
                //     configFile.SetValue("application", "config/tags", strippedTags.ToArray());
                // }
                // else if (configFile.HasSectionKey("application", "config/tags"))
                //     configFile.EraseSectionKey("application", "config/tags");
            }

            return true;
        }

        private static string WriteProjectToJSON(in ProjectStats project)
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
            renderStr = renderStr.Replace("+", " Plus");
            WriteEntry(writer, "Renderer", renderStr);
            // Project Name
            WriteEntry(writer, "Name", project.ProjectName);
            // Icon Path
            WriteEntry(writer, "IconPath", project.IconData.Path);
            // Root Path
            WriteEntry(writer, "RootPath", project.PathData.RootFolder);
            // project.godot folder path additions
            WriteEntry(writer, "ProjectPathAdditions", project.PathData.ProjectGodotFile);
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
        private ProjectStats GetProject(in string projectKey)
        {
            if (_data.TryGetValue(projectKey, out ProjectStats data)) return data;
            return null;
        }

        private void SetFavorite(in ProjectStats project, in bool state)
        {
            if (state == project.IsFavorited) return;

            IsDirty = true;
            project.IsFavorited = state;
        }

        private void SetBuild(in ProjectStats project, in BuildType buildType)
        {
            if (buildType == project.BuildType) return;

            IsDirty = true;
            project.BuildType = buildType;
        }

        private void SetRenderer(in ProjectStats project, in Renderer renderer)
        {
            if (renderer == project.Renderer) return;

            IsDirty = true;
            project.SetRenderer(renderer);
        }

        private void SetVersion(in ProjectStats project, in Version version)
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

    }
}