using System.Collections.Generic;
using UnityEngine;

namespace Physalia.ExcelDataExporter
{
    public class TypeData
    {
        public string name;
        public List<FieldData> fieldDatas = new();

        public bool IsSystemType => TypeUtility.IsSystemType(name);

        public bool Validate()
        {
            // Check if field name is duplicate
            var fieldNames = new HashSet<string>();
            for (var i = 0; i < fieldDatas.Count; i++)
            {
                FieldData fieldData = fieldDatas[i];
                if (fieldNames.Contains(fieldData.name))
                {
                    Debug.LogError($"The type '{name}' already contains a definition for '{fieldData.name}'");
                    return false;
                }

                fieldNames.Add(fieldData.name);
            }

            return true;
        }
    }
}
