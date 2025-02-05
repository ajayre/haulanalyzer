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
                    DataSet = Importer.Load(ImportFileDialog.FileName, GridSize);

                    CFMap = new CutFillMap(DataSet, 800, 800, GridSize);
                    Map = CFMap.Update(true);
                    CutFillMapDisp.Image = Map;
                }
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Haul
            (
            AGDataSet DataSet,
            double GridSize,
            CutFillMap CFMap
            )
        {
            /*Random Rnd = new Random();
            int Index;
            do
            {
                Index = Rnd.Next(DataSet.Data.Count);
            } while (DataSet.Data[Index].CutFillHeight >= 0);


            for (int pass = 0; pass < 1000; pass++)
            {
                Cut(DataSet.Data[Index], CutWidthGrid, CutLengthGrid, CutDepth);
                Map = CFMap.Update(false);
                CutFillMapDisp.Refresh();
            }*/
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
                Planner.Start(DataSet, GridSize);
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
            Map = CFMap.Update(false);
            CutFillMapDisp.Refresh();
        }
    }
}
