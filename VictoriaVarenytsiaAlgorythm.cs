using Robot.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VarenytsiaVictoria.RobotChallange
{
    internal class VarenytsiaVictoriaAlgorithm : IRobotAlgorithm
    {

        private int roundNumber = 0;
        private Dictionary<int, EnergyStation> myStations = new Dictionary<int, EnergyStation>();

        public VarenytsiaVictoriaAlgorithm()
        {
            Logger.OnLogRound += Logger_OnLogRound;
        }

        private void Logger_OnLogRound(object sender, LogRoundEventArgs e)
        {
            roundNumber++;
        }

        public RobotCommand DoStep(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            var robot = robots[robotToMoveIndex];

            if (!haveEnergyStation(robotToMoveIndex))
            {
                List<EnergyStation> energyStations = map.GetNearbyResources(robot.Position, 1000);
                EnergyStation energyStation = FindClosestNotOccupiedStation(map, energyStations, robot.Position, robots);
                if (energyStation != null)
                {
                    myStations.Add(robotToMoveIndex, energyStation);
                    return new MoveCommand() { NewPosition = MoveTowards(energyStation.Position, robot.Position, robot.Energy) };
                }
                else
                {
                    return new MoveCommand() { NewPosition = new Position(robot.Position.X + 1, robot.Position.Y + 1) };
                }
            }

            EnergyStation station = getStation(robotToMoveIndex);

            if (Math.Abs(station.Position.X - robot.Position.X) > 2 ||
                Math.Abs(station.Position.Y - robot.Position.Y) > 2)
            {
                return new MoveCommand() { NewPosition = MoveTowards(station.Position, robot.Position, robot.Energy) };
            }

            if (roundNumber < 40 && robot.Energy > 600 && myRobots(robots) < 50)
            {
                return new CreateNewRobotCommand() { NewRobotEnergy = robot.Energy - 201 };
            }

            Robot.Common.Robot enemyRobot = FindEnemyRobotOnStation(robots, station, robotToMoveIndex);
            if (enemyRobot != null && enemyRobot.Energy > 500)
            {
                return new MoveCommand() { NewPosition = enemyRobot.Position };
            }

            Position enemyPosition = findEnemy(map, station, robots);
            if (enemyPosition != null)
            {
                return new MoveCommand() { NewPosition = enemyPosition };
            }

            return new CollectEnergyCommand();

        }
        private int myRobots(IList<Robot.Common.Robot> robots)
        {
            return robots.Count(robot => robot.OwnerName == Author);
        }
        private EnergyStation FindClosestNotOccupiedStation(Map map, List<EnergyStation> stationList, Position position, IList<Robot.Common.Robot> robots)
        {
            if (stationList.Count == 0)
            {
                return null;
            }
            EnergyStation closestStation = null;
            EnergyStation spareStation = null;
            int closestRoad = int.MaxValue;
            int spareRoad = int.MaxValue;

            foreach (var station in stationList)
            {
                if (!isContain(station))
                {
                    int road = CalculateRoad(station.Position, position);
                    if (road < closestRoad)
                    {
                        if (findEnemy(map, station, robots) == null)
                        {
                            closestStation = station;
                            closestRoad = road;
                        }
                        else if (road < spareRoad)
                        {
                            spareStation = station;
                            spareRoad = road;
                        }
                    }
                }
            }

            return closestStation ?? spareStation;
        }

        private Position MoveTowards(Position targetPosition, Position currentPosition, int energy)
        {
            int deltaX = targetPosition.X - currentPosition.X;
            int deltaY = targetPosition.Y - currentPosition.Y;
            int maxMove = 2; 

            int newX = currentPosition.X + Math.Min(maxMove, Math.Abs(deltaX)) * Math.Sign(deltaX);
            int newY = currentPosition.Y + Math.Min(maxMove, Math.Abs(deltaY)) * Math.Sign(deltaY);

            return new Position(newX, newY);
        }

        private Robot.Common.Robot FindEnemyRobotOnStation(IList<Robot.Common.Robot> robots, EnergyStation station, int currentRobotIndex)
        {
            return robots.FirstOrDefault(robot =>
                robot.Position.X == station.Position.X &&
                robot.Position.Y == station.Position.Y &&
                robot.OwnerName != Author &&
                robot.Energy > 30);
        }

        private Position findEnemy(Map map, EnergyStation station, IList<Robot.Common.Robot> robots)
        {
            Position position = station.Position;

            int[,] offsets = new int[,]
            {
                { 1, 1 },
                { 0, 0 },
                { 1, 0 },
                { 1, -1 },
                { 0, -1 },
                { -1, -1 },
                { -1, 0 },
                { -1, 1 },
                { 0, 1 }
            };

            for (int i = 0; i < offsets.GetLength(0); i++)
            {
                Position newPosition = new Position
                {
                    X = position.X + offsets[i, 0],
                    Y = position.Y + offsets[i, 1]
                };

                if (!map.IsValid(newPosition))
                {
                    Robot.Common.Robot rob = FindRobotAtPosition(robots, newPosition);
                    if (rob != null && !rob.OwnerName.Equals(Author))
                    {
                        return newPosition;
                    }
                }
            }

            return null;
        }

        private Robot.Common.Robot FindRobotAtPosition(IList<Robot.Common.Robot> robots, Position targetPosition)
        {
            return robots.FirstOrDefault(robot => robot.Position.X == targetPosition.X &&
                                                  robot.Position.Y == targetPosition.Y);
        }

        private bool haveEnergyStation(int id)
        {
            return myStations.ContainsKey(id);
        }

        private bool isContain(EnergyStation station)
        {
            foreach (var kvp in myStations)
            {
                if (kvp.Value.Position.X == station.Position.X && kvp.Value.Position.Y == station.Position.Y)
                {
                    return true;
                }
            }

            return false;
        }

        private EnergyStation getStation(int id)
        {
            return myStations[id];
        }

        private int CalculateRoad(Position p1, Position p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }

        public string Author => "Varenytsia Victoria";
    }
}
