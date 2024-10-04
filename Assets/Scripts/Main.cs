using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessboardMapping
{
    private static Dictionary<string, Vector2> chessToVectorMap;
    private static Dictionary<Vector2, string> vectorToChessMap;

    static ChessboardMapping()
    {
        chessToVectorMap = new Dictionary<string, Vector2>();
        vectorToChessMap = new Dictionary<Vector2, string>();
        float unitSize = 1.28f;

        for (int col = 0; col < 8; col++)
        {
            for (int row = 0; row < 8; row++)
            {
                string chessPosition = ((char)('a' + col)).ToString() + (row + 1).ToString();
                Vector2 vectorPosition = new Vector2(col * unitSize, row * unitSize);
                chessToVectorMap[chessPosition] = vectorPosition;
                vectorToChessMap[vectorPosition] = chessPosition;
            }
        }
    }

    public static int GetPowerLevel(string piece)
    {
        switch (piece)
        {
            case "p": return 1;
            case "n": return 3;
            case "b": return 3;
            case "r": return 5;
            case "q": return 9;
            case "k": return 0;
            default: return -1;
        }
    }

    public static string GetDestinationSquare(string move)
    {
        if (move == "O-O")
            return "k_castle";
        else if (move == "O-O-O")
            return "q_castle";
        move = move.TrimEnd('+', '#');

        if (move.Contains("="))
            move = move.Split('=')[0];

        return move.Substring(move.Length - 2);
    }

    public static LegalMove DetectSpecialMoves(LegalMove move)
    {
        // castling
        if (move.notation == "O-O" || move.notation == "O-O-O")
        {
            move.castle = true;
            return move; // No need to check further
        }

        // checkmate
        if (move.notation.EndsWith("#"))
        {
            move.mate = true;
        }
        else if (move.notation.EndsWith("+"))
        {
            move.check = true;
        }

        //promotion
        if (move.notation.Contains("="))
        {
            move.promotion = true;
        }

        //capture
        if (move.notation.Contains("x"))
        {
            move.capture = true;
        }

        return move;
    }

    public static Vector2 GetVectorFromChessPosition(string chessPosition)
    {
        if (chessToVectorMap.TryGetValue(chessPosition, out Vector2 vectorPos))
        {
            return vectorPos;
        }
        else
        {
            Debug.LogError("Invalid chess position: " + chessPosition);
            return Vector2.zero;
        }
    }

    public static string GetChessPositionFromVector(Vector2 vectorPosition)
    {
        if (vectorToChessMap.TryGetValue(vectorPosition, out string chessPos))
        {
            return chessPos;
        }
        else
        {
            if(vectorToChessMap.TryGetValue(GetClosestSquarePosition(vectorPosition), out string chessPos2))
            {
                return chessPos2;
            }
            else
                Debug.LogError("Invalid vector position: " + vectorPosition);
            return "";
        }
    }

    public static Vector2 GetClosestSquarePosition(Vector2 position)
    {
        float minDistance = float.MaxValue;
        Vector2 closestSquare = Vector2.zero;

        foreach (var square in chessToVectorMap.Values)
        {
            float distance = Vector2.Distance(position, square);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestSquare = square;
            }
        }

        return closestSquare;
    }
}

[Serializable]
public class LegalMove
{
    public string notation;
    public Transform box;
    public bool capture = false;
    public bool enpassant = false;
    public bool castle = false;
    public bool check = false, mate = false;
    public bool promotion = false;
    public string chessPosition
    {
        get
        {
            return ChessboardMapping.GetChessPositionFromVector(box.position);
        }
    }
}
