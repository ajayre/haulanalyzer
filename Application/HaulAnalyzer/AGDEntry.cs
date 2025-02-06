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

        public AGDEntry North;
        public AGDEntry South;
        public AGDEntry East;
        public AGDEntry West;

        public AGDEntry
            (
            )
        {
            North = South = East = West = null;
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
    }
}
