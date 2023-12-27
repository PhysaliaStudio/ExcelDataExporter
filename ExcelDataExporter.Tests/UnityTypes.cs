using System.Collections.Generic;

namespace Physalia.ExcelDataExporter.Tests
{
    internal class UnityTypes
    {
        public const string NamespaceName = "Physalia.ExcelDataRuntime";

        public static readonly List<TypeData> List = new()
        {
            new()
            {
                namespaceName = NamespaceName,
                name = "Vector2",
                fieldDatas = new List<FieldData>
                {
                    new() { name = "x", typeData = TypeUtility.TypeDataFloat },
                    new() { name = "y", typeData = TypeUtility.TypeDataFloat },
                }
            },
            new()
            {
                namespaceName = NamespaceName,
                name = "Vector3",
                fieldDatas = new List<FieldData>
                {
                    new() { name = "x", typeData = TypeUtility.TypeDataFloat },
                    new() { name = "y", typeData = TypeUtility.TypeDataFloat },
                    new() { name = "z", typeData = TypeUtility.TypeDataFloat },
                }
            },
            new()
            {
                namespaceName = NamespaceName,
                name = "Vector4",
                fieldDatas = new List<FieldData>
                {
                    new() { name = "x", typeData = TypeUtility.TypeDataFloat },
                    new() { name = "y", typeData = TypeUtility.TypeDataFloat },
                    new() { name = "z", typeData = TypeUtility.TypeDataFloat },
                    new() { name = "w", typeData = TypeUtility.TypeDataFloat },
                }
            },
            new()
            {
                namespaceName = NamespaceName,
                name = "Vector2Int",
                fieldDatas = new List<FieldData>
                {
                    new() { name = "x", typeData = TypeUtility.TypeDataInt },
                    new() { name = "y", typeData = TypeUtility.TypeDataInt },
                }
            },
            new()
            {
                namespaceName = NamespaceName,
                name = "Vector3Int",
                fieldDatas = new List<FieldData>
                {
                    new() { name = "x", typeData = TypeUtility.TypeDataInt },
                    new() { name = "y", typeData = TypeUtility.TypeDataInt },
                    new() { name = "z", typeData = TypeUtility.TypeDataInt },
                }
            },
        };
    }
}
