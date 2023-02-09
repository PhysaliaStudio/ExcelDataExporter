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
        private DataTablePanel dataTablePanel;

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

            var browseDataFolderbutton = rootVisualElement.Q<Button>("browse-data-folder-button");
            browseDataFolderbutton.clicked += BrowseDataFolder;

            var dataFolderField = rootVisualElement.Q<TextField>("data-folder-field");
            if (dataFolderField != null)
            {
                dataFolderField.value = gameDatabase.DataPath;
            }

            var reloadButton = rootVisualElement.Q<Button>("reload-button");
            reloadButton.clicked += Reload;

            var dataTablePanelElement = rootVisualElement.Q<TemplateContainer>(nameof(DataTablePanel));
            dataTablePanel = new DataTablePanel(gameDatabase, dataTablePanelElement);

            if (!string.IsNullOrEmpty(gameDatabase.DataPath))
            {
                Reload();
            }
        }

        private void BrowseDataFolder()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Select Data Folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }

            gameDatabase.Load(fullPath);

            var pathField = rootVisualElement.Q<TextField>("data-folder-field");
            if (pathField != null)
            {
                pathField.value = fullPath;
            }
        }

        private void Reload()
        {
            gameDatabase.Reload();
        }
    }
}
