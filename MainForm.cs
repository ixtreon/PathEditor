using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PathEditor
{
    public partial class MainForm : Form
    {
        const string VarName = "PATH";

        RegistryKey userKey = Registry.CurrentUser
            .OpenSubKey(@"Environment\");

        RegistryKey machineKey = Registry.LocalMachine
            .OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment\");

        string[] userData;
        string[] machineData;

        string defaultTitle;

        bool isDirty
        {
            set
            {
                btnSave.Enabled = value;
                Text = defaultTitle + (value ? " *" : string.Empty);
            }
        }

        public MainForm()
        {
            InitializeComponent();

            doAdminCheck();
            loadPaths();

            defaultTitle = Text;

            machineGrid.CellValueChanged += onGridValueChanged;
            userGrid.CellValueChanged += onGridValueChanged;
        }

        void onGridValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            isDirty = true;
        }

        void doAdminCheck()
        {
            if (!Program.IsAdministrator())
            {
                machineGrid.AllowUserToAddRows = false;
                machineGrid.AllowUserToDeleteRows = false;
                machineGrid.ReadOnly = true;

                tabControl1.TabPages[0].Text += " (Read Only)";
                tabControl1.TabPages[0].ToolTipText = "Run as administrator to make changes to this page.";
            }
        }

        public void loadPaths()
        {
            Func<RegistryKey, string[]> loadKey = (k) =>
                k.GetValue(VarName, "", RegistryValueOptions.DoNotExpandEnvironmentNames)
                .ToString()
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            Action<PathGridView, string[]> loadGridView = (v, dirs) =>
            {
                v.Rows.Clear();
                for(int i = 0; i < dirs.Length; i++)
                {
                    v.Rows.Add(dirs[i]);
                }
            };

            machineData = loadKey(machineKey);
            userData = loadKey(userKey);

            loadGridView(machineGrid, machineData);
            loadGridView(userGrid, userData);
        }


        string expand(string s)
        {
            return Environment.ExpandEnvironmentVariables(s);
        }

        public async void savePaths()
        {
            btnSave.Enabled = false;

            Cursor.Current = Cursors.WaitCursor;
            await Task.Run(() =>
            {
                Func<PathGridView, string> getRows = (gv) =>
                    gv.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Select(r => r.Cells[0].Value.ToString())
                    .Aggregate("", (a, b) => (a + ";" + b)) + ";";

                var md = getRows(machineGrid);
                var ud = getRows(userGrid);

                if (Program.IsAdministrator())
                    Environment.SetEnvironmentVariable(VarName, md, EnvironmentVariableTarget.Machine);
                Environment.SetEnvironmentVariable(VarName, ud, EnvironmentVariableTarget.User);
            });
            Cursor.Current = Cursors.Default;

            isDirty = false;
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            loadPaths();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            savePaths();
        }
    }
}
