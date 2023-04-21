using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Physalia.ExcelDataExporter
{
    public class DataExporterJson
    {
        public string Export(TypeData typeData, SheetRawData sheetRawData)
        {
            Metadata metadata = sheetRawData.Metadata;

            var sb = new StringBuilder();

            if (metadata.SheetLayout == SheetLayout.Horizontal)
            {
                for (var i = Const.DataTableStartRow; i < sheetRawData.RowCount; i++)
                {
                    string json = ExportDataCellsAsJson(typeData, sheetRawData.GetRow(i));
                    sb.Append(json);
                    sb.Append('\n');
                }
            }
            else
            {
                for (var i = Const.DataTableStartColumn; i < sheetRawData.ColumnCount; i++)
                {
                    string json = ExportDataCellsAsJson(typeData, sheetRawData.GetColumn(i));
                    sb.Append(json);
                    sb.Append('\n');
                }
            }

            return sb.ToString();
        }

        private string ExportDataCellsAsJson(TypeData typeData, string[] dataCells)
        {
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var writer = new JsonTextWriter(sw);

            writer.WriteStartObject();

            var cellIndex = 0;
            for (var i = 0; i < typeData.fieldDatas.Count; i++)
            {
                FieldData fieldData = typeData.fieldDatas[i];

                // If the field is system type, write it directly
                if (fieldData.IsSystemTypeOrEnum)
                {
                    WritePropertyForSystemType(writer, fieldData, dataCells[cellIndex]);
                    cellIndex++;
                    continue;
                }

                // If the field is custom type, write it recursively
                cellIndex = WritePropertyForCustomType(writer, fieldData, dataCells, cellIndex);
            }

            writer.WriteEndObject();
            return sb.ToString();
        }

        private int WritePropertyForCustomType(JsonTextWriter writer, FieldData fieldData, string[] dataRow, int columnIndex)
        {
            writer.WritePropertyName(fieldData.NameForPublicField);

            var iterator = new TypeFieldIterator(fieldData);
            if (fieldData.IsArray)
            {
                writer.WriteStartArray();
            }

            while (true)
            {
                bool isCurrentCellApplicable = !fieldData.IsArray || dataRow[columnIndex] != Const.NotApplicable;

                if (iterator.IsAtFirstMember())
                {
                    if (isCurrentCellApplicable)
                    {
                        writer.WriteStartObject();
                    }
                }

                FieldData currentMemberFieldData = iterator.CurrentMember;
                if (currentMemberFieldData.IsSystemTypeOrEnum)
                {
                    if (isCurrentCellApplicable)
                    {
                        WritePropertyForSystemType(writer, currentMemberFieldData, dataRow[columnIndex]);
                    }
                    columnIndex++;
                }
                else
                {
                    if (isCurrentCellApplicable)
                    {
                        columnIndex = WritePropertyForCustomType(writer, currentMemberFieldData, dataRow, columnIndex);
                    }
                    else
                    {
                        columnIndex += currentMemberFieldData.EvaluateColumnCount();
                    }
                }

                // Increase array index if not finished
                if (!iterator.IsAtLastMember())
                {
                    iterator.IncreaseMemberIndex();
                }
                else if (iterator.IsArray && iterator.ArrayIndex < iterator.FieldData.arraySize - 1)
                {
                    if (isCurrentCellApplicable)
                    {
                        writer.WriteEndObject();
                    }
                    iterator.IncreaseArrayIndex();
                }
                else
                {
                    if (isCurrentCellApplicable)
                    {
                        writer.WriteEndObject();
                    }
                    break;
                }
            }

            if (fieldData.IsArray)
            {
                writer.WriteEndArray();
            }

            return columnIndex;
        }

        private void WritePropertyForSystemType(JsonTextWriter writer, FieldData fieldData, string dataText)
        {
            writer.WritePropertyName(fieldData.NameForPublicField);
            if (fieldData.IsArray)
            {
                writer.WriteStartArray();

                string[] splits = dataText.Split(',');
                for (var j = 0; j < splits.Length; j++)
                {
                    WriteValueForSystemType(writer, fieldData.IsEnum ? "int" : fieldData.BaseTypeName, splits[j]);
                }

                writer.WriteEndArray();
            }
            else
            {
                string text = dataText;
                WriteValueForSystemType(writer, fieldData.IsEnum ? "int" : fieldData.BaseTypeName, text);
            }
        }

        private void WriteValueForSystemType(JsonTextWriter writer, string typeName, string text)
        {
            object value = TypeUtility.ParseValueToSystemType(typeName, text);
            writer.WriteValue(value);
        }
    }
}
