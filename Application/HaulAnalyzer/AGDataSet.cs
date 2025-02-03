using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaulAnalyzer
{
    internal class AGDataSet
    {
        public List<AGDEntry> Data;
        public AGDEntry MasterBenchmark;
        public List<AGDEntry> Benchmarks;
        public List<AGDEntry> BoundaryPoints;

        private bool _ExtentsCalculated;
        private double _MinX;
        private double _MinY;
        private double _MaxX;
        private double _MaxY;
 
        public AGDataSet
            (
            )
        {
            Data = new List<AGDEntry>();
            MasterBenchmark = new AGDEntry();
            Benchmarks = new List<AGDEntry>();
            BoundaryPoints = new List<AGDEntry>();

            _ExtentsCalculated = false;
        }

        /// <summary>
        /// Gets the extents of the data in UTM coords
        /// </summary>
        /// <param name="MinX">On return set to smallest east/west point</param>
        /// <param name="MinY">On return set to smallest north/south point</param>
        /// <param name="MaxX">On return set to largest east/west point</param>
        /// <param name="MaxY">On return set to the largest east/west point</param>
        public void GetUTMExtents
            (
            out double MinX,
            out double MinY,
            out double MaxX,
            out double MaxY
            )
        {
            if (!_ExtentsCalculated)
            {
                double? minx = null;
                double? miny = null;
                double? maxx = null;
                double? maxy = null;

                foreach (AGDEntry Entry in Data)
                {
                    if (!minx.HasValue || Entry.UTMEasting < minx) minx = Entry.UTMEasting;
                    if (!maxx.HasValue || Entry.UTMEasting > maxx) maxx = Entry.UTMEasting;

                    if (!miny.HasValue || Entry.UTMNorthing < miny) miny = Entry.UTMNorthing;
                    if (!maxy.HasValue || Entry.UTMNorthing > maxy) maxy = Entry.UTMNorthing;
                }

                _MinX = MinX = minx.HasValue ? minx.Value : 0.0;
                _MaxX = MaxX = maxx.HasValue ? maxx.Value : 0.0;

                _MinY = MinY = miny.HasValue ? miny.Value : 0.0;
                _MaxY = MaxY = maxy.HasValue ? maxy.Value : 0.0;

                _ExtentsCalculated = true;
            }
            else
            {
                MinX = _MinX;
                MaxX = _MaxX;
                MinY = _MinY;
                MaxY = _MaxY;
            }
        }
    }
}
