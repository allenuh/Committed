using TMPro;
using UnityEngine;

public class Enemy4AIPrototype : MonoBehaviour
{
    private const int PreviewTurns = 15;
    [SerializeField] private TMP_Text moveOutputText;
    private int currentTurn = 1;
    private int bulkUpsQueued = 0;

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
            Debug.LogWarning("Enemy4AIPrototype: No TMP text field assigned.");
        }

        currentTurn++;
    }

    public void ResetTurnCounter()
    {
        currentTurn = 1;
        bulkUpsQueued = 0;

        if (moveOutputText != null)
        {
            moveOutputText.text = "Turn 1: [move]";
        }
    }

    private void LogInitialPreviewToConsole()
    {
        Random.State savedRandomState = Random.state;
        int savedBulkUpsQueued = bulkUpsQueued;

        for (int turn = 1; turn <= PreviewTurns; turn++)
        {
            Debug.Log($"Turn {turn}: {GetMoveForTurn(turn)}");
        }

        Random.state = savedRandomState;
        bulkUpsQueued = savedBulkUpsQueued;
    }

    private string GetMoveForTurn(int turn)
    {
        if (bulkUpsQueued >= 3)
        {
            return "Queue Attack";
        }

        int remainder = turn % 3;

        if (remainder == 1)
        {
            bulkUpsQueued++;
            return "Queue Bulk Up";
        }

        float roll = Random.value;

        if (roll < 0.3f)
        {
            return "Queue Attack";
        }

        if (roll < 0.7f)
        {
            return "Queue Block";
        }

        return "Wait";
    }
}
