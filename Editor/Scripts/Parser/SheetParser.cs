using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Physalia.ExcelDataExporter
{
    public class SheetParser
    {
        public TypeData ExportTypeData(string typeName, SheetRawData sheetRawData)
        {
            string[] fieldNameRow = sheetRawData.GetRow(0);
            string[] fieldTypeNameRow = sheetRawData.GetRow(1);

            var classData = new TypeData { name = typeName };
            for (var i = 0; i < sheetRawData.ColumnCount; i++)
            {
                string fieldName = fieldNameRow[i];
                string fieldTypeName = fieldTypeNameRow[i];

                var fieldData = new FieldData
                {
                    name = fieldName.Length > 1 ? char.ToLower(fieldName[0]) + fieldName[1..] : char.ToLower(fieldName[0]).ToString(),
                    typeData = null,
                };
                classData.fieldDatas.Add(fieldData);
            }

            return classData;
        }

        public string ExportDataTableAsJson(SheetRawData sheetRawData)
        {
            var sb = new StringBuilder();

            TypeData typeData = ExportTypeData("", sheetRawData);  // TODO: class name
            for (var i = 2; i < sheetRawData.RowCount; i++)
            {
                string json = ExportDataRowAsJson(typeData, sheetRawData.GetRow(i));
                sb.Append(json);
                sb.Append('\n');
            }

            return sb.ToString();
        }

        private string ExportDataRowAsJson(TypeData typeData, string[] dataRow)
        {
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var writer = new JsonTextWriter(sw);

            writer.WriteStartObject();

            for (var i = 0; i < dataRow.Length; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];

                writer.WritePropertyName(fieldData.name);
                if (fieldData.IsArray)
                {
                    writer.WriteStartArray();

                    string[] splits = dataRow[i].Split(',');
                    for (var j = 0; j < splits.Length; j++)
                    {
                        WriteForBuiltInTypes(writer, fieldData.BaseTypeName, splits[j]);
                    }

                    writer.WriteEndArray();
                }
                else
                {
                    string text = dataRow[i];
                    WriteForBuiltInTypes(writer, fieldData.BaseTypeName, text);
                }
            }

            writer.WriteEndObject();
            return sb.ToString();
        }

        private void WriteForBuiltInTypes(JsonTextWriter writer, string typeName, string text)
        {
            object value = TypeUtility.ParseValueToSystemType(typeName, text);
            writer.WriteValue(value);
        }
    }
}
