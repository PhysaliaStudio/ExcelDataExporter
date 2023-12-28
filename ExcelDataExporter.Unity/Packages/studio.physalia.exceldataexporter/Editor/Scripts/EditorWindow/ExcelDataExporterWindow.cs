using System.Collections.Generic;
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

        private TextField _textFieldSourceDataPath;
        private TextField _textFieldExportDataPath;
        private TextField _textFieldExportScriptPath;
        private DropdownField _dropdownSetting;

        [MenuItem("Tools/ExcelDataExporter")]
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

            gameDatabase.Reloaded += SetupList;

            // Setting Dropdown
            var settingNames = new List<string>(gameDatabase.Settings.Count);
            for (var i = 0; i < gameDatabase.Settings.Count; i++)
            {
                settingNames.Add(gameDatabase.Settings[i].name);
            }
            _dropdownSetting = rootVisualElement.Q<DropdownField>("export-format-dropdown");
            _dropdownSetting.choices = settingNames;
            _dropdownSetting.index = gameDatabase.CurrentSettingIndex;
            _dropdownSetting.RegisterValueChangedCallback(OnDropdownSettingChanged);

            var reloadButton = rootVisualElement.Q<Button>("reload-button");
            reloadButton.clicked += Reload;

            // Paths
            _textFieldSourceDataPath = rootVisualElement.Q<TextField>("text-field-source-data-path");
            _textFieldSourceDataPath.SetValueWithoutNotify(gameDatabase.SourceDataPath);

            _textFieldExportScriptPath = rootVisualElement.Q<TextField>("text-field-export-script-path");
            _textFieldExportScriptPath.SetValueWithoutNotify(gameDatabase.ExportScriptPath);

            _textFieldExportDataPath = rootVisualElement.Q<TextField>("text-field-export-data-path");
            _textFieldExportDataPath.SetValueWithoutNotify(gameDatabase.ExportDataPath);

            // Ping Buttons
            var showDataFolderbutton = rootVisualElement.Q<Button>("show-data-folder-button");
            showDataFolderbutton.clicked += ShowDataFolder;

            var pingCodeFolderbutton = rootVisualElement.Q<Button>("ping-code-folder-button");
            pingCodeFolderbutton.clicked += PingCodeFolder;

            var pingExportFolderbutton = rootVisualElement.Q<Button>("ping-export-folder-button");
            pingExportFolderbutton.clicked += PingExportFolder;

            // Browse Buttons
            var browseDataFolderbutton = rootVisualElement.Q<Button>("browse-data-folder-button");
            browseDataFolderbutton.clicked += BrowseDataFolder;

            var browseCodeFolderbutton = rootVisualElement.Q<Button>("browse-code-folder-button");
            browseCodeFolderbutton.clicked += BrowseCodeFolder;

            var browseExportFolderbutton = rootVisualElement.Q<Button>("browse-export-folder-button");
            browseExportFolderbutton.clicked += BrowseExportFolder;

            // Feature Buttons
            var selectAllButton = rootVisualElement.Q<Button>("select-all-button");
            selectAllButton.clicked += SelectAll;

            var deselectAllButton = rootVisualElement.Q<Button>("deselect-all-button");
            deselectAllButton.clicked += DeselectAll;

            var codeGenerateCustomTypeButton = rootVisualElement.Q<Button>("code-generate-custom-type-button");
            codeGenerateCustomTypeButton.clicked += GenerateCodeForCustomTypes;

            var codeGenerateButton = rootVisualElement.Q<Button>("code-generate-button");
            codeGenerateButton.clicked += GenerateCode;

            var exportButton = rootVisualElement.Q<Button>("export-button");
            exportButton.clicked += Export;

            if (!string.IsNullOrEmpty(gameDatabase.SourceDataPath))
            {
                Reload();
            }
        }

        private void Reload()
        {
            gameDatabase.Reload();
        }

        private void SetupList()
        {
            var container = rootVisualElement.Q<VisualElement>("data-table-view");
            container.Clear();

            for (var i = 0; i < gameDatabase.dataTables.Count; i++)
            {
                WorksheetData worksheetData = gameDatabase.dataTables[i];
                var element = new WorksheetDataButton(worksheetData);
                container.Add(element);
            }
        }

        private void ShowDataFolder()
        {
            string fullDataPath = Path.GetFullPath(gameDatabase.SourceDataPath, Application.dataPath + "/../");
            EditorUtility.RevealInFinder(fullDataPath);
        }

        private void PingCodeFolder()
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(gameDatabase.ExportScriptPath);
            EditorGUIUtility.PingObject(folder);
        }

        private void PingExportFolder()
        {
            Object folder = AssetDatabase.LoadAssetAtPath<Object>(gameDatabase.ExportDataPath);
            EditorGUIUtility.PingObject(folder);
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

        private void BrowseCodeFolder()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Select Code Folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }

            string assetPath = fullPath.Replace(Application.dataPath, "Assets");
            gameDatabase.SetExportScriptPath(assetPath);
            _textFieldExportScriptPath.SetValueWithoutNotify(gameDatabase.ExportScriptPath);
        }

        private void BrowseExportFolder()
        {
            string fullPath = EditorUtility.OpenFolderPanel("Select Export Folder", Application.dataPath, "");
            if (string.IsNullOrEmpty(fullPath))
            {
                return;
            }

            string assetPath = fullPath.Replace(Application.dataPath, "Assets");
            gameDatabase.SetExportDataPath(assetPath);
            _textFieldExportDataPath.SetValueWithoutNotify(gameDatabase.ExportDataPath);
        }

        private void OnDropdownSettingChanged(ChangeEvent<string> evt)
        {
            gameDatabase.SetSettingIndex(_dropdownSetting.index);
            _textFieldSourceDataPath.SetValueWithoutNotify(gameDatabase.SourceDataPath);
            _textFieldExportDataPath.SetValueWithoutNotify(gameDatabase.ExportDataPath);
            _textFieldExportScriptPath.SetValueWithoutNotify(gameDatabase.ExportScriptPath);
        }

        private void SelectAll()
        {
            gameDatabase.SelectAll();

            var container = rootVisualElement.Q<VisualElement>("data-table-view");
            container.Query<WorksheetDataButton>().ForEach(button =>
            {
                button.Refresh();
            });
        }

        private void DeselectAll()
        {
            gameDatabase.DeselectAll();

            var container = rootVisualElement.Q<VisualElement>("data-table-view");
            container.Query<WorksheetDataButton>().ForEach(button =>
            {
                button.Refresh();
            });
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
