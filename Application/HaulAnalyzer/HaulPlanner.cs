using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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

            while (TerminateRequest == false)
            {
                int Index = Rnd.Next(DataSet.Data.Count);
                if (DataSet.Data[Index].CutFillHeight < 0)
                {
                    Cut(DataSet.Data[Index], CutWidthGrid, CutLengthGrid, CutDepth);
                }

                Thread.Sleep(1);
            }

            TerminateRequest = false;
            _Running = false;
        }

        private void Cut
            (
            AGDEntry Entry,
            int CutWidthGrid,
            int CutLengthGrid,
            double CutDepth
            )
        {
            AGDEntry CurrY = Entry;

            for (int y = 0; y < CutWidthGrid; y++)
            {
                if (CurrY.CutFillHeight != 0)
                {
                    if (CurrY.CutFillHeight < CutDepth)
                        CurrY.CutFillHeight += CutDepth;
                    else
                        CurrY.CutFillHeight = 0.0;
                }

                AGDEntry CurrX = CurrY;
                for (int x = 0; x < CutLengthGrid; x++)
                {
                    if (CurrX.CutFillHeight != 0)
                    {
                        if (CurrX.CutFillHeight < CutDepth)
                            CurrX.CutFillHeight += CutDepth;
                        else
                            CurrX.CutFillHeight = 0.0;
                    }

                    if (CurrX.West != null) CurrX = CurrX.West;
                }
                if (CurrY.South != null) CurrY = CurrY.South;
            }
        }

        private void Fill
            (
            AGDEntry Entry,
            int CutWidthGrid,
            int CutLengthGrid,
            double CutDepth
            )
        {
            AGDEntry CurrY = Entry;
            for (int y = 0; y < CutWidthGrid; y++)
            {
                CurrY.CutFillHeight -= CutDepth;

                AGDEntry CurrX = CurrY;
                for (int x = 0; x < CutLengthGrid; x++)
                {
                    CurrX.CutFillHeight -= CutDepth;
                    if (CurrX.West != null) CurrX = CurrX.West;
                }
                if (CurrY.South != null) CurrY = CurrY.South;
            }
        }

        private AGDEntry GetHighPoint
            (
            AGDataSet DataSet
            )
        {
            foreach (AGDEntry E in DataSet.Data)
            {
                if (E.CutFillHeight < 0) return E;
            }

            return null;
        }

        private AGDEntry GetLowPoint
            (
            AGDataSet DataSet
            )
        {
            foreach (AGDEntry E in DataSet.Data)
            {
                if (E.CutFillHeight > 0) return E;
            }

            return null;
        }
    }
}
