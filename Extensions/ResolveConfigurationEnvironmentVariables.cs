using CloudflareWorkerBundler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudflareWorkerBundler.Extensions
{
    public static class ResolveConfigurationEnvironmentVariables
    {
        private static Type TryGetUnderlyingNullable(this Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType == null)
                return type;
            return nullableType;
        }

        public static void ResolveConfigurationDatafromEnvironmentVariables<T>(this T inputClass) where T : class
        {
            var props = inputClass.GetType().GetProperties();
            foreach (var prop in props)
            {
                var resolvedPropertyType = prop.PropertyType.TryGetUnderlyingNullable();
                var attrs = prop.GetCustomAttributes(true);
                foreach (var attr in attrs)
                    if (attr is ConfigurationEnvironmentVariableProperty envProperty)
                    {
                        var envName = envProperty.EnvironmentVariable;
                        var envVariable = Environment.GetEnvironmentVariable(envName);
                        if (String.IsNullOrWhiteSpace(envVariable) == false)
                        {
                            if (resolvedPropertyType == typeof(string))
                            {
                                prop.SetValue(inputClass, envVariable, null);
                            }
                            else if (resolvedPropertyType == typeof(bool))
                            {
                                if (Boolean.TryParse(envVariable, out var foundValue))
                                    prop.SetValue(inputClass, foundValue, null);
                            }
                            else if (resolvedPropertyType == typeof(long))
                            {
                                if (long.TryParse(envVariable, out var foundValue))
                                    prop.SetValue(inputClass, foundValue, null);
                            }
                            else if (resolvedPropertyType == typeof(int))
                            {
                                if (int.TryParse(envVariable, out var foundValue))
                                    prop.SetValue(inputClass, foundValue, null);
                            }
                            else if (resolvedPropertyType.IsEnum)
                            {
                                if (Enum.TryParse(resolvedPropertyType, envVariable, true, out var foundValue))
                                    prop.SetValue(inputClass, foundValue, null);
                            }
                            else
                            {
                                throw new InvalidOperationException(
                                    $"Cannot resolve {resolvedPropertyType.Name} for Property {prop.Name} from Env Variable. This type probably isn't supported.");
                            }
                        }
                    }
            }
        }
    }
}
