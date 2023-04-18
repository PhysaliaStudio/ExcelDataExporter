using System;

namespace Physalia.ExcelDataExporter
{
    internal class TypeFieldIterator
    {
        private readonly FieldData fieldData;
        private readonly string fieldName;
        private readonly TypeData typeData;

        private int arrayIndex = -1;
        private int memberIndex;

        internal FieldData FieldData => fieldData;
        internal FieldData CurrentMember => typeData.fieldDatas[memberIndex];
        internal bool IsArray => arrayIndex >= 0;
        internal int ArrayIndex => arrayIndex;

        internal TypeFieldIterator(FieldData fieldData)
        {
            this.fieldData = fieldData;
            fieldName = fieldData.name;
            typeData = fieldData.typeData;
            arrayIndex = fieldData.IsArray ? 0 : -1;
        }

        internal bool IsAtFirstMember()
        {
            return memberIndex == 0;
        }

        internal bool IsAtLastMember()
        {
            return memberIndex == typeData.fieldDatas.Count - 1;
        }

        internal void IncreaseArrayIndex()
        {
            if (!IsAtLastMember())
            {
                throw new InvalidOperationException();
            }

            arrayIndex++;
            memberIndex = 0;
        }

        internal void IncreaseMemberIndex()
        {
            if (memberIndex < typeData.fieldDatas.Count - 1)
            {
                memberIndex++;
            }
        }

        internal string ExpectedName()
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
