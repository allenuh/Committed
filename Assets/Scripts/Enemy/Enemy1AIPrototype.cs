using TMPro;
using UnityEngine;

public class EnemyAIPrototype : MonoBehaviour
{
    private const int PreviewTurns = 15;
    [SerializeField] private TMP_Text moveOutputText;
    private int currentTurn = 1;

    private void Start()
    {
        LogInitialPreviewToConsole();
    }

    public void OutputNextMove()
    {
        string nextMoveText = $"Turn {currentTurn}: {GetMoveForTurn(currentTurn)}";

        if (moveOutputText != null)
        {
            moveOutputText.text = nextMoveText;
        }
        else
        {
            Debug.LogWarning("EnemyAIPrototype: No TMP text field assigned.");
        }

        currentTurn++;
    }

    public void ResetTurnCounter()
    {
        currentTurn = 1;

        if (moveOutputText != null)
        {
            moveOutputText.text = "Turn 1: [move]";
        }
    }

    private void LogInitialPreviewToConsole()
    {
        for (int turn = 1; turn <= PreviewTurns; turn++)
        {
            Debug.Log($"Turn {turn}: {GetMoveForTurn(turn)}");
        }
    }

    private string GetMoveForTurn(int turn)
    {
        int patternIndex = (turn - 1) % 3;

        switch (patternIndex)
        {
            case 0:
                return "Wait";
            case 1:
                return "Queue Attack";
            default:
                return "Queue Block";
        }
    }
}
