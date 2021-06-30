namespace ChessModule.Pieces
{
    public class Knight : ChessPiece
    {
        public Knight(Position position, string material, qkChessModule module, char PlayColor) :
            base(position, material, module, PlayColor, PieceType.Knight)
        {
            _PossibleMovements = new Movement[]
            {
                new Movement(1, 2),
                new Movement(1, -2),
                new Movement(-1, 2),
                new Movement(-1, -2),
                new Movement(2, 1),
                new Movement(2, -1),
                new Movement(-2, 1),
                new Movement(-2, -1)
            };
        }
    }
}