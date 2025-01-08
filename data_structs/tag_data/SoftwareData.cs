using System;
using System.IO;
using System.Linq;
using Godot;
using Newtonsoft.Json;

public partial class TagData
{
    public struct SoftwareData
    {
        private bool _isNull = true;

        public readonly bool IsNull => _isNull;
        public bool Favorited { get; set; } = false;
        public string ColorCode { get; set; }
        public CommandParts CommandData { get; private set; }
        public string RAWCommand { get; private set; }
        public string PrettyRawCommand { get; private set; }

        public SoftwareData() { }
        public SoftwareData(string colorCode, string path, string argStr, bool favorited)
        {
            ColorCode = colorCode;
            RAWCommand = path + " " + argStr;
            PrettyRawCommand = ParseForExecutable(path) + " " + argStr;
            Favorited = favorited;
            _isNull = !UpdateCommand(path, argStr);
        }
        public SoftwareData(string colorCode, string path, string[] args, bool favorited)
        {
            ColorCode = colorCode;
            string argString = args.Join(" ");
            RAWCommand = path + " " + argString;
            PrettyRawCommand = ParseForExecutable(path) + " " + argString;
            CommandData = new(path, args);
            Favorited = favorited;
            _isNull = false;
        }

        public bool UpdateCommand(string path, string argStr)
        {
            CommandParts data = SplitCommand(path, argStr);
            if (data.IsNull)
                return false;

            CommandData = data;
            return true;
        }

        public readonly string ToJSONString()
        {
            StringWriter sw = new();
            JsonTextWriter writer = new(sw);

            writer.WriteStartObject();
            Cache.WriteEntry(writer, "color", ColorCode);
            Cache.WriteEntry(writer, "favorited", Favorited);
            Cache.WriteEntry(writer, "path", CommandData.Command);
            Cache.WriterEntries(writer, "args", [.. CommandData.Args]);
            writer.WriteEndObject();

            return sw.ToString();
        }

        public static SoftwareData FromJSONString(string data)
        {
            StringReader sr = new(data);
            JsonTextReader reader = new(sr);

            reader.Read();
            string color = Cache.ReadEntry<string>(reader, null);
            bool favorited = Cache.ReadEntry(reader, false);
            string path = Cache.ReadEntry<string>(reader, null);
            string[] args = [.. Cache.ReadEntries<string>(reader)];
            reader.Read();

            return new SoftwareData(color, path, args, favorited);
        }

        private static CommandParts SplitCommand(string path, string argStr)
        {
            string[] parts = argStr.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            return new(path, parts);
        }

        private static string ParseForExecutable(string path) => path.Split("/").Last();
    }

    public readonly struct CommandParts
    {
        private readonly bool _isNull = true;

        public readonly string Command { get; }
        public readonly string[] Args { get; }
        public readonly string ArgString { get; }
        public readonly bool IsNull => _isNull;

        public CommandParts() { }
        public CommandParts(string command, string[] args)
        {
            Command = command;
            Args = args;
            ArgString = args.Join(" ");
            _isNull = false;
        }
    }
}