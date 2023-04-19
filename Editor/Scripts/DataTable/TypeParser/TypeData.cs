using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class TypeData
    {
        public string name;
        public List<FieldData> fieldDatas = new();

        public bool IsSystemType => TypeUtility.IsSystemType(name);
    }
}
