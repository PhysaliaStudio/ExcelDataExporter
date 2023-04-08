namespace Physalia.ExcelDataExporter
{
    public class FieldData
    {
        public string name;
        public TypeData typeData;
        public bool isArray;
        public int arraySize = -1;

        public bool IsArray => isArray;
        public string TypeName => isArray ? $"{typeData.name}[]" : typeData.name;
        public string BaseTypeName => typeData.name;

        public string PropertyName => name.Length > 1 ? char.ToUpper(name[0]) + name[1..] : char.ToUpper(name[0]).ToString();
    }
}
