namespace Physalia.ExcelDataExporter
{
    public class FieldData
    {
        public string name;
        public TypeData typeData;
        public string comment;
        public bool isArray;
        public int arraySize = -1;
        public int enumValue;

        public bool IsArray => isArray;
        public bool IsSystemType => typeData.IsSystemType;
        public string TypeName => isArray ? $"{typeData.name}[]" : typeData.name;
        public string BaseTypeName => typeData.name;
        public string Comment => comment;

        public string NameForPrivateField => name.Length > 1 ? "_" + char.ToLower(name[0]) + name[1..] : "_" + char.ToLower(name[0]).ToString();
        public string NameForPublicField => name.Length > 1 ? char.ToLower(name[0]) + name[1..] : char.ToLower(name[0]).ToString();
        public string NameForProperty => name.Length > 1 ? char.ToUpper(name[0]) + name[1..] : char.ToUpper(name[0]).ToString();
        public string NameForEnumMember => name.Length > 1 ? char.ToUpper(name[0]) + name[1..] : char.ToUpper(name[0]).ToString();

        public int EvaluateColumnCount()
        {
            if (IsArray && arraySize == -1)
            {
                return -1;
            }

            int fieldCountForNonArray;
            if (IsSystemType)
            {
                fieldCountForNonArray = 1;
            }
            else
            {
                fieldCountForNonArray = typeData.EvaluateColumnCount();
            }

            if (fieldCountForNonArray == -1)
            {
                return -1;
            }
            else if (IsArray)
            {
                return fieldCountForNonArray * arraySize;
            }
            else
            {
                return fieldCountForNonArray;
            }
        }
    }
}
