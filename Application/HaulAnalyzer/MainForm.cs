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
        private AGDataSet DataSetCopy;
        private List<Region> Regions = new List<Region>();

        public MainForm()
        {
            InitializeComponent();

            // these numbers are obtained by creating a breakline around the region in
            // optisurface and then exporting the table of values
            // these are in ft
            Region Reg = new Region();
            Reg.Vertices.Add(new PointD(747.543, 393.085));
            Reg.Vertices.Add(new PointD(717.257, 355.515));
            Reg.Vertices.Add(new PointD(696.172, 353.982));
            Reg.Vertices.Add(new PointD(641.733, 296.859));
            Reg.Vertices.Add(new PointD(619.498, 292.642));
            Reg.Vertices.Add(new PointD(610.297, 279.991));
            Reg.Vertices.Add(new PointD(591.128, 277.307));
            Reg.Vertices.Add(new PointD(580.394, 263.889));
            Reg.Vertices.Add(new PointD(560.458, 263.889));
            Reg.Vertices.Add(new PointD(552.408, 250.855));
            Reg.Vertices.Add(new PointD(514.071, 248.171));
            Reg.Vertices.Add(new PointD(494.519, 224.785));
            Reg.Vertices.Add(new PointD(455.415, 187.598));
            Reg.Vertices.Add(new PointD(-48.427, -317.290));
            Reg.Vertices.Add(new PointD(-74.897, -342.105));
            Reg.Vertices.Add(new PointD(-75.073, -424.078));
            Reg.Vertices.Add(new PointD(-89.447, -429.554));
            Reg.Vertices.Add(new PointD(-90.131, -592.452));
            Reg.Vertices.Add(new PointD(-112.718, -607.510));
            Reg.Vertices.Add(new PointD(-113.402, -647.893));
            Reg.Vertices.Add(new PointD(807.154, -651.191));
            Reg.Vertices.Add(new PointD(800.025, 392.575));
            Reg.Vertices.Add(new PointD(751.964, 392.574));
            Regions.Add(Reg);

            Reg = new Region();
            Reg.Vertices.Add(new PointD(-145.240, 722.625));
            Reg.Vertices.Add(new PointD(-141.716, 638.062));
            Reg.Vertices.Add(new PointD(-150.876, 520.379));
            Reg.Vertices.Add(new PointD(-178.359, 473.869));
            Reg.Vertices.Add(new PointD(-284.062, 359.709));
            Reg.Vertices.Add(new PointD(-285.471, 327.294));
            Reg.Vertices.Add(new PointD(-299.565, 299.811));
            Reg.Vertices.Add(new PointD(-333.390, 265.985));
            Reg.Vertices.Add(new PointD(-358.758, 257.529));
            Reg.Vertices.Add(new PointD(-427.113, 229.341));
            Reg.Vertices.Add(new PointD(-456.005, 206.086));
            Reg.Vertices.Add(new PointD(-487.546, 190.064));
            Reg.Vertices.Add(new PointD(-487.546, 190.064));
            Reg.Vertices.Add(new PointD(-591.024, 165.587));
            Reg.Vertices.Add(new PointD(-597.967, 407.202));
            Reg.Vertices.Add(new PointD(-594.456, 702.786));
            Reg.Vertices.Add(new PointD(-145.240, 722.625));
            Regions.Add(Reg);
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

            foreach (Region Reg in Regions)
            {
                foreach (PointD P in Reg.Vertices)
                {
                    // convert to m and add to master benchmark
                    P.x = (P.x * 0.3048) + DataSet.MasterBenchmark.UTMEasting;
                    P.y = (P.y * 0.3048) + DataSet.MasterBenchmark.UTMNorthing;
                }
            }

            Planner.SetRegions(Regions);

            CFMap = new CutFillMap(800, 800, GridSize);
            CFMap.SetRegions(Regions);
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
            Map = CFMap.Update(DataSetCopy, true);
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

        /// <summary>
        /// Called when user clicks on the button to save the data as a survey
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportSurveyBtn_Click(object sender, EventArgs e)
        {
            if (SaveSurveyDialog.ShowDialog() == DialogResult.OK)
            {
                SurveyExporter Exporter = new SurveyExporter();

                Exporter.Export(DataSetCopy, SaveSurveyDialog.FileName);
            }
        }
    }
}
