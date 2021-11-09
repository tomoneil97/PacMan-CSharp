using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.GameLogic.Ghosts
{
    class GhostMTS
    {
        // 1) Collect current location of pacman and ghost.
        // 2) Calculate "distances" IF moved up/down/left/right.
        //3) Extract the shortest move.
        //4) implement the move.

        public static int[] getPacmanLocation(Pacman e)
        {
            int[] loc = new int[2];

            loc[0] = e.X;
            loc[1] = e.Y;
                
            return loc;
        }

        public static int[] getGhostLocation(Ghost ghost) //need to pass in ghost type (e.g. redGhost)
        {

            int[] loc = new int[2];

            loc[0] = ghost.X;
            loc[1] = ghost.Y;

            return loc;
        }

        public static Direction[] rankedDirections(Ghost ghost, Pacman p) //this method could call the other two tbf. Take ghost type. Return a Direction type.
        {

            /* Location arrays no longer needed:
            int[] ghostLoc = getGhostLocation(ghost);

            int[] pacmanLoc = getPacmanLocation(p);

            */

            //must always be called on ghost so that values are the same.

            float distanceCurrent = ghost.Distance(p);

            double leftX = ghost.X - ghost.Speed;
            float distanceIfLeft = p.Distance(leftX, ghost.Y);

            double rightX = ghost.X + ghost.Speed;
            float distanceIfRight = p.Distance(rightX, ghost.Y);

            double upY = ghost.Y - ghost.Speed;
            float distanceIfUp = p.Distance(ghost.X, upY);

            double downY = ghost.Y + ghost.Speed;
            float distanceIfDown = p.Distance(ghost.X, downY);


            IDictionary<Direction, float> dirOrder = new Dictionary<Direction, float>(); //keys must be directions, otherwise you run the risk of same keys being implemented twice, especially if we are running quickly! Causes a runtime error.

            dirOrder.Add(Direction.Left, distanceIfLeft);
            dirOrder.Add(Direction.Right, distanceIfRight);
            dirOrder.Add(Direction.Up, distanceIfUp);
            dirOrder.Add(Direction.Down, distanceIfDown);

            var dirOrdered = dirOrder.OrderBy(x => x.Value);

            /* we no longer care about the values of distance, only directions!
             * Next section, extract directions, tryGo in order
             * 
             */

            Direction[] ordered = new Direction[4];

            int i = 0;

            foreach(var val in dirOrdered)
            {
                ordered[i] = val.Key;

                i++;
            }

            return ordered;

        }



    }
}
