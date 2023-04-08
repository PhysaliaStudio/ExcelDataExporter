using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Physalia.ExcelDataExporter
{
    public class SheetParser
    {
        private class TypeFieldIterator
        {
            private readonly string fieldName;
            private readonly TypeData typeData;

            private int arrayIndex = -1;
            private int memberIndex;

            internal bool IsArray => arrayIndex >= 0;

            internal TypeFieldIterator(string fieldName, TypeData typeData, bool isArray)
            {
                this.fieldName = fieldName;
                this.typeData = typeData;
                arrayIndex = isArray ? 0 : -1;
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

        private class TypeRawField
        {
            private readonly string fieldName;
            private readonly string typeName;

            internal TypeRawField(string fieldName, string typeName)
            {
                this.typeName = typeName;
                this.fieldName = fieldName;
            }

            internal string FieldName => fieldName;
            internal string TypeName => typeName;
        }

        private readonly CustomTypeTable customTypeTable;

        public SheetParser()
        {
            customTypeTable = new CustomTypeTable();
        }

        public SheetParser(CustomTypeTable customTypeTable)
        {
            this.customTypeTable = customTypeTable;
        }

        public TypeData ExportTypeData(string typeName, SheetRawData sheetRawData)
        {
            string[] fieldNameRow = sheetRawData.GetRow(0);
            string[] fieldTypeNameRow = sheetRawData.GetRow(1);
            var rawFields = new List<TypeRawField>(fieldNameRow.Length);
            for (var i = 0; i < sheetRawData.ColumnCount; i++)
            {
                rawFields.Add(new TypeRawField(fieldNameRow[i], fieldTypeNameRow[i]));
            }

            var classData = new TypeData { name = typeName };
            var iterators = new Stack<TypeFieldIterator>();

            for (var i = 0; i < rawFields.Count; i++)
            {
                TypeRawField rawField = rawFields[i];

                // If we are not in a nested type, record the new field and check if we are starting a nested type
                if (iterators.Count == 0)
                {
                    FieldData newFieldData = RecordNewField(rawField);
                    classData.fieldDatas.Add(newFieldData);

                    // Start new nested type if necessary
                    if (!newFieldData.typeData.IsSystemType)
                    {
                        var iterator = new TypeFieldIterator(newFieldData.name, newFieldData.typeData, newFieldData.isArray);
                        iterators.Push(iterator);
                    }
                }

                // If we are in a nested type, check if current field is legal and check if we are done with the current type
                if (iterators.Count > 0)
                {
                    TypeFieldIterator iterator = iterators.Peek();
                    CheckIfFieldIsLegalWhileNestingType(iterator, rawField);
                    PopIteratorsIfFinished(iterators, rawFields, i);
                }
            }

            if (iterators.Count > 0)
            {
                throw new InvalidDataException("Invalid Data! The nested type does not finish.");
            }

            if (!classData.Validate())
            {
                throw new InvalidDataException("Invalid Data! Final validation failed. See above errors.");
            }

            return classData;
        }

        private FieldData RecordNewField(TypeRawField nextField)
        {
            string fieldName = nextField.FieldName;
            string fieldTypeName = nextField.TypeName;
            bool isArray;

            TypeData fieldTypeData;
            if (fieldTypeName.EndsWith("[]"))  // Expected as array of an system type
            {
                isArray = true;

                fieldTypeName = fieldTypeName[0..^2];
                fieldTypeData = customTypeTable.GetTypeData(fieldTypeName);
                if (fieldTypeData == null || !fieldTypeData.IsSystemType)
                {
                    throw new InvalidDataException($"Invalid field type cell: {fieldTypeName}");
                }
            }
            else
            {
                fieldTypeData = customTypeTable.GetTypeData(fieldTypeName);
                if (fieldTypeData == null)
                {
                    throw new InvalidDataException($"Invalid field type cell: {fieldTypeName}");
                }

                bool isSystemType = fieldTypeData.IsSystemType;

                // If not system type, then it must be a custom type, and the fieldName must have dot in it
                if (!isSystemType)
                {
                    int indexOfDot = fieldName.IndexOf('.');
                    if (indexOfDot == -1)
                    {
                        throw new InvalidDataException($"Invalid field name cell: {fieldName}");
                    }

                    fieldName = fieldName[0..indexOfDot];
                }

                // The fieldName may have array index in it
                int indexOfArrayChar = fieldName.IndexOf('[');
                isArray = indexOfArrayChar != -1;
                if (isArray)
                {
                    if (isSystemType)
                    {
                        throw new InvalidDataException($"Invalid array declaration: {fieldName} for {fieldTypeName}");
                    }

                    fieldName = fieldName[0..indexOfArrayChar];
                }
            }

            var fieldData = new FieldData
            {
                name = fieldName.Length > 1 ? char.ToLower(fieldName[0]) + fieldName[1..] : char.ToLower(fieldName[0]).ToString(),
                typeData = fieldTypeData,
                isArray = isArray,
            };

            return fieldData;
        }

        private void CheckIfFieldIsLegalWhileNestingType(TypeFieldIterator iterator, TypeRawField rawField)
        {
            if (!iterator.IsAtFirstMember() && !string.IsNullOrEmpty(rawField.TypeName))
            {
                throw new InvalidDataException($"Invalid Data! The field type cell should be empty while in nested type, but was {rawField.TypeName}");
            }

            string expectedName = iterator.ExpectedName();
            if (expectedName != rawField.FieldName)
            {
                throw new InvalidDataException($"Invalid Data! The field name cell should be {expectedName}, but was {rawField.FieldName}");
            }
        }

        private void PopIteratorsIfFinished(Stack<TypeFieldIterator> iterators, List<TypeRawField> rawFields, int fieldIndex)
        {
            while (iterators.Count > 0)
            {
                TypeFieldIterator iterator = iterators.Peek();
                if (!iterator.IsAtLastMember())
                {
                    iterator.IncreaseMemberIndex();
                    break;
                }
                else
                {
                    if (iterator.IsArray)
                    {
                        // If we are iterating array type and end, check if there are more elements
                        if (fieldIndex < rawFields.Count - 1 && string.IsNullOrEmpty(rawFields[fieldIndex + 1].TypeName))
                        {
                            iterator.IncreaseArrayIndex();
                            break;
                        }
                        else
                        {
                            _ = iterators.Pop();
                        }
                    }
                    else
                    {
                        _ = iterators.Pop();
                    }
                }
            }
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
