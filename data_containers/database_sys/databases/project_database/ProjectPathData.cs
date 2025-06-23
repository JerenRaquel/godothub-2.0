namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public readonly struct ProjectPathData
        (in string rootFolder, in string projectGodotFile, in string gdhubMetaFile)
    {
        public readonly string RootFolder = rootFolder;
        public readonly string ProjectGodotFile = projectGodotFile;
        public readonly string GDHubMetaFile = gdhubMetaFile;
    }
}