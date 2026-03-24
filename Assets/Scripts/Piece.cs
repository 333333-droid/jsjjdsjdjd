using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private int ownerId;
    [SerializeField] private PieceSize size;

    public int OwnerId => ownerId;
    public PieceSize Size => size;

    public void Initialize(int newOwnerId, PieceSize newSize)
    {
        ownerId = newOwnerId;
        size = newSize;
    }
}
