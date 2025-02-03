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
        private AGDataSet DataSet;
        private int MapWidthPx;
        private int MapHeightPx;
        Bitmap Map;


        public CutFillMap
            (
            AGDataSet DataSet,
            int WidthPx,
            int HeightPx
            )
        {
            this.DataSet = DataSet;
            this.MapWidthPx = WidthPx;
            this.MapHeightPx = HeightPx;

            Map = new Bitmap(MapWidthPx, MapHeightPx);
        }

        /// <summary>
        /// Updates the cut/fill map with the current data
        /// </summary>
        /// <returns></returns>
        public Bitmap Update
            (
            )
        {
            using (Graphics graph = Graphics.FromImage(Map))
            {
                int px;
                int py;

                Rectangle ImageSize = new Rectangle(0, 0, MapWidthPx, MapHeightPx);

                foreach (AGDEntry Entry in DataSet.Data)
                {
                    UTMToPixel(DataSet, Entry.UTMEasting, Entry.UTMNorthing, MapWidthPx, MapHeightPx, out px, out py);

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
                }

                UTMToPixel(DataSet, DataSet.MasterBenchmark.UTMEasting, DataSet.MasterBenchmark.UTMNorthing, MapWidthPx, MapHeightPx, out px, out py);
                graph.FillRectangle(Brushes.Black, (float)(px - 8), (float)(py - 8), 16, 16);

                foreach (AGDEntry Benchmark in DataSet.Benchmarks)
                {
                    UTMToPixel(DataSet, Benchmark.UTMEasting, Benchmark.UTMNorthing, MapWidthPx, MapHeightPx, out px, out py);
                    graph.FillRectangle(Brushes.Gray, (float)(px - 8), (float)(py - 8), 16, 16);
                }
            }

            return Map;
        }

        /// <summary>
        /// Converts a UTM coordinate into a pixel coordinate
        /// </summary>
        /// <param name="DataSet">Set of data that contains the UTM data</param>
        /// <param name="UTMEasting">UTM easting to convert</param>
        /// <param name="UTMNorthing">UTM northing to convert</param>
        /// <param name="MapWidthPx">Width of map in pixels</param>
        /// <param name="MapHeightPx">Height of map in pixels</param>
        /// <param name="px">On return set to pixel x coordinate</param>
        /// <param name="py">On return set to pixel y coordinate</param>
        private void UTMToPixel
            (
            AGDataSet DataSet,
            double UTMEasting,
            double UTMNorthing,
            int MapWidthPx,
            int MapHeightPx,
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

            // flip in y because bitmap origin is top left
            py = MapHeightPx - py;
        }
    }
}
