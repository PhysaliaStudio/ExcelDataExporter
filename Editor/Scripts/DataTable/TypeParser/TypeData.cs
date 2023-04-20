using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class TypeData
    {
        public enum Define { Class, Struct, Enum }

        public bool isTypeWithId;
        public string namespaceName;
        public Define define;
        public string name;
        public List<FieldData> fieldDatas = new();

        public bool IsTypeWithId => isTypeWithId;
        public bool IsSystemType => TypeUtility.IsSystemType(name);

        public int EvaluateColumnCount()
        {
            if (IsSystemType)
            {
                return 1;
            }

            var total = 0;
            for (var i = 0; i < fieldDatas.Count; i++)
            {
                int count = fieldDatas[i].EvaluateColumnCount();
                if (count == -1)  // If there's any field cannot be evaluated, return -1.
                {
                    return -1;
                }

                total += count;
            }

            return total;
        }
    }
}
