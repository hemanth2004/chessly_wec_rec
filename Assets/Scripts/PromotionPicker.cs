using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromotionPicker : MonoBehaviour
{
    public Engine engine;
    public CanvasGroup canvasGroup;

    private BoardPiece _piece;
    private List<LegalMove> _allLegalMoves;
    private Vector2 _targetSquare;
    public void InitiatePromotion(BoardPiece piece, List<LegalMove> allLegalMoves, Vector2 targetSquare)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;

        _piece= piece;
        _allLegalMoves = allLegalMoves;
        _targetSquare = targetSquare;
    }
    public void OnClickPiece(string name)
    {
        foreach (var legalMove in _allLegalMoves)
        {
            if (legalMove.notation.Contains(name) && legalMove.chessPosition == ChessboardMapping.GetChessPositionFromVector(_targetSquare))
            {
                HandlePromotion(legalMove, name);
                return;
            }
        }
        Debug.Log("No matching legal move found for the clicked piece.");
    }

    private void HandlePromotion(LegalMove legalMove, string piece)
    {
        Debug.Log($"Promoting with move: {legalMove.notation}");
        engine.main_chess.Move(legalMove.notation);

        string c = _piece.color;
        Destroy(_piece.gameObject);

        engine.HandlePromotion(piece, c, _targetSquare);


        canvasGroup.alpha = 0.0f;
        canvasGroup.blocksRaycasts = false;
    }

}
