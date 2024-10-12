using UnityEngine;
using TMPro;
using System;

public class ChessTimer : MonoBehaviour
{
    public float rapidTime = 900f;
    public float blitzTime = 300f;
    public float classicTime = 1800f;
    public TMP_Text playerOneTimeText;
    public TMP_Text playerTwoTimeText;

    public float playerOneTime;
    public float playerTwoTime;
    private bool isPlayerOneTurn;
    public bool timerRunning;

    public Action<int> timerUpCallback;

    void Update()
    {
        if (timerRunning)
        {
            if (isPlayerOneTurn)
            {
                playerOneTime -= Time.deltaTime;
                if (playerOneTime <= 0)
                {
                    timerRunning = false;
                    playerOneTime = 0;
                    TimerUp(1);
                }
                UpdateTimerDisplay(playerOneTimeText, playerOneTime);
            }
            else
            {
                playerTwoTime -= Time.deltaTime;
                if (playerTwoTime <= 0)
                {
                    timerRunning = false;
                    playerTwoTime = 0;
                    TimerUp(2);
                }
                UpdateTimerDisplay(playerTwoTimeText, playerTwoTime);
            }
        }
    }

    public void InitDisplay(bool whiteTurn)
    {
        isPlayerOneTurn= whiteTurn;
        UpdateTimerDisplay(playerOneTimeText, playerOneTime);
        UpdateTimerDisplay(playerTwoTimeText, playerTwoTime);
    }

    public void TimerUp(int playerNumber)
    {
        timerUpCallback?.Invoke(playerNumber);
    }

    public void SetTime(string mode)
    {
        switch (mode.ToLower())
        {
            case "rapid":
                playerOneTime = rapidTime;
                playerTwoTime = rapidTime;
                break;
            case "blitz":
                playerOneTime = blitzTime;
                playerTwoTime = blitzTime;
                break;
            case "classic":
                playerOneTime = classicTime;
                playerTwoTime = classicTime;
                break;
        }
        ResetTimers();
    }

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void PauseTimer()
    {
        timerRunning = false;
    }

    public void ResetTimers()
    {
        timerRunning = false;
        isPlayerOneTurn = true;
        UpdateTimerDisplay(playerOneTimeText, playerOneTime);
        UpdateTimerDisplay(playerTwoTimeText, playerTwoTime);
    }

    public void SwitchTurn(bool white)
    {
        isPlayerOneTurn = white;
    }
    public float GetTime(bool white)
    {
        return white ? playerOneTime : playerTwoTime;
    }

    public void UpdateTimerDisplay(TMP_Text timerText, float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        if (isPlayerOneTurn)
            UserDataManager.SetFloat("whiteTimer", playerOneTime);
        else
            UserDataManager.SetFloat("blackTimer", playerTwoTime);
    }
}
