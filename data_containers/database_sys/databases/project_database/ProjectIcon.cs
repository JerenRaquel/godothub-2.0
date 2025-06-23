using Godot;

namespace DataContainer.DatabaseSys.Databases.ProjectDatabase
{
    public readonly struct ProjectIconData(in Texture2D icon, in string path)
    {
        public readonly Texture2D Icon = icon;
        public readonly string path = path;
    }
}