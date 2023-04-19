using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class TypeDataValidator
    {
        public class Result
        {
            private readonly TypeData typeData;
            private readonly bool hasIntIdField;
            private readonly bool hasNoDuplicatedName;

            public TypeData TypeData => typeData;
            public bool IsValid => hasIntIdField && hasNoDuplicatedName;
            public bool HasIntIdField => hasIntIdField;
            public bool HasNoDuplicatedName => hasNoDuplicatedName;

            public Result(TypeData typeData, bool hasIntIdField, bool hasNoDuplicatedName)
            {
                this.typeData = typeData;
                this.hasIntIdField = hasIntIdField;
                this.hasNoDuplicatedName = hasNoDuplicatedName;
            }
        }

        public Result Validate(TypeData typeData)
        {
            bool hasIntIdField = HasIntIdField(typeData);
            bool hasNoDuplicatedName = HasNoDuplicatedName(typeData);
            return new Result(typeData, hasIntIdField, hasNoDuplicatedName);
        }

        private bool HasIntIdField(TypeData typeData)
        {
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                if (fieldData.name == "id" && fieldData.TypeName == "int")
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasNoDuplicatedName(TypeData typeData)
        {
            var names = new HashSet<string>();
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                string fieldName = typeData.fieldDatas[i].name;
                if (names.Contains(fieldName))
                {
                    return false;
                }

                names.Add(fieldName);
            }

            return true;
        }
    }
}
