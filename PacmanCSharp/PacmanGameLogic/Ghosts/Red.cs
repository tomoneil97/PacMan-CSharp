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


        public override void Reversal()
        {       
           //deliberately empty
        }
        public override void Move() {
            

            Direction[] order = GhostMTS.rankedDirections(this, GameState.Pacman);

            /* Original attempt:
            foreach (Direction d in order)
            {
                if (d != InverseDirection(this.Direction))
                {

                    if the direction does not cause an inverse
                    if (TryGoInverseAllowed(d))
                    {
                        Console.WriteLine("Red is moving " + d.ToString());
                        break; //if it works will break out of the loop and go to after.
                    }
                }
    
            }
            */
            MoveInFavoriteDirection(order[0],order[1],order[2],order[3]);
            
            
            base.Move(); //executes the movement. //base is ghost ("base class")

            
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
