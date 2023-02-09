namespace Physalia.ExcelDataExporter
{
    public class FieldData
    {
        public string name;
        public string typeName;

        public bool IsArray => typeName.EndsWith("[]");

        public string BaseTypeName
        {
            get
            {
                if (typeName.EndsWith("[]"))
                {
                    return typeName[..^2];
                }

                return typeName;
            }
        }
    }
}
