using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToe : MonoBehaviour
{
    public enum GameStatus
    {
        WinX = 1,
        WinO = 2,
        NoOneWin = 3,
        Continue = 4,
    }
    
    private readonly int width = 140;
    private readonly int height = 140;

    private Transform templeButton;
    private Transform btnRoot;
    private Button resetButton;
    private TextMeshProUGUI resultText;
    private List<GameObject> pool = new List<GameObject>();

    private Sprite playerX;
    private Sprite playerO;
    
    private char[] board = new char[9];
    private char currentPlayer = 'X';
    private bool isPlayerTurn = true;

    private void Start()
    {
        ResetGame();
    }

    private void ResetGame()
    {
        InitBoardInfo();

        playerX = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AssetRes/icon/player_X.jpg");
        playerO = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/AssetRes/icon/player_O.png");

        templeButton = transform.Find("templeButton");
        templeButton.gameObject.SetActive(false);

        btnRoot = transform.Find("btnRoot");
        for (int i = 0; i < btnRoot.transform.childCount; i++)
        {
            Button button = btnRoot.GetChild(i).GetComponent<Button>();
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = null;
            }
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                HandleButtonClick(button.transform.name);
            });
        }

        resetButton = transform.Find("resetButton").GetComponent<Button>();
        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(() =>
        {
            ResetGame();
        });

        resultText = transform.Find("resultRoot").Find("resultLabel").GetComponent<TextMeshProUGUI>();
        resultText.text = "";
    }
    
    // 处理玩家点击
    public void HandleButtonClick(string transformName)
    {
        string[] splits = transformName.Split('_');
        int index = int.Parse(splits[0]);
        if (isPlayerTurn && board[index] == ' ')
        {
            PlayerMove(index);
        }
    }
    
    // 初始化棋盘信息
    public void InitBoardInfo()
    {
        isPlayerTurn = true;
        currentPlayer = 'X';
        for (int i = 0; i < board.Length; i++)
        {
            board[i] = ' ';
        }
    }
    
    // 玩家移动
    public void PlayerMove(int index)
    {
        if (isPlayerTurn && board[index] == ' ')
        {
            board[index] = currentPlayer;
            isPlayerTurn = false;

            DrawBoardInfo();
            GameStatus status = CheckForGameOver();
            if (status == GameStatus.Continue)
            {
                StartCoroutine(ComputerMove());
            }
            else
            {
                ShowResultInfo(status);
            }
        }
    }

    // 计算机移动
    public IEnumerator ComputerMove()
    {
        yield return new WaitForSeconds(1);     // 模拟计算机思考
        int bestMove = GetBestMove(board, 'O');
        if (bestMove >= 0)
        {
            board[bestMove] = 'O';
            DrawBoardInfo();
            GameStatus status = CheckForGameOver();
            if (status == GameStatus.Continue)
            {
                isPlayerTurn = true;
            }
            else
            {
                ShowResultInfo(status);
            }
        }
    }
    
    // 检查游戏是否结束
    private GameStatus CheckForGameOver()
    {
        GameStatus status = GameStatus.Continue;
        if (IsWinner(board, 'X'))
        {
            status = GameStatus.WinX;
        }
        else if (IsWinner(board, 'O'))
        {
            status = GameStatus.WinO;
        }
        else if (IsBoardFull(board))
        {
            status = GameStatus.NoOneWin;
        }
        else
        {
            status = GameStatus.Continue;
        }

        return status;
    }

    private void ShowResultInfo(GameStatus status)
    {
        if (status == GameStatus.WinX)
        {
            Debug.LogError("X Win");
            resultText.text = "X Win";
        }
        else if (status == GameStatus.WinO)
        {
            Debug.LogError("O Win");
            resultText.text = "O Win";
        }
        else if (status == GameStatus.NoOneWin)
        {
            Debug.LogError("No one win");
            resultText.text = "No one Win";
        }
        else
        {
            resultText.text = "";
        }
    }

    private void DrawBoardInfo()
    {
        for (int i = 0; i < btnRoot.transform.childCount; i++)
        {
            string spriteName = "";
            if (board[i] == 'X')
            {
                spriteName = "player_X";
            }
            else if (board[i] == 'O')
            {
                spriteName = "player_O";
            }
            
            Button button = btnRoot.transform.GetChild(i).GetComponent<Button>();
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                if (!string.IsNullOrEmpty(spriteName))
                {
                    image.sprite = spriteName == "player_X" ? playerX : playerO;
                }
                else
                {
                    image.sprite = null;
                }
            }
        }
    }
    
    # region 静态方法
    
    public static bool IsWinner(char[] bo, char le)
    {
        return ((bo[0] == le && bo[1] == le && bo[2] == le) ||
                (bo[3] == le && bo[4] == le && bo[5] == le) ||
                (bo[6] == le && bo[7] == le && bo[8] == le) ||
                (bo[0] == le && bo[3] == le && bo[6] == le) ||
                (bo[1] == le && bo[4] == le && bo[7] == le) ||
                (bo[2] == le && bo[5] == le && bo[8] == le) ||
                (bo[0] == le && bo[4] == le && bo[8] == le) ||
                (bo[2] == le && bo[4] == le && bo[6] == le));
    }

    public static bool IsSpaceFree(char[] bo, int index)
    {
        return bo[index] == ' ';
    }

    public static bool IsBoardFull(char[] bo)
    {
        bool isFull = true;
        for (int i = 0; i < bo.Length; i++)
        {
            if (bo[i] != ' ')
            {
                isFull = false;
                break;
            }
        }

        return isFull;
    }

    public static int MiniMax(char[] bo, int depth, bool isMaximizing, int alpha, int beta)
    {
        if (IsWinner(bo, 'X'))
        {
            return 10 - depth;
        }

        if (IsWinner(bo, 'O'))
        {
            return depth - 10;
        }

        if (IsBoardFull(bo))
        {
            return 0;
        }

        if (isMaximizing)
        {
            int bestScore = int.MinValue;
            for (int i = 0; i < bo.Length; i++)
            {
                if (IsSpaceFree(bo, i))
                {
                    bo[i] = 'X';
                    int score = MiniMax(bo, depth + 1, false, alpha, beta);
                    bo[i] = ' ';
                    bestScore = Math.Max(bestScore, score);
                    alpha = Math.Max(alpha, score);
                    if(beta <= alpha)
                        break;
                }
            }

            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            for (int i = 0; i < bo.Length; i++)
            {
                if (IsSpaceFree(bo, i))
                {
                    bo[i] = 'O';
                    int score = MiniMax(bo, depth + 1, true, alpha, beta);
                    bo[i] = ' ';
                    bestScore = Math.Min(bestScore, score);
                    beta = Math.Min(beta, score);
                    if(beta <= alpha)
                        break;
                }
            }

            return bestScore;
        }
    }

    public static int GetBestMove(char[] bo, char player)
    {
        int bestScore = player == 'X' ? int.MinValue : int.MaxValue;
        int bestMove = -1;
        for (int i = 0; i < bo.Length; i++)
        {
            if (IsSpaceFree(bo, i))
            {
                bo[i] = player;
                int score = MiniMax(bo, 0, player == 'O', int.MinValue, int.MaxValue);
                Debug.LogError($"index={i}, score = {score}");
                bo[i] = ' ';
                if (player == 'X' && score > bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
                else if (player == 'O' && score < bestScore)
                {
                    bestScore = score;
                    bestMove = i;
                }
            }
        }

        return bestMove;
    }
    
    # endregion
}
