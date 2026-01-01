namespace NMapper.Tests.TestData
{
    public class SourceTargetEnumMapping : IMapping<SourceEnum, TargetEnum>, IMapping<TargetEnum, SourceEnum>
    {
        public TargetEnum Map(SourceEnum source)
        {
            return source switch
            {
                SourceEnum.Second => TargetEnum.Second,
                _ => TargetEnum.Default,
            };
        }

        public SourceEnum Map(TargetEnum source)
        {
            return source switch
            {
                TargetEnum.Second => SourceEnum.Second,
                _ => SourceEnum.Default,
            };
        }
    }
}