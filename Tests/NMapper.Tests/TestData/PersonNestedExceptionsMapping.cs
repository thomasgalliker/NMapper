namespace NMapper.Tests.TestData
{
    public class PersonNestedExceptionsMapping : IMappingWithContext<Person, double?>
    {
        public double? Map(Person source, IMappingContext context)
        {
            var intValue = context.Map<int>(source);
            var floatValue = context.Map<float>(source);
            return null;
        }
    }
}
