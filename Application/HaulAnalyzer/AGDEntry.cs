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

        public AGDEntry
            (
            )
        {
        }

        public override string ToString()
        {
            return string.Format("{0}: {1},{2}: {3}", EntryType, Lat, Lon, CutFillHeight);
        }
    }
}
