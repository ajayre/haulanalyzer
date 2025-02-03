using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace HaulAnalyzer
{
    internal class AGDImporter
    {
        /// <summary>
        /// Loads in an AGD file
        /// </summary>
        /// <param name="FileName">Path and name of file</param>
        /// <returns>Data set</returns>
        public AGDataSet Load
            (
            string FileName
            )
        {
            AGDataSet DataSet = new AGDataSet();

            if (!File.Exists(FileName))
            {
                throw new Exception(String.Format("Input file {0} not found", FileName));
            }

            char Delimiter = ',';

            // read csv file
            int LineNumber = 0;
            using (CsvReader Reader = new CsvReader(new StreamReader(new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)), true, Delimiter))
            {
                // get number of fields
                int FieldCount = Reader.FieldCount;
                if (FieldCount != 7)
                {
                    throw new Exception("File must have seven columns");
                }

                Reader.SkipEmptyLines = true;

                // get column header text
                string[] Headers = Reader.GetFieldHeaders();
                LineNumber++;

                // read in rows
                while (Reader.ReadNextRecord())
                {
                    LineNumber++;

                    string LatStr         = Reader[0].Trim();
                    string LonStr         = Reader[1].Trim();
                    string ExistingEleStr = Reader[2].Trim();
                    string ProposedEleStr = Reader[3].Trim();
                    string CutFillStr     = Reader[4].Trim();
                    string Code           = Reader[5];
                    string Comments       = Reader[6];

                    if (Code.Trim() == "0MB")
                    {
                    }

                    AGDEntry Entry = Parse(LatStr, LonStr, ExistingEleStr, ProposedEleStr, CutFillStr, Code, Comments, FileName, LineNumber);
                    if (Entry != null)
                    {
                        Geo.LLtoUTM(Entry.Lat, Entry.Lon, out Entry.UTMNorthing, out Entry.UTMEasting, out Entry.UTMZone);

                        switch (Entry.EntryType)
                        {
                            case AGDEntryType.MasterBenchmark:
                                DataSet.MasterBenchmark = Entry;
                                break;

                            case AGDEntryType.Benchmark:
                                DataSet.Benchmarks.Add(Entry);
                                break;

                            case AGDEntryType.Boundary:
                                DataSet.BoundaryPoints.Add(Entry);
                                break;

                            case AGDEntryType.GridPoint:
                                DataSet.Data.Add(Entry);
                                break;
                        }
                    }
                }
            }

            return DataSet;
        }

        /// <summary>
        /// Parses data into a AGD file entry
        /// </summary>
        /// <param name="LatitudeStr">String containing latitude in degrees</param>
        /// <param name="LongitudeStr">String containing longitude in degrees</param>
        /// <param name="ExistingEleStr">String containing existing elevation in meters</param>
        /// <param name="ProposedEleStr">String containing proposed elevation in meters</param>
        /// <param name="CutFillHeightStr">String containing the cut/fill height in meters</param>
        /// <param name="Code">Name for the point</param>
        /// <param name="Comments">Comments for the point</param>
        /// <param name="FileName">Path and name of the file the data came from</param>
        /// <param name="LineNumber">The line number in the file the data came from</param>
        /// <returns>A new AGD entry or null for none/skipped</returns>
        private AGDEntry Parse
            (
            string LatitudeStr,
            string LongitudeStr,
            string ExistingEleStr,
            string ProposedEleStr,
            string CutFillHeightStr,
            string Code,
            string Comments,
            string FileName,
            int LineNumber
            )
        {
            try
            {
                AGDEntry Entry = new AGDEntry();

                Entry.Code = Code.Trim();
                Entry.Comments = Comments.Trim();
                Entry.Lat = double.Parse(LatitudeStr);
                Entry.Lon = double.Parse(LongitudeStr);

                if (Code == "3GRD")
                {
                    Entry.EntryType = AGDEntryType.GridPoint;

                    Entry.CutFillHeight = double.Parse(CutFillHeightStr);

                    if (ExistingEleStr.Trim().Length == 0)
                        ExistingEleStr = ProposedEleStr;

                    Entry.ExistingEle = double.Parse(ExistingEleStr);
                    Entry.ProposedEle = double.Parse(ProposedEleStr);
                }
                else if (Code == "2PER")
                {
                    Entry.EntryType = AGDEntryType.Boundary;
                }
                else if (Code == "0MB")
                {
                    Entry.EntryType = AGDEntryType.MasterBenchmark;
                }
                else if (Code.StartsWith("0BM"))
                {
                    Entry.EntryType = AGDEntryType.Benchmark;
                }

                return Entry;
            }
            catch (Exception Exc)
            {
                throw new Exception(String.Format("Failed to parse CSV line: {1}", LineNumber, Exc.Message));
            }
        }
    }
}
