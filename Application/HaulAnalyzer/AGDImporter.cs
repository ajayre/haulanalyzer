using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LumenWorks.Framework.IO.Csv;

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
            this.Lat           = Lat;
            this.Lon           = Lon;
            this.ExistingEle   = ExistingEle;
            this.ProposedEle   = ProposedEle;
            this.CutFillHeight = CutFillHeight;
            this.Code          = Code;
            this.Comments      = Comments;
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

    internal class AGDImporter
    {
        public List<AGDEntry> Entries = new List<AGDEntry>();
        public double? MasterBenchmarkLatitude;
        public double? MasterBenchmarkLongitude;

        public AGDImporter
            (
            )
        {
            MasterBenchmarkLatitude = null;
            MasterBenchmarkLongitude = null;
        }

        /// <summary>
        /// Loads in an AGD file
        /// </summary>
        /// <param name="FileName">Path and name of file</param>
        public void Load
            (
            string FileName
            )
        {
            if (!File.Exists(FileName))
            {
                throw new Exception(String.Format("Input file {0} not found", FileName));
            }

            Entries.Clear();
            MasterBenchmarkLatitude = null;
            MasterBenchmarkLongitude = null;

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

                    AGDEntry Entry = Parse(Reader[0], Reader[1], Reader[2], Reader[3], Reader[4], Reader[5], Reader[6], FileName, LineNumber);

                    if (Entry != null)
                    {
                        Geo.LLtoUTM(Entry.Lat, Entry.Lon, out Entry.UTMNorthing, out Entry.UTMEasting, out Entry.UTMZone);
                        Entries.Add(Entry);
                    }
                }
            }
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

                if (Code.Trim() == "0MB")
                {
                    MasterBenchmarkLatitude = double.Parse(LatitudeStr);
                    MasterBenchmarkLongitude = double.Parse(LongitudeStr);
                    return null;
                }

                if (ExistingEleStr.Trim().Length == 0)
                    ExistingEleStr = ProposedEleStr;

                if (ProposedEleStr.Trim().Length == 0) return null;

                Entry.Lat = double.Parse(LatitudeStr);
                Entry.Lon = double.Parse(LongitudeStr);
                Entry.ExistingEle = double.Parse(ExistingEleStr);
                Entry.ProposedEle = double.Parse(ProposedEleStr);
                Entry.CutFillHeight = double.Parse(CutFillHeightStr);
                Entry.Code = Code.Trim();
                Entry.Comments = Comments.Trim();

                return Entry;
            }
            catch (Exception Exc)
            {
                throw new Exception(String.Format("Failed to parse CSV line: {1}", LineNumber, Exc.Message));
            }
        }
    }
}
