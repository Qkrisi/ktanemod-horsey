using System;
using UnityEngine;

namespace ChessModule.Pieces
{
    public class Rook : ChessPiece
    {
        public bool HasMoved;

        protected override bool AfterMove(Position NewPosition, string MoveString, Func<string, bool> Callback)
        {
            if (Callback(MoveString))
            {
                HasMoved = true;
                return true;
            }
            return false;
        }

        public Rook(Position position, string material, qkChessModule module, char PlayColor, GameObject PieceOBJ) : 
            base(position, material, module, PlayColor, PieceType.Rook, PieceOBJ)
        {
            _PossibleMovements = new Movement[]
            {
                new Movement(1, 0, 7),
                new Movement(0, 1, 7),
                new Movement(-1, 0, 7),
                new Movement(0, -1, 7)
            };
        }
    }
}