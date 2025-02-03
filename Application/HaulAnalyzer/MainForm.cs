using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaulAnalyzer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when user chooses menu item to close application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Called when user chooses to import a data file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void importCutFillFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (ImportFileDialog.ShowDialog() == DialogResult.OK)
                {
                    AGDImporter Importer = new AGDImporter();
                    AGDataSet DataSet = Importer.Load(ImportFileDialog.FileName);

                    Bitmap Map = CutFillMap.Generate(DataSet);
                    CutFillMapDisp.Image = Map;
                }
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
