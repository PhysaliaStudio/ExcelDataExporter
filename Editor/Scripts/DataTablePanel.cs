using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Physalia.ExcelDataExporter
{
    public class DataTableButton : BindableElement
    {
        private static readonly Color SelectedColor = new Color32(0, 150, 0, 255);

        private readonly WorksheetData worksheetData;
        private readonly Button button;

        public DataTableButton(WorksheetData worksheetData)
        {
            this.worksheetData = worksheetData;

            var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Const.UiAssetFolderPath}{nameof(DataTableButton)}.uxml");
            uiAsset.CloneTree(this);

            button = this.Q<Button>();
            button.text = worksheetData.Name;
            button.clicked += Switch;
        }

        private void Switch()
        {
            worksheetData.Switch();

            bool isSelected = worksheetData.IsSelected;
            button.style.backgroundColor = isSelected ? SelectedColor : StyleKeyword.Null;
        }
    }

    public class DataTablePanel
    {
        private readonly GameDatabase gameDatabase;
        private readonly VisualElement visualElement;

        public DataTablePanel(GameDatabase gameDatabase, VisualElement visualElement)
        {
            this.gameDatabase = gameDatabase;
            this.visualElement = visualElement;

            gameDatabase.Reloaded += SetupList;

            var pingCodeFolderbutton = visualElement.Q<Button>("ping-code-folder-button");
            pingCodeFolderbutton.clicked += PingCodeFolder;

            var pingExportFolderbutton = visualElement.Q<Button>("ping-export-folder-button");
            pingExportFolderbutton.clicked += PingExportFolder;

            var browseCodeFolderbutton = visualElement.Q<Button>("browse-code-folder-button");
            browseCodeFolderbutton.clicked += BrowseCodeFolder;

            var browseExportFolderbutton = visualElement.Q<Button>("browse-export-folder-button");
            browseExportFolderbutton.clicked += BrowseExportFolder;

            var namespaceField = visualElement.Q<TextField>("namespace-field");
            namespaceField.RegisterValueChangedCallback(evt => gameDatabase.SaveNamespace());

            var codeGenerateCustomTypeButton = visualElement.Q<Button>("code-generate-custom-type-button");
            codeGenerateCustomTypeButton.clicked += GenerateCodeForCustomTypes;

            var codeGenerateButton = visualElement.Q<Button>("code-generate-button");
            codeGenerateButton.clicked += GenerateCode;

            var exportButton = visualElement.Q<Button>("export-button");
            exportButton.clicked += Export;
        }

        private void PingCodeFolder()
        {
            string assetPath = gameDatabase.CodePath.Replace(Application.dataPath, "Assets");
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            EditorGUIUtility.PingObject(folder);
        }

        private void PingExportFolder()
        {
            string assetPath = gameDatabase.ExportPath.Replace(Application.dataPath, "Assets");
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            EditorGUIUtility.PingObject(folder);
        }

        private void BrowseCodeFolder()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Select Code Folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }

            gameDatabase.SetCodePath(fullPath);
        }

        private void BrowseExportFolder()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Select Export Folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }

            gameDatabase.SetExportPath(fullPath);
        }

        public void SetupList()
        {
            var container = visualElement.Q<VisualElement>("data-table-view");
            container.Clear();

            for (var i = 0; i < gameDatabase.dataTables.Count; i++)
            {
                WorksheetData worksheetData = gameDatabase.dataTables[i];
                var element = new DataTableButton(worksheetData);
                container.Add(element);
            }
        }

        private void GenerateCodeForCustomTypes()
        {
            gameDatabase.GenerateCodeForCustomTypes();
        }

        private void GenerateCode()
        {
            gameDatabase.GenerateCodeForSelectedTables();
        }

        private void Export()
        {
            gameDatabase.ExportSelectedTables();
        }
    }
}
