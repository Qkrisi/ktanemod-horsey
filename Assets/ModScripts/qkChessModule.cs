using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ChessModule.Pieces;

public class qkChessModule : MonoBehaviour
{
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
    
    private char CurrentPlayer;
    private char PlayerColor;

    [HideInInspector]
    public char AutoPromote;

    public bool SubmitMovement(string MovementString)
    {
        MovementString = MovementString.ToLowerInvariant();
        Debug.LogFormat("Expected move: {0}, Moved: {1}", CurrentPuzzle.CurrentMove.Value, MovementString);
        if (CurrentPuzzle.CurrentMove.Value == MovementString)
        {
            CurrentPuzzle.CurrentMove = CurrentPuzzle.CurrentMove.Next;
            if (CurrentPuzzle.CurrentMove == null)
            {
                SetAllNone();
                GetComponent<KMBombModule>().HandlePass();
            }
            else TogglePlayer(); 
            return true;
        }
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

    void HandleOpponent()
    {
        SetAllNone();
        string MoveString = CurrentPuzzle.CurrentMove.Value;
        Position StartPos = Position.FromA1(MoveString.Substring(0, 2));
        AutoPromote = MoveString.Length == 5 ? MoveString[4] : '_';
        if (PlayerColor == 'B')
            AutoPromote = AutoPromote.ToString().ToUpperInvariant()[0];
        Debug.LogFormat("Piece type: {0}", Board[StartPos.Y, StartPos.X].type);
        Debug.Log(StartPos.ToString());
        Board[StartPos.Y, StartPos.X].HandleMove(Position.FromA1(MoveString.Substring(2, 2)));
    }
    
    private void TogglePlayer()
    {
        CurrentPlayer = CurrentPlayer == 'W' ? 'B' : 'W';
        if (CurrentPlayer != PlayerColor)
            HandleOpponent();
        else ResetSelections(null);
    }

    private void ParseFEN()
    {
        Debug.LogFormat("Game URL: {0}", CurrentPuzzle.TrainingUrl);
        Debug.LogFormat("FEN: {0}", CurrentPuzzle.FEN);
        var splitted = CurrentPuzzle.FEN.Split(' ');
        if (splitted[3] != "-")
            EnPassant = Position.FromA1(splitted[3]);
        Castlings = splitted[2].ToCharArray();
        PlayerColor = splitted[1] == "w" ? 'B' : 'W';
        CurrentPlayer = splitted[1].ToUpperInvariant()[0];
        var Positions = splitted[0].Split('/');
        int AddEmpty = 0;
        for (int i = 0; i <= 7; i++)
        {
            int j = -1;
            Debug.LogFormat("New row: {0}", Positions[i]);
            foreach(char piece in Positions[i])
            {
                j += 1;
                Debug.LogFormat("Got to char: {0}", piece);
                int col = PlayerColor == 'W' ? i : 7 - i;
                int row = PlayerColor == 'W' ? j : 7 - j;
                Position pos = new Position(row, col);
                if (char.IsDigit(piece))
                {
                    Debug.Log("Creating empty");
                    Board[col, row] = new Empty(pos, this, PlayerColor);
                    int n = int.Parse(piece.ToString()) - 1;
                    for (int a = 0; a < n; a++)
                    {
                        j += 1;
                        col = PlayerColor == 'W' ? i : 7 - i;
                        row = PlayerColor == 'W' ? j : 7 - j;
                        pos = new Position(row, col);
                        Debug.Log("Creating empty");
                        Board[col, row] = new Empty(pos, this, PlayerColor);
                    }
                }
                else
                {
                    Debug.LogFormat("Creating {0}", PieceInfos[piece].MaterialName);
                    Board[col, row] = PieceInfos[piece].Create(pos, this, PlayerColor);
                }
            }
        }
        promotionHandler.Initialize(this, PlayerColor == 'W' ? "White" : "Black");
        HandleOpponent();
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
        if(!Application.isEditor)
            Initialize();
    }

    void Start()
    {
        if(Application.isEditor)
            Initialize();
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
        foreach(var piece in Board)
            piece.SetInteraction(piece.Color != PlayerColor || (position !=null && piece.CurrentPosition.Equals((Position)position)) ? InteractionType.None : InteractionType.Select);
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
                if(SelectedPiece.CanAttack(CurrentPosition, movement.RequiresAttack))
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