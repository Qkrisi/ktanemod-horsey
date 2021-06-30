using System.Collections;
using System.Linq;
using UnityEngine;

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

        public PieceType type;
        private GameObject MoveObject;

        private string PieceName;

        public Position CurrentPosition;
        
        private readonly char PlayerColor;

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

        public virtual void AfterMove(Position NewPosition)
        {
        }

        public void HandleMove(Position NewPosition)
        {
            Module.SetAllNone();
            Module.EnPassant.X = -1;
            Module.EnPassant.Y = -1;
            Module.StartCoroutine(MovePiece(NewPosition));
        }

        IEnumerator MovePiece(Position NewPosition)
        {
            Vector3 finish = RelativeToAbsolute(NewPosition);
            while (Vector3.Distance(Piece.transform.localPosition, finish) >= 0.001f)
            {
                Piece.transform.localPosition =
                    Vector3.MoveTowards(Piece.transform.localPosition, finish, .25f * Time.deltaTime);
                yield return null;
            }
            AfterMove(NewPosition);
            Module.Board[NewPosition.Y, NewPosition.X].Destroy(CurrentPosition);
            CurrentPosition = NewPosition;
            Module.Board[NewPosition.Y, NewPosition.X] = this;
            //Piece.transform.localPosition = RelativeToAbsolute(NewPosition);
            SetInteraction(InteractionType.Select);
        }

        public void Destroy(Position position)
        {
            SetInteraction(InteractionType.None);
            Object.Destroy(Piece);
            Module.Board[position.Y, position.X] = new Empty(position, Module, PlayerColor);
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
            Module = module;
            Piece = Object.Instantiate(Module.PiecePrefab, Module.transform.Find("Objects").Find("Pieces"));
            Piece.transform.localPosition = RelativeToAbsolute(position);
            CurrentPosition = position;
            MoveObject = Piece.transform.Find(type == PieceType.Empty ? "EmptyMove" : "Attack").gameObject;
            ModuleSelectable = Module.GetComponent<KMSelectable>();
            PieceSelectable = Piece.GetComponent<KMSelectable>();
            PieceSelectable.Parent = ModuleSelectable;
            PieceSelectable.OnHighlight += () => Module.ToggleHighlight(CurrentPosition, true);
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
            Debug.Log("Selected");
            Module.SelectedPiece = this;
            Module.SelectPiece();
            return false;
        }

        private bool Move()
        {
            Module.SelectedPiece.HandleMove(CurrentPosition);
            Module.SelectedPiece = null;
            return false;
        }

        public void SetInteraction(InteractionType interaction)
        {
            MoveObject.SetActive(false);
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