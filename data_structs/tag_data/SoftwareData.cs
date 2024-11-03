using System;
using System.Linq;

public partial class TagData
{
    public struct SoftwareData
    {
        private bool _isNull = true;

        public readonly bool IsNull => _isNull;
        public string ColorCode { get; set; }
        public CommandParts CommandData { get; private set; }
        public string RAWCommand { get; private set; }

        public SoftwareData() { }
        public SoftwareData(string colorCode, string path, string argStr)
        {
            ColorCode = colorCode;
            RAWCommand = path + " " + argStr;
            _isNull = !UpdateCommand(path, argStr);
        }

        public bool UpdateCommand(string path, string argStr)
        {
            CommandParts data = SplitCommand(path, argStr);
            if (data.IsNull)
                return false;

            CommandData = data;
            return true;
        }

        private static CommandParts SplitCommand(string path, string argStr)
        {
            // TODO: Santized for '\ ' chars and mid spaces withing params
            string[] parts = argStr.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            return new(path, parts);
        }
    }

    public readonly struct CommandParts
    {
        private readonly bool _isNull = true;

        public readonly string Command { get; }
        public readonly string[] Args { get; }
        public readonly bool IsNull => _isNull;

        public CommandParts() { }
        public CommandParts(string command, string[] args)
        {
            Command = command;
            Args = args;
            _isNull = false;
        }
    }
}