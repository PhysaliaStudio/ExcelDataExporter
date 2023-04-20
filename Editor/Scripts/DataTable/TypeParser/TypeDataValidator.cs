using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class TypeDataValidator
    {
        public class Result
        {
            private readonly TypeData typeData;
            private readonly bool isIdFieldMissing;
            private readonly bool hasNoDuplicatedName;

            public TypeData TypeData => typeData;
            public bool IsValid => !isIdFieldMissing && !hasNoDuplicatedName;
            public bool IsIdFieldMissing => isIdFieldMissing;
            public bool HasDuplicatedName => hasNoDuplicatedName;

            public Result(TypeData typeData, bool isIdFieldMissing, bool hasNoDuplicatedName)
            {
                this.typeData = typeData;
                this.isIdFieldMissing = isIdFieldMissing;
                this.hasNoDuplicatedName = hasNoDuplicatedName;
            }
        }

        public Result Validate(TypeData typeData)
        {
            bool isIdFieldMissing = CheckIfIdFieldMissing(typeData);
            bool hasDuplicatedName = CheckHasDuplicatedName(typeData);
            return new Result(typeData, isIdFieldMissing, hasDuplicatedName);
        }

        private bool CheckIfIdFieldMissing(TypeData typeData)
        {
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];
                if (fieldData.name == "id" && fieldData.TypeName == "int")
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckHasDuplicatedName(TypeData typeData)
        {
            var names = new HashSet<string>();
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                string fieldName = typeData.fieldDatas[i].name;
                if (names.Contains(fieldName))
                {
                    return true;
                }

                names.Add(fieldName);
            }

            return false;
        }
    }
}
