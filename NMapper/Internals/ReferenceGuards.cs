namespace NMapper.Internals
{
    internal static class ReferenceGuards
    {
        public static bool IsTrackable(object value)
        {
            var type = value.GetType();
            return !type.IsValueType && type != typeof(string);
        }
    }

}
