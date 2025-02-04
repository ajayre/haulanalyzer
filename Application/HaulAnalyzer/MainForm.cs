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
                    // grid size in feet
                    double GridSizeFt = 5.0;
                    // convert to meters
                    double GridSize = GridSizeFt * 0.3048;

                    AGDImporter Importer = new AGDImporter();
                    AGDataSet DataSet = Importer.Load(ImportFileDialog.FileName, GridSize);

                    CutFillMap CFMap = new CutFillMap(DataSet, 800, 800, GridSize);
                    Bitmap Map = CFMap.Update(true);
                    CutFillMapDisp.Image = Map;

                    double CutDepth = 0.06096;  // 0.2' in meters
                    double ScraperWidth = 4.572;  // 15ft in meters
                    double ScraperCapacity = 20.0; // cu yd
                    double CutSwell = 1.3;

                    // how much we can cut in one go in cu m
                    double ScraperCut = (ScraperCapacity / CutSwell) * 0.764555;
                    // how long each cut is in m
                    double CutLength = ScraperCut / CutDepth / ScraperWidth;

                    int CutLengthGrid = (int)(CutLength / GridSize);
                    int CutWidthGrid = (int)(ScraperWidth / GridSize);

                    Random Rnd = new Random();
                    for (int pass = 0; pass < 10000000; pass++)
                    {
                        int Index = Rnd.Next(DataSet.Data.Count);
                        if (DataSet.Data[Index].CutFillHeight < 0)
                        {
                            DataSet.Data[Index].CutFillHeight += ScraperCut;

                            AGDEntry CurrY = DataSet.Data[Index];
                            for (int y = 0; y < CutWidthGrid; y++)
                            {
                                CurrY.CutFillHeight += ScraperCut;
                                if (CurrY.South != null) CurrY = CurrY.South;

                                AGDEntry CurrX = CurrY;
                                for (int x = 0; x < CutLengthGrid; x++)
                                {
                                    CurrX.CutFillHeight += ScraperCut;
                                    if (CurrX.West != null) CurrX = CurrX.West;
                                }
                            }

                            if (pass % 20 == 0)
                            {
                                Map = CFMap.Update(false);
                                CutFillMapDisp.Refresh();
                            }
                        }
                    }
                }
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
    }
}
