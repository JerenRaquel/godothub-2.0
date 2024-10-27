using System.Collections.Generic;

public partial class SettingsData
{
    public enum Type { NULL, BOOL, INT, STRING_LIST }

    public readonly struct Data
    {
        private readonly Type _type;
        private readonly long _value = 0;
        private readonly string[] _data = null;

        public bool IsNull => _type == Type.NULL;
        public bool IsArray => _type == Type.STRING_LIST;
        public Type DataType => _type;

        #region Constructors
        public Data() { _type = Type.NULL; }

        public Data(bool value)
        {
            _type = Type.BOOL;
            _value = value ? 1 : 0;
        }

        public Data(long value)
        {
            _type = Type.INT;
            _value = value;
        }

        public Data(string[] value)
        {
            _type = Type.STRING_LIST;
            _data = value;
        }

        #endregion

        #region Cast Getters
        private readonly bool AsBool()
        {
            if (_type != Type.BOOL) return false;

            return _value != 0;
        }

        private readonly long AsInt64()
        {
            if (_type != Type.INT) return -1;

            return _value;
        }

        private readonly string[] AsArray()
        {
            if (_type != Type.STRING_LIST) return [];

            return _data;
        }

        #endregion

        #region Implicit/Explicit Castors
        // Data -> Type 
        public static implicit operator bool(Data data) => data.AsBool();
        public static implicit operator long(Data data) => data.AsInt64();
        public static implicit operator int(Data data) => (int)data.AsInt64();
        public static implicit operator string[](Data data) => data.AsArray();
        public static implicit operator List<string>(Data data) => [.. data.AsArray()];

        // Type -> Data
        public static implicit operator Data(bool data) => new(data);
        public static implicit operator Data(long data) => new(data);
        public static implicit operator Data(int data) => new(data);
        public static implicit operator Data(string[] data) => new(data);

        #endregion

        #region Overloaded Functions
        public static bool operator ==(Data x, Data y)
        {
            if (x._type != y._type) return false;

            return x._type switch
            {
                Type.BOOL => x._value == y._value,
                Type.INT => x._value == y._value,
                Type.STRING_LIST => x._data == y._data,
                _ => false
            };
        }

        public static bool operator !=(Data x, Data y) => !(x == y);

        public override readonly bool Equals(object obj)
        {
            if (!(obj is Data)) return false;


            if (_type != ((Data)obj)._type) return false;

            return _type switch
            {
                Type.BOOL => _value == ((Data)obj)._value,
                Type.INT => _value == ((Data)obj)._value,
                Type.STRING_LIST => _data == ((Data)obj)._data,
                _ => false
            };
        }

        public override readonly int GetHashCode() => base.GetHashCode();

        #endregion
    }
}