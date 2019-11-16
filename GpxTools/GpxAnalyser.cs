using GpxTools.Gpx;
using System;
using System.IO;
using System.Net.Http;

namespace GpxTools
{
    public class GpxAnalyser
    {
        /// <summary>
        /// Options of analyser
        /// </summary>
        public GpxAnalyserOptions Options { get; set; }
       

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
        public double KilometerEffort
        {
            get
            {
                return TotalLenght + PosHeightDif / Options.KmEffortAscCoefficient + NegHeightDif / Options.KmEffortDescCoefficient;
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
                if (PosHeightDif / (AscDist * 1000) * 100 > Options.LimitSlope)
                    lenght -= AscDist;
                if (NegHeightDif / (DescDist * 1000) * 100 > Options.LimitSlope)
                    lenght -= DescDist;
                var time = PosHeightDif / Options.AverageAscSpeed + NegHeightDif / Options.AverageDescSpeed + lenght / Options.AverageFlatSpeed;
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
        /// Calculation of time based on Kilometer-effort algorithm
        /// </summary>
        public TimeSpan CalculatedDurationTimeKmEffort
        {
            get
            {
                return TimeSpan.FromHours(KilometerEffort / Options.KmEffortHour);
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
        /// <summary>
        /// Create an instance of a Gpx Analyser
        /// </summary>
        /// <param name="options">options of Analyser</param>
        private GpxAnalyser(GpxAnalyserOptions options = null)
        {
            if(options != null)
            {
                Options = options;
            } 
            else
            {
                Options = new GpxAnalyserOptions();
            }
        }
        /// <summary>
        /// Create an instance of a Gpx Analyser
        /// </summary>
        /// <param name="gpxReader">Gpx Reader cannot be null</param>
        /// <param name="options">options of Analyser</param>
        public GpxAnalyser(GpxReader gpxReader, GpxAnalyserOptions options = null) : this(options)
        {
            this.gpxReader = gpxReader ?? throw new ArgumentNullException(nameof(gpxReader));
        }
        /// <summary>
        /// Create an instance of a Gpx Analyser
        /// </summary>
        /// <param name="Url">Url of gpx File</param>
        /// <param name="options">options of Analyser</param>
        public GpxAnalyser(string url, GpxAnalyserOptions options = null) : this(options)
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
                if (heightDif < Options.LimitElevationDif)
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
                if (-heightDif > Options.LimitElevationDif)
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
                if (heightDif < Options.LimitElevationDif)
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
                if (-heightDif > Options.LimitElevationDif)
                {
                    lastLimitPointEleNeg = null;
                }
            }
            return 0;
        }

    }
}
