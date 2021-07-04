using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChessModule.Pieces;

public partial class qkChessModule : MonoBehaviour
{
    public bool EnableCheckCastle;
    
    private ChessModuleService.Puzzle CurrentPuzzle;

    public GameObject PiecePrefab;
    
    public Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    public Material GreenMaterial;
    public Material BlueMaterial;

    public PromotionHandler promotionHandler;
    
    [SerializeField]
    private List<Material> _Materials = new List<Material>();

    //TL: (0, 0), [col, row]
    public ChessPiece[,] Board = new ChessPiece[8, 8];

    [HideInInspector]
    public char[] Castlings = new char[] {};

    public Position EnPassant;
    
    public Transform PiecesObject;

    [HideInInspector]
    public int ChildCounter;

    private ChessPiece _SelectedPiece;
    
    public ChessPiece SelectedPiece
    {
        get
        {
            return _SelectedPiece;
        }
        set
        {
            if (_SelectedPiece != null)
                ChangeColor(ToggleHighlight(_SelectedPiece.CurrentPosition, false, true), GreenMaterial);
            _SelectedPiece = value;
            if(_SelectedPiece != null)
                ChangeColor(ToggleHighlight(_SelectedPiece.CurrentPosition, true, true), BlueMaterial);
        }
    }

    public readonly Dictionary<char, PieceInfo> PieceInfos = new Dictionary<char, PieceInfo>()
    {
        {'P', new PieceInfo(typeof(Pawn), "Pawn_White")},
        {'N', new PieceInfo(typeof(Knight), "Knight_White")},
        {'B', new PieceInfo(typeof(Bishop), "Bishop_White")},
        {'R', new PieceInfo(typeof(Rook), "Rook_White")},
        {'Q', new PieceInfo(typeof(Queen), "Queen_White")},
        {'K', new PieceInfo(typeof(King), "King_White")},
        {'p', new PieceInfo(typeof(Pawn), "Pawn_Black")},
        {'n', new PieceInfo(typeof(Knight), "Knight_Black")},
        {'b', new PieceInfo(typeof(Bishop), "Bishop_Black")},
        {'r', new PieceInfo(typeof(Rook), "Rook_Black")},
        {'q', new PieceInfo(typeof(Queen), "Queen_Black")},
        {'k', new PieceInfo(typeof(King), "King_Black")}
    };
    
    public Dictionary<char, King> Kings = new Dictionary<char, King>();
    
    private char CurrentPlayer;
    private char PlayerColor;

    [HideInInspector]
    public char AutoPromote = '_';

    private static int ModuleIDCounter;
    private int ModuleID;
    
    [HideInInspector]
    public bool SelectEnabled;

    [SerializeField]
    private Material WhiteMaterial;
    
    [SerializeField]
    private Material GrayMaterial;

    [SerializeField]
    private Renderer CurrentPlayerDisplay;

    public bool SubmitMovement(string MovementString)
    {
        MovementString = MovementString.ToLowerInvariant();
        Log("Expected move: {0}, Moved: {1}", CurrentPuzzle.CurrentMove.Value, MovementString);
        if (CurrentPuzzle.CurrentMove.Value == MovementString)
        {
            Log("Correct move. Advancing...");
            CurrentPuzzle.CurrentMove = CurrentPuzzle.CurrentMove.Next;
            if (CurrentPuzzle.CurrentMove == null)
            {
                SetAllNone();
                Log("All moves were successful! Solving module...");
                GetComponent<KMBombModule>().HandlePass();
                CurrentPlayerDisplay.enabled = false;
            }
            else TogglePlayer(); 
            return true;
        }
        Log("Incorrect move. Strike!");
        GetComponent<KMBombModule>().HandleStrike();
        ResetSelections(null);
        return false;
    }

    public void PlaySound(GameObject piece)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, piece.transform);
    }
    
    private void ChangeColor(Transform highlight, Material color)
    {
        foreach (Transform child in highlight)
            child.GetComponent<Renderer>().material = color;
    }
    private void Log(string message, params object[] args)
    {
        if(CurrentPlayer == PlayerColor)
            Debug.LogFormat("[Horsey #{0}] {1}", ModuleID, String.Format(message, args));
    }

    void HandleOpponent()
    {
		SetAllNone();
        string MoveString = CurrentPuzzle.CurrentMove.Value;
        bool reverse = PlayerColor == 'W';
        Position StartPos = Position.FromA1(MoveString.Substring(0, 2), reverse);
        AutoPromote = MoveString.Length == 5 ? MoveString.ToLowerInvariant()[4] : '_';
        if (PlayerColor == 'B')
            AutoPromote = AutoPromote.ToString().ToUpperInvariant()[0];
        Board[StartPos.Y, StartPos.X].HandleMove(Position.FromA1(MoveString.Substring(2, 2), reverse));
    }

    private void SetMaterial()
    {
        CurrentPlayerDisplay.material = CurrentPlayer == 'W' ? WhiteMaterial : GrayMaterial;
    }
    
    private void TogglePlayer()
    {
        CurrentPlayer = CurrentPlayer == 'W' ? 'B' : 'W';
        SetMaterial();
        Log("Next expected move: {0}", CurrentPuzzle.CurrentMove.Value);
        if (CurrentPlayer != PlayerColor)
            HandleOpponent();
        else ResetSelections(null);
    }

    private void ParseFEN()
    {
        //Log("Game URL: {0}", CurrentPuzzle.TrainingUrl);
        Log("FEN: {0}", CurrentPuzzle.FEN);
        var splitted = CurrentPuzzle.FEN.Split(' ');
        PlayerColor = splitted[1] == "w" ? 'B' : 'W';
        if (splitted[3] != "-")
            EnPassant = Position.FromA1(splitted[3], PlayerColor == 'W');
        Castlings = splitted[2].ToCharArray();
        CurrentPlayer = splitted[1].ToUpperInvariant()[0];
        SetMaterial();
        var Positions = splitted[0].Split('/');
        int AddEmpty = 0;
        for (int i = 0; i <= 7; i++)
        {
            int j = -1;
            foreach(char piece in Positions[i])
            {
                j += 1;
                int col = PlayerColor == 'W' ? i : 7 - i;
                int row = PlayerColor == 'W' ? j : 7 - j;
                Position pos = new Position(row, col);
                if (char.IsDigit(piece))
                {
                    Board[col, row] = new Empty(pos, this, PlayerColor, null);
                    int n = int.Parse(piece.ToString()) - 1;
                    for (int a = 0; a < n; a++)
                    {
                        j += 1;
                        col = PlayerColor == 'W' ? i : 7 - i;
                        row = PlayerColor == 'W' ? j : 7 - j;
                        pos = new Position(row, col);
                        Board[col, row] = new Empty(pos, this, PlayerColor, null);
                    }
                }
                else
                    Board[col, row] = PieceInfos[piece].Create(pos, this, PlayerColor);
            }
        }
    }

    public void ClearKingCaches()
    {
        foreach (var king in Kings.Values)
            king.CheckCache.Clear();
    }

    void Initialize()
    {
        Materials = _Materials.ToDictionary(m => m.name, m => m);
        Materials.Add("Empty_-", null);
        EnPassant.X = -1;
        EnPassant.Y = -1;
        CurrentPuzzle = ChessModuleService.ParsedPuzzle;
        ParseFEN();
    }
    
    void Awake()
    {
        ModuleIDCounter = 0;
        if(!Application.isEditor)
            Initialize();
    }

    void Start()
    {
        ModuleID = ++ModuleIDCounter;
        if(Application.isEditor)
            Initialize();
        Debug.LogFormat("[Horsey #{0}] Player color is {1}.", ModuleID, PlayerColor == 'W' ? "white" : "black");
        promotionHandler.Initialize(this, PlayerColor == 'W' ? "White" : "Black");
        foreach(var king in Kings.Values)
            king.ToggleCheckMove();
        HandleOpponent();
        SelectEnabled = true;
    }

    public static bool IsValid(Position position)
    {
        return position.X >= 0 && position.X <= 7 && position.Y >= 0 && position.Y <= 7;
    }

    public void SetAllNone()
    {
        foreach(var piece in Board)
            piece.SetInteraction(InteractionType.None);
    }

    public void ResetSelections(Position? position)
    {
        foreach (var piece in Board)
            piece.SetInteraction(
                piece.Color != PlayerColor || (position != null && piece.CurrentPosition.Equals((Position) position))
                    ? InteractionType.None
                    : InteractionType.Select);
    }

    public void SelectPiece()
    {
        Position position = SelectedPiece.CurrentPosition;
        ResetSelections(position);
        var movements = SelectedPiece.PossibleMovements;
        foreach (var movement in movements)
        {
            Position CurrentPosition = position;
            for (int i = 0; i < movement.MaxRepeats; i++)
            {
                CurrentPosition.X += movement.Horizontal;
                CurrentPosition.Y += movement.Vertical;
                if (!IsValid(CurrentPosition))
                    break;
                ChessPiece OtherPiece = Board[CurrentPosition.Y, CurrentPosition.X];
                List<Position> FillPositions = new List<Position>() {CurrentPosition};
                List<Position> EmptyPositions = new List<Position>() {SelectedPiece.CurrentPosition};
                if (EnableCheckCastle && movement.Castle != null)
                {
                    FillPositions.Add(movement.Castle.FillPosition);
                    EmptyPositions.Add(movement.Castle.EmptyPosition);
                }
                if(SelectedPiece.CanAttack(CurrentPosition, movement.RequiresAttack) && !Kings[PlayerColor].IsInCheck(FillPositions.ToArray(), EmptyPositions.ToArray(), SelectedPiece, !EnableCheckCastle || movement.Castle == null ? null : (Position?)movement.Castle.FillPosition))
                    OtherPiece.SetInteraction(InteractionType.Move);
                if (OtherPiece.type != PieceType.Empty)
                    break;
            }
        }
    }

    
    public Transform HighlightObject;

    private Dictionary<string, GameObject> Highlights = new Dictionary<string, GameObject>();
    public Transform ToggleHighlight(Position position, bool state, bool force = false)
    {
        if (!force && SelectedPiece != null && SelectedPiece.CurrentPosition.Equals(position))
            return null;
        string HighlightName = String.Format("{0}-{1}", position.Y, position.X);
        if(!Highlights.ContainsKey(HighlightName))
            Highlights.Add(HighlightName, HighlightObject.Find(HighlightName).gameObject);
        Highlights[HighlightName].SetActive(state);
        return Highlights[HighlightName].transform;
    }
}
