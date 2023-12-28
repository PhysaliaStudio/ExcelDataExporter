using System.Collections.Generic;
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
        private MaskField _dropdownExportFlags;

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
            _dropdownExportFlags = rootVisualElement.Q<MaskField>("dropdown-export-flags");
            _dropdownExportFlags.choices = settingNames;
            _dropdownExportFlags.value = gameDatabase.ExportFlags;
            _dropdownExportFlags.RegisterValueChangedCallback(OnDropdownExportFlagsChanged);

            var reloadButton = rootVisualElement.Q<Button>("reload-button");
            reloadButton.clicked += Reload;

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

            Reload();
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

        private void OnDropdownExportFlagsChanged(ChangeEvent<int> evt)
        {
            gameDatabase.SetExportFlags(evt.newValue);
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
