using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pacman.GameLogic.Ghosts
{
    [Serializable()]
	public class Red : Ghost, ICloneable
	{
		public const int StartX = 111, StartY = 93;

		public Red(int x, int y, GameState gameState, double Speed, double FleeSpeed)
			: base(x, y, gameState) {
			this.name = "Red";
			ResetPosition();
            this.Speed = Speed;
            this.FleeSpeed = FleeSpeed;
        }

		public override void PacmanDead() {
			waitToEnter = 0;
			ResetPosition();			
		}

		public override void ResetPosition() {
			x = StartX;
			y = StartY;
			waitToEnter = 0;
			direction = Direction.Left;
			base.ResetPosition();
			entered = true;
		}

		public override void Move() {
            //if (Distance(GameState.Pacman) > randomMoveDist && GameState.Random.Next(0, randomMove) == 0)
            //{ //if too far away from pacman...
            //    MoveRandom();
            //}
            //else
            //{
            //    MoveAsRed();
            //}


            Direction[] order = GhostMTS.rankedDirections(this, GameState.Pacman);

            foreach(Direction d in order)
            {
                //if (d != InverseDirection(this.Direction))
                //{

                    //if the direction does not cause an inverse
                    if (TryGo(d))
                    {
                        Console.WriteLine("Red is moving " + d.ToString());
                        break; //if it works will break out of the loop and go to after.
                    }
                //}
                
            }
            //setNextDirection();
            
            base.Move(); //executes the movement. //base is ghost ("base class")

            //just to make it work..
            

            //Direction d = GhostMTS.shortestDistance(this, GameState.Pacman);
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public new Red Clone()
        {
            Red _temp = (Red)this.MemberwiseClone();
            _temp.Node = Node.Clone();

            return _temp;
        }

        #endregion
    }
}
