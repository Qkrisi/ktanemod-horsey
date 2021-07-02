using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ChessModule.Pieces
{
    public enum PieceType
    {
        Empty,
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    public enum InteractionType
    {
        None,
        Select,
        Move
    }
    
    public abstract class ChessPiece
    {
        protected qkChessModule Module;
        protected GameObject Piece;
        private KMSelectable ModuleSelectable;
        private KMSelectable PieceSelectable;
        public readonly char Color;
        protected readonly char OtherColor;

        public PieceType type;
        public GameObject MoveObject;

        private string PieceName;

        public Position CurrentPosition;
        
        protected readonly char PlayerColor;

        private bool SelectableEnabled;

        protected bool ForceMoveObject = false;

        public virtual Movement[] GetPossibleMovements(Position position)
        {
            return _PossibleMovements;
        }

        public Movement[] PossibleMovements
        {
            get
            {
                return GetPossibleMovements(CurrentPosition);
            }
        }

        protected Movement[] _PossibleMovements = new Movement[] {};

        public virtual bool CanAttack(Position position, bool RequireAttack)
        {
            var OtherPiece = Module.Board[position.Y, position.X];
            return (!RequireAttack || OtherPiece.type != PieceType.Empty) && OtherPiece.Color != Color;
        }

        protected virtual bool AfterMove(Position NewPosition, string MoveString, Func<string, bool> Callback)
        {
            return Callback(MoveString);
        }

        public void HandleMove(Position NewPosition, bool SkipSubmit = false)
        {
            Debug.LogFormat("New position: {0}", NewPosition.ToString());
            Module.SetAllNone();
            Module.EnPassant.X = -1;
            Module.EnPassant.Y = -1;
            Module.StartCoroutine(MovePiece(NewPosition, SkipSubmit));
        }

        IEnumerator MoveTo(Position NewPosition)
        {
            Vector3 finish = RelativeToAbsolute(NewPosition);
            while (Vector3.Distance(Piece.transform.localPosition, finish) >= 0.001f)
            {
                Piece.transform.localPosition =
                    Vector3.MoveTowards(Piece.transform.localPosition, finish, .25f * Time.deltaTime);
                yield return null;
            }
        }

        private Dictionary<char, char> ReversedChars = new Dictionary<char, char>()
        {
            {'A', 'H'},
            {'B', 'G'},
            {'C', 'F'},
            {'D', 'E'},
            {'E', 'D'},
            {'F', 'C'},
            {'G', 'B'},
            {'H', 'A'}
        };

        private string ReverseA1(string pos)
        {
            return PlayerColor == 'B'
                ? pos
                : String.Format("{0}{1}", ReversedChars[pos[0]], 9 - int.Parse(pos[1].ToString()));
        }

        private string PositionToA1(Position position)
        {
            return ReverseA1(String.Format("{0}{1}", Position.Letters[position.X], position.Y + 1));
        }
        
        IEnumerator MovePiece(Position NewPosition, bool SkipSubmit)
        {
            SetInteraction(InteractionType.None);
            yield return MoveTo(NewPosition);
            Position OldPosition = CurrentPosition;
            Func<string, bool> CB = MoveString =>
            {
                if (!SkipSubmit && !Module.SubmitMovement(MoveString))
                {
                    Debug.Log("Moving back");
                    Module.StartCoroutine(MoveTo(OldPosition));
                    return false;
                }

                Module.Board[NewPosition.Y, NewPosition.X].Destroy(OldPosition);
                Module.Board[NewPosition.Y, NewPosition.X] = this;
                //Piece.transform.localPosition = RelativeToAbsolute(NewPosition);
                return true;
            };
            if (AfterMove(NewPosition, String.Format("{0}{1}", PositionToA1(CurrentPosition), PositionToA1(NewPosition)), CB))
            {
                CurrentPosition = NewPosition;
                Module.Kings[OtherColor].ToggleCheckMove();
                Module.Kings[Color].MoveObject.SetActive(false);
                Module.ClearKingCaches();
            }
        }

        public void Destroy(Position position)
        {
            Module.ToggleHighlight(position, false, true);
            if (!CurrentPosition.Equals(position))
                Module.ToggleHighlight(CurrentPosition, false, true);
            SetInteraction(InteractionType.None);
            Object.Destroy(Piece);
            Module.Board[position.Y, position.X] = new Empty(position, Module, PlayerColor);
        }

        public void Destroy()
        {
            Destroy(CurrentPosition);
        }

        private Vector3 RelativeToAbsolute(Position position)
        {
            return new Vector3(-0.0138f-0.0174f*position.X, 0.0158f, 0.0131f+0.017f*position.Y);
        }
        
        protected ChessPiece(Position position, string material, qkChessModule module, char PlayColor, PieceType _type)
        {
            type = _type;
            PlayerColor = PlayColor;
            var splitted = material.Split('_');
            PieceName = material;
            Color = splitted[1][0];
            OtherColor = Color == 'W' ? 'B' : 'W';
            Module = module;
            Piece = Object.Instantiate(Module.PiecePrefab, Module.transform.Find("Objects").Find("Pieces"));
            Piece.transform.localPosition = RelativeToAbsolute(position);
            CurrentPosition = position;
            MoveObject = Piece.transform.Find(type == PieceType.Empty ? "EmptyMove" : "Attack").gameObject;
            ModuleSelectable = Module.GetComponent<KMSelectable>();
            PieceSelectable = Piece.GetComponent<KMSelectable>();
            PieceSelectable.Parent = ModuleSelectable;
            PieceSelectable.OnHighlight += () => Module.ToggleHighlight(CurrentPosition, SelectableEnabled);
            PieceSelectable.OnHighlightEnded += () => Module.ToggleHighlight(CurrentPosition, false);
            Material mat = Module.Materials[material];
            Renderer renderer = Piece.GetComponent<Renderer>();
            if (mat == null)
                renderer.enabled = false;
            else renderer.material = mat;
            if(Color == PlayColor)
                SetInteraction(InteractionType.Select);
        }

        private bool Select()
        {
            Module.PlaySound(Piece);
            Module.SelectedPiece = this;
            Module.SelectPiece();
            return false;
        }

        private bool Move()
        {
            Module.PlaySound(Piece);
            Module.SelectedPiece.HandleMove(CurrentPosition);
            Module.SelectedPiece = null;
            return false;
        }

        public void SetInteraction(InteractionType interaction)
        {
            MoveObject.SetActive(ForceMoveObject);
            SelectableEnabled = true;
            var children = ModuleSelectable.Children.ToList();
            switch (interaction)
            {
                case InteractionType.None:
                    if (children.Contains(PieceSelectable))
                    {
                        children.Remove(PieceSelectable);
                        ModuleSelectable.Children = children.ToArray();
                        ModuleSelectable.UpdateChildren();
                    }
                    PieceSelectable.OnInteract = () => false;
                    SelectableEnabled = false;
                    break;
                case InteractionType.Move:
                    PieceSelectable.OnInteract = Move;
                    MoveObject.SetActive(true);
                    goto default;
                case InteractionType.Select:
                    PieceSelectable.OnInteract = Select;
                    goto default;
                default:
                    if (!children.Contains(PieceSelectable))
                    {
                        ModuleSelectable.Children =
                            ModuleSelectable.Children.Concat(new KMSelectable[] {PieceSelectable}).ToArray();
                        ModuleSelectable.UpdateChildren();
                    }
                    break;
            }
        }
    }
}