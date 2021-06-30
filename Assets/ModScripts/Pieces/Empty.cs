namespace ChessModule.Pieces
{
    public class Empty : ChessPiece
    {
        public Empty(Position position, qkChessModule module, char PlayColor) :
            base(position, "Empty_-", module, PlayColor, PieceType.Empty)
        {
        }
    }
}