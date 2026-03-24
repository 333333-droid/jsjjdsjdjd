using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private enum ScreenState
    {
        Title,
        Playing
    }

    [Header("Board")]
    [SerializeField] private int boardSize = 3;
    [SerializeField] private BoardCell[] boardCells;
    [SerializeField] [Range(2, 4)] private int playerCount = 4;

    [Header("Piece Prefabs")]
    [SerializeField] private GameObject smallPiecePrefab;
    [SerializeField] private GameObject midPiecePrefab;
    [SerializeField] private GameObject bigPiecePrefab;

    [Header("Piece Colors")]
    [SerializeField] private Color player1Color = new Color(0.95f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color player2Color = new Color(0.25f, 0.55f, 0.95f, 1f);
    [SerializeField] private Color player3Color = new Color(0.95f, 0.82f, 0.2f, 1f);
    [SerializeField] private Color player4Color = new Color(0.3f, 0.8f, 0.42f, 1f);

    [Header("Game State")]
    [SerializeField] private int currentPlayer = 0;
    [SerializeField] private GameResult currentResult = GameResult.InProgress;
    [SerializeField] private int winnerPlayerId = -1;

    [Header("Debug Placement")]
    [SerializeField] private PieceSize selectedPieceSize = PieceSize.Mid;

    private CellState[,] board;
    private GUIStyle panelStyle;
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;
    private GUIStyle heroTitleStyle;
    private GUIStyle subtitleStyle;
    private Texture2D panelTexture;
    private ScreenState screenState = ScreenState.Title;

    public int CurrentPlayer => currentPlayer;
    public GameResult CurrentResult => currentResult;
    public bool IsGameOver => currentResult != GameResult.InProgress;
    public PieceSize SelectedPieceSize => selectedPieceSize;

    private void Awake()
    {
        CreateBoardState();
        ResetBoardCells();
    }

    private void OnGUI()
    {
        EnsureGuiStyles();

        if (screenState == ScreenState.Title)
        {
            DrawTitleScreen();
            return;
        }

        DrawStatusPanel();
        DrawSizePanel();
        DrawControlPanel();
    }

    private void DrawTitleScreen()
    {
        float width = 620f;
        float height = 450f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;

        GUILayout.BeginArea(new Rect(x, y, width, height), panelStyle);
        GUILayout.Label("OTRIO", heroTitleStyle);
        GUILayout.Label("サイズの違うリングを並べて勝利を目指そう", subtitleStyle);
        GUILayout.Space(18f);
        GUILayout.Label("1P <color=#F25959><b>赤</b></color>  2P <color=#408CFF><b>青</b></color>  3P <color=#F2D133><b>黄</b></color>  4P <color=#4DCC6B><b>緑</b></color>", labelStyle);
        GUILayout.Space(24f);
        GUILayout.Label("遊ぶ人数を選んでください", labelStyle);
        GUILayout.Space(14f);

        if (GUILayout.Button("2人でスタート", GUILayout.Height(52f)))
        {
            StartMatch(2);
        }

        if (GUILayout.Button("3人でスタート", GUILayout.Height(52f)))
        {
            StartMatch(3);
        }

        if (GUILayout.Button("4人でスタート", GUILayout.Height(52f)))
        {
            StartMatch(4);
        }

        GUILayout.Space(12f);
        if (GUILayout.Button("アプリを終了", GUILayout.Height(46f)))
        {
            QuitGame();
        }

        GUILayout.Space(18f);
        GUILayout.Label("勝ち方", titleStyle);
        GUILayout.Label("・同じ大きさを3つ一直線にそろえる", labelStyle);
        GUILayout.Label("・小中大、または大中小で一直線にそろえる", labelStyle);
        GUILayout.Label("・1マスに小中大を重ねる", labelStyle);
        GUILayout.EndArea();
    }

    private void DrawStatusPanel()
    {
        GUILayout.BeginArea(new Rect(16f, 16f, 340f, 150f), panelStyle);
        GUILayout.Label("ゲーム状況", titleStyle);
        GUILayout.Label(GetTurnLabel(), labelStyle);
        GUILayout.Label($"選択中のコマ: {GetPieceSizeLabel(selectedPieceSize)}", labelStyle);
        GUILayout.Label(GetResultLabel(), labelStyle);
        GUILayout.EndArea();
    }

    private void DrawSizePanel()
    {
        const float panelWidth = 200f;
        const float panelHeight = 210f;
        float panelY = Screen.height - panelHeight - 16f;

        GUILayout.BeginArea(new Rect(16f, panelY, panelWidth, panelHeight), panelStyle);
        GUILayout.Label("コマを選ぶ", titleStyle);
        DrawSizeButton("小", PieceSize.Small);
        DrawSizeButton("中", PieceSize.Mid);
        DrawSizeButton("大", PieceSize.Big);
        GUILayout.EndArea();
    }

    private void DrawControlPanel()
    {
        const float panelWidth = 220f;
        const float panelHeight = 200f;
        float panelX = Screen.width - panelWidth - 16f;
        float panelY = Screen.height - panelHeight - 16f;

        GUILayout.BeginArea(new Rect(panelX, panelY, panelWidth, panelHeight), panelStyle);
        GUILayout.Label("操作", titleStyle);
        if (GUILayout.Button("はじめから", GUILayout.Height(42f)))
        {
            ResetGame();
        }

        GUILayout.Space(12f);
        if (GUILayout.Button("タイトルへ", GUILayout.Height(42f)))
        {
            ReturnToTitle();
        }

        GUILayout.Space(12f);
        if (GUILayout.Button("アプリを終了", GUILayout.Height(42f)))
        {
            QuitGame();
        }
        GUILayout.EndArea();
    }

    private void Update()
    {
        if (screenState != ScreenState.Playing)
        {
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogWarning("Main Camera was not found.");
            return;
        }

        Vector3 screenPosition = mouse.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        Vector2 rayOrigin = new Vector2(worldPosition.x, worldPosition.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero);

        if (!hit.collider)
        {
            return;
        }

        BoardCell clickedCell = hit.collider.GetComponent<BoardCell>();
        if (clickedCell == null)
        {
            return;
        }

        Debug.Log($"Clicked cell ({clickedCell.Row}, {clickedCell.Column}) on {clickedCell.name}");
        HandleCellClicked(clickedCell);
    }

    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        currentPlayer = 0;
        currentResult = GameResult.InProgress;
        winnerPlayerId = -1;
        CreateBoardState();
        ResetBoardCells();
        Debug.Log("Game reset.");
    }

    public void StartMatch(int newPlayerCount)
    {
        playerCount = Mathf.Clamp(newPlayerCount, 2, 4);
        screenState = ScreenState.Playing;
        ResetGame();
    }

    public void ReturnToTitle()
    {
        ResetGame();
        screenState = ScreenState.Title;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public bool TryPlacePiece(int row, int column, PieceSize size)
    {
        if (IsGameOver)
        {
            Debug.LogWarning("The game is already over.");
            return false;
        }

        if (!IsInsideBoard(row, column))
        {
            Debug.LogWarning($"Cell ({row}, {column}) is outside the board.");
            return false;
        }

        if (board[row, column].HasPiece(size))
        {
            Debug.LogWarning($"Cell ({row}, {column}) already has a {size} piece.");
            return false;
        }

        board[row, column].SetOwner(size, currentPlayer);
        ApplyStateToCell(row, column);
        SpawnPieceVisual(row, column, size, currentPlayer);

        Debug.Log($"{GetPlainPlayerLabel(currentPlayer)}が{GetPieceSizeLabel(size)}を ({row}, {column}) に置きました。");

        currentResult = EvaluateBoard();
        if (currentResult == GameResult.InProgress)
        {
            currentPlayer = GetNextPlayer();
        }
        else
        {
            Debug.Log($"Game ended with result: {currentResult}");
        }

        return true;
    }

    public void HandleCellClicked(BoardCell cell)
    {
        if (cell == null)
        {
            return;
        }

        TryPlacePiece(cell.Row, cell.Column, selectedPieceSize);
    }

    public void SetSelectedPieceSize(PieceSize size)
    {
        selectedPieceSize = size;
    }

    [ContextMenu("Select Small")]
    private void SelectSmall()
    {
        selectedPieceSize = PieceSize.Small;
        Debug.Log("Selected piece size: Small");
    }

    [ContextMenu("Select Mid")]
    private void SelectMid()
    {
        selectedPieceSize = PieceSize.Mid;
        Debug.Log("Selected piece size: Mid");
    }

    [ContextMenu("Select Big")]
    private void SelectBig()
    {
        selectedPieceSize = PieceSize.Big;
        Debug.Log("Selected piece size: Big");
    }

    public CellState GetCellState(int row, int column)
    {
        return IsInsideBoard(row, column) ? board[row, column] : null;
    }

    private void CreateBoardState()
    {
        board = new CellState[boardSize, boardSize];

        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                board[row, column] = new CellState();
            }
        }
    }

    private void ResetBoardCells()
    {
        if (boardCells == null)
        {
            return;
        }

        for (int i = 0; i < boardCells.Length; i++)
        {
            BoardCell cell = boardCells[i];
            if (cell == null)
            {
                continue;
            }

            if (!IsInsideBoard(cell.Row, cell.Column))
            {
                continue;
            }

            cell.ClearState();
            cell.ClearVisuals();
            cell.State.CopyFrom(board[cell.Row, cell.Column]);
        }
    }

    private void ApplyStateToCell(int row, int column)
    {
        if (boardCells == null)
        {
            return;
        }

        for (int i = 0; i < boardCells.Length; i++)
        {
            BoardCell cell = boardCells[i];
            if (cell == null)
            {
                continue;
            }

            if (cell.Row == row && cell.Column == column)
            {
                cell.State.CopyFrom(board[row, column]);
                return;
            }
        }
    }

    private void SpawnPieceVisual(int row, int column, PieceSize size, int playerId)
    {
        BoardCell cell = GetBoardCell(row, column);
        if (cell == null)
        {
            return;
        }

        GameObject prefab = GetPiecePrefab(size);
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab assigned for {size} pieces.");
            return;
        }

        GameObject pieceObject = Instantiate(prefab, cell.transform, false);
        pieceObject.transform.localPosition = Vector3.zero;
        pieceObject.transform.localRotation = Quaternion.identity;
        pieceObject.transform.localScale = GetCompensatedLocalScale(cell.transform, prefab.transform.localScale);

        Piece piece = pieceObject.GetComponent<Piece>();
        if (piece == null)
        {
            piece = pieceObject.AddComponent<Piece>();
        }

        piece.Initialize(playerId, size);
        ApplyPieceColor(pieceObject, playerId);
        cell.SetVisual(size, pieceObject);
    }

    private void ApplyPieceColor(GameObject pieceObject, int playerId)
    {
        SpriteRenderer spriteRenderer = pieceObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = GetPlayerColor(playerId);
        spriteRenderer.sortingOrder = 10;
    }

    private GameObject GetPiecePrefab(PieceSize size)
    {
        return size switch
        {
            PieceSize.Small => smallPiecePrefab,
            PieceSize.Mid => midPiecePrefab,
            PieceSize.Big => bigPiecePrefab,
            _ => null
        };
    }

    private Vector3 GetCompensatedLocalScale(Transform parentTransform, Vector3 targetWorldScale)
    {
        Vector3 parentScale = parentTransform.lossyScale;

        return new Vector3(
            DivideOrFallback(targetWorldScale.x, parentScale.x),
            DivideOrFallback(targetWorldScale.y, parentScale.y),
            DivideOrFallback(targetWorldScale.z, parentScale.z));
    }

    private float DivideOrFallback(float value, float divisor)
    {
        return Mathf.Approximately(divisor, 0f) ? value : value / divisor;
    }

    private BoardCell GetBoardCell(int row, int column)
    {
        if (boardCells == null)
        {
            return null;
        }

        for (int i = 0; i < boardCells.Length; i++)
        {
            BoardCell cell = boardCells[i];
            if (cell != null && cell.Row == row && cell.Column == column)
            {
                return cell;
            }
        }

        return null;
    }

    private GameResult EvaluateBoard()
    {
        winnerPlayerId = -1;

        for (int playerId = 0; playerId < playerCount; playerId++)
        {
            if (HasLineForPlayer(playerId))
            {
                winnerPlayerId = playerId;
                return GameResult.Win;
            }
        }

        return IsBoardFull() ? GameResult.Draw : GameResult.InProgress;
    }

    private bool HasLineForPlayer(int playerId)
    {
        List<Vector2Int[]> lines = GetLines();

        for (int i = 0; i < lines.Count; i++)
        {
            if (IsUniformSizeLine(lines[i], playerId))
            {
                return true;
            }

            if (IsOrderedLine(lines[i], playerId, true))
            {
                return true;
            }

            if (IsOrderedLine(lines[i], playerId, false))
            {
                return true;
            }
        }

        return HasConcentricWin(playerId);
    }

    private bool IsUniformSizeLine(Vector2Int[] line, int playerId)
    {
        foreach (PieceSize size in new[] { PieceSize.Small, PieceSize.Mid, PieceSize.Big })
        {
            bool complete = true;

            for (int i = 0; i < line.Length; i++)
            {
                Vector2Int pos = line[i];
                if (board[pos.x, pos.y].GetOwner(size) != playerId)
                {
                    complete = false;
                    break;
                }
            }

            if (complete)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsOrderedLine(Vector2Int[] line, int playerId, bool ascending)
    {
        if (line.Length < 3)
        {
            return false;
        }

        PieceSize[] order = ascending
            ? new[] { PieceSize.Small, PieceSize.Mid, PieceSize.Big }
            : new[] { PieceSize.Big, PieceSize.Mid, PieceSize.Small };

        for (int i = 0; i < line.Length; i++)
        {
            Vector2Int pos = line[i];
            if (board[pos.x, pos.y].GetOwner(order[i]) != playerId)
            {
                return false;
            }
        }

        return true;
    }

    private bool HasConcentricWin(int playerId)
    {
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                CellState cell = board[row, column];
                if (cell.GetOwner(PieceSize.Small) == playerId &&
                    cell.GetOwner(PieceSize.Mid) == playerId &&
                    cell.GetOwner(PieceSize.Big) == playerId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<Vector2Int[]> GetLines()
    {
        List<Vector2Int[]> lines = new List<Vector2Int[]>();

        for (int row = 0; row < boardSize; row++)
        {
            Vector2Int[] line = new Vector2Int[boardSize];
            for (int column = 0; column < boardSize; column++)
            {
                line[column] = new Vector2Int(row, column);
            }
            lines.Add(line);
        }

        for (int column = 0; column < boardSize; column++)
        {
            Vector2Int[] line = new Vector2Int[boardSize];
            for (int row = 0; row < boardSize; row++)
            {
                line[row] = new Vector2Int(row, column);
            }
            lines.Add(line);
        }

        Vector2Int[] diagonalA = new Vector2Int[boardSize];
        Vector2Int[] diagonalB = new Vector2Int[boardSize];
        for (int i = 0; i < boardSize; i++)
        {
            diagonalA[i] = new Vector2Int(i, i);
            diagonalB[i] = new Vector2Int(i, boardSize - 1 - i);
        }

        lines.Add(diagonalA);
        lines.Add(diagonalB);

        return lines;
    }

    private bool IsBoardFull()
    {
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                CellState cell = board[row, column];
                if (!cell.HasPiece(PieceSize.Small) || !cell.HasPiece(PieceSize.Mid) || !cell.HasPiece(PieceSize.Big))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsInsideBoard(int row, int column)
    {
        return row >= 0 && row < boardSize && column >= 0 && column < boardSize;
    }

    private int GetNextPlayer()
    {
        return (currentPlayer + 1) % playerCount;
    }

    private void DrawSizeButton(string label, PieceSize size)
    {
        bool isSelected = selectedPieceSize == size;
        Color previousColor = GUI.backgroundColor;
        GUI.backgroundColor = isSelected ? new Color(0.85f, 0.85f, 0.4f, 1f) : previousColor;

        if (GUILayout.Button(label, GUILayout.Height(38f)))
        {
            SetSelectedPieceSize(size);
        }

        GUI.backgroundColor = previousColor;
    }

    private string GetTurnLabel()
    {
        if (IsGameOver)
        {
            return GetVictoryBanner();
        }

        return $"現在の手番: {GetColoredPlayerLabel(currentPlayer)}";
    }

    private string GetResultLabel()
    {
        return currentResult switch
        {
            GameResult.Win => "おめでとうございます！",
            GameResult.Draw => "引き分け",
            _ => "勝敗: まだ決まっていません"
        };
    }

    private string GetVictoryBanner()
    {
        return currentResult switch
        {
            GameResult.Win when winnerPlayerId >= 0 => $"{GetColoredPlayerLabel(winnerPlayerId)}の勝利！",
            GameResult.Win => "勝利！",
            GameResult.Draw => "引き分け",
            _ => "対局中"
        };
    }

    private string GetPlayerLabel(int playerId)
    {
        return $"{playerId + 1}P";
    }

    private string GetPlainPlayerLabel(int playerId)
    {
        return $"{GetPlayerColorName(playerId)}の{GetPlayerLabel(playerId)}";
    }

    private string GetColoredPlayerLabel(int playerId)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(GetPlayerColor(playerId));
        return $"<color=#{hexColor}><b>{GetPlayerLabel(playerId)}</b></color>";
    }

    private Color GetPlayerColor(int playerId)
    {
        return playerId switch
        {
            0 => player1Color,
            1 => player2Color,
            2 => player3Color,
            3 => player4Color,
            _ => Color.white
        };
    }

    private string GetPlayerColorName(int playerId)
    {
        return playerId switch
        {
            0 => "赤",
            1 => "青",
            2 => "黄",
            3 => "緑",
            _ => "白"
        };
    }

    private string GetPieceSizeLabel(PieceSize size)
    {
        return size switch
        {
            PieceSize.Small => "小",
            PieceSize.Mid => "中",
            PieceSize.Big => "大",
            _ => size.ToString()
        };
    }

    private void EnsureGuiStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        if (panelTexture == null)
        {
            panelTexture = new Texture2D(1, 1);
            panelTexture.SetPixel(0, 0, new Color(0.08f, 0.1f, 0.14f, 0.9f));
            panelTexture.Apply();
        }

        panelStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(14, 14, 12, 12),
            normal =
            {
                background = panelTexture,
                textColor = Color.white
            }
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 21,
            richText = true,
            normal =
            {
                textColor = new Color(0.93f, 0.95f, 0.98f, 1f)
            }
        };

        titleStyle = new GUIStyle(labelStyle)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold
        };

        heroTitleStyle = new GUIStyle(titleStyle)
        {
            fontSize = 52,
            alignment = TextAnchor.MiddleCenter
        };

        subtitleStyle = new GUIStyle(labelStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 22
        };
    }
}
