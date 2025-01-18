using System;
using System.Linq;
using UnityEngine;

namespace ChessModule.Pieces
{
    public class Pawn : ChessPiece
    {
        private bool FirstMove;
        private int VerticalMove;

        public override Movement[] GetPossibleMovements(Position position)
        {
            var moves = base.GetPossibleMovements(position).ToList();
            var y2 = CurrentPosition.Y + VerticalMove;
            if(y2 >= 0 && y2 <= 7 && Module.Board[y2, CurrentPosition.X].type != PieceType.Empty)
                moves.RemoveAt(0);
            else if (FirstMove)
                moves[0] = new Movement(0, VerticalMove, 2, disableAttack: true);
            position.Y += VerticalMove;
            if (position.Y >= 0 && position.Y <= 7)
            {
                position.X++;
                if (position.X < 8 && CanAttack(position, true))
                    moves.Add(new Movement(1, VerticalMove, requiresAttack: true));
                position.X -= 2;
                if (position.X > -1 && CanAttack(position, true))
                    moves.Add(new Movement(-1, VerticalMove, requiresAttack: true));
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
                        Destroy(Module.Board[CurrentPosition.Y, CurrentPosition.X].Piece);
                        Module.Board[NewPosition.Y, NewPosition.X] =
                            (ChessPiece) Activator.CreateInstance(piece, NewPosition, material, Module, PlayerColor, Piece);
                        if(Module.Solved)
                            Module.Board[NewPosition.Y, NewPosition.X].SetInteraction(InteractionType.None);
                        CurrentPosition = NewPosition;
                        Module.Kings[OtherColor].ToggleCheckMove();
                        Module.Kings[Color].MoveObject.SetActive(false);
                        Module.Kings[Color].ForceMoveObject = false;
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
        
        public Pawn(Position position, string material, qkChessModule module, char PlayColor, GameObject PieceOBJ) : 
            base(position, material, module, PlayColor, PieceType.Pawn, PieceOBJ)
        {
            VerticalMove = Color == PlayerColor ? -1 : 1;
            _PossibleMovements = new Movement[]
            {
                new Movement(0, VerticalMove, disableAttack: true)
            };
            FirstMove = position.Y == (Color == PlayerColor ? 6 : 1);
        }
    }
}