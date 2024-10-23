using System.Reflection;

namespace Common.SharedKernel.Utilities
{
    public static class ObjectUtils
    {
        public static bool AreAllPropertiesNullOrEmpty(object obj)
        {
            if (obj == null) return true;

            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                var value = property.GetValue(obj);

                // Check if value is null
                if (value != null)
                {
                    // Special handling for strings
                    if (property.PropertyType == typeof(string) && string.IsNullOrEmpty(value as string))
                        continue;

                    // If it's not a string or other nullable type, check if it's empty or default
                    if (property.PropertyType.IsValueType && Activator.CreateInstance(property.PropertyType).Equals(value))
                        continue;

                    // If property is not null and doesn't match default value, return false
                    return false;
                }
            }

            return true;
        }
    }
}
