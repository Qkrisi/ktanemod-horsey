using System;
using ChessModule.Pieces;
using UnityEngine;

public class PromotionButton : MonoBehaviour
{
    public PieceType type;

    private string MaterialName;
    public void Initialize(qkChessModule module, PromotionHandler handler, string ColorName)
    {
        MaterialName = String.Format("{0}_{1}", type.ToString(), ColorName);
        GetComponent<Renderer>().material = module.Materials[MaterialName];
        GetComponent<KMSelectable>().OnInteract += () =>
        {
            module.PlaySound(gameObject);
            handler.Submit(type, MaterialName);
            return false;
        };
    }
}