using System.IO;
using Newtonsoft.Json;

namespace DataContainer.DatabaseSys.Databases.TagDatabase
{
    public class TagData
    {
        public bool IsSoftware { get; private set; } = false;
        public string HTMLColor { get; private set; } = "FFFFFF";
        public TagCommand CommandData { get; private set; } = null;

        public bool IsFavorited { get; set; } = false;

        private TagData() { }

        public TagData(in bool isSoftware, in string htmlColor)
        {
            IsSoftware = isSoftware;
            HTMLColor = htmlColor;
        }

        public void SetCommandData(string path, string argStr, in bool favorited)
        {
            if (!IsSoftware) return;
            IsFavorited = favorited;
            CommandData = new(path, argStr);
        }

        public void SetCommandData(string path, string[] args, in bool favorited)
        {
            if (!IsSoftware) return;
            IsFavorited = favorited;
            CommandData = new(path, args);
        }

        public string Pack()
        {
            StringWriter sw = new();
            JsonTextWriter writer = new(sw);

            writer.WriteStartObject();
            TagDatabase.WriteEntry(writer, "color", HTMLColor);
            if (IsSoftware)
            {
                TagDatabase.WriteEntry(writer, "favorited", IsFavorited);
                TagDatabase.WriteEntry(writer, "path", CommandData.Path);
                TagDatabase.WriterEntries(writer, "args", [.. CommandData.Args]);
            }
            writer.WriteEndObject();

            return sw.ToString();
        }

        public static TagData Unpack(in string jsonStrData, bool isSoftware)
        {
            StringReader sr = new(jsonStrData);
            JsonTextReader reader = new(sr);

            reader.Read();
            string color = Cache.ReadEntry<string>(reader, null);

            TagData data = new(isSoftware, color);
            if (isSoftware)
            {
                bool favorited = Cache.ReadEntry(reader, false);
                string path = Cache.ReadEntry<string>(reader, null);
                string[] args = [.. Cache.ReadEntries<string>(reader)];

                data.SetCommandData(path, args, favorited);
            }
            reader.Read();

            return data;
        }
    }
}