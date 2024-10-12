// Script that handles user interaction with the board, and saving and loading of games

using UnityEngine;
using TMPro;
using ChessValidator;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask _boardMask, _piecesMask;
    public Engine engine;
    private BoardPiece selectedPiece;    
    private Vector3 originalPosition;      
    private bool _isDragging = false; // Track whether a piece is being dragged

    public int selectedPieceLayer = 4;

    [SerializeField] private TMP_Text historyText;
    [SerializeField] private TMP_Text whitePower; // lmao
    [SerializeField] private TMP_Text blackPower;
    public GameObject wTM, bTM;
    public ChessTimer timer;

    public bool gameBegan = false;

    public TMP_Dropdown arrayNewGameTime;
    public TMP_Dropdown overNewGameTime;
    public TMP_Dropdown endGameDropdown;

    public GameObject gameOverWindow;
    public TMP_Text gameOverCause;

    private GameObject CheckBox;
    public GameObject checkBoxPrefab;

    private void Start() // Unity callback when game starts
    {
        timer.timerUpCallback = TimerUp;
        timer.playerOneTime = UserDataManager.GetFloat("whiteTimer");
        timer.playerTwoTime = UserDataManager.GetFloat("blackTimer");

        string storedFen = UserDataManager.GetString("fen");
        engine.blackHeldPieces = UserDataManager.GetInt("blackPower");
        engine.whiteHeldPieces = UserDataManager.GetInt("whitePower");


        if(!string.IsNullOrEmpty(storedFen)) // Found a saved game
        {
            gameBegan = true;
            engine.StartGame(storedFen);
        }
        else // Start a new game
        {  
            NewGame("rapid");
        }
        UpdateUI(); // We don't control which Start() callback is called first
        timer.InitDisplay(engine.main_chess.Turn() == "w");
    }

    void Update()
    {
        timer.timerRunning = gameBegan;
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // On Enter
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 10, _piecesMask);

            if (hit.collider != null && hit.collider.CompareTag("Piece"))
            {
                selectedPiece = hit.collider.GetComponent<BoardPiece>();
                originalPosition = selectedPiece.transform.position;
                _isDragging = true;
                engine.StartDisplayLegalMoves(selectedPiece, originalPosition);
                selectedPiece.GetComponent<SpriteRenderer>().sortingOrder = selectedPieceLayer;
            }
        }

        if (_isDragging && selectedPiece != null) // During
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectedPiece.transform.position = new Vector3(mousePosition.x, mousePosition.y, originalPosition.z);
        }

        if (Input.GetMouseButtonUp(0) && _isDragging) // On Exit
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100, _boardMask);
            if(hit == false) // Outside the board
            {
                Debug.Log("Didn't hit the board");
                LerpTween.MoveTo(selectedPiece.transform, originalPosition, 0.05f);
                engine.ClearLegalMoveIndicators();
            }
            if (hit && hit.collider.CompareTag("Square")) // On any square of the board
            {
                if(selectedPiece!= null)
                    engine.PieceDropped(selectedPiece, hit.collider.transform.position, originalPosition);
            }
            _isDragging = false;
            selectedPiece.GetComponent<SpriteRenderer>().sortingOrder = 2;
            selectedPiece = null;
            UpdateUI();
        }
    }

    uint moveCount = 0;
    private void UpdateText()
    {
        string[] hist = engine.main_chess.MoveHistory();
        string actual = "";
        foreach (string histItem in hist)
        {
            actual += histItem + (moveCount%2==0?" + ": ", ");
            moveCount++;
        }

        historyText.text = actual;
    }

    private void UpdateUI() // Refreshing all UI elements every move.
    {
        if(CheckBox != null)
            Destroy(CheckBox);
        CheckBox = null;

        bool whiteTurn = engine.main_chess.Turn() == "w";

        if (engine.main_chess.GameOver())
        {
            if(engine.main_chess.InCheckmate())
            {
                if (whiteTurn)
                    EndGame("Black wins by checkmate.");
                else
                    EndGame("White wins by checkmate.");
            }
            else if(engine.main_chess.InStalemate())
            {
                   EndGame("Draw by stalemate.");
            }
            else if(engine.main_chess.InThreefoldRepetition())
            {
                   EndGame("Draw by three-fold repitition.");
            }
            else if(engine.main_chess.InsufficientMaterial())
            {
                   EndGame("Draw by insufficient material.");
            }
            else if(engine.main_chess.FiftyMoveRule())
            {
                   EndGame("Draw by fifty-move rule.");
            }
        }
        else if(engine.main_chess.InCheck())
        {
            if (whiteTurn)
                CheckBox = Instantiate(checkBoxPrefab, engine.GetPositionOfKing("w"), Quaternion.identity);
            else
                CheckBox = Instantiate(checkBoxPrefab, engine.GetPositionOfKing("b"), Quaternion.identity);
        }

        UpdateText();
        
        if (wTM.activeSelf != whiteTurn)
            timer.SwitchTurn(whiteTurn);
        wTM.SetActive(whiteTurn);
        bTM.SetActive(!whiteTurn);

        int wPwr = engine.whiteHeldPieces;
        int bPwr = engine.blackHeldPieces;
        if (wPwr > bPwr)
        {
            blackPower.text = "" ;
            whitePower.text = "+" + (wPwr - bPwr).ToString();
        }
        else if(wPwr < bPwr)
        {
            whitePower.text = "";
            blackPower.text = "+" + (bPwr - wPwr).ToString();
        }
        else
        {
            whitePower.text = "";
            blackPower.text = "";
        }
        

        // Save game state
        string currentFen = engine.main_chess.Fen();
        UserDataManager.SetString("fen", currentFen);
        UserDataManager.SetInt("blackPower", bPwr);
        UserDataManager.SetInt("whitePower", wPwr);
    }

    public void TimerUp(int lostPlayer)
    {
        if (lostPlayer == 1)
            EndGame("Black wins on time.");
        else
            EndGame("White wins on time.");
    }


    public void OnChangeEndGame()
    {
        switch(endGameDropdown.value)
        {
            case 0:
                if (engine.main_chess.Turn() == "w")
                    EndGame("Black wins by resignation.");
                else
                    EndGame("White wins by resignation.");
                break;
            case 1:
                EndGame("Draw by agreement.");
                break;
        }
        endGameDropdown.value = 2;
    }
    public void EndGame(string result) // General End Game
    {
        timer.PauseTimer();
        gameOverWindow.SetActive(true);
        gameOverCause.text = result;
        gameBegan = false;
        UserDataManager.SetString("fen", "");
        UserDataManager.SetInt("blackPower", 0);
        UserDataManager.SetInt("whitePower", 0);
        if (CheckBox != null)
            Destroy(CheckBox.gameObject);
        CheckBox = null;
        
    }

    // Re render board upon Undo
    public void OnClickUndo()
    {
        UndoMoveArgs args = engine.main_chess.Undo();
        if(args != null)
        {
            StartCoroutine(engine.RenderBoard(engine.main_chess));
            UpdateUI();
        }
        else
        {
            Debug.Log("Undo unsuccessful");
        }
    }

    private string _arrayGameType = "blitz";
    private string _overGameType = "blitz";
    public void OnChangeArrayNewGame()
    {
        int choice = arrayNewGameTime.value;
        switch(choice)
        {
            case 0: _arrayGameType = "blitz"; break;
            case 1: _arrayGameType = "rapid"; break;
            case 2: _arrayGameType = "classic"; break;
            default: _arrayGameType = "blitz"; break;
        }
    }
    public void OnChangeOverNewGame()
    {
        int choice = overNewGameTime.value;
        switch (choice)
        {
            case 0: _overGameType = "blitz"; break;
            case 1: _overGameType = "rapid"; break;
            case 2: _overGameType = "classic"; break;
            default: _overGameType = "blitz"; break;
        }
    }
    public void OnClickNewGameArr()
    {
        NewGame(_arrayGameType);
    }
    public void OnClickNewGameOver()
    {
        NewGame(_overGameType);
    }

    public void NewGame(string mode)
    {
        UserDataManager.SetFloat("whiteTimer", 0);
        UserDataManager.SetFloat("blackTimer", 0);
        StartCoroutine(engine.ClearBoardPieces());
        engine.StartGame();
        timer.SetTime(mode);
        gameOverWindow.SetActive(false);
        engine.blackHeldPieces = 0;
        engine.whiteHeldPieces = 0;
        gameBegan = true;
    }
}
