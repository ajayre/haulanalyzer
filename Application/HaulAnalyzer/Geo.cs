using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaulAnalyzer
{
    internal class Geo
    {
        /// <summary>
        /// Converts lat/lon to UTM coordinates
        /// WGS-84
        /// Adapted from http://www.gpsy.com/gpsinfo/geotoutm/
        /// </summary>
        /// <param name="Lat">Latitude to convert</param>
        /// <param name="Long">Longitude to convert</param>
        /// <param name="UTMNorthing">On return set to UTM northing</param>
        /// <param name="UTMEasting">On return set to UTM easting</param>
        /// <param name="UTMZone">On return set to UTM zone</param>
        static public void LLtoUTM
            (
            double Lat,
            double Long,
            out double UTMNorthing,
            out double UTMEasting,
            out string UTMZone
            )
        {
            //converts lat/long to UTM coords.  Equations from USGS Bulletin 1532 
            //East Longitudes are positive, West longitudes are negative. 
            //North latitudes are positive, South latitudes are negative
            //Lat and Long are in decimal degrees
            //Written by Chuck Gantz- chuck.gantz@globalstar.com

            double deg2rad = Math.PI / 180;

            double a = 6378137; // equatorial radius WGS-84
            double eccSquared = 0.00669438; // eccentricity squared WGS-84

            double k0 = 0.9996;

            double LongOrigin;
            double eccPrimeSquared;
            double N, T, C, A, M;

            //Make sure the longitude is between -180.00 .. 179.9
            double LongTemp = (Long + 180) - (int)((Long + 180) / 360) * 360 - 180; // -180.00 .. 179.9;

            double LatRad = Lat * deg2rad;
            double LongRad = LongTemp * deg2rad;
            double LongOriginRad;
            int ZoneNumber;

            ZoneNumber = (int)((LongTemp + 180)/6) + 1;
  
	        if(Lat >= 56.0 && Lat< 64.0 && LongTemp >= 3.0 && LongTemp< 12.0 )
                ZoneNumber = 32;

            // Special zones for Svalbard
	        if(Lat >= 72.0 && Lat< 84.0 ) 
	        {
	            if(LongTemp >= 0.0  && LongTemp<  9.0 ) ZoneNumber = 31;
	            else if(LongTemp >= 9.0  && LongTemp< 21.0 ) ZoneNumber = 33;
	            else if(LongTemp >= 21.0 && LongTemp< 33.0 ) ZoneNumber = 35;
	            else if(LongTemp >= 33.0 && LongTemp< 42.0 ) ZoneNumber = 37;
	        }
            LongOrigin = (ZoneNumber - 1)*6 - 180 + 3;  //+3 puts origin in middle of zone
	        LongOriginRad = LongOrigin* deg2rad;

            //compute the UTM Zone from the latitude and longitude
            UTMZone = string.Format("{0}{1}", ZoneNumber, UTMLetterDesignator(Lat));

            eccPrimeSquared = (eccSquared)/(1-eccSquared);

	        N = a/Math.Sqrt(1-eccSquared* Math.Sin(LatRad) * Math.Sin(LatRad));
            T = Math.Tan(LatRad)* Math.Tan(LatRad);
            C = eccPrimeSquared* Math.Cos(LatRad)* Math.Cos(LatRad);
            A = Math.Cos(LatRad)* (LongRad-LongOriginRad);

	        M = a* ((1	- eccSquared/4		- 3* eccSquared* eccSquared/64	- 5* eccSquared* eccSquared* eccSquared/256)* LatRad
				        - (3* eccSquared/8	+ 3* eccSquared* eccSquared/32	+ 45* eccSquared* eccSquared* eccSquared/1024)* Math.Sin(2*LatRad)
									        + (15* eccSquared* eccSquared/256 + 45* eccSquared* eccSquared* eccSquared/1024)* Math.Sin(4*LatRad)
									        - (35* eccSquared* eccSquared* eccSquared/3072)* Math.Sin(6*LatRad));
	
	        UTMEasting = (double) (k0* N* (A+(1-T+C)* A* A* A/6
					        + (5-18* T+T* T+72* C-58* eccPrimeSquared)* A* A* A* A* A/120)
					        + 500000.0);

	        UTMNorthing = (double) (k0*(M+N* Math.Tan(LatRad)* (A* A/2+(5-T+9* C+4* C* C)* A* A* A* A/24
				         + (61-58* T+T* T+600* C-330* eccPrimeSquared)* A* A* A* A* A* A/720)));
	        if(Lat< 0)
                UTMNorthing += 10000000.0; //10000000 meter offset for southern hemisphere
        }

        /// <summary>
        /// Converts latitude to a UTM letter designator
        /// Adapted from http://www.gpsy.com/gpsinfo/geotoutm/
        /// </summary>
        /// <param name="Lat">Latitude to convert</param>
        /// <returns>UTM letter designator</returns>
        static private char UTMLetterDesignator
            (
            double Lat
            )
        {
            //This routine determines the correct UTM letter designator for the given latitude
            //returns 'Z' if latitude is outside the UTM limits of 84N to 80S
            //Written by Chuck Gantz- chuck.gantz@globalstar.com
            char LetterDesignator;

            if ((84 >= Lat) && (Lat >= 72)) LetterDesignator = 'X';
            else if ((72 > Lat) && (Lat >= 64)) LetterDesignator = 'W';
            else if ((64 > Lat) && (Lat >= 56)) LetterDesignator = 'V';
            else if ((56 > Lat) && (Lat >= 48)) LetterDesignator = 'U';
            else if ((48 > Lat) && (Lat >= 40)) LetterDesignator = 'T';
            else if ((40 > Lat) && (Lat >= 32)) LetterDesignator = 'S';
            else if ((32 > Lat) && (Lat >= 24)) LetterDesignator = 'R';
            else if ((24 > Lat) && (Lat >= 16)) LetterDesignator = 'Q';
            else if ((16 > Lat) && (Lat >= 8)) LetterDesignator = 'P';
            else if ((8 > Lat) && (Lat >= 0)) LetterDesignator = 'N';
            else if ((0 > Lat) && (Lat >= -8)) LetterDesignator = 'M';
            else if ((-8 > Lat) && (Lat >= -16)) LetterDesignator = 'L';
            else if ((-16 > Lat) && (Lat >= -24)) LetterDesignator = 'K';
            else if ((-24 > Lat) && (Lat >= -32)) LetterDesignator = 'J';
            else if ((-32 > Lat) && (Lat >= -40)) LetterDesignator = 'H';
            else if ((-40 > Lat) && (Lat >= -48)) LetterDesignator = 'G';
            else if ((-48 > Lat) && (Lat >= -56)) LetterDesignator = 'F';
            else if ((-56 > Lat) && (Lat >= -64)) LetterDesignator = 'E';
            else if ((-64 > Lat) && (Lat >= -72)) LetterDesignator = 'D';
            else if ((-72 > Lat) && (Lat >= -80)) LetterDesignator = 'C';
            else LetterDesignator = 'Z'; //This is here as an error flag to show that the Latitude is outside the UTM limits

            return LetterDesignator;
        }

        /// <summary>
        /// Converts UTM coordinates to lat/lon
        /// WGS-84
        /// Adapted from http://www.gpsy.com/gpsinfo/geotoutm/
        /// </summary>
        /// <param name="UTMNorthing">UTM northing</param>
        /// <param name="UTMEasting">UTM easting</param>
        /// <param name="UTMZone">UTM zone</param>
        /// <param name="Lat">On return set to latitude</param>
        /// <param name="Long">On return set to longitude</param>
        static public void UTMtoLL
            (
            double UTMNorthing,
            double UTMEasting,
            string UTMZone,
            out double Lat,
            out double Long
            )
        {
            //converts UTM coords to lat/long.  Equations from USGS Bulletin 1532 
            //East Longitudes are positive, West longitudes are negative. 
            //North latitudes are positive, South latitudes are negative
            //Lat and Long are in decimal degrees. 
            //Written by Chuck Gantz- chuck.gantz@globalstar.com

            double rad2deg = 180.0 / Math.PI;

            double a = 6378137; // equatorial radius WGS-84
            double eccSquared = 0.00669438; // eccentricity squared WGS-84

            double k0 = 0.9996;
            double eccPrimeSquared;
            double e1 = (1 - Math.Sqrt(1 - eccSquared)) / (1 + Math.Sqrt(1 - eccSquared));
            double N1, T1, C1, R1, D, M;
            double LongOrigin;
            double mu, phi1, phi1Rad;
            double x, y;
            int ZoneNumber;
            char ZoneLetter;
            int NorthernHemisphere; //1 for northern hemisphere, 0 for southern

            x = UTMEasting - 500000.0; //remove 500,000 meter offset for longitude
	        y = UTMNorthing;

            ZoneNumber = int.Parse(UTMZone.Substring(0, UTMZone.Length - 1));
            ZoneLetter = char.Parse(UTMZone.Substring(UTMZone.Length - 1, 1));

            if ((ZoneLetter - 'N') >= 0)
            {
                NorthernHemisphere = 1;//point is in northern hemisphere
            }
            else
            {
                NorthernHemisphere = 0;//point is in southern hemisphere
                y -= 10000000.0;//remove 10,000,000 meter offset used for southern hemisphere
            }

            LongOrigin = (ZoneNumber - 1)*6 - 180 + 3;  //+3 puts origin in middle of zone

	        eccPrimeSquared = (eccSquared)/(1-eccSquared);

	        M = y / k0;
	        mu = M/(a*(1-eccSquared/4-3* eccSquared* eccSquared/64-5* eccSquared* eccSquared* eccSquared/256));

	        phi1Rad = mu	+ (3* e1/2-27* e1* e1* e1/32)* Math.Sin(2*mu)
				        + (21* e1* e1/16-55* e1* e1* e1* e1/32)* Math.Sin(4*mu)
				        +(151* e1* e1* e1/96)* Math.Sin(6*mu);
            phi1 = phi1Rad* rad2deg;

            N1 = a/Math.Sqrt(1-eccSquared* Math.Sin(phi1Rad) * Math.Sin(phi1Rad));
            T1 = Math.Tan(phi1Rad)* Math.Tan(phi1Rad);
            C1 = eccPrimeSquared* Math.Cos(phi1Rad)* Math.Cos(phi1Rad);
            R1 = a* (1-eccSquared)/Math.Pow(1-eccSquared* Math.Sin(phi1Rad) * Math.Sin(phi1Rad), 1.5);
            D = x/(N1* k0);

	        Lat = phi1Rad - (N1* Math.Tan(phi1Rad)/R1)* (D* D/2-(5+3* T1+10* C1-4* C1* C1-9* eccPrimeSquared)* D* D* D* D/24
					        +(61+90* T1+298* C1+45* T1* T1-252* eccPrimeSquared-3* C1* C1)* D* D* D* D* D* D/720);
	        Lat = Lat* rad2deg;

            Long = (D-(1+2* T1+C1)* D* D* D/6+(5-2* C1+28* T1-3* C1* C1+8* eccPrimeSquared+24* T1* T1)

                            * D* D* D* D* D/120)/Math.Cos(phi1Rad);
            Long = LongOrigin + Long* rad2deg;
        }
    }
}
