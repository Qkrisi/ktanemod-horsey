using System;
using ChessModule.Pieces;

public class PieceInfo
{
    public readonly Type type;
    public readonly string MaterialName;

    public ChessPiece Create(Position position, qkChessModule module, char PlayColor)
    {
        return (ChessPiece) Activator.CreateInstance(type, position, MaterialName, module, PlayColor);
    }
    
    public PieceInfo(Type _type, string _MaterialName)
    {
        type = _type;
        MaterialName = _MaterialName;
    }
}