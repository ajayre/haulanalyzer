using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace HaulAnalyzer
{
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

            while (TerminateRequest == false)
            {
                AGDEntry CutSpot = GetCutSpot();
                AGDEntry FillSpot = GetFillSpot();

                // nothing left to cut
                if (CutSpot == null) break;

                if (FillSpot != null)
                {
                    // fixme - remove x100
                    TotalCut += DataSet.Cut(CutSpot, CutDepth, CutLength, ScraperWidth, Rnd.Next(359));
                    DataSet.Fill(FillSpot, CutDepth, CutLength, ScraperWidth, Rnd.Next(359));
                }

                Thread.Sleep(1);
            }

            // convert total moved to cu yd
            TotalCut = TotalCut / 0.764555;

            TerminateRequest = false;
            _Running = false;
        }

        private AGDEntry GetCutSpot
            (
            )
        {
            AGDEntry HighestEntry = null;

            foreach (AGDEntry E in DataSet.Data)
            {
                if (E.CutFillHeight < 0)
                {
                    if (HighestEntry == null)
                    {
                        HighestEntry = E;
                    }
                    else if (E.CutFillHeight < HighestEntry.CutFillHeight)
                    {
                        HighestEntry = E;
                    }
                }
            }

            return HighestEntry;
        }

        private AGDEntry GetFillSpot
            (
            )
        {
            AGDEntry LowestEntry = null;

            foreach (AGDEntry E in DataSet.Data)
            {
                if (E.CutFillHeight > 0)
                {
                    if (LowestEntry == null)
                    {
                        LowestEntry = E;
                    }
                    else if (E.CutFillHeight > LowestEntry.CutFillHeight)
                    {
                        LowestEntry = E;
                    }
                }
            }

            return LowestEntry;
        }
    }
}
