using Robot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace VarenytsiaVictoria.RobotChallange
{
    internal class VarenytsiaVictoriaAlgorithm : IRobotAlgorithm
    {
        private int roundNumber;
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
            if (Math.Abs(station.Position.X - robot.Position.X) > 1 ||
                Math.Abs(station.Position.Y - robot.Position.Y) > 1)
            {
                return new MoveCommand() { NewPosition = MoveTowards(station.Position, robot.Position, robot.Energy) };
            }

            if (roundNumber < 17)
            {
                //if (robot.Energy > 1000)
                //{
                //    return new CreateNewRobotCommand() { NewRobotEnergy = robot.Energy - 201 };
                //}
            }
            else if (roundNumber < 45 && myRobots(robots) < 101)
            {
                if (robot.Energy > 800)
                {
                    return new CreateNewRobotCommand() { NewRobotEnergy = robot.Energy - 201 };
                }
            }

            Position position = findEnemy(map, station, robots);
            if (position != null)
            {
                return new MoveCommand()
                {
                    NewPosition = position

                };
            }

            return new CollectEnergyCommand() { };

        }

        private int myRobots(IList<Robot.Common.Robot> robots)
        {
            return robots.Count(robot => robot.OwnerName == Author);
        }
        public string Author { get { return "Varenytsia Victoria"; } }

        private int CalculateRoad(Position p1, Position p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }

        private Position MoveTowards(Position B, Position A, int energy)
        {
            if (energy > 15)
            {
                while (Math.Pow(B.X - A.X, 2) + Math.Pow(B.Y - A.Y, 2) > energy)
                {
                    if (B.X > A.X)
                    {
                        B.X--;
                    }
                    else if (B.X < A.X)
                    {
                        B.X++;
                    }

                    if (B.Y > A.Y)
                    {
                        B.Y--;
                    }
                    else if (B.Y < A.Y)
                    {
                        B.Y++;
                    }
                }

                return new Position(B.X, B.Y);
            }

            int deltaX = B.X - A.X;
            int deltaY = B.Y - A.Y;
            int deltaXMOD = Math.Abs(B.X - A.X);
            int deltaYMOD = Math.Abs(B.Y - A.Y);

            int maxGo = 2;

            int newX = A.X + (deltaXMOD == 0 ? 0 : Math.Min(maxGo, deltaXMOD) * (deltaX / deltaXMOD));
            int newY = A.Y + (deltaYMOD == 0 ? 0 : Math.Min(maxGo, deltaYMOD) * (deltaY / deltaYMOD));
            return new Position(newX, newY);
        }

        private EnergyStation FindClosestNotOccupiedStation(Map map, List<EnergyStation> stationList, Position position, IList<Robot.Common.Robot> robots)
        {
            if (stationList.Count == 0)
            {
                return null;
            }
            EnergyStation closestStation = null;
            EnergyStation spareStation = null;
            int closestRoad = Int32.MaxValue;
            int spareRoad = Int32.MaxValue;
            foreach (var station in stationList)
            {
                if (!isContain(station))
                {
                    int road = CalculateRoad(station.Position, position);
                    if (road < closestRoad/*&& checkStation(robots, station, position)*/)
                    {
                        if (findEnemy(map, station, robots) == null)
                        {
                            closestStation = station;
                            closestRoad = road;
                            spareStation = station;
                            spareRoad = road;
                        }
                        else
                        {
                            if (road < spareRoad)
                            {
                                spareStation = station;
                                spareRoad = road;
                            }
                        }

                    }
                }
            }

            return closestStation ?? spareStation;
        }

        private bool checkStation(IList<Robot.Common.Robot> robots, EnergyStation station, Position robotPosition)
        {
            List<Robot.Common.Robot> myRobots = robots.Where(robot => robot.OwnerName.Equals(Author)).Where(robot => !myStations.ContainsKey(robots.IndexOf(robot))).ToList();

            Robot.Common.Robot closestRobot = myRobots
                .OrderBy(robot => CalculateRoad(robot.Position, station.Position))
                .FirstOrDefault();

            return closestRobot.Position.X == robotPosition.X && closestRobot.Position.Y == robotPosition.Y;
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
        private EnergyStation getStation(int id)
        {
            return myStations[id];
        }
        private Robot.Common.Robot FindRobotAtPosition(IList<Robot.Common.Robot> robots, Position targetPosition)
        {
            return robots.FirstOrDefault(robot => robot.Position.X == targetPosition.X &&
                                                  robot.Position.Y == targetPosition.Y);
        }
    }
}
