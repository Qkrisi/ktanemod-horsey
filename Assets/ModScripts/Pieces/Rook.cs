namespace ChessModule.Pieces
{
    public class Rook : ChessPiece
    {
        public bool HasMoved;

        public override void AfterMove(Position NewPosizion)
        {
            HasMoved = true;
        }

        public Rook(Position position, string material, qkChessModule module, char PlayColor) : 
            base(position, material, module, PlayColor, PieceType.Rook)
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