using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robot.Common;

namespace VictoriaVarenytsia
{

    public class VarenytsiaAlgorithm : IRobotAlgorithm
    {
        private int _round;
        private int _ownRobotsCount;

        public const int MaxRoundsCount = 50;
        public const int EnergyForCreate = 100;
        public const int EnergyPerCollect = 300;
        public const int DefaultParentEnergyForCreate = 200;
        public const string AuthorName = "Varenytsia Victoria";

        public string Author => AuthorName;

        public VarenytsiaAlgorithm()
        {
            _ownRobotsCount = 10;
            Logger.OnLogRound += Logger_OnLogRound;
        }

        private void Logger_OnLogRound(object sender, LogRoundEventArgs e)
        {
            _round = e.Number;
        }

        public RobotCommand DoStep(IList<Robot.Common.Robot> robots, int robotToMoveIndex, Map map)
        {
            Robot.Common.Robot movingRobot = robots[robotToMoveIndex];

            if (robots.Where(x => x.OwnerName == Author).Sum(x => x.Energy) > 313_671)
            {
                return new MoveCommand { NewPosition = movingRobot.Position };
            }

            int ownRobotsInStation;
            EnergyStation station;
            Position positionToMove = DistanceHelper.NearestFreeStation(movingRobot, robots, map, _round, out station, out ownRobotsInStation);

            bool isOnCollectingPosition = movingRobot.Position == positionToMove;
            bool hasEnoughEnergyForNewRobot = movingRobot.Energy >= (DefaultParentEnergyForCreate + 50); 
            bool isProfitable = ((station.RecoveryRate - (ownRobotsInStation - 1) * EnergyPerCollect) * (MaxRoundsCount - _round) > EnergyForCreate);
            bool hasRoomForNewRobot = _ownRobotsCount < 100; 
            bool isEnergyProfitWorthCreating = (station.RecoveryRate * (MaxRoundsCount - _round)) > EnergyForCreate + 50; 

            if (station != null &&
                isOnCollectingPosition &&
                hasEnoughEnergyForNewRobot &&
                isProfitable &&
                hasRoomForNewRobot &&
                isEnergyProfitWorthCreating)
            {
                _ownRobotsCount++;
                return new CreateNewRobotCommand();
            }

            if (isOnCollectingPosition)
                return new CollectEnergyCommand();

            return positionToMove != null ? new MoveCommand() { NewPosition = positionToMove } : null;
        }

    }

}
