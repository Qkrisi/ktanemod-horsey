using System;
using System.Collections.Generic;
using ChessModule.Pieces;
using UnityEngine;

public class PromotionHandler : MonoBehaviour
{
    public static readonly Dictionary<PieceType, Type> PieceTypes = new Dictionary<PieceType, Type>()
    {
        {PieceType.Bishop, typeof(Bishop)},
        {PieceType.Knight, typeof(Knight)},
        {PieceType.Rook, typeof(Rook)},
        {PieceType.Queen, typeof(Queen)}
    };

    private Action<Type, string> _OnSelected = (piece, material) => { };

    private qkChessModule Module;

    public Action<Type, string> OnSelected
    {
        get
        {
            return _OnSelected;
        }
        set
        {
            if (value == null)
            {
                _OnSelected = (piece, material) => { };
                gameObject.SetActive(false);
                return;
            }
            _OnSelected = value;
            gameObject.SetActive(true);
            if (Module.AutoPromote != '_')
            {
                var pieceInfo = Module.PieceInfos[Module.AutoPromote];
                _OnSelected(pieceInfo.type, pieceInfo.MaterialName);
                OnSelected = null;
                Module.AutoPromote = '_';
                gameObject.SetActive(false);
            }
        }
    }
    

    public void Submit(PieceType piece, string Material)
    {
        OnSelected(PieceTypes[piece], Material);
        OnSelected = null;
    }
    
    public void Initialize(qkChessModule module, string ColorName)
    {
        Module = module;
        foreach(var button in GetComponentsInChildren<PromotionButton>())
            button.Initialize(module, this, ColorName);
        gameObject.SetActive(false);
    }
}