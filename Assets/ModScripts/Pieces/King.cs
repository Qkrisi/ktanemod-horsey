using System.Linq;

namespace ChessModule.Pieces
{
    public class King : ChessPiece
    {
        public bool QueenCastling;
        public bool KingCastling;
        
        private bool HasMoved;

        private bool IsEmpty(int col)
        {
            return Module.Board[col, 7].type == PieceType.Empty;
        }

        public override Movement[] GetPossibleMovements(Position position)
        {
            var moves = base.GetPossibleMovements(position).ToList();
            if (!HasMoved)
            {
                ChessPiece QueenPiece = Module.Board[0, 7];
                ChessPiece KingPiece = Module.Board[7, 7];
                if (QueenCastling && QueenPiece.type == PieceType.Rook && !((Rook) QueenPiece).HasMoved && Enumerable.Range(1, 3).All(IsEmpty))
                    moves.Add(new Movement(-2, 0));
                if (KingCastling && KingPiece.type == PieceType.Rook && !((Rook) KingPiece).HasMoved && Enumerable.Range(5, 2).All(IsEmpty))
                    moves.Add(new Movement(2, 0));
            }
            return moves.ToArray();
        }

        public override void AfterMove(Position NewPosizion)
        {
            HasMoved = true;
            if (NewPosizion.X - CurrentPosition.X == 2)
                Module.Board[7, 7].HandleMove(new Position(5, 7));
            if(NewPosizion.X - CurrentPosition.X == -2)
                Module.Board[0, 7].HandleMove(new Position(3, 7));
        }

        private char GetCastle(char CastleType)
        {
            return Color == 'W' ? CastleType.ToString().ToUpperInvariant()[0] : CastleType;
        }

        public King(Position position, string material, qkChessModule module, char PlayColor) :
            base(position, material, module, PlayColor, PieceType.King)
        {
            _PossibleMovements = new Movement[]
            {
                new Movement(-1, 1),
                new Movement(0, 1),
                new Movement(1, 1),
                new Movement(1, 0),
                new Movement(1, -1),
                new Movement(0, -1),
                new Movement(-1, -1),
                new Movement(-1, 0)
            };
            QueenCastling = Module.Castlings.Contains(GetCastle('q'));
            KingCastling = Module.Castlings.Contains(GetCastle('k'));
        }
    }
}