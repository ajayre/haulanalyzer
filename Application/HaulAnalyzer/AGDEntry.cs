using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaulAnalyzer
{
    internal enum AGDEntryType
    {
        MasterBenchmark,
        Benchmark,
        Boundary,
        GridPoint
    }

    internal class AGDEntry
    {
        public AGDEntryType EntryType;
        public double Lat;
        public double Lon;
        public double ExistingEle;
        public double ProposedEle;
        public double CutFillHeight;
        public string Code;
        public string Comments;

        public double UTMNorthing;
        public double UTMEasting;
        public string UTMZone;

        public int Index;

        public int North;
        public int South;
        public int East;
        public int West;

        public const int INVALID_INDEX = -1;

        public AGDEntry
            (
            )
        {
            North = South = East = West = INVALID_INDEX;
        }

        /// <summary>
        /// Clones the entry
        /// </summary>
        /// <returns>Clone of entry</returns>
        public AGDEntry Clone
            (
            )
        {
            AGDEntry NewEntry = new AGDEntry();
            NewEntry.EntryType = EntryType;
            NewEntry.Lat = Lat;
            NewEntry.Lon = Lon;
            NewEntry.ExistingEle = ExistingEle;
            NewEntry.ProposedEle = ProposedEle;
            NewEntry.CutFillHeight = CutFillHeight;
            NewEntry.Code = Code;
            NewEntry.Comments = Comments;
            NewEntry.UTMEasting = UTMEasting;
            NewEntry.UTMNorthing = UTMNorthing;
            NewEntry.UTMZone = UTMZone;
            NewEntry.Index = Index;
            NewEntry.North = North;
            NewEntry.South = South;
            NewEntry.East = East;
            NewEntry.West = West;

            return NewEntry;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1},{2}: {3}", EntryType, Lat, Lon, CutFillHeight);
        }

        /// <summary>
        /// Gets the distance between two entries in meters
        /// </summary>
        /// <param name="OtherEntry">The entry to measure to</param>
        /// <returns>Distance in meters</returns>
        public double DistanceToEntry
            (
            AGDEntry OtherEntry
            )
        {
            double X = Math.Abs(OtherEntry.UTMEasting - UTMEasting);
            double Y = Math.Abs(OtherEntry.UTMNorthing - UTMNorthing);

            return Math.Sqrt((X * X) + (Y * Y));
        }

        /// <summary>
        /// Gets the angle in degrees to another entry
        /// </summary>
        /// <param name="OtherEntry">The other entry to calculate to</param>
        /// <returns>The angle in degrees</returns>
        public double AngleToEntry
            (
            AGDEntry OtherEntry
            )
        {
            double X = Math.Abs(OtherEntry.UTMEasting - UTMEasting);
            double Y = Math.Abs(OtherEntry.UTMNorthing - UTMNorthing);

            double Angle = (Math.Atan(X / Y) * 180.0 / Math.PI) - 90;
            if (Angle < 0) Angle += 360.0;

            return Angle;
        }

        /// <summary>
        /// Checks if entry is inside a polygon
        /// From: https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        /// </summary>
        /// <param name="Polygon">List of vertices in UTM coordinates</param>
        /// <returns>true if inside polygon, false if outside</returns>
        public bool IsInsidePolygon
            (
            List<PointD> Polygon
            )
        {
            bool result = false;
            int j = Polygon.Count - 1;
            for (int i = 0; i < Polygon.Count; i++)
            {
                if (Polygon[i].y < UTMNorthing && Polygon[j].y >= UTMNorthing ||
                    Polygon[j].y < UTMNorthing && Polygon[i].y >= UTMNorthing)
                {
                    if (Polygon[i].x + (UTMNorthing - Polygon[i].y) /
                       (Polygon[j].y - Polygon[i].y) *
                       (Polygon[j].x - Polygon[i].x) < UTMEasting)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
    }
}
