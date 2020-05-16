using System;

namespace Repository.Utils
{
    internal static class ObjectExtension
    {
        internal static void CopyPropertiesTo(this object source, object target)
        {
            var toObjectProperties = target.GetType().GetProperties();
            foreach (var propTo in toObjectProperties)
            {
                var propFrom = source.GetType().GetProperty(propTo.Name);
                if (propFrom != null && propFrom.CanWrite)
                    propTo.SetValue(target, propFrom.GetValue(source, null), null);
            }
        }

        internal static T CopyPropertiesToNewObject<T>(this object source)
        {
            var result = (T)Activator.CreateInstance(typeof(T));
            source.CopyPropertiesTo(result);
            return result;
        }
    }
}