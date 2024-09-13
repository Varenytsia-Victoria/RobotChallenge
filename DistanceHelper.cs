using System;
using System.Collections.Generic;
using System.Linq;
using Robot.Common;
using System.Threading;
using System.Threading.Tasks;
using VictoriaVarenytsia;

namespace IlnytskyiMykhailoAlgo
{
    public static class DistanceHelper
    {
        private const int MinPosition = 0;
        private const int MaxPosition = 99;
        private const int TotalPositionCount = 100;

        public static int FindDistance(Position a, Position b)
        {
            int xDistance = Math.Abs(b.X - a.X), yDistance = Math.Abs(b.Y - a.Y);
            return (int)(Math.Pow(Math.Min(TotalPositionCount - xDistance, xDistance), 2) + Math.Pow(Math.Min(TotalPositionCount - yDistance, yDistance), 2));
        }

        public static Position NearestFreeStation(
            Robot.Common.Robot movingRobot,
            IList<Robot.Common.Robot> robots,
            Map map,
            int round,
            out EnergyStation energyStation,
            out int robotsInStation)
        {
            Position cellPosition = null;
            int maxProfit = 0;
            energyStation = null;
            robotsInStation = 0;
            EnergyStation returnEnergyStation = null;
            int ris = 0;

            Parallel.ForEach(map.Stations, station =>
            {
                int ownRobotsCount = 0, minDistance = int.MaxValue;
                Position tempNearestPosition = null;

                for (int x = Math.Max(station.Position.X - 2, MinPosition); x <= Math.Min(station.Position.X + 2, MaxPosition); x++)
                {
                    for (int y = Math.Max(station.Position.Y - 2, MinPosition); y <= Math.Min(station.Position.Y + 2, MaxPosition); y++)
                    {
                        var cell = new Position(x, y);

                        int distance = FindDistance(movingRobot.Position, cell);

                        Parallel.ForEach(robots, robot =>
                        {
                            if (robot.Position == cell && robot != movingRobot)
                            {
                                distance += 30;

                                if (robot.OwnerName == VarenytsiaAlgorithm.AuthorName)
                                {
                                    ownRobotsCount++;
                                    distance = int.MaxValue;
                                }
                            }

                        });

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            tempNearestPosition = cell;
                        }
                    }
                }

                int profit = Math.Min(
                    (station.Energy + (station.RecoveryRate * (VarenytsiaAlgorithm.MaxRoundsCount - round))) / (ownRobotsCount == 0 ? 1 : ownRobotsCount),
                        (VarenytsiaAlgorithm.MaxRoundsCount - round) * VarenytsiaAlgorithm.EnergyPerCollect
                    ) - minDistance;

                if (tempNearestPosition != null &&
                    minDistance <= movingRobot.Energy &&
                    profit > maxProfit)
                {
                    maxProfit = profit;
                    cellPosition = tempNearestPosition;
                    returnEnergyStation = station;
                    ris = ownRobotsCount;
                }
            });

            energyStation = returnEnergyStation;
            robotsInStation = ris;

            if (cellPosition == null)
            {
                cellPosition = new Position(
                    movingRobot.Position.X + 1 > MaxPosition ? 0 : movingRobot.Position.X + 1,
                    movingRobot.Position.Y + 1 > MaxPosition ? 0 : movingRobot.Position.Y + 1);
            }

            return cellPosition;
        }


    }

}
