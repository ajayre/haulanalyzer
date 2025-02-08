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
        private HaulPlanner Planner = new HaulPlanner();
        private Bitmap Map;
        private CutFillMap CFMap;
        private AGDataSet DataSet;
        private double GridSize;
        AGDataSet DataSetCopy;

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
                    // grid size in feet
                    double GridSizeFt = 5.0;
                    // convert to meters
                    GridSize = GridSizeFt * 0.3048;

                    AGDImporter Importer = new AGDImporter();
                    Importer.ImportCompleted += ImporterCompleted;
                    Importer.ImportError += ImporterError;
                    Importer.Progress += ImporterProgress;
                    Importer.Import(ImportFileDialog.FileName, GridSize);
                }
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Called when the file import has completed
        /// </summary>
        /// <param name="sender">File loader object</param>
        /// <param name="DataSet">Set of data created</param>
        private void ImporterCompleted
            (
            object sender,
            AGDataSet DataSet
            )
        {
            this.DataSet = DataSet;

            CFMap = new CutFillMap(800, 800, GridSize);
            Map = CFMap.Update(DataSet, true);
            CutFillMapDisp.Image = Map;
        }

        /// <summary>
        /// Called when the file failed to import
        /// </summary>
        /// <param name="sender">File importer object</param>
        /// <param name="ErrorMessage">Error message</param>
        private void ImporterError
            (
            object sender,
            string ErrorMessage
            )
        {
            MessageBox.Show(ErrorMessage, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        /// <summary>
        /// Called when the importer wants to update the progress
        /// </summary>
        /// <param name="sender">Importer object</param>
        /// <param name="Progress">Percentage completed</param>
        private void ImporterProgress
            (
            object sender,
            int Progress
            )
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, int>(ImporterProgress), sender, Progress );
                return;
            }

            ProgressBar.Value = Progress;
        }

        /// <summary>
        /// Called when user clicks on the start/stop button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartStopBtn_Click(object sender, EventArgs e)
        {
            if (Planner.Running)
            {
                Planner.Stop();
                MapRefreshTimer.Enabled = false;
            }
            else
            {
                DataSetCopy = DataSet.Clone();
                Planner.Start(DataSetCopy, GridSize);
                MapRefreshTimer.Enabled = true;
            }
        }

        /// <summary>
        /// Called periodically to refresh the cut/fill map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapRefreshTimer_Tick(object sender, EventArgs e)
        {
            Map = CFMap.Update(DataSetCopy, false);
            CutFillMapDisp.Refresh();
        }

        /// <summary>
        /// Called when main form is closing
        /// Stops the planner if it is running
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Planner.Running) Planner.Stop();
        }
    }
}
