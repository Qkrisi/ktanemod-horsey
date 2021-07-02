using System;
using UnityEngine;

public class CastleInfo
{
    public readonly Position EmptyPosition;
    public readonly Position FillPosition;

    public CastleInfo(Position empty, Position fill)
    {
        EmptyPosition = empty;
        FillPosition = fill;
    }
}

public struct Movement
{
    public readonly int Horizontal;
    public readonly int Vertical;
    public int MaxRepeats;
    public readonly bool RequiresAttack;
    public CastleInfo Castle;

    public Movement(int horizontal, int vertical, int maxRepeats = 1, bool requiresAttack = false, CastleInfo castleInfo = null)
    {
        Horizontal = horizontal;
        Vertical = vertical;
        MaxRepeats = maxRepeats;
        RequiresAttack = requiresAttack;
        Castle = castleInfo;
    }
}

public struct Position
{
    public int X;
    public int Y;
    
    public override string ToString()
    {
        return String.Format("({0} {1})", X, Y);
    }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static readonly char[] Letters = new char[8]
    {
        'H', 'G', 'F', 'E', 'D', 'C', 'B', 'A'
    };

    public static Position FromA1(string pos, bool reverse = false)
    {
        pos = pos.ToUpperInvariant();
        Position _pos;
        Debug.Log(pos);
        Debug.Log(Array.IndexOf(Letters, pos[0]));
        _pos.X = Array.IndexOf(Letters, pos[0]);
        if (reverse)
            _pos.X = 7 - _pos.X;
        _pos.Y = int.Parse(pos[1].ToString())-1;
        if (reverse)
            _pos.Y = 7 - _pos.Y;
        return _pos;
    }
}