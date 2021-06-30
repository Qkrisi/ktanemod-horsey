namespace ChessModule.Pieces
{
    public class Queen : ChessPiece
    {
        public Queen(Position position, string material, qkChessModule module, char PlayColor) :
            base(position, material, module, PlayColor, PieceType.Queen)
        {
            _PossibleMovements = new Movement[]
            {
                new Movement(-1, 1, 7),
                new Movement(0, 1, 7),
                new Movement(1, 1, 7),
                new Movement(1, 0, 7),
                new Movement(1, -1, 7),
                new Movement(0, -1, 7),
                new Movement(-1, -1, 7),
                new Movement(-1, 0, 7)
            };
        }
    }
}