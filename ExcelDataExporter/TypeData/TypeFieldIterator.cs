using System;

namespace Physalia.ExcelDataExporter
{
    public class TypeFieldIterator
    {
        private readonly FieldData fieldData;
        private readonly string fieldName;
        private readonly TypeData typeData;

        private int arrayIndex = -1;
        private int memberIndex;

        public FieldData FieldData => fieldData;
        public FieldData CurrentMember => typeData.fieldDatas[memberIndex];
        public bool IsArray => arrayIndex >= 0;
        public int ArrayIndex => arrayIndex;

        public TypeFieldIterator(FieldData fieldData)
        {
            this.fieldData = fieldData;
            fieldName = fieldData.name;
            typeData = fieldData.typeData;
            arrayIndex = fieldData.IsArray ? 0 : -1;
        }

        public bool IsAtFirstMember()
        {
            return memberIndex == 0;
        }

        public bool IsAtLastMember()
        {
            return memberIndex == typeData.fieldDatas.Count - 1;
        }

        public void IncreaseArrayIndex()
        {
            if (!IsAtLastMember())
            {
                throw new InvalidOperationException();
            }

            arrayIndex++;
            memberIndex = 0;
        }

        public void IncreaseMemberIndex()
        {
            if (memberIndex < typeData.fieldDatas.Count - 1)
            {
                memberIndex++;
            }
        }

        public string ExpectedName()
        {
            if (memberIndex >= typeData.fieldDatas.Count)
            {
                return null;
            }

            string fieldName = typeData.fieldDatas[memberIndex].name;

            if (arrayIndex == -1)
            {
                return $"{this.fieldName}.{fieldName}";
            }

            return $"{this.fieldName}[{arrayIndex}].{fieldName}";
        }
    }
}
