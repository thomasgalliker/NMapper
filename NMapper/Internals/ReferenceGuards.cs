using System.Diagnostics.CodeAnalysis;

namespace NMapper.Internals
{
    internal static class ReferenceGuards
    {
        public static bool IsTrackable(object? value)
        {
            if (value == null)
            {
                return false;
            }

            var type = value.GetType();
            return !type.IsValueType && type != typeof(string);
        }
    }
}
