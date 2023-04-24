using System;
using System.Reflection;

namespace Physalia.ExcelDataExporter
{
    public static class ReflectionUtility
    {
        private static Assembly[] assembliesCache;

        public static Assembly[] GetAssemblies()
        {
            if (assembliesCache == null)
            {
                assembliesCache = AppDomain.CurrentDomain.GetAssemblies();
            }

            return assembliesCache;
        }

        public static Type FindType(Func<Type, bool> filter)
        {
            Assembly[] assemblies = GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].FindType(filter);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        public static Type FindType(this Assembly assembly, Func<Type, bool> filter)
        {
            Type[] types = assembly.GetTypes();
            for (var i = 0; i < types.Length; i++)
            {
                if (filter(types[i]))
                {
                    return types[i];
                }
            }

            return null;
        }
    }
}
