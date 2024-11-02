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
        public SoftwareData(string colorCode, string command)
        {
            ColorCode = colorCode;
            RAWCommand = command;
            _isNull = !UpdateCommand(command);
        }

        public bool UpdateCommand(string command)
        {
            CommandParts data = SplitCommand(command);
            if (data.IsNull)
                return false;

            CommandData = data;
            return true;
        }

        private static CommandParts SplitCommand(string commandStr)
        {
            // TODO: Santized for '\ ' chars and mid spaces withing params
            string[] parts = commandStr.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return new();
            else if (parts.Length == 1)
                return new(parts[0], []);
            else
                return new(parts[0], [.. parts.Skip(1)]);
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