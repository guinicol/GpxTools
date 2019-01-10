using GpxTools.Gpx;
using System;
using System.IO;
using System.Net.Http;

namespace GpxTools
{
    public class GpxAnalyser
    {
        /// <summary>
        /// Positive height difference
        /// </summary>
        public double PosHeightDif { get; internal set; }
        /// <summary>
        /// Negative height difference 
        /// </summary>
        public double NegHeightDif { get; internal set; }

        /// <summary>
        /// Total lenght
        /// </summary>
        public double TotalLenght
        {
            get
            {
                if (Points != null)
                    return Points.GetLength();
                return 0;
            }
        }
        /// <summary>
        /// Calculated duration Time of the track with setted average speeds;
        /// </summary>
        public TimeSpan CalculatedDurationTime
        {
            get
            {
                var lenght = TotalLenght;
                if (PosHeightDif / (AscDist * 1000) * 100 > LimitSlope)
                    lenght -= AscDist;
                if (NegHeightDif / (DescDist * 1000) * 100 > LimitSlope)
                    lenght -= DescDist;
                var time = PosHeightDif / AverageAscSpeed + NegHeightDif / AverageDescSpeed + lenght / AverageFlatSpeed;
                try
                {
                    return TimeSpan.FromHours(time);
                }
                catch (Exception)
                {
                    return new TimeSpan();
                }
            }
        }
        /// <summary>
        /// Real Duration time extracted from the track
        /// </summary>
        public TimeSpan? RealDurationTime
        {
            get
            {
                if (Points != null && Points.EndPoint != null && Points.StartPoint != null)
                    return Points.EndPoint.Time - Points.StartPoint.Time;
                return null;
            }
        }
        /// <summary>
        /// Elevation Max
        /// </summary>
        public double MaxElevation
        {
            get
            {
                if (Points != null)
                    return Points.GetMaxElevation() ?? 0;
                return 0;
            }
        }
        /// <summary>
        /// Elevation Min
        /// </summary>
        public double MinElevation
        {
            get
            {
                if (Points != null)
                    return Points.GetMinElevation() ?? 0;
                return 0;
            }
        }

        /// <summary>
        /// Limit of Elevation difference calculation
        /// </summary>
        public int LimitElevationDif { get; set; }

        /// <summary>
        /// Limit of Slope for Time Calculation.(HeightDif/AscendLenght*100)
        /// Under this limit. Time Calculation don't care of Height Dif.
        /// </summary>
        public int LimitSlope { get; set; }
        /// <summary>
        /// speed on Flat terrain in Meter / hour
        /// Default 5000m/h
        /// </summary>
        public int AverageFlatSpeed { get; set; }
        /// <summary>
        /// Ascentional speed in KiloMeter / Hour
        /// Default 350m/h
        /// </summary>
        public int AverageAscSpeed { get; set; }
        /// <summary>
        /// Descending speed in Meter / Hour
        /// Default 550m/h
        /// </summary>
        public int AverageDescSpeed { get; set; }

        /// <summary>
        /// List of points of the track
        /// </summary>
        public GpxPointCollection<GpxPoint> Points
        {
            get
            {
                if (gpxReader.Track != null)
                {
                    var gpxpoints = gpxReader.Track.ToGpxPoints();
                    //provisoire need to be move in GpxTrack / GpxSegment
                    gpxpoints.CalculateDistanceFromStart();
                    return gpxpoints;
                }
                else
                    return null;
            }
        }

        private readonly GpxReader gpxReader;

        private GpxAnalyser()
        {
            LimitElevationDif = 10;
            LimitSlope = 6;
            AverageAscSpeed = 350;
            AverageDescSpeed = 550;
            AverageFlatSpeed = 5;
        }
        /// <summary>
        /// Create an instance of a Gpx Analyser
        /// </summary>
        /// <param name="gpxReader">Gpx Reader cannot be null</param>
        public GpxAnalyser(GpxReader gpxReader) : this()
        {
            this.gpxReader = gpxReader ?? throw new ArgumentNullException(nameof(gpxReader));
        }
        /// <summary>
        /// Create an instance of a Gpx Analyser
        /// </summary>
        /// <param name="Url">Url of gpx File</param>
        public GpxAnalyser(string url) : this()
        {
            Stream stream = null;
            if (IsLocalPath(url))
            {
                stream = new FileStream(url, FileMode.Open);
            }
            else
            {
                using (var client = new HttpClient())
                {
                    var get = client.GetStreamAsync(url);
                    get.Wait();
                    stream = get.Result;
                }
            }
            gpxReader = new GpxReader(stream);
        }
        private static bool IsLocalPath(string p)
        {
            try
            {
                return new Uri(p).IsFile;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// calculate metrics from Gpx File
        /// </summary>
        public void Analyse()
        {
            while (gpxReader.Read())
            {

            }
            Points.CalculateDistanceFromStart();
            for (int i = 1; i < Points.Count; i++)
            {
                PosHeightDif += GetPositiveHeightDif(Points[i]);
                NegHeightDif += GetNegativeHeightDif(Points[i]);
            }
        }
        private GpxPoint lastLimitPointElePos;
        private double AscDist;
        private double GetPositiveHeightDif(GpxPoint point)
        {
            var heightDif = 0d;
            if (lastLimitPointElePos != null)
            {
                heightDif = point.GetElevationDifFrom(lastLimitPointElePos) ?? 0;
            }
            else
            {
                lastLimitPointElePos = point;
            }
            if (heightDif > 0)
            {
                if (heightDif < LimitElevationDif)
                {
                    return 0;
                }
                else
                {
                    if (lastLimitPointElePos != null)
                        AscDist += point.GetDistanceFrom(lastLimitPointElePos);
                    lastLimitPointElePos = point;
                    return heightDif;
                }
            }
            else
            {
                if (-heightDif > LimitElevationDif)
                {
                    lastLimitPointElePos = null;
                }
            }
            return 0;
        }

        private GpxPoint lastLimitPointEleNeg;
        private double DescDist;
        private double GetNegativeHeightDif(GpxPoint point)
        {
            var heightDif = 0d;
            if (lastLimitPointEleNeg != null)
            {
                heightDif = -(point.GetElevationDifFrom(lastLimitPointEleNeg) ?? 0);
            }
            else
            {
                lastLimitPointEleNeg = point;
            }
            if (heightDif > 0)
            {
                if (heightDif < LimitElevationDif)
                {
                    return 0;
                }
                else
                {
                    if (lastLimitPointEleNeg != null)
                        DescDist += point.GetDistanceFrom(lastLimitPointEleNeg);
                    lastLimitPointEleNeg = point;
                    return heightDif;
                }
            }
            else
            {
                if (-heightDif > LimitElevationDif)
                {
                    lastLimitPointEleNeg = null;
                }
            }
            return 0;
        }

    }
}
