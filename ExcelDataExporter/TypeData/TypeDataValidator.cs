using System.Collections.Generic;

namespace Physalia.ExcelDataExporter
{
    public class TypeDataValidator
    {
        public class Result
        {
            private readonly TypeData _typeData;
            private readonly bool _isIdFieldMissing;
            private readonly bool _hasDuplicatedName;

            public TypeData TypeData => _typeData;
            public bool IsValid => !_isIdFieldMissing && !_hasDuplicatedName;
            public bool IsIdFieldMissing => _isIdFieldMissing;
            public bool HasDuplicatedName => _hasDuplicatedName;

            public Result(TypeData typeData, bool isIdFieldMissing, bool hasDuplicatedName)
            {
                _typeData = typeData;
                _isIdFieldMissing = isIdFieldMissing;
                _hasDuplicatedName = hasDuplicatedName;
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
            if (!typeData.IsTypeWithId)
            {
                return false;
            }

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
