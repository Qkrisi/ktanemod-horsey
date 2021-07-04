using System;
using System.Text.RegularExpressions;
using System.Collections;
using ChessModule.Pieces;
using UnityEngine;

public partial class qkChessModule
{
    #pragma warning disable 414
    [HideInInspector] public const string TwitchHelpMessage = "Use '!{0} <movement>' to move a piece. A movement consists of 2 locations and an optional promotion: <start><end>[promotion]. A position consists of a letter (from left to right) A-H, and a number (from bottom to top) 1-8. Promotion can be: R = Rook, Q = Queen, B = Bishop, N = Knight.";
    #pragma warning  restore 414
    
    public IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToLowerInvariant();
        var match = Regex.Match(command, @"^(([a-h][1-8]){2})([rqbn])?$");
        if (match.Success)
        {
            yield return null;
            Position pos1 = Position.FromA1(match.Groups[1].Value.Substring(0, 2), true);
            Position pos2 = Position.FromA1(match.Groups[2].Value, true);
            ChessPiece piece = Board[pos1.Y, pos1.X];
            string PromotionValue = match.Groups[3].Value;
            char Promotion = PromotionValue == String.Empty ? '_' : PromotionValue[0];
            if (SelectedPiece != piece && piece.LastInteraction != InteractionType.Select)
            {
                yield return "sendtochaterror This piece cannot be selected!";
                yield break;
            }
            bool EnablePromotion = false;
            if (Promotion != '_')
            {
                if (piece.type != PieceType.Pawn)
                {
                    yield return "sendtochaterror Unable to promote a " + piece.type;
                    yield break;
                }

                if (pos2.Y > 0)
                {
                    yield return "sendtochaterror You can only promote at row 8";
                    yield break;
                }
                EnablePromotion = true;
            }

            if (SelectedPiece != piece)
            {
                piece.HandleInteract();
                yield return null;
            }
            ChessPiece MovePiece = Board[pos2.Y, pos2.X];
            if (MovePiece.LastInteraction != InteractionType.Move)
            {
                yield return "sendtochaterror Cannot move to the specified location!";
                yield break;
            }
            if (EnablePromotion)
                AutoPromote = Promotion;
            MovePiece.HandleInteract();
        }
    }
}