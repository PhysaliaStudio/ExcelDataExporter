using UnityEngine;
using UnityEngine.UIElements;

namespace Physalia.ExcelDataExporter
{
    public class WorksheetDataButton : BindableElement
    {
        private static readonly Vector2 ButtonSize = new(150, 30);
        private static readonly Color SelectedColor = new Color32(0, 150, 0, 255);

        private readonly WorksheetData worksheetData;
        private readonly Button button;

        public WorksheetDataButton(WorksheetData worksheetData)
        {
            this.worksheetData = worksheetData;

            button = new Button(Switch) { text = worksheetData.Name };
            button.style.width = ButtonSize.x;
            button.style.height = ButtonSize.y;
            Refresh();
            Add(button);
        }

        private void Switch()
        {
            worksheetData.Switch();
            Refresh();
        }

        public void Refresh()
        {
            bool isSelected = worksheetData.IsSelected;
            button.style.backgroundColor = isSelected ? SelectedColor : StyleKeyword.Null;
        }
    }
}
