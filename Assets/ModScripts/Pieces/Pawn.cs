using System;
using System.Linq;
using UnityEngine;

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
            if (--position.Y >= 0)
            {
                position.X++;
                if (position.X < 8 && CanAttack(position, true))
                    moves.Add(new Movement(1, -1, requiresAttack: true));
                position.X -= 2;
                if (position.X > -1 && CanAttack(position, true))
                    moves.Add(new Movement(-1, -1, requiresAttack: true));
            }
            return moves.ToArray();
        }

        public override bool CanAttack(Position position, bool RequireAttack)
        {
            return Module.EnPassant.Equals(position) || base.CanAttack(position, RequireAttack);
        }

        protected override bool AfterMove(Position NewPosition, string MoveString, Func<string, bool> Callback)
        {
            if (NewPosition.Y == (PlayerColor == Color ? 0 : 7))
            {
                Module.SetAllNone();
                Module.promotionHandler.OnSelected = (piece, material) =>
                {
                    char PieceName = piece.Name[0];
                    MoveString += PieceName == 'K' ? 'N' : PieceName;
                    if (Callback(MoveString))
                    {
                        Destroy();
                        Module.Board[NewPosition.Y, NewPosition.X] =
                            (ChessPiece) Activator.CreateInstance(piece, NewPosition, material, Module, PlayerColor);
                        CurrentPosition = NewPosition;
                        Module.Kings[OtherColor].ToggleCheckMove();
                        Module.Kings[Color].MoveObject.SetActive(false);
                        Module.ClearKingCaches();
                        Module.EnPassant.X = -1;
                        Module.EnPassant.Y = -1;
                    }
                };
                return false;
            }
            if (Callback(MoveString))
            {
                int decrement = Color != PlayerColor ? -1 : 1;
                if (Module.EnPassant.Equals(NewPosition))
                    Module.Board[NewPosition.Y + decrement, NewPosition.X].Destroy();
                if (FirstMove && Math.Abs(CurrentPosition.Y - NewPosition.Y) == 2)
                {
                    Module.EnPassant.X = NewPosition.X;
                    Module.EnPassant.Y = CurrentPosition.Y - decrement;
                }
                else
                {
                    Module.EnPassant.X = -1;
                    Module.EnPassant.Y = -1;
                }
                FirstMove = false;
                return true;
            }
            return false;
        }
        
        public Pawn(Position position, string material, qkChessModule module, char PlayColor) : 
            base(position, material, module, PlayColor, PieceType.Pawn)
        {
            _PossibleMovements = new Movement[]
            {
                new Movement(0, -1)
            };
            FirstMove = position.Y == (Color == PlayerColor ? 6 : 1);
        }
    }
}