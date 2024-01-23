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
        public bool IsEnum => typeData.IsEnum;
        public bool IsSystemTypeOrEnum => typeData.IsSystemTypeOrEnum;
        public string TypeName => isArray ? $"{typeData.name}[]" : typeData.name;
        public string BaseTypeName => typeData.name;
        public string Comment => comment;

        public string NameWithCamelCaseUnderscore => name.Length > 1 ? "_" + char.ToLower(name[0]) + name[1..] : "_" + char.ToLower(name[0]).ToString();
        public string NameWithCamelCase => name.Length > 1 ? char.ToLower(name[0]) + name[1..] : char.ToLower(name[0]).ToString();
        public string NameWithPascalCase => name.Length > 1 ? char.ToUpper(name[0]) + name[1..] : char.ToUpper(name[0]).ToString();

        public int EvaluateColumnCount()
        {
            if (IsArray && arraySize == -1)
            {
                return -1;
            }

            int fieldCountForNonArray;
            if (typeData.IsSystemTypeOrEnum)
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
