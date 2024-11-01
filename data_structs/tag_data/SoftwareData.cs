public partial class TagData
{
    public struct Data
    {
        private bool _isNull = true;

        public string ColorCode { get; set; }
        public string Path { get; set; }
        public string Command { get; set; }
        public readonly bool IsNull => _isNull;

        public Data() => _isNull = true;

        public Data(string colorCode, string path, string command)
        {
            ColorCode = colorCode;
            Path = path;
            Command = command;
            _isNull = false;
        }

        public Data(string colorCode, string path)
        {
            ColorCode = colorCode;
            Path = path;
            _isNull = false;
        }
    }
}