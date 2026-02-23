using TMPro;
using UnityEngine;

public class Enemy3AIPrototype : MonoBehaviour
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
            Debug.LogWarning("Enemy3AIPrototype: No TMP text field assigned.");
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
        Random.State savedRandomState = Random.state;

        for (int turn = 1; turn <= PreviewTurns; turn++)
        {
            Debug.Log($"Turn {turn}: {GetMoveForTurn(turn)}");
        }

        Random.state = savedRandomState;
    }

    private string GetMoveForTurn(int turn)
    {
        if (turn == 1)
        {
            return "Queue Self-Destruct Mode";
        }

        float roll = Random.value;

        if (roll < 0.4f)
        {
            return "Queue Block";
        }

        if (roll < 0.6f)
        {
            return "Queue Heal";
        }

        return "Wait";
    }
}
