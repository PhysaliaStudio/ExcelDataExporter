using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Physalia.ExcelDataExporter
{
    public class ExcelDataExporterWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset uiAsset;

        [MenuItem("Tools/Excel Data Exporter")]
        private static void Open()
        {
            var window = GetWindow<ExcelDataExporterWindow>();
            window.titleContent = new GUIContent("Excel Data Exporter");
            window.Show();
        }

        private void CreateGUI()
        {
            uiAsset.CloneTree(rootVisualElement);
        }
    }
}
