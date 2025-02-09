using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HaulAnalyzer
{
    internal class SurveyExporter
    {
        /// <summary>
        /// Exports a data set as survey data for import into Optisurface
        /// </summary>
        /// <param name="DataSet">Set of data to export</param>
        /// <param name="FileName">Path and name of file to save</param>
        public void Export
            (
            AGDataSet DataSet,
            string FileName
            )
        {
            using (FileStream Stream = new FileStream(FileName, FileMode.Create))
            {
                using (StreamWriter Writer = new StreamWriter(Stream, new ASCIIEncoding()))
                {
                    int LineNumber = 1;

                    string LatLetter = "N";
                    if (DataSet.MasterBenchmark.Lat < 0) LatLetter = "S";
                    int LatDeg = (int)Math.Abs(DataSet.MasterBenchmark.Lat);
                    double LatMinDecimal = (Math.Abs(DataSet.MasterBenchmark.Lat) - LatDeg) * 60.0;
                    int LatMin = (int)LatMinDecimal;
                    double LatSec = (LatMinDecimal - LatMin) * 60.0;

                    string LonLetter = "E";
                    if (DataSet.MasterBenchmark.Lon < 0) LonLetter = "W";
                    int LonDeg = (int)Math.Abs(DataSet.MasterBenchmark.Lon);
                    double LonMinDecimal = (Math.Abs(DataSet.MasterBenchmark.Lon) - LonDeg) * 60.0;
                    int LonMin = (int)LonMinDecimal;
                    double LonSec = (LonMinDecimal - LonMin) * 60.0;

                    Writer.WriteLine("0001\t0.000\t0.000\t100.000\tMB {0}{1}:{2}:{3} / {4}{5}:{6}:{7}\t0.000",
                        LatLetter, LatDeg, LatMin, LatSec,
                        LonLetter, LonDeg, LonMin, LonSec);

                    // output benchmarks
                    int BenchmarkNumber = 1;
                    foreach (AGDEntry E in DataSet.Benchmarks)
                    {
                        Writer.WriteLine("{0}\t{1}\t{2}\t{3}\tBM{4}",
                            LineNumber++,
                            (E.UTMEasting - DataSet.MasterBenchmark.UTMEasting) * 3.28084,
                            (E.UTMNorthing - DataSet.MasterBenchmark.UTMNorthing) * 3.28084,
                            E.ExistingEle * 3.28084,
                            BenchmarkNumber++);
                    }

                    // output points
                    foreach (AGDEntry E in DataSet.Data)
                    {
                        Writer.WriteLine("{0}\t{1}\t{2}\t{3}",
                            LineNumber++,
                            (E.UTMEasting - DataSet.MasterBenchmark.UTMEasting) * 3.28084,
                            (E.UTMNorthing - DataSet.MasterBenchmark.UTMNorthing) * 3.28084,
                            (E.ProposedEle - E.CutFillHeight) * 3.28084);
                    }
                }
            }
        }
    }
}
