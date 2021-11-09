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
            int[] ghostLoc = getGhostLocation(ghost);

            int[] pacmanLoc = getPacmanLocation(p);


            //Do maths




            //Direction d = 0; //figure out what the direction are....

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


            //check for tunnels
            
            
            //figure out shortest.
            //array is probs easiest, if slightly inefficient.

            //float[] order = new float[] { distanceIfLeft, distanceIfRight, distanceIfUp, distanceIfDown };

            //dictionary could be better as we can keep direction with float value.

            IDictionary<Direction, float> dirOrder = new Dictionary<Direction, float>(); //keys must be directions, otherwise you run the risk of same keys being implemented twice, especially if we are running quickly! Causes a runtime error.

            //dirOrder.Add(distanceIfLeft, Direction.Left);
            //dirOrder.Add(distanceIfRight, Direction.Right);
            //dirOrder.Add(distanceIfUp, Direction.Up);
            //dirOrder.Add(distanceIfDown, Direction.Down);

            dirOrder.Add(Direction.Left, distanceIfLeft);
            dirOrder.Add(Direction.Right, distanceIfRight);
            dirOrder.Add(Direction.Up, distanceIfUp);
            dirOrder.Add(Direction.Down, distanceIfDown);

            //this should add a stationery command.

            //dirOrder.Add(Direction.Stall, distanceCurrent);


            //put in favoured order.
            //Array.Sort(order);


            var dirOrdered = dirOrder.OrderBy(x => x.Value);

            //List<float> dirList = dirOrder.Values.ToList<float>();

            //dirList.Sort();

            //foreach (KeyValuePair<Direction, float> distance in dirOrder.OrderBy(key => key.Value)) ;




            /* we no longer care about the values of distance, only directions!
             * Next section, extract directions, tryGo in order
             * 
             */

            Direction[] ordered = new Direction[4];

            //for(int i = 0; i < ordered.Length; i++)
            //{
            //    ordered[i] = dirOrder.Keys[i];
            //}
            int i = 0;
            foreach(var val in dirOrdered)
            {
                ordered[i] = val.Key;

                i++;
            }


            //Insert the "stay still" command between numbers greater/lower than current distance score. Research: How to insert value into array (and resize it).
            
            //tryGo in order.

            
            


            return ordered;

        }



    }
}
