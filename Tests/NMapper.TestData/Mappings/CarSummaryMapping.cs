namespace NMapper.TestData.Mappings
{
    /// <summary>
    /// Deliberately a plain <see cref="IMapping{TSource,TTarget}"/> so that collections of
    /// <see cref="Car"/> also exercise the context-free collection mapping plans.
    /// </summary>
    public sealed class CarSummaryMapping : IMapping<Car, CarSummaryDto>
    {
        public CarSummaryDto Map(Car source)
        {
            return new CarSummaryDto
            {
                Text = $"<{source.Model}>",
            };
        }
    }
}