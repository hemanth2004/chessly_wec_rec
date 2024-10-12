// Script that handles interaction between Chess.cs validator and Player.cs's board management

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChessValidator;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net.NetworkInformation;

public class Engine : MonoBehaviour
{
    public string FEN;

    [SerializeField]
    public BoardPiece[] allPieces;
    public Transform pieceHolder;

    public Transform main_pieces;

    public Transform box_prefab;

    public BoardPiece currentSelected = null;
    public Chess main_chess = null;
    [SerializeField]
    private PromotionPicker promotionPicker;



    private const string W_KING = "e1";
    private const string B_KING= "e8";
    private const string Q_W_ROOK = "a1";
    private const string K_W_ROOK = "h1";
    private const string Q_B_ROOK = "a8";
    private const string K_B_ROOK = "h8";

    private const string WK_POSTKCASTLE = "g1";
    private const string WK_POSTQCASTLE = "c1";
    private const string BK_POSTKCASTLE = "g8";
    private const string BK_POSTQCASTLE = "c8";

    private const string WR_POSTKCASTLE = "f1";
    private const string WR_POSTQCASTLE = "d1";
    private const string BR_POSTKCASTLE = "f8";
    private const string BR_POSTQCASTLE = "d8";

    public int blackHeldPieces = 0;
    public int whiteHeldPieces = 0;


    public Vector2 GetPositionOfKing(string color) // Utility for when I want to highlight a check
    {
        BoardPiece piece;
        foreach(Transform t in pieceHolder)
        {
            piece = t.GetComponent<BoardPiece>();
            if (piece.type.ToLower() == "k" && piece.color == color)
            {
                return ChessboardMapping.GetClosestSquarePosition(piece.piece.transform.position);
            }
            
        }

        return Vector2.zero;
    }


    // Starts game according to chess.cs
    // Different from starting a new game
    public void StartGame(string starting_position = "") 
    {
        Chess chess;
        if (string.IsNullOrEmpty(starting_position))
        {
            chess = new Chess();
        }
        else
        {
            chess = new Chess(starting_position);
        }

        StartCoroutine(RenderBoard(chess));
        main_chess= chess;
    }

    // Used only when start game, promotion and undo
    public IEnumerator RenderBoard(Chess chess)
    {

        yield return StartCoroutine(ClearBoardPieces());
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        for (int i = 'a'; i <= 'h'; i++)
        {
            for (int j = 1; j <= 8; j++)
            {

                string pos1 = "" + ((char)i);
                string pos2 = "" + j.ToString();
                string position = pos1 + pos2;
                Piece piece = chess.GetPiece(position);
                if (piece != null)
                {

                    foreach (BoardPiece p in allPieces)
                    {
                        if (p.type == piece.type && p.color == piece.color)
                        {
                            BoardPiece bPiece = Instantiate(p.gameObject, pieceHolder, true).GetComponent<BoardPiece>();
                            Vector2 pos = ChessboardMapping.GetVectorFromChessPosition(position);
                            bPiece.transform.position = new Vector3(pos.x, pos.y, 0);
                        }
                    }
                }

            }
        }
    }


    // Utility to clear the board of all the sprites.
    // Doesn't actually remove the pieces on the engine 
    public IEnumerator ClearBoardPieces()
    {
        while(pieceHolder.childCount > 0)
        {
            Destroy(pieceHolder.GetChild(0).gameObject);
            yield return null;
        }
    }

    // Called when the player picks up a piece
    public void StartDisplayLegalMoves(BoardPiece piece, Vector2 position)
    {
        ClearLegalMoveIndicators();
        string[] moves = main_chess.LegalMovesSquare(ChessboardMapping.GetChessPositionFromVector(position));

        foreach(string move in moves)
        {
            string destination = ChessboardMapping.GetDestinationSquare(move);

            Vector2 targetBox = Vector2.zero;
            GameObject g = Instantiate(box_prefab.gameObject);

             
            if (destination == "k_castle")
            {
                if (piece.color == "w")
                    targetBox = ChessboardMapping.GetVectorFromChessPosition(K_W_ROOK);
                else
                    targetBox = ChessboardMapping.GetVectorFromChessPosition(K_B_ROOK);
            }
            else if(destination == "q_castle")
            {
                if (piece.color == "w")
                    targetBox = ChessboardMapping.GetVectorFromChessPosition(Q_W_ROOK);
                else
                    targetBox = ChessboardMapping.GetVectorFromChessPosition(Q_B_ROOK);
            }
            else
                targetBox = ChessboardMapping.GetVectorFromChessPosition(destination);

            g.transform.position = targetBox;

            LegalMove lm = new LegalMove()
            {
                notation = move,
                box = g.transform
            };
            lm = ChessboardMapping.DetectSpecialMoves(lm);
            //Debug.Log(move);
            allLegalMoves.Add(lm);
            
        }

        
    }
    public void ClearLegalMoveIndicators()
    {
        while (allLegalMoves.Count > 0)
        {
            Destroy(allLegalMoves[0].box.gameObject); allLegalMoves.RemoveAt(0);
        }
    }



    // More Utility
    public BoardPiece GetPieceAt(string position)
    {
        foreach(Transform t in pieceHolder) 
        { 
            if(ChessboardMapping.GetChessPositionFromVector(ChessboardMapping.GetClosestSquarePosition(t.position)) == position)
            {
                return t.GetComponent<BoardPiece>();
            }
        }
        return null;
    }
    public BoardPiece GetPieceAt(Vector2 position)
    {
        foreach (Transform t in pieceHolder)
        {
            Vector3 p = t.GetComponent<BoardPiece>().piece.position;
            if (new Vector2(p.x, p.y) == position)
            {
                return t.GetComponent<BoardPiece>();
            }
        }
        return null;
    }




    public List<LegalMove> allLegalMoves = new List<LegalMove>();

    // When player lets go of a piece
    public void PieceDropped(BoardPiece piece, Vector2 position, Vector2 originalPosition)
    {

        if(position == originalPosition) // Dropped on the same square
        {
            Debug.Log("original position");
            LerpTween.MoveTo(piece.transform, ChessboardMapping.GetClosestSquarePosition(originalPosition), 0.05f);
            ClearLegalMoveIndicators();
            return;
        }

        bool isLegalPosition = false;

        promotionPicker = GameObject.FindObjectOfType<PromotionPicker>(); 
        foreach(LegalMove lm in allLegalMoves)
        {
            if(new Vector2(lm.box.position.x, lm.box.position.y) == position)
            {
                isLegalPosition = true;
                if(lm.castle) // special case of castling
                {
                    BoardPiece king = piece;
                    king.transform.position = originalPosition;
                    BoardPiece rook = null;

                    string kingToPos = "";
                    string rookToPos = "";

                    string castleMoveNotation = "";


                    if (lm.notation == "O-O") // kings side castling
                    {
                        if (piece.color == "w")
                        {
                            rook = GetPieceAt(K_W_ROOK);
                            kingToPos = WK_POSTKCASTLE;
                            rookToPos = WR_POSTKCASTLE;
                        }
                        else
                        {
                            rook = GetPieceAt(K_B_ROOK);
                            kingToPos = BK_POSTKCASTLE;
                            rookToPos = BR_POSTKCASTLE;
                        }
                        castleMoveNotation = "O-O";
                    }
                    else // queens side castling
                    {
                        if (piece.color == "w")
                        {
                            rook = GetPieceAt(Q_W_ROOK);
                            kingToPos = WK_POSTQCASTLE;
                            rookToPos = WR_POSTQCASTLE;
                        }
                        else
                        {
                            rook = GetPieceAt(Q_B_ROOK);
                            kingToPos = BK_POSTQCASTLE;
                            rookToPos= BR_POSTQCASTLE;
                        }
                        castleMoveNotation = "O-O-O";
                    }

                    Debug.Log("Moving " + castleMoveNotation);

                    LerpTween.MoveTo(king.transform, ChessboardMapping.GetVectorFromChessPosition(kingToPos), 0.25f);
                    LerpTween.MoveTo(rook.transform, ChessboardMapping.GetVectorFromChessPosition(rookToPos), 0.25f);

                    main_chess.Move(castleMoveNotation);
                    ClearLegalMoveIndicators();
                    break;
                }
                if (lm.capture) // if the move also happens to be a capture
                {
                    string capturedPos = ChessboardMapping.GetDestinationSquare(lm.notation);
                    if(IsThisAnEnPassantSituation(capturedPos)) // En passant special case
                    {
                        int rank = int.Parse("" + capturedPos[capturedPos.Length - 1]);
                        string file = capturedPos.Remove(capturedPos.Length - 1);
                     
                        rank += (piece.color == "w") ? -1 : 1;

                        string pawnToBeRemovedPos = file + rank;
                        Vector2 v_pawnToBeRemovedPos = ChessboardMapping.GetVectorFromChessPosition(pawnToBeRemovedPos);

                        BoardPiece passantCaptured = GetPieceAt(v_pawnToBeRemovedPos);
                        LerpTween.ScaleTo(passantCaptured.transform, Vector3.zero, 0.25f, () => Destroy(passantCaptured.gameObject));
                        int _power = 1;
                        if (main_chess.Turn() == "w") { whiteHeldPieces += _power; } else { blackHeldPieces += _power; }
                        if (!lm.promotion) // For special case of enpassanting into promotion. even though that'll never happen
                        {
                            main_chess.Move(lm.notation);
                            LerpTween.MoveTo(piece.transform, ChessboardMapping.GetVectorFromChessPosition(capturedPos), 0.35f);

                            ClearLegalMoveIndicators();
                            break;
                        }
                    }


                    BoardPiece captured = GetPieceAt(position);
                    Vector2 captured_position = captured.transform.position;
                    LerpTween.ScaleTo(captured.transform, Vector3.zero, 0.25f, () => Destroy(captured.gameObject));
                    int power = ChessboardMapping.GetPowerLevel(captured.type.ToLower());
                    if (main_chess.Turn() == "w") { whiteHeldPieces += power; } else { blackHeldPieces += power; }
                    if (!lm.promotion) // When we capture without promotion
                    {
                        Debug.Log("Non promotion capture");
                        main_chess.Move(lm.notation);
                        LerpTween.MoveTo(piece.transform, captured_position, 0.35f);
                        ClearLegalMoveIndicators();
                        break;
                    }
                }
                if (lm.promotion) // When we promote
                {
                    promotionPicker.InitiatePromotion(piece, allLegalMoves, position);
                    if(!(lm.mate || lm.check))
                        break;
                } 

                // Normal move
                LerpTween.MoveTo(piece.transform, position, 0.25f);
                main_chess.Move(lm.notation);

                if (!lm.promotion) // Because promotion is handled by a different function call.
                                   // similar to a seperate thread. So we clear it when the user ends up choosing some piece
                    ClearLegalMoveIndicators();

                break;
            }
        }     
    
        if(!isLegalPosition) // Invalid move case
        {
            LerpTween.MoveTo(piece.transform, originalPosition, 0.1f);
            ClearLegalMoveIndicators();
            return;
        }
    }


    private bool IsThisAnEnPassantSituation(string capturedPosition) 
    {
        return main_chess.GetPiece(capturedPosition) == null; // Only situation where you capture a square without a piece
    }


    public void HandlePromotion(string type, string color, Vector2 targetSquare)
    {
        foreach(BoardPiece bp in allPieces)
        {
            if(bp.type==type.ToLower() && bp.color == color)
            {
                GameObject g = Instantiate(bp.gameObject, pieceHolder);
                g.transform.position = targetSquare;
                LerpTween.ScaleTo(g.transform, Vector3.one, 0.25f);

                int power = ChessboardMapping.GetPowerLevel(bp.type.ToLower());
                if (main_chess.Turn() == "b") { whiteHeldPieces += power; } else { blackHeldPieces += power; }
                // I don't get the piece strength advantages by calculating strength of all pieces. 
                // I do it by keeping track of all captures since thats less intensive. 
                // The only special case is during promotion

                ClearLegalMoveIndicators();
            }
        }
    }
}
