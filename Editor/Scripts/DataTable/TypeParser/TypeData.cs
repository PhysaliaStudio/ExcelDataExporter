using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class TypeData
    {
        public string namespaceName;
        public string name;
        public List<FieldData> fieldDatas = new();

        public bool IsSystemType => TypeUtility.IsSystemType(name);
    }
}
