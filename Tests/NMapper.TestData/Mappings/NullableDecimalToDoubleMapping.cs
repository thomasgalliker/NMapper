using System;

namespace NMapper.TestData.Mappings
{
    public class NullableDecimalToDoubleMapping : IMapping<decimal?, double>
    {
        public double Map(decimal? source)
        {
            if (source == null)
            {
                return double.NaN;
            }

            return Convert.ToDouble(source);
        }
    }
}
