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

        public TagData(in bool isSoftware, in string htmlColor, in bool favorited)
        {
            IsSoftware = isSoftware;
            HTMLColor = htmlColor;
            IsFavorited = favorited;
        }

        public void SetCommandData(string path, string argStr)
        {
            if (!IsSoftware) return;
            CommandData = new(path, argStr);
        }

        public void SetCommandData(string path, string[] args)
        {
            if (!IsSoftware) return;
            CommandData = new(path, args);
        }

        public string Pack()
        {
            StringWriter sw = new();
            JsonTextWriter writer = new(sw);

            writer.WriteStartObject();
            TagDatabase.WriteEntry(writer, "color", HTMLColor);
            TagDatabase.WriteEntry(writer, "favorited", IsFavorited);
            if (IsSoftware)
            {
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
            bool favorited = Cache.ReadEntry(reader, false);

            TagData data = new(isSoftware, color, favorited);
            if (isSoftware)
            {
                string path = Cache.ReadEntry<string>(reader, null);
                string[] args = [.. Cache.ReadEntries<string>(reader)];

                data.SetCommandData(path, args);
            }
            reader.Read();

            return data;
        }
    }
}