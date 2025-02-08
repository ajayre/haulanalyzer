using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace HaulAnalyzer
{
    internal class HaulRoute
    {
        public AGDEntry Start;
        public AGDEntry End;
        public double DistanceM;
        public int Score;

        public HaulRoute
            (
            )
        {
            Start = null;
            End = null;
            DistanceM = 0;
            Score = 0;
        }
    }

    internal class HaulPlanner
    {
        private const int STOPTIMEOUT = 4000;

        private bool _Running = false;
        public bool Running
        {
            get { return _Running; }
        }

        private Thread PlannerThread;
        private bool TerminateRequest;
        private AGDataSet DataSet;
        private double GridSize;
        private Random Rnd;

        private List<PointD> Region1;
        private List<PointD> Region2;
     
        public HaulPlanner
            (
            )
        {
            Rnd = new Random();
        }

        /// <summary>
        /// Starts the planner
        /// </summary>
        public void Start
            (
            AGDataSet DataSet,
            double GridSize
            )
        {
            if (_Running)
            {
                Stop();
            }

            this.DataSet = DataSet;
            this.GridSize = GridSize;

            // these numbers are obtained by creating a breakline around the region in
            // optisurface and then exporting the table of values
            // these are in ft
            Region1 = new List<PointD>();
            Region1.Add(new PointD(747.543, 393.085));
            Region1.Add(new PointD(717.257, 355.515));
            Region1.Add(new PointD(696.172, 353.982));
            Region1.Add(new PointD(641.733, 296.859));
            Region1.Add(new PointD(619.498, 292.642));
            Region1.Add(new PointD(610.297, 279.991));
            Region1.Add(new PointD(591.128, 277.307));
            Region1.Add(new PointD(580.394, 263.889));
            Region1.Add(new PointD(560.458, 263.889));
            Region1.Add(new PointD(552.408, 250.855));
            Region1.Add(new PointD(514.071, 248.171));
            Region1.Add(new PointD(494.519, 224.785));
            Region1.Add(new PointD(455.415, 187.598));
            Region1.Add(new PointD(-48.427, -317.290));
            Region1.Add(new PointD(-74.897, -342.105));
            Region1.Add(new PointD(-75.073, -424.078));
            Region1.Add(new PointD(-89.447, -429.554));
            Region1.Add(new PointD(-90.131, -592.452));
            Region1.Add(new PointD(-112.718, -607.510));
            Region1.Add(new PointD(-113.402, -647.893));
            Region1.Add(new PointD(807.154, -651.191));
            Region1.Add(new PointD(800.025, 392.575));
            Region1.Add(new PointD(751.964, 392.574));

            Region2 = new List<PointD>();
            Region2.Add(new PointD(-145.240, 722.625));
            Region2.Add(new PointD(-141.716, 638.062));
            Region2.Add(new PointD(-150.876, 520.379));
            Region2.Add(new PointD(-178.359, 473.869));
            Region2.Add(new PointD(-284.062, 359.709));
            Region2.Add(new PointD(-285.471, 327.294));
            Region2.Add(new PointD(-299.565, 299.811));
            Region2.Add(new PointD(-333.390, 265.985));
            Region2.Add(new PointD(-358.758, 257.529));
            Region2.Add(new PointD(-427.113, 229.341));
            Region2.Add(new PointD(-456.005, 206.086));
            Region2.Add(new PointD(-487.546, 190.064));
            Region2.Add(new PointD(-487.546, 190.064));
            Region2.Add(new PointD(-591.024, 165.587));
            Region2.Add(new PointD(-597.967, 407.202));
            Region2.Add(new PointD(-594.456, 702.786));
            Region2.Add(new PointD(-145.240, 722.625));

            foreach (PointD P in Region1)
            {
                // convert to m and add to master benchmark
                P.x = (P.x * 0.3048) + DataSet.MasterBenchmark.UTMEasting;
                P.y = (P.y * 0.3048) + DataSet.MasterBenchmark.UTMNorthing;
            }

            foreach (PointD P in Region2)
            {
                // convert to m and add to master benchmark
                P.x = (P.x * 0.3048) + DataSet.MasterBenchmark.UTMEasting;
                P.y = (P.y * 0.3048) + DataSet.MasterBenchmark.UTMNorthing;
            }

            TerminateRequest = false;
            PlannerThread = new Thread(new ThreadStart(Planner));
            PlannerThread.Name = "Planner thread";
            PlannerThread.Start();
        }

        /// <summary>
        /// Stops the planner
        /// </summary>
        public void Stop
            (
            )
        {
            TerminateRequest = true;
            DateTime Start = DateTime.Now;
            while (_Running)
            {
                Thread.Sleep(10);

                if ((DateTime.Now - Start).Milliseconds > STOPTIMEOUT) break;
            }
        }

        private void Planner
            (
            )
        {
            _Running = true;

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

            double TotalCut = 0;

            double MinX;
            double MinY;
            double MaxX;
            double MaxY;
            DataSet.GetUTMExtents(out MinX, out MinY, out MaxX, out MaxY);

            HaulRoute LastRoute = null;

            int pass = 0;

            while (TerminateRequest == false)
            {
                // get a list of possible routes to choose from
                List<HaulRoute> Routes = new List<HaulRoute>();
                for (int i = 0; i < 1000; i++)
                {
                    HaulRoute Route = CreateRoute();
                    if (Route != null) Routes.Add(Route);
                }
                if (Routes.Count == 0) break;

                // score each route
                foreach (HaulRoute R in Routes)
                {
                    // prefer shorter routes
                    if (R.DistanceM > 100) R.Score -= 80;
                    else if (R.DistanceM > 80) R.Score -= 70;
                    else if (R.DistanceM > 60) R.Score -= 60;
                    else if (R.DistanceM > 40) R.Score -= 50;
                    else if (R.DistanceM > 20) R.Score -= 40;
                    else if (R.DistanceM > 10) R.Score -= 20;

                    // prefer higher ground for cut
                    if (R.Start.CutFillHeight < -1.0) R.Score += 90;
                    else if (R.Start.CutFillHeight < -0.8) R.Score += 60;
                    else if (R.Start.CutFillHeight < -0.5) R.Score += 50;
                    else if (R.Start.CutFillHeight < -0.3) R.Score += 40;
                    else if (R.Start.CutFillHeight < -0.2) R.Score += 30;
                    else if (R.Start.CutFillHeight < -0.1) R.Score += 10;

                    // prefer lower ground for fill
                    if (R.End.CutFillHeight > 1.0) R.Score += 40;
                    else if (R.End.CutFillHeight > 0.5) R.Score += 30;
                    else if (R.End.CutFillHeight > 0.3) R.Score += 20;
                    else if (R.End.CutFillHeight > 0.2) R.Score += 10;
                    else if (R.End.CutFillHeight > 0.1) R.Score += 5;

                    // prefer easterly for fill
                    double DistanceFromEdge = (MaxX - MinX) - (R.End.UTMEasting - MinX);
                    if (DistanceFromEdge < 50) R.Score += 50;
                    else if (DistanceFromEdge < 100) R.Score += 40;
                    else if (DistanceFromEdge < 200) R.Score += 30;
                    else if (DistanceFromEdge < 300) R.Score += 20;
                    else if (DistanceFromEdge < 400) R.Score += 10;

                    // prefer easterly for cut
                    DistanceFromEdge = (MaxX - MinX) - (R.Start.UTMEasting - MinX);
                    if (DistanceFromEdge < 50) R.Score += 50;
                    else if (DistanceFromEdge < 100) R.Score += 40;
                    else if (DistanceFromEdge < 200) R.Score += 30;
                    else if (DistanceFromEdge < 300) R.Score += 20;
                    else if (DistanceFromEdge < 400) R.Score += 10;

                    if (LastRoute != null)
                    {
                        // prefer start that is close to previous start
                        double DistanceFromLastStart = R.Start.DistanceToEntry(LastRoute.Start);
                        if (DistanceFromLastStart > 400) R.Score -= 80;
                        else if (DistanceFromLastStart > 300) R.Score -= 70;
                        else if (DistanceFromLastStart > 200) R.Score -= 60;
                        else if (DistanceFromLastStart > 100) R.Score -= 50;
                        else if (DistanceFromLastStart > 50) R.Score -= 40;
                        else if (DistanceFromLastStart > 20) R.Score -= 30;

                        // prefer end that is close to previous end
                        double DistanceFromLastEnd = R.End.DistanceToEntry(LastRoute.End);
                        if (DistanceFromLastEnd > 400) R.Score -= 80;
                        else if (DistanceFromLastEnd > 300) R.Score -= 70;
                        else if (DistanceFromLastEnd > 200) R.Score -= 60;
                        else if (DistanceFromLastEnd > 100) R.Score -= 50;
                        else if (DistanceFromLastEnd > 50) R.Score -= 40;
                        else if (DistanceFromLastEnd > 20) R.Score -= 30;
                    }

                    /*// favour a start that is surrounded by equally high ground
                    List<AGDEntry> NearStartEntries = DataSet.GetEntriesInRadius(R.Start, 20);
                    foreach (AGDEntry E in NearStartEntries)
                    {
                        if (E.CutFillHeight < -1.0) R.Score += 5;
                    }*/

                    /*// favour direction based on region
                    if (R.Start.IsInsidePolygon(Region1))
                    {
                        double Angle = R.Start.AngleToEntry(R.End);
                        if (Angle > 260 && Angle < 280) R.Score += 40;
                    }
                    else if (R.Start.IsInsidePolygon(Region2))
                    {
                        double Angle = R.Start.AngleToEntry(R.End);
                        if (Angle > 260 && Angle < 280) R.Score += 40;
                    }
                    else
                    {
                        double Angle = R.Start.AngleToEntry(R.End);
                        if (Angle > 170 && Angle < 190) R.Score += 40;
                    }*/

                    /*if (pass % 2 == 0)
                    {
                        if (R.Start.IsInsidePolygon(Region1)) R.Score -= 100;
                        else if (R.Start.IsInsidePolygon(Region2)) R.Score -= 100;
                    }
                    else
                    {
                        if (R.Start.IsInsidePolygon(Region1)) R.Score += 100;
                        else if (R.Start.IsInsidePolygon(Region2)) R.Score += 40;
                    }*/
                }

                // sort so that highest scoring route is first
                List<HaulRoute> SortedRoutes = Routes.OrderByDescending(o => o.Score).ToList();

                // use highest scoring route
                double CutAngle = SortedRoutes[0].Start.AngleToEntry(SortedRoutes[0].End);

                TotalCut += DataSet.Cut(SortedRoutes[0].Start, CutDepth, CutLength, ScraperWidth, CutAngle);
                DataSet.Fill(SortedRoutes[0].End, CutDepth, CutLength, ScraperWidth, CutAngle);

                LastRoute = SortedRoutes[0];
                pass++;

                Thread.Sleep(0);
            }

            // convert total moved to cu yd
            TotalCut = TotalCut / 0.764555;

            TerminateRequest = false;
            _Running = false;
        }

        /// <summary>
        /// Creates a route from a cut to a fill
        /// </summary>
        /// <returns>New route</returns>
        private HaulRoute CreateRoute
            (
            )
        {
            HaulRoute Route = new HaulRoute();

            int TotalEntries = DataSet.Data.Count;

            // get a start point that needs a cut
            for (int pass = 0; pass < TotalEntries; pass++)
            {
                int Index = Rnd.Next(TotalEntries);
                if (DataSet.Data[Index].CutFillHeight < 0)
                {
                    Route.Start = DataSet.Data[Index];
                    break;
                }
            }
            if (Route.Start == null) return null;

            // get an end point that needs a fill
            for (int pass = 0; pass < TotalEntries; pass++)
            {
                int Index = Rnd.Next(TotalEntries);
                if (DataSet.Data[Index].CutFillHeight > 0)
                {
                    Route.End = DataSet.Data[Index];
                    break;
                }
            }
            if (Route.End == null) return null;

            // start and end must be inside the same region
            if (Route.Start.IsInsidePolygon(Region1) && !Route.End.IsInsidePolygon(Region1)) return null;
            if (!Route.Start.IsInsidePolygon(Region1) && Route.End.IsInsidePolygon(Region1)) return null;

            if (Route.Start.IsInsidePolygon(Region2) && !Route.End.IsInsidePolygon(Region2)) return null;
            if (!Route.Start.IsInsidePolygon(Region2) && Route.End.IsInsidePolygon(Region2)) return null;

            Route.DistanceM = Route.Start.DistanceToEntry(Route.End);

            return Route;
        }
    }
}
