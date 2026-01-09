namespace NMapper.TestData
{
    public class Source
    {
        public Source1 Inner { get; set; } = null!;
    }

    public class Source1
    {
        public Source2 Inner { get; set; } = null!;
    }

    public class Source2
    {
        public Source3 Inner { get; set; } = null!;
    }

    public class Source3
    {
        public Source4 Inner { get; set; } = null!;
    }

    public class Source4
    {
        public Source5 Inner { get; set; } = null!;
    }

    public class Source5
    {
        public Source6 Inner { get; set; } = null!;
    }

    public class Source6
    {
        public int Value { get; set; }
    }

    public class Target
    {
        public Target1 Inner { get; set; } = null!;
    }

    public class Target1
    {
        public Target2 Inner { get; set; } = null!;
    }

    public class Target2
    {
        public Target3 Inner { get; set; } = null!;
    }

    public class Target3
    {
        public Target4 Inner { get; set; } = null!;
    }

    public class Target4
    {
        public Target5 Inner { get; set; } = null!;
    }

    public class Target5
    {
        public Target6 Inner { get; set; } = null!;
    }

    public class Target6
    {
        public int Value { get; set; }
    }
}
