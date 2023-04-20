using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class TypeData
    {
        public enum Define { Class, Struct, Enum }

        public string namespaceName;
        public Define define;
        public string name;
        public List<FieldData> fieldDatas = new();

        public bool IsSystemType => TypeUtility.IsSystemType(name);
    }
}
