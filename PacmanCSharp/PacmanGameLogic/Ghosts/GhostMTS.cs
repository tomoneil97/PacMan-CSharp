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

        public static Direction shortestDistance(Ghost ghost, Pacman p) //this method could call the other two tbf. Take ghost type. Return a Direction type.
        {
            int[] ghostLoc = getGhostLocation(ghost);

            int[] pacmanLoc = getPacmanLocation(p);


            //Do maths


            Direction d = 0; //figure out what the direction are....

            return d;

        }



    }
}
