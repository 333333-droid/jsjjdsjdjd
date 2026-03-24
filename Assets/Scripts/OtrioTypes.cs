using System;

public enum PieceSize
{
    Small,
    Mid,
    Big
}

public enum GameResult
{
    InProgress,
    Win,
    Draw
}

[Serializable]
public class CellState
{
    public int SmallOwner = -1;
    public int MidOwner = -1;
    public int BigOwner = -1;

    public void Clear()
    {
        SmallOwner = -1;
        MidOwner = -1;
        BigOwner = -1;
    }

    public bool HasPiece(PieceSize size)
    {
        return GetOwner(size) != -1;
    }

    public int GetOwner(PieceSize size)
    {
        return size switch
        {
            PieceSize.Small => SmallOwner,
            PieceSize.Mid => MidOwner,
            PieceSize.Big => BigOwner,
            _ => -1
        };
    }

    public void SetOwner(PieceSize size, int playerId)
    {
        switch (size)
        {
            case PieceSize.Small:
                SmallOwner = playerId;
                break;
            case PieceSize.Mid:
                MidOwner = playerId;
                break;
            case PieceSize.Big:
                BigOwner = playerId;
                break;
        }
    }

    public void CopyFrom(CellState other)
    {
        if (other == null)
        {
            Clear();
            return;
        }

        SmallOwner = other.SmallOwner;
        MidOwner = other.MidOwner;
        BigOwner = other.BigOwner;
    }
}
