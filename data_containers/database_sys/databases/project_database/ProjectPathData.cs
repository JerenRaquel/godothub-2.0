namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    // TODO: Look into refactoring this
    public readonly struct ProjectPathData
        (in string rootFolder, in string projectGodotFileRelative)
    {
        public readonly string RootFolder = rootFolder;
        public readonly string ProjectGodotFileRelativePath = projectGodotFileRelative;

        public string ProjectGodotFileLocation
            => GetProjGodotFileLocWithAddition($"/{ProjectGodotFileRelativePath}");
        public string ProjectGodotFileLocationPretty
            => GetProjGodotFileLocWithAddition($"/[{ProjectGodotFileRelativePath}]");
        public string ProjectGodotFile => ProjectGodotFileLocation + "/project.godot";

        private string GetProjGodotFileLocWithAddition(string addition)
        {
            if (ProjectGodotFileRelativePath == null
                || ProjectGodotFileRelativePath.Length == 0)
                return RootFolder;
            return RootFolder + addition;
        }
    }
}