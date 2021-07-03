using UnityEngine;

namespace ChessModule.Pieces
{
    public class Bishop : ChessPiece
    {
        public Bishop(Position position, string material, qkChessModule module, char PlayColor, GameObject PieceOBJ) : 
            base(position, material, module, PlayColor, PieceType.Bishop, PieceOBJ)
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