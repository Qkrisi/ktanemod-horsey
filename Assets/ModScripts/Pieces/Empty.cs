using UnityEngine;

namespace ChessModule.Pieces
{
    public class Empty : ChessPiece
    {
        public Empty(Position position, qkChessModule module, char PlayColor, GameObject PieceOBJ) :
            base(position, "Empty_-", module, PlayColor, PieceType.Empty, PieceOBJ)
        {
        }
    }
}