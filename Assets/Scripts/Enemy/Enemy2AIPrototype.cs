using TMPro;
using UnityEngine;

public class Enemy2AIPrototype : MonoBehaviour
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
            Debug.LogWarning("Enemy2AIPrototype: No TMP text field assigned.");
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
        int remainder = turn % 3;
        float roll = Random.value;

        switch (remainder)
        {
            case 1:
                return roll < 0.7f ? "Queue Attack" : "Queue Block";
            case 2:
                if (roll < 0.45f)
                {
                    return "Queue Counter";
                }

                if (roll < 0.9f)
                {
                    return "Queue Heal";
                }

                return "Queue Heavy Attack";
            default:
                // turn % 3 == 0 is the third step in the pattern cycle.
                return "Wait";
        }
    }
}
