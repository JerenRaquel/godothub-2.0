namespace DataContainer
{
    public struct Renderer(Renderer.FlagType flagType)
    {
        public enum FlagType { INVALID, COMPAT, MOBILE, FORWARD }

        public FlagType Type { get; set; } = flagType;
        public readonly string HTMLColor => Type switch
        {
            FlagType.COMPAT => ColorTheme.Compat,
            FlagType.MOBILE => ColorTheme.Mobile,
            FlagType.FORWARD => ColorTheme.Forward,
            _ => ColorTheme.Unknown
        };

        public override readonly string ToString()
        {
            return Type switch
            {
                FlagType.COMPAT => "Compatibility",
                FlagType.MOBILE => "Mobile",
                FlagType.FORWARD => "Forward+",
                _ => "Unkwown"
            };
        }

        public static implicit operator string(Renderer renderer)
            => renderer.ToString();
        public static implicit operator Renderer(FlagType flagType)
            => new(flagType);

        public static explicit operator Renderer(string renderStr)
        {
            FlagType type = renderStr switch
            {
                "Compatibility" => FlagType.COMPAT,
                "Mobile" => FlagType.MOBILE,
                "Forward+" => FlagType.FORWARD,
                _ => FlagType.INVALID
            };
            return new(type);
        }
    }
}