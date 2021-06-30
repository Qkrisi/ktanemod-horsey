using System;

public struct Movement
{
    public readonly int Horizontal;
    public readonly int Vertical;
    public int MaxRepeats;
    public readonly bool RequiresAttack;

    public Movement(int horizontal, int vertical, int maxRepeats = 1, bool requiresAttack = false)
    {
        Horizontal = horizontal;
        Vertical = vertical;
        MaxRepeats = maxRepeats;
        RequiresAttack = requiresAttack;
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
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'
    };

    public static Position FromA1(string pos)
    {
        pos = pos.ToUpperInvariant();
        Position _pos;
        _pos.X = Array.IndexOf(Letters, pos[0]);
        _pos.Y = int.Parse(pos[1].ToString());
        return _pos;
    }
}