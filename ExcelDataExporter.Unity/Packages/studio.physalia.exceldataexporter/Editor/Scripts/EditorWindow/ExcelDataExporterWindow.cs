using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Physalia.ExcelDataExporter
{
    public class ExcelDataExporterWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset uiAsset;
        [SerializeField]
        private GameDatabase gameDatabase;

        private SerializedObject serializedObject;
        private DataTablePanel dataTablePanel;

        [MenuItem("Tools/ExcelDataExporter &2")]
        private static void Open()
        {
            var window = GetWindow<ExcelDataExporterWindow>();
            window.titleContent = new GUIContent("ExcelDataExporter");
            window.Show();
        }

        private void CreateGUI()
        {
            if (gameDatabase == null)
            {
                gameDatabase = CreateInstance<GameDatabase>();
            }

            serializedObject = new SerializedObject(gameDatabase);

            uiAsset.CloneTree(rootVisualElement);
            rootVisualElement.Bind(serializedObject);

            var showDataFolderbutton = rootVisualElement.Q<Button>("show-data-folder-button");
            showDataFolderbutton.clicked += ShowDataFolder;

            var browseDataFolderbutton = rootVisualElement.Q<Button>("browse-data-folder-button");
            browseDataFolderbutton.clicked += BrowseDataFolder;

            var reloadButton = rootVisualElement.Q<Button>("reload-button");
            reloadButton.clicked += Reload;

            var dataTablePanelElement = rootVisualElement.Q<TemplateContainer>(nameof(DataTablePanel));
            dataTablePanel = new DataTablePanel(gameDatabase, dataTablePanelElement);

            if (!string.IsNullOrEmpty(gameDatabase.DataPath))
            {
                Reload();
            }
        }

        private void ShowDataFolder()
        {
            string fullDataPath = Path.GetFullPath(gameDatabase.DataPath, Application.dataPath + "/../");
            EditorUtility.RevealInFinder(fullDataPath);
        }

        private void BrowseDataFolder()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Select Data Folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }

            gameDatabase.Load(fullPath);
        }

        private void Reload()
        {
            gameDatabase.Reload();
        }

        private void OnGUI()
        {
            if (EditorApplication.isCompiling)
            {
                if (rootVisualElement.enabledSelf)
                {
                    rootVisualElement.SetEnabled(false);
                }
            }
            else
            {
                if (!rootVisualElement.enabledSelf)
                {
                    rootVisualElement.SetEnabled(true);
                }
            }
        }
    }
}
