using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pacman.GameLogic.Ghosts
{
    [Serializable()]
	public abstract class Ghost : Entity, ICloneable
	{
		protected int drawOffset = 0;
		public int DrawOffset { get { return drawOffset; } }
		protected int waitToEnter = 0; // crap, but ... -3 = eaten, -2 going upwards to start, -1 going center, >0 up/down trips remaining
		private bool isEaten = false;
		public bool IsEaten { get { return isEaten; } }
		protected bool entered = false;
		public bool Entered { get { return entered; } }
		private bool entering = false;
		public bool Entering { get { return entering; } }
		private const double nestSpeed = 2.0f; 
		private const double enterSpeed = 1.0f;
		public double TunnelSpeed = 1.0f;
		public double FleeSpeed = 1.5f;		
		protected string name = "";
        private bool fleeing = false;

        public int TimesEatenPacman { get; set; }

        public bool Fleeing
        {
            get { return fleeing; }
            set { fleeing = value; }
        }

		public string Name{ get { return name; } }
		// some of this should come from levels ...
		private bool chasing = true;
		public bool Chasing { get { return chasing; } set { chasing = value; } }
		private long fleeStart;
		private const long fleeLength = 4000; //amount of time ghosts flee for
		public long RemainingFlee;		
		
		protected const int randomMove = 5;
		protected const float randomMoveDist = 30.0f;

		public bool Enabled = true;

		public Ghost(int x, int y, GameState gameState)
			: base(x, y, gameState) {
			Speed = 2.8f;
            TimesEatenPacman = 0;
		}

        public override void Draw(System.Drawing.Graphics g, System.Drawing.Image sprites) { //draws the ghost. Don't touch this.
			if( isEaten ) {
				g.DrawImage(sprites, new Rectangle(ImgX, ImgY, 14, 13), new Rectangle(28, 70, 13, 13), GraphicsUnit.Pixel);
			} else if( chasing ) {
				int offset = 0;
				switch( Direction ) {
					case Direction.Down: offset += Width; break;
					case Direction.None:
					case Direction.Left: offset += Width * 2; break;
					case Direction.Right: offset += Width * 3; break;
				}
				g.DrawImage(sprites, new Rectangle(ImgX, ImgY, 14, 13), new Rectangle(offset, 14 + drawOffset, 14, 13), GraphicsUnit.Pixel);
			} else {				 
				//RemainingFlee = (fleeStart + fleeLength) - gameState.Timer;
				if( RemainingFlee < 2000 && RemainingFlee % 400 < 200 ) {
					g.DrawImage(sprites, new Rectangle(ImgX, ImgY, 14, 13), new Rectangle(14, 70, 13, 13), GraphicsUnit.Pixel);
				} else {
					g.DrawImage(sprites, new Rectangle(ImgX, ImgY, 14, 13), new Rectangle(0, 70, 13, 13), GraphicsUnit.Pixel);
				}
			}
		}

		public void SetPosition(int x, int y, bool chasing, bool entered, Direction direction, bool isEaten) {
			this.x = x;
			this.y = y;
			this.chasing = chasing;
			this.entered = entered;
			this.direction = direction;
			this.isEaten = isEaten;
			Node = GameState.Map.GetNode(X, Y);
		}

		public void SetPosition(int x, int y, Direction direction) {
			this.x = x;
			this.y = y;
			this.direction = direction;
			Node nextNode = GameState.Map.GetNode(X, Y);
			if( nextNode.Type != Node.NodeType.Wall ) {
				Node = nextNode;
				//this.x = node.CenterX;
				//this.y = node.CenterY;
			}
		}

		public void SetEntered(bool value) {
			entered = value;
		}

		public abstract void PacmanDead();

		public void Flee() { //don't touch this. Fleeing when roles are reversed.
			fleeStart = GameState.Timer;
			chasing = false;
            fleeing = true; // new
		}

		public virtual void Reversal() {		//why? in event of wall crash?	
			if( entered ) {
				NextDirection = InverseDirection(direction);
				setNextDirection();
			}
		}

		public void Eaten() {			//if eaten. don't touch.
			entered = false;
			isEaten = true;
			waitToEnter = -3;
		}

		public virtual void ResetPosition() { //start of new game.
			resetPosition();
		}

		private void resetPosition() {
			chasing = true;
			entered = false;
			isEaten = false;
            fleeing = false;
            fleeStart = 0;
			Node = GameState.Map.GetNode(X, Y);
		}
				
		public override void Move() {
			if( !Enabled ) { //ghost has enabled property.
				return;
			}
			RemainingFlee = (fleeStart + fleeLength) - GameState.Timer;
			if( !chasing && RemainingFlee < 0 && entered ) { //turn back to chasing if flee time is over.
				chasing = true;
                fleeing = false; // new
			}
			// nest logic
			if( !entered ) { //if we are waiting in the cage...
				if( waitToEnter > 0 ) { // going up and down before entering
					if( Y < 113 || Y > 124 )
						waitToEnter--;
					if( waitToEnter % 2 == 0 ) {
						y -= nestSpeed;
						direction = Direction.Up;
					} else {
						y += nestSpeed;
						direction = Direction.Down;
					}
				} else if( waitToEnter == 0 ) { // moving to Y center
					if( y < 118 ) {
						y += nestSpeed;
						direction = Direction.Down;
						if( y >= 118 ) {
							y = 118;
							waitToEnter = -1;
						}
					}
					else if( y >= 118 ) {
						direction = Direction.Up;
						y -= nestSpeed;
						if( y <= 118 ) {
							y = 118;
							waitToEnter = -1;
						}
					}
				} else if( waitToEnter == -1 ) { // moving to X center
					if( x < 111 ) {
						x += enterSpeed;
						direction = Direction.Right;
						if( x >= 111 ) {
							x = 111;
							waitToEnter = -2;
						}
					}
					else if( x >= 111 ) {
						direction = Direction.Left;
						x -= enterSpeed;
						if( x <= 111 ) {
							x = 111;
							waitToEnter = -2;
						}
					}
				} else if( waitToEnter == -2 ) { // moving up and entering
					if( Y <= 93 ) {
						y = 93;
						entered = true;
						entering = false;						
					} else {
						Node = GameState.Map.GetNode(X, 93); // inefficient, fix!
						entering = true;
						direction = Direction.Up;
						y -= enterSpeed;
					}
				} else if( waitToEnter == -3 ) { // eaten, head home
					Node startNode = GameState.Map.GetNode(Red.StartX, Red.StartY);					
					if( Node == startNode ) {
						if( x < Pink.StartX ) {
							x += Speed;
							if( x > Pink.StartX ) x = Pink.StartX;
						} else if( x > Pink.StartX ) {
							x -= Speed;
							if( x < Pink.StartX ) x = Pink.StartX;
						} else if( y < Pink.StartY ) {
							y += Speed;
						} else {
							resetPosition();
							y = Pink.StartY;
							waitToEnter = -1;
							direction = Direction.Down;
						}					
					} else {

                        // THIS HAS BEEN CHANGED //shortest path? Is this not MTS?
                        if (Node.ShortestPath[startNode.X, startNode.Y] != null)
                        {
                            NextDirection = Node.ShortestPath[startNode.X, startNode.Y].Direction; //direction class has next direction member.
                        }
                        else
                        {
                            NextDirection = Direction.None;
                        }
 
                        base.Move();
					}
				}
			} else {
				if( !chasing ) {
					evade();
				}
				base.Move();
			}
		}

		private void evade() {
			MoveRandom(); //if evading then completely random.
		}

		protected bool TryGo(Direction d) { //if going to hit wall... don't do it.
			if( d == InverseDirection(Direction) )
				return false;
			switch( d ) { //use this logic to check if the ghost can move in the favoured direction
				case Direction.Up: if( Node.Up.Type != Node.NodeType.Wall ) { NextDirection = d; return true; } break;
				case Direction.Down: if( Node.Down.Type != Node.NodeType.Wall ) { NextDirection = d; return true; } break;
				case Direction.Left: if( Node.Left.Type != Node.NodeType.Wall ) { NextDirection = d; return true; } break;
				case Direction.Right: if( Node.Right.Type != Node.NodeType.Wall ) { NextDirection = d; return true; } break;
			}
			return false;
		}

        protected bool TryGoInverseAllowed(Direction d) // had to do this. InverseDirection returning false causes problems when up becomes down in certain instances.
        { //something to do with comparing the previous direction when we were looking at the next direction(??)
            switch (d)
            { //use this logic to check if the ghost can move in the favoured direction
                case Direction.Up: if (Node.Up.Type != Node.NodeType.Wall) { NextDirection = d; return true; } break;
                case Direction.Down: if (Node.Down.Type != Node.NodeType.Wall) { NextDirection = d; return true; } break;
                case Direction.Left: if (Node.Left.Type != Node.NodeType.Wall) { NextDirection = d; return true; } break;
                case Direction.Right: if (Node.Right.Type != Node.NodeType.Wall) { NextDirection = d; return true; } break;
				case Direction.Stall: NextDirection = d; return true;
            }
            return false;
		}

		protected void MoveRandom() { //completely random.
			List<Direction> possible = PossibleDirections();
			if( possible.Count > 0 ) {
				int select = GameState.Random.Next(0, possible.Count);
				if( possible[select] != InverseDirection(Direction) ) {
					NextDirection = possible[select];
				}
			}
		}

		protected void MoveInFavoriteDirection(Direction d1, Direction d2, Direction d3, Direction d4) { //favourite direction appears to be d1.
			//favourite direction is dependent on ghost and params passed.
			if( Direction != InverseDirection(d1) && checkDirection(d1) )
				NextDirection = d1;
			else if( Direction != InverseDirection(d2) && checkDirection(d2) )
				NextDirection = d2;
			else if( Direction != InverseDirection(d3) && checkDirection(d3) )
				NextDirection = d3;
			else if( Direction != InverseDirection(d4) && checkDirection(d4) )
				NextDirection = d4;
		}

		protected void MoveAsRed() { //standard directions.
			MoveAsRed(Direction.Up, Direction.Left, Direction.Down, Direction.Right);
		}

		protected void MoveAsRed(Direction d1, Direction d2, Direction d3, Direction d4) { //... can also call with specific directions.
			Direction preferredDirection = Direction;
			// minimize X
			if( Math.Abs(Node.X - GameState.Pacman.Node.X) > Math.Abs(Node.Y - GameState.Pacman.Node.Y) ) {
				if( IsLeft(GameState.Pacman) ) {
					preferredDirection = Direction.Left;
				} else {
					preferredDirection = Direction.Right;
				}
				if( !checkDirection(preferredDirection) || preferredDirection == InverseDirection(Direction) ) {
					if( IsBelow(GameState.Pacman) ) {
						preferredDirection = Direction.Down;
					} else {
						preferredDirection = Direction.Up;
					}
				}
			} else {
				if( IsBelow(GameState.Pacman) ) {
					preferredDirection = Direction.Down;
				} else {
					preferredDirection = Direction.Up;
				}
				if( !checkDirection(preferredDirection) || preferredDirection == InverseDirection(Direction) ) {
					if( IsLeft(GameState.Pacman) ) {
						preferredDirection = Direction.Left;
					} else {
						preferredDirection = Direction.Right;
					}
				}
			}

			if( preferredDirection == InverseDirection(Direction) ) {
				preferredDirection = Direction;
			}
			if( checkDirection(preferredDirection) ) {
				NextDirection = preferredDirection;
			} else {
				// just find something
				MoveInFavoriteDirection(d1, d2, d3, d4);
			}
		}

        #region ICloneable Members

        public object Clone()
        {
            Ghost _newghost = (Ghost)this.MemberwiseClone();
            _newghost.Node = Node.Clone();
            return _newghost;
        }

        #endregion
    }
}
