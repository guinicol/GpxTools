using System;
using System.Collections.Generic;
using System.Text;

namespace GpxTools
{
    public class GpxAnalyserOptions
    {
        /// <summary>
        /// Km-e for a beginners
        /// </summary>
        public const int KmeBeginner = 4;
        /// <summary>
        /// Km-e for a intermediate practionners
        /// </summary>
        public const int KmeIntermediate = 5;
        /// <summary>
        /// Km-e for a good hiker
        /// </summary>
        public const int KmeAdvanced = 6;

        /// <summary>
        /// Distance in meter in ascent equivalent in time for 1 km horizontally
        /// default value : 125m
        /// </summary>
        public int KmEffortAscCoefficient { get; set; }
        /// <summary>
        /// Distance in meter in descent equivalent in time for 1 km horizontally
        /// default value : 400m
        /// </summary>
        public int KmEffortDescCoefficient { get; set; }
        /// <summary>
        /// number of "Kilometer-Effort" make by hour
        /// default value : Beginner
        /// </summary>
        public int KmEffortHour { get; set; }

        /// <summary>
        /// Limit of Elevation difference calculation
        /// </summary>
        public int LimitElevationDif { get; set; }
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

        public GpxAnalyserOptions()
        {
            LimitElevationDif = 10;
            AverageAscSpeed = 350;
            AverageDescSpeed = 550;
            AverageFlatSpeed = 5;
            KmEffortAscCoefficient = 125;
            KmEffortDescCoefficient = 400;
            KmEffortHour = KmeBeginner;
        }

    }
}
