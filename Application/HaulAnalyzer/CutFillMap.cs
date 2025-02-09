using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HaulAnalyzer
{
    internal class CutFillMap
    {
        private int MapWidthPx;
        private int MapHeightPx;
        private Bitmap Map;
        private double GridSize;
        private List<Region> Regions;

        public CutFillMap
            (
            int WidthPx,
            int HeightPx,
            double GridSize
            )
        {
            MapWidthPx = WidthPx;
            MapHeightPx = HeightPx;
            this.GridSize = GridSize;

            Map = new Bitmap(MapWidthPx, MapHeightPx);
        }

        public void SetRegions
            (
            List<Region> Regions
            )
        {
            this.Regions = Regions;
        }

        /// <summary>
        /// Updates the cut/fill map with the current data
        /// </summary>
        /// <param name="ShowBenchmarks">true to display benchmarks</param>
        /// <returns>Bitmap containing map</returns>
        public Bitmap Update
            (
            AGDataSet DataSet,
            bool ShowBenchmarks
            )
        {
            using (Graphics graph = Graphics.FromImage(Map))
            {
                int px;
                int py;

                Rectangle ImageSize = new Rectangle(0, 0, MapWidthPx, MapHeightPx);

                foreach (AGDEntry Entry in DataSet.Data)
                {
                    UTMToPixel(DataSet, Entry.UTMEasting, Entry.UTMNorthing, out px, out py);

                    Brush PixelColor;
                    if (Entry.CutFillHeight >= 2.7 * 0.3048) PixelColor = Brushes.Violet;
                    else if (Entry.CutFillHeight >= 1.8 * 0.3048) PixelColor = Brushes.Indigo;
                    else if (Entry.CutFillHeight >= 0.9 * 0.3048) PixelColor = Brushes.Blue;
                    else if (Entry.CutFillHeight >= 0.05 * 0.3048) PixelColor = Brushes.Cyan;
                    else if (Entry.CutFillHeight >= -0.05 * 0.3048) PixelColor = Brushes.Green;
                    else if (Entry.CutFillHeight >= -0.9 * 0.3048) PixelColor = Brushes.Yellow;
                    else if (Entry.CutFillHeight >= -1.8 * 0.3048) PixelColor = Brushes.Orange;
                    else if (Entry.CutFillHeight >= -2.7 * 0.3048) PixelColor = Brushes.Red;
                    else PixelColor = Brushes.DarkRed;

                    graph.FillRectangle(PixelColor, (float)(px - 2), (float)(py - 2), 4, 4);
                    //graph.FillRectangle(PixelColor, (float)(px - 1), (float)(py - 1), 2, 2);
                }

                if (ShowBenchmarks)
                {
                    UTMToPixel(DataSet, DataSet.MasterBenchmark.UTMEasting, DataSet.MasterBenchmark.UTMNorthing, out px, out py);
                    DrawBenchmark(graph, Brushes.Black, px, py);

                    foreach (AGDEntry Benchmark in DataSet.Benchmarks)
                    {
                        UTMToPixel(DataSet, Benchmark.UTMEasting, Benchmark.UTMNorthing, out px, out py);
                        DrawBenchmark(graph, Brushes.Gray, px, py);
                    }
                }
            }

            return Map;
        }

        /// <summary>
        /// Converts a UTM coordinate into a pixel coordinate
        /// </summary>
        /// <param name="DataSet">Set of data being used</param>
        /// <param name="UTMEasting">UTM easting to convert</param>
        /// <param name="UTMNorthing">UTM northing to convert</param>
        /// <param name="px">On return set to pixel x coordinate</param>
        /// <param name="py">On return set to pixel y coordinate</param>
        private void UTMToPixel
            (
            AGDataSet DataSet,
            double UTMEasting,
            double UTMNorthing,
            out int px,
            out int py
            )
        {
            double MinX;
            double MinY;
            double MaxX;
            double MaxY;

            DataSet.GetUTMExtents(out MinX, out MinY, out MaxX, out MaxY);

            double UTMWidth = MaxX - MinX;
            double UTMHeight = MaxY - MinY;

            double X = UTMEasting - MinX;
            double Y = UTMNorthing - MinY;

            double PxPerMeter = (double)MapWidthPx / UTMWidth;

            px = (int)(X * PxPerMeter);
            py = (int)(Y * PxPerMeter);

            // flip y because bitmap origin is top left
            py = MapHeightPx - py;
        }

        private void DrawBenchmark
            (
            Graphics graph,
            Brush BrushColor,
            int px,
            int py
            )
        {
            graph.FillPolygon(BrushColor, new Point[] { new Point(px, py - 8), new Point(px - 8, py + 8), new Point(px + 8, py + 8) });
        }
    }
}
