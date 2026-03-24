using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BoardCell : MonoBehaviour
{
    [SerializeField] private int row;
    [SerializeField] private int column;
    [SerializeField] private GameManager gameManager;

    private CellState state = new CellState();
    private readonly GameObject[] pieceVisuals = new GameObject[3];

    public int Row => row;
    public int Column => column;
    public CellState State => state;

    public void Initialize(int newRow, int newColumn)
    {
        row = newRow;
        column = newColumn;
        state = new CellState();
    }

    public void ClearState()
    {
        state.Clear();
    }

    public void ClearVisuals()
    {
        for (int i = 0; i < pieceVisuals.Length; i++)
        {
            if (pieceVisuals[i] != null)
            {
                Destroy(pieceVisuals[i]);
                pieceVisuals[i] = null;
            }
        }
    }

    public bool HasVisual(PieceSize size)
    {
        return pieceVisuals[(int)size] != null;
    }

    public void SetVisual(PieceSize size, GameObject visual)
    {
        int index = (int)size;
        if (pieceVisuals[index] != null)
        {
            Destroy(pieceVisuals[index]);
        }

        pieceVisuals[index] = visual;
    }

    public bool CanPlace(PieceSize size)
    {
        return !state.HasPiece(size);
    }

    public bool TryPlace(int playerId, PieceSize size)
    {
        if (!CanPlace(size))
        {
            return false;
        }

        state.SetOwner(size, playerId);
        return true;
    }

    private void Awake()
    {
        EnsureCollider();
        ResolveGameManager();
    }

    private GameManager ResolveGameManager()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        return gameManager;
    }

    private void Reset()
    {
        EnsureCollider();
    }

    private void OnValidate()
    {
        EnsureCollider();
    }

    private void EnsureCollider()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }
}
