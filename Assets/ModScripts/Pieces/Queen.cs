using UnityEngine;

namespace ChessModule.Pieces
{
    public class Queen : ChessPiece
    {
        public Queen(Position position, string material, qkChessModule module, char PlayColor, GameObject PieceOBJ) :
            base(position, material, module, PlayColor, PieceType.Queen, PieceOBJ)
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