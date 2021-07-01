using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessModule.Pieces
{
    public class King : ChessPiece
    {
        public bool QueenCastling;
        public bool KingCastling;
        
        private bool HasMoved;

        private bool IsEmpty(int col)
        {
            return Module.Board[col, 7].type == PieceType.Empty;
        }

        public override Movement[] GetPossibleMovements(Position position)
        {
            var moves = base.GetPossibleMovements(position).ToList();
            if (!HasMoved)
            {
                ChessPiece QueenPiece = Module.Board[0, 7];
                ChessPiece KingPiece = Module.Board[7, 7];
                if (QueenCastling && QueenPiece.type == PieceType.Rook && !((Rook) QueenPiece).HasMoved && Enumerable.Range(1, 3).All(IsEmpty))
                    moves.Add(new Movement(-2, 0));
                if (KingCastling && KingPiece.type == PieceType.Rook && !((Rook) KingPiece).HasMoved && Enumerable.Range(5, 2).All(IsEmpty))
                    moves.Add(new Movement(2, 0));
            }
            return moves.ToArray();
        }

        protected override bool AfterMove(Position NewPosition, string MoveString, Func<string, bool> Callback)
        {
            if (Callback(MoveString))
            {
                HasMoved = true;
                if (NewPosition.X - CurrentPosition.X == 2)
                    Module.Board[7, 7].HandleMove(new Position(5, 7));
                if (NewPosition.X - CurrentPosition.X == -2)
                    Module.Board[0, 7].HandleMove(new Position(3, 7));
                return true;
            }

            return false;
        }

        private char GetCastle(char CastleType)
        {
            return Color == 'W' ? CastleType.ToString().ToUpperInvariant()[0] : CastleType;
        }

        public List<Position> CheckCache = new List<Position>();

        public bool IsInCheck(Position? Fill = null, Position? Empty = null, ChessPiece AskingPiece = null)
        {
            Position FillPosition = Fill ?? new Position(-1, -1);
            Position EmptyPosition = Empty ?? new Position(-1, -1);
            if (CheckCache.Contains(EmptyPosition))
                return true;
            foreach (var piece in Module.Board)
            {
                Position PiecePosition = piece.CurrentPosition;
                if (piece.Color == OtherColor && !piece.CurrentPosition.Equals(FillPosition))
                {
                    foreach (var movement in piece.PossibleMovements)
                    {
                        Position MovementPosition = PiecePosition;
                        for (int i = 0; i < movement.MaxRepeats; i++)
                        {
                            MovementPosition.X += movement.Horizontal;
                            MovementPosition.Y += movement.Vertical;
                            if (!qkChessModule.IsValid(MovementPosition))
                                break;
                            if (AskingPiece != this ? Module.Board[MovementPosition.Y, MovementPosition.X] == this : MovementPosition.Equals(FillPosition))
                            {
                                /*if(Empty!=null)
                                    CheckCache.Add((Position)Empty);*/
                                return true;
                            }
                            PieceType type = MovementPosition.Equals(FillPosition) ? PieceType.Pawn :
                                MovementPosition.Equals(EmptyPosition) ? PieceType.Empty :
                                Module.Board[MovementPosition.Y, MovementPosition.X].type;
                            if (type != PieceType.Empty)
                                break;
                        }
                    }
                }
            }
            return false;
        }

        public void ToggleCheckMove()
        {
            ForceMoveObject = IsInCheck();
            MoveObject.SetActive(ForceMoveObject);
        }

        public King(Position position, string material, qkChessModule module, char PlayColor) :
            base(position, material, module, PlayColor, PieceType.King)
        {
            _PossibleMovements = new Movement[]
            {
                new Movement(-1, 1),
                new Movement(0, 1),
                new Movement(1, 1),
                new Movement(1, 0),
                new Movement(1, -1),
                new Movement(0, -1),
                new Movement(-1, -1),
                new Movement(-1, 0)
            };
            QueenCastling = Module.Castlings.Contains(GetCastle('q'));
            KingCastling = Module.Castlings.Contains(GetCastle('k'));
            Module.Kings.Add(Color, this);
        }
    }
}