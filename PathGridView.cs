using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PathEditor
{
    public class PathGridView : DataGridView
    {

        protected override void OnCellValueChanged(DataGridViewCellEventArgs e)
        {
            doRowPathCheck(e.RowIndex);

            base.OnCellValueChanged(e);
        }

        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            for(int i = 0; i < e.RowCount; i++)
                doRowPathCheck(e.RowIndex + i);

            base.OnRowsAdded(e);
        }

        void doRowPathCheck(int rowId)
        {
            if (rowId < 0 || rowId >= NewRowIndex)
                return;

            var cell = Rows[rowId].Cells[0];
            if (cell == null)
                return;

            if(cell.Value == null || !Directory.Exists(Environment.ExpandEnvironmentVariables(cell.Value.ToString())))
                cell.ErrorText = "Directory does not exist";
            else
                cell.ErrorText = string.Empty;


            //update tooltip
            var c = Rows[rowId].Cells[0];
            c.ToolTipText = Environment.ExpandEnvironmentVariables(c.Value.ToString());
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteSelectedRows();

            base.OnKeyUp(e);
        }

        public void DeleteSelectedRows()
        {
            var z = SelectedCells.Cast<DataGridViewCell>()
                    .Select(c => c.RowIndex)
                    .Where(i => i != NewRowIndex)
                    .Distinct()
                    .ToList();

            foreach (var id in z)
                Rows.RemoveAt(id);
        }
    }
}
