using GpxTools.Gpx;
using GpxTools;
using System;
using System.IO;

namespace GpxTools.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Gpx Analyser";
            Console.WriteLine("Enter Gpx path :");
            var path = Console.ReadLine();
            var GpxAnalyser = new GpxAnalyser(path);
            GpxAnalyser.Analyse();
            Console.Clear();
            Console.SetWindowPosition(0, 0);
            Console.SetWindowSize(175, 50);
            Console.WriteLine($"distance : {Math.Round(GpxAnalyser.TotalLenght, 2)}");
            Console.WriteLine($"denivele + : {Math.Round(GpxAnalyser.PosHeightDif, 2)}");
            Console.WriteLine($"denivele - : {Math.Round(GpxAnalyser.NegHeightDif, 2)}");
            Console.WriteLine($"Alt Max : {Math.Round(GpxAnalyser.MaxElevation, 0)}");
            Console.WriteLine($"Alt Min : {Math.Round(GpxAnalyser.MinElevation, 0)}");
            Console.WriteLine($"CalculatedTime : {GpxAnalyser.CalculatedDurationTime}");
            Console.WriteLine($"Time : {GpxAnalyser.RealDurationTime}");
            for (int i = 0; i < Console.BufferWidth - 2; i++)
            {
                Console.Write("-");
            }
            Console.WriteLine("-");
            Console.ForegroundColor = ConsoleColor.Blue;
            var MinHeightCon = Console.CursorTop+1;
            var MaxHeightCon = Console.WindowHeight-2;
            var MinWightCon = 10;
            var MaxWightCon = Console.BufferWidth - 10;
            var WidghtCon = MaxWightCon - MinWightCon;
            var HeightCon = MaxHeightCon - MinHeightCon;

            double? MinCoordX = null;
            double? MaxCoordX = null;
            double? MinCoordY = null;
            double? MaxCoordY = null;
            foreach (var item in GpxAnalyser.Points)
            {
                if (MinCoordX == null || item.Latitude < MinCoordX)
                    MinCoordX = item.Latitude;
                if (MaxCoordX == null || item.Latitude > MaxCoordX)
                    MaxCoordX = item.Latitude;
                if (MinCoordY == null || item.Longitude < MinCoordY)
                    MinCoordY = item.Longitude;
                if (MaxCoordY == null || item.Longitude > MaxCoordY)
                    MaxCoordY = item.Longitude;
            }
            var Xnegatif = false;
            if (MinCoordX < 0)
            {
                Xnegatif = true;
            }
            var Ynegatif = false;
            if (MinCoordY < 0)
            {
                Ynegatif = true;
            }
            var sizeX = Math.Abs((MaxCoordX - MinCoordX).Value);
            var sizeY = Math.Abs((MaxCoordY - MinCoordY).Value);
            Func<GpxPoint,dynamic> getPosition = new Func<GpxPoint, dynamic>((GpxPoint item) => {
                var posX = item.Latitude;
                if (Xnegatif)
                {
                    posX += -MinCoordX ?? 0;
                }
                else
                {
                    posX -= MinCoordX ?? 0;
                }
                var posY = item.Longitude;
                if (Ynegatif)
                {
                    posY += -MinCoordY ?? 0;
                }
                else
                {
                    posY -= MinCoordY ?? 0;
                }
                posX = HeightCon - posX * HeightCon / sizeX + MinHeightCon;
                posY = posY * WidghtCon / sizeY + MinWightCon;
                return new { X = posX, Y = posY }; ;
            });
            foreach (var item in GpxAnalyser.Points)
            {
                var pos = getPosition(item);
                Console.CursorLeft = (int)pos.Y;
                Console.CursorTop = (int)pos.X;
                Console.Write(".");
            }
            var posStart = getPosition(GpxAnalyser.Points.StartPoint);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.CursorLeft = (int)posStart.Y;
            Console.CursorTop = (int)posStart.X;
            Console.Write("S");
            var posEnd = getPosition(GpxAnalyser.Points.EndPoint);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.CursorLeft = (int)posEnd.Y;
            Console.CursorTop = (int)posEnd.X;
            Console.Write("F");
            Console.ReadKey();

        }
    }
}
