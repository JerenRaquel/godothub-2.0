namespace DataContainer.DatabaseSys.Databases.TagDatabase
{
    public class TagCommand
    {
        public string FullCommand { get; private set; }
        public string PrettyCommand { get; private set; }
        public string Command => Path;

        public string[] Args { get; private set; }
        public string ArgString { get; private set; }
        public string Path { get; private set; }

        private TagCommand() { }

        public TagCommand(in string path, in string argStr)
        {
            ParseArgsFromStr(in argStr, out string[] args);

            Args = args;
            ArgString = argStr;
            SetCommandMembers(path);
        }

        public TagCommand(in string path, in string[] args)
        {
            Args = args;
            ArgString = string.Join(' ', args);
            SetCommandMembers(path);
        }

        private void SetCommandMembers(in string path)
        {
            Path = path;
            FullCommand = $"{path} {ArgString}";
            PrettyCommand = $"{ParseForExecutable(path)} {ArgString}";
        }

        private static string ParseForExecutable(string path) => path.Split("/")[^1];

        // TODO: Rewrite this to not be naive
        private static void ParseArgsFromStr(in string argString, out string[] args)
        {
            args = argString.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        }
    }
}