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

            var exportButton = visualElement.Q<Button>("export-button");
            exportButton.clicked += Export;
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

        private void Export()
        {
            gameDatabase.ExportSelectedTables();
        }
    }
}
