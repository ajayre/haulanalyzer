using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HaulAnalyzer
{
    static internal class CutFillMap
    {
        static public Bitmap Generate
            (
            AGDataSet DataSet
            )
        {
            double MinX;
            double MinY;
            double MaxX;
            double MaxY;

            DataSet.GetUTMExtents(out MinX, out MinY, out MaxX, out MaxY);

            double UTMWidth = MaxX - MinX;
            double UTMHeight = MaxY - MinY;

            int MapWidthpx = 800;
            int MapHeightpx = 800;

            double PxPerMeter = (double)MapWidthpx / UTMWidth;
            double PointSpacingft = 5.0;
            double PxPerSpacing = PointSpacingft * 0.3048 * PxPerMeter;

            Bitmap Map = new Bitmap(MapWidthpx, MapHeightpx);

            using (Graphics graph = Graphics.FromImage(Map))
            {
                Rectangle ImageSize = new Rectangle(0, 0, MapWidthpx, MapHeightpx);
                //graph.FillRectangle(Brushes.White, ImageSize);

                //graph.FillRectangle(Brushes.Red, 0, 0, 1, 1);

                foreach (AGDEntry Entry in DataSet.Data)
                {
                    double X = Entry.UTMEasting - MinX;
                    double Y = Entry.UTMNorthing - MinY;

                    double px = X * (PxPerMeter / 2);
                    double py = Y * (PxPerMeter / 1);

                    // flip in y because bitmap origin is top left
                    py = MapHeightpx - py;

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

                    graph.FillRectangle(PixelColor, (float)px, (float)py, (float)(px + 1), (float)(py + 1));
                }
            }

            return Map;
        }
    }
}
