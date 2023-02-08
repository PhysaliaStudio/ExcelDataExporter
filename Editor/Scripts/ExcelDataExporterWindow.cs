using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Physalia.ExcelDataExporter
{
    public class ExcelDataExporterWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset uiAsset;

        private GameDatabase gameDatabase;

        [MenuItem("Tools/Excel Data Exporter")]
        private static void Open()
        {
            var window = GetWindow<ExcelDataExporterWindow>();
            window.titleContent = new GUIContent("Excel Data Exporter");
            window.Show();
        }

        private void CreateGUI()
        {
            gameDatabase = CreateInstance<GameDatabase>();

            uiAsset.CloneTree(rootVisualElement);

            var button = rootVisualElement.Q<Button>("browse-data-folder-button");
            button.clicked += BrowseDataFolder;
        }

        private void BrowseDataFolder()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Select Data Folder", Application.dataPath, "");
            gameDatabase.Load(fullPath);

            var pathField = rootVisualElement.Q<TextField>("data-folder-field");
            if (pathField != null)
            {
                pathField.value = fullPath;
            }
        }
    }
}
