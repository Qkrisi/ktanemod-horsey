using System.Linq;

namespace ChessModule.Pieces
{
    public class Pawn : ChessPiece
    {
        private bool FirstMove;

        public override Movement[] GetPossibleMovements(Position position)
        {
            var moves = base.GetPossibleMovements(position).ToList();
            if(CurrentPosition.Y > 0 && Module.Board[CurrentPosition.Y-1, CurrentPosition.X].type != PieceType.Empty)
                moves.RemoveAt(0);
            else if (FirstMove)
                moves[0] = new Movement(0, -1, 2);
            position.Y++;
            if (position.Y >= 0)
            {
                position.X++;
                if (position.X < 8 && CanAttack(position, true))
                    moves.Add(new Movement(1, -1, requiresAttack: true));
                position.X -= 2;
                if(position.X > -1 && CanAttack(position, true))
                    moves.Add(new Movement(-1, -1, requiresAttack: true));
            }
            return moves.ToArray();
        }

        public override bool CanAttack(Position position, bool RequireAttack)
        {
            return base.CanAttack(position, RequireAttack) || Module.EnPassant.Equals(position);
        }

        public override void AfterMove(Position NewPosition)
        {
            if (FirstMove && CurrentPosition.Y - NewPosition.Y == 2)
            {
                Module.EnPassant.X = NewPosition.X;
                Module.EnPassant.Y = CurrentPosition.Y - 1;
            }
            FirstMove = false;
            if (CurrentPosition.Y == 0)
            {
                //TODO: Promotion
            }
        }
        
        public Pawn(Position position, string material, qkChessModule module, char PlayColor) : 
            base(position, material, module, PlayColor, PieceType.Pawn)
        {
            _PossibleMovements = new Movement[]
            {
                new Movement(0, -1)
            };
            FirstMove = position.Y == 6;
        }
    }
}