using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChessModule.Pieces
{
    public class King : ChessPiece
    {
        public bool QueenCastling;
        public bool KingCastling;
        
        private bool HasMoved;

        private bool IsEmpty(int row)
        {
            return Module.Board[7, row].type == PieceType.Empty;
        }

        private bool CastleCheck(int row)
        {
            return IsInCheck(new Position[] {new Position(row, 7)}, new Position[] {CurrentPosition}, this);
        }

        public override Movement[] GetPossibleMovements(Position position)
        {
            var moves = base.GetPossibleMovements(position).ToList();
            if (!HasMoved && Color == PlayerColor)
            {
                bool Reverse = Color == 'B';
                ChessPiece QueenPiece = Module.Board[7, Reverse ? 7 : 0];
                ChessPiece KingPiece = Module.Board[7, Reverse ? 0 : 7];
                if (QueenCastling && QueenPiece.type == PieceType.Rook && !((Rook) QueenPiece).HasMoved &&
                    Enumerable.Range(Reverse ? 4 : 1, 3).All(IsEmpty) && !CastleCheck(Reverse ? 4 : 3))
                    moves.Add(new Movement(Reverse ? 2 : -2, 0,
                        castleInfo: new CastleInfo(QueenPiece.CurrentPosition, new Position(Reverse ? 4 : 3, 7))));
                if (KingCastling && KingPiece.type == PieceType.Rook && !((Rook) KingPiece).HasMoved &&
                    Enumerable.Range(Reverse ? 1 : 5, 2).All(IsEmpty) && !CastleCheck(Reverse ? 2 : 5))
                    moves.Add(new Movement(Reverse ? -2 : 2, 0,
                        castleInfo: new CastleInfo(KingPiece.CurrentPosition, new Position(Reverse ? 2 : 5, 7))));
            }
            return moves.ToArray();
        }

        protected override bool AfterMove(Position NewPosition, string MoveString, Func<string, bool> Callback)
        {
            if (Callback(MoveString))
            {
                HasMoved = true;
                bool Reverse = Color == 'B';
                if (NewPosition.X - CurrentPosition.X == 2)
                    Module.Board[7, 7].HandleMove(new Position(Reverse ? 4 : 5, 7), true);
                if (NewPosition.X - CurrentPosition.X == -2)
                    Module.Board[7, 0].HandleMove(new Position(Reverse ? 2 : 3, 7), true);
                return true;
            }
            return false;
        }

        private char GetCastle(char CastleType)
        {
            return Color == 'W' ? CastleType.ToString().ToUpperInvariant()[0] : CastleType;
        }

        public List<Position> CheckCache = new List<Position>();

        public bool IsInCheck(Position[] Fill = null, Position[] Empty = null, ChessPiece AskingPiece = null, Position? CastleRook = null)
        {
            /*if (CheckCache.Contains(EmptyPosition))
                return true;*/
            Position CastlePos = CastleRook ?? new Position(-1, -1);
            foreach (var piece in Module.Board)
            {
                Position PiecePosition = piece.CurrentPosition;
                if (piece.Color == OtherColor && (Fill==null || !Fill.Contains(PiecePosition)))
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
                            bool FillContains = Fill != null && Fill.Contains(MovementPosition);
                            bool EmptyContains = Empty != null && Empty.Contains(MovementPosition);
                            if(MovementPosition.ToString() == "(2 7)")
                                Debug.Log(FillContains);
                            if (AskingPiece != this ? Module.Board[MovementPosition.Y, MovementPosition.X] == this : FillContains && !CastlePos.Equals(MovementPosition))
                            {
                                /*if(Empty!=null)
                                    CheckCache.Add((Position)Empty);*/
                                return true;
                            }
                            PieceType type = FillContains ? PieceType.Pawn :
                                EmptyContains ? PieceType.Empty :
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