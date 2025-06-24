namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public readonly struct ProjectPathData
        (in string rootFolder, in string projectGodotFileRelative)
    {
        public readonly string RootFolder = rootFolder;
        public readonly string ProjectGodotFileRelative = projectGodotFileRelative;

        public string ProjectGodotFileFullPath
        {
            get
            {
                if (ProjectGodotFileRelative == null || ProjectGodotFileRelative.Length == 0)
                    return RootFolder;
                return RootFolder + "/" + ProjectGodotFileRelative;
            }
        }
        public string ProjectGodotFile => ProjectGodotFileFullPath + "/project.godot";
    }
}