using System.Collections.Generic;

namespace DataContainer.DatabaseSys.Databases.SettingDatabase
{
    public class SettingsData
    {
        public enum Type { NULL, BOOL, LONG, STRING_LIST }

        private readonly Type _type;
        private readonly long _value;
        private readonly string[] _data;

        public bool IsArray => _type == Type.STRING_LIST;
        public Type DataType => _type;
        public string DataTypeStr => SettingsTag.TypeToStr(_type);

        #region Constructors 
        private SettingsData() { }

        public SettingsData(bool value)
        {
            _type = Type.BOOL;
            _value = value ? 1 : 0;
        }

        public SettingsData(long value)
        {
            _type = Type.LONG;
            _value = value;
        }

        public SettingsData(in string[] values)
        {
            _type = Type.STRING_LIST;
            _data = values;
        }
        #endregion

        #region Cast Getters
        private bool AsBool()
        {
            if (_type != Type.BOOL) return false;

            return _value != 0;
        }

        private long AsInt()
        {
            if (_type != Type.LONG) return -1;

            return _value;
        }

        private string[] AsArray()
        {
            if (_type != Type.STRING_LIST) return [];

            return _data;
        }

        #endregion

        #region Implicit/Explicit Castors
        // SettingsData -> Type 
        public static implicit operator bool(SettingsData data) => data.AsBool();
        public static implicit operator long(SettingsData data) => data.AsInt();
        public static implicit operator int(SettingsData data) => (int)data.AsInt();
        public static implicit operator string[](SettingsData data) => data.AsArray();
        public static implicit operator List<string>(SettingsData data) => [.. data.AsArray()];

        // Type -> SettingsData
        public static implicit operator SettingsData(bool data) => new(data);
        public static implicit operator SettingsData(long data) => new(data);
        public static implicit operator SettingsData(int data) => new(data);
        public static implicit operator SettingsData(string[] data) => new(data);

        #endregion

        #region Overloaded Functions
        public static bool operator ==(SettingsData x, SettingsData y)
        {
            if (x._type != y._type) return false;

            return x._type switch
            {
                Type.BOOL => x._value == y._value,
                Type.LONG => x._value == y._value,
                Type.STRING_LIST => x._data == y._data,
                _ => false
            };
        }

        public static bool operator !=(SettingsData x, SettingsData y) => !(x == y);

        public override bool Equals(object obj)
        {
            if (!(obj is SettingsData)) return false;


            if (_type != ((SettingsData)obj)._type) return false;

            return _type switch
            {
                Type.BOOL => _value == ((SettingsData)obj)._value,
                Type.LONG => _value == ((SettingsData)obj)._value,
                Type.STRING_LIST => _data == ((SettingsData)obj)._data,
                _ => false
            };
        }

        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            return _type switch
            {
                Type.BOOL => _value != 0 ? "TRUE" : "FALSE",
                Type.LONG => _value.ToString(),
                Type.STRING_LIST => ListDataToString(),
                _ => ""
            };
        }

        private string ListDataToString()
        {
            string result = "[";
            for (int i = 0; i < _data.Length; i++)
            {
                result += _data[i];
                if (i < _data.Length - 1) result += ",";
            }
            return result += "]";
        }

        #endregion
    }
}