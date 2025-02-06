using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaulAnalyzer
{
    /// <summary>
    /// Represents a high-precision 2D point
    /// </summary>
    internal class PointD
    {
        public double x;
        public double y;

        public PointD
            (
            double x,
            double y
            )
        {
            this.x = x;
            this.y = y;
        }
    }

    internal class AGDataSet
    {
        public List<AGDEntry> Data;
        public AGDEntry MasterBenchmark;
        public List<AGDEntry> Benchmarks;
        public List<AGDEntry> BoundaryPoints;
        public double GridSizeM;

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

            GridSizeM = 0;

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

        /// <summary>
        /// Gets an entry that is relative to a specific entry
        /// </summary>
        /// <param name="Target">Entry to search from</param>
        /// <param name="XOffset">X offset, negative to go west and positive to go east</param>
        /// <param name="YOffset">Y offset, negative to go south and positive to go north</param>
        /// <returns>Entry that is relative to the target entry, or null if outside of grid</returns>
        public AGDEntry GetRelativeEntry
            (
            AGDEntry Target,
            int XOffset,
            int YOffset
            )
        {
            AGDEntry Curr = Target;

            if (XOffset > 0)
            {
                for (int x = 0; x < XOffset; x++)
                {
                    Curr = Curr.East;
                    if (Curr == null) return null;
                }
            }
            else if (XOffset < 0)
            {
                for (int x = 0; x > XOffset; x--)
                {
                    Curr = Curr.West;
                    if (Curr == null) return null;
                }
            }

            if (YOffset > 0)
            {
                for (int y = 0; y < YOffset; y++)
                {
                    Curr = Curr.North;
                    if (Curr == null) return null;
                }
            }
            else if (YOffset < 0)
            {
                for (int y = 0; y > YOffset; y--)
                {
                    Curr = Curr.South;
                    if (Curr == null) return null;
                }
            }

            return Curr;
        }

        /// <summary>
        /// Gets all of the data entries in a specific radius of an entry
        /// </summary>
        /// <param name="Center">Center entry</param>
        /// <param name="Radius">Search radius in m</param>
        /// <returns>List of entries within the radius</returns>
        public List<AGDEntry> GetEntriesInRadius
            (
            AGDEntry Center,
            double Radius
            )
        {
            List<AGDEntry> Entries = new List<AGDEntry>();
            Entries.Add(Center);

            int RadiusGrid = (int)(Radius / GridSizeM);

            for (int x = -RadiusGrid; x < RadiusGrid; x++)
            {
                for (int y = -RadiusGrid; y < RadiusGrid; y++)
                {
                    AGDEntry Rel = GetRelativeEntry(Center, x, y);
                    if ((Rel != null) && (Rel.DistanceToEntry(Center) <= Radius))
                    {
                        Entries.Add(Rel);
                    }
                }
            }

            return Entries;
        }

        /// <summary>
        /// Determines if the given point is inside the polygon
        /// From: https://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        private bool IsPointInPolygon(PointD[] polygon, PointD testPoint)
        {
            bool result = false;
            int j = polygon.Length - 1;
            for (int i = 0; i < polygon.Length; i++)
            {
                if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y ||
                    polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
                {
                    if (polygon[i].x + (testPoint.y - polygon[i].y) /
                       (polygon[j].y - polygon[i].y) *
                       (polygon[j].x - polygon[i].x) < testPoint.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        /// <summary>
        /// Gets a list of entries that are inside a polygon
        /// </summary>
        /// <param name="SearchStartEntry">An entry inside or close to the polygon to start searching from</param>
        /// <param name="Vertices">Vertices of polygon in meters</param>
        /// <returns>Entries that are inside the polygon</returns>
        public List<AGDEntry> GetEntriesInsidePolygon
            (
            AGDEntry SearchStartEntry,
            PointD[] Vertices
            )
        {
            // get extents of polygon
            double? MaxX = null;
            double? MaxY = null;
            double? MinX = null;
            double? MinY = null;

            foreach (PointD Vertex in Vertices)
            {
                if (MaxX == null)
                    MaxX = Vertex.x;
                else if (Vertex.x > MaxX)
                    MaxX = Vertex.x;

                if (MinX == null)
                    MinX = Vertex.x;
                else if (Vertex.x < MinX)
                    MinX = Vertex.x;

                if (MaxY == null)
                    MaxY = Vertex.y;
                else if (Vertex.y > MaxY)
                    MaxY = Vertex.y;

                if (MinY == null)
                    MinY = Vertex.y;
                else if (Vertex.y < MinY)
                    MinY = Vertex.y;
            }

            double SearchRadius = MaxX.Value - MinX.Value;
            if ((MaxY.Value - MinY.Value) > SearchRadius) SearchRadius = MaxY.Value - MinY.Value;

            List<AGDEntry> SearchEntries = GetEntriesInRadius(SearchStartEntry, SearchRadius);

            List<AGDEntry> FoundEntries = new List<AGDEntry>();

            foreach (AGDEntry E in SearchEntries)
            {
                PointD CurrPoint = new PointD(E.UTMEasting, E.UTMNorthing);
                if (IsPointInPolygon(Vertices, CurrPoint))
                {
                    FoundEntries.Add(E);
                }
            }

            return FoundEntries;
        }

        /// <summary>
        /// Rotates a 2D point
        /// </summary>
        /// <param name="point">Point to rotate</param>
        /// <param name="origin">Origin of rotation</param>
        /// <param name="angle">Rotation angle</param>
        /// <returns>Rotated point</returns>
        private PointD RotatePoint(PointD point, PointD origin, float angle)
        {
            float anglerad = (float)(angle * Math.PI / 180.0);

            PointD translated = new PointD(point.x - origin.x, point.y - origin.y);
            PointD rotated = new PointD(
                translated.x * Math.Cos(anglerad) - translated.y * Math.Sin(anglerad),
                translated.x * Math.Sin(anglerad) + translated.y * Math.Cos(anglerad)
            );

            return new PointD(rotated.x + origin.x, rotated.y + origin.y);
        }

        /// <summary>
        /// Performs a cut
        /// </summary>
        /// <param name="Entry">Starting point for cut</param>
        /// <param name="CutDepth">Depth of cut in meters</param>
        /// <param name="CutLength">Length of cut in meters</param>
        /// <param name="CutWidth">Width of cut in meters</param>
        /// <param name="Angle">Direction of cut in degrees. 0 = E, 45 = NE, 90 = N, etc.</param>
        /// <returns>Amount of material cut in cubic m</returns>
        public double Cut
            (
            AGDEntry Entry,
            double CutDepth,
            double CutLength,
            double CutWidth,
            float Angle
            )
        {
            AGDEntry CurrY = Entry;

            double TotalCut = 0;

            // get bounds for cut
            PointD TopLeft = new PointD(Entry.UTMEasting, Entry.UTMNorthing);
            PointD TopRight = new PointD(Entry.UTMEasting + CutLength, Entry.UTMNorthing);
            PointD BottomRight = new PointD(Entry.UTMEasting + CutLength, Entry.UTMNorthing + CutWidth);
            PointD BottomLeft = new PointD(Entry.UTMEasting, Entry.UTMNorthing + CutWidth);
            TopLeft = RotatePoint(TopLeft, TopLeft, Angle);
            TopRight = RotatePoint(TopRight, TopLeft, Angle);
            BottomRight = RotatePoint(BottomRight, TopLeft, Angle);
            BottomLeft = RotatePoint(BottomLeft, TopLeft, Angle);

            // gets entries inside bound and cut them
            List<AGDEntry> Entries = GetEntriesInsidePolygon(Entry, new PointD[] { TopLeft, TopRight, BottomRight, BottomLeft });
            foreach (AGDEntry E in Entries)
            {
                if (E.CutFillHeight >= 0)
                {
                    // do nothing
                }
                else if (E.CutFillHeight < CutDepth)
                {
                    E.CutFillHeight += CutDepth;
                    //if (E.CutFillHeight >= 0 && E.CutFillHeight < 0.05) E.CutFillHeight = 0.0;
                    TotalCut += (GridSizeM * GridSizeM * CutDepth);
                }
                else
                {
                    TotalCut += (GridSizeM * GridSizeM * E.CutFillHeight);
                    E.CutFillHeight = 0.0;
                }
            }

            return TotalCut;
        }

        /// <summary>
        /// Performs a fill
        /// </summary>
        /// <param name="Entry">Starting point for fill</param>
        /// <param name="FillDepth">Height of fill in meters</param>
        /// <param name="FillLength">Length of fill in meters</param>
        /// <param name="FillWidth">Width of fill in meters</param>
        /// <param name="Angle">Direction of fill in degrees. 0 = E, 45 = NE, 90 = N, etc.</param>
        public void Fill
            (
            AGDEntry Entry,
            double FillDepth,
            double FillLength,
            double FillWidth,
            float Angle
            )
        {
            AGDEntry CurrY = Entry;

            // get bounds for cut
            PointD TopLeft = new PointD(Entry.UTMEasting, Entry.UTMNorthing);
            PointD TopRight = new PointD(Entry.UTMEasting + FillLength, Entry.UTMNorthing);
            PointD BottomRight = new PointD(Entry.UTMEasting + FillLength, Entry.UTMNorthing + FillWidth);
            PointD BottomLeft = new PointD(Entry.UTMEasting, Entry.UTMNorthing + FillWidth);
            TopLeft = RotatePoint(TopLeft, TopLeft, Angle);
            TopRight = RotatePoint(TopRight, TopLeft, Angle);
            BottomRight = RotatePoint(BottomRight, TopLeft, Angle);
            BottomLeft = RotatePoint(BottomLeft, TopLeft, Angle);

            // gets entries inside bound and cut them
            List<AGDEntry> Entries = GetEntriesInsidePolygon(Entry, new PointD[] { TopLeft, TopRight, BottomRight, BottomLeft });
            foreach (AGDEntry E in Entries)
            {
                if (E.CutFillHeight <= 0)
                {
                    // do nothing
                }
                else if (E.CutFillHeight > FillDepth)
                {
                    E.CutFillHeight -= FillDepth;
                }
                else
                {
                    E.CutFillHeight = 0.0;
                }
            }
        }
    }
}
