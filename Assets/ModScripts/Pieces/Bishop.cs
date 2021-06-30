namespace ChessModule.Pieces
{
    public class Bishop : ChessPiece
    {
        public Bishop(Position position, string material, qkChessModule module, char PlayColor) : 
            base(position, material, module, PlayColor, PieceType.Bishop)
        {
            _PossibleMovements = new Movement[]
            {
                new Movement(1, 1, 7),
                new Movement(-1, 1, 7),
                new Movement(1, -1, 7),
                new Movement(-1, -1, 7)
            };
        }
    }
}