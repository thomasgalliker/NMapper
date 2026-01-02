using System.Reflection;

namespace NMapper.Extensions
{
    internal static class TypeExtensions
    {
        internal static string GetFormattedName(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return type.Name;
            }

            return string.Format("{0}<{1}>", type.Name.Substring(0, type.Name.IndexOf('`')), string.Join(", ", typeInfo.GenericTypeArguments.Select(t => t.GetFormattedName())));
        }

        internal static string GetFormattedFullname(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericType)
            {
                return type.ToString();
            }

            return string.Format("{0}.{1}<{2}>", type.Namespace, type.Name.Substring(0, type.Name.IndexOf('`')), string.Join(", ", typeInfo.GenericTypeArguments.Select(t => t.GetFormattedFullname())));
        }
    }
}