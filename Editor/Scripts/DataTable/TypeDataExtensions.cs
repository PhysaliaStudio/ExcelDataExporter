using System;

namespace Physalia.ExcelDataExporter
{
    public static class TypeDataExtensions
    {
        public static Type GetTableType(this TypeData typeData)
        {
            Type tableType = ReflectionUtility.FindType((Type type) =>
            {
                return type.Namespace == typeData.namespaceName &&
                    type.Name == typeData.name + "Table" &&
                    type.BaseType.GetGenericTypeDefinition() == typeof(DataTable<>);
            });
            return tableType;
        }
    }
}
