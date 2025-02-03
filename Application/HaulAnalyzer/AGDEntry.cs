using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaulAnalyzer
{
    internal class AGDEntry
    {
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
            double Lat,
            double Lon,
            double ExistingEle,
            double ProposedEle,
            double CutFillHeight,
            string Code,
            string Comments
            )
        {
            this.Lat = Lat;
            this.Lon = Lon;
            this.ExistingEle = ExistingEle;
            this.ProposedEle = ProposedEle;
            this.CutFillHeight = CutFillHeight;
            this.Code = Code;
            this.Comments = Comments;
        }

        public AGDEntry
            (
            )
        {
        }

        public override string ToString()
        {
            return string.Format("{0},{1}: {2}", Lat, Lon, CutFillHeight);
        }
    }
}
