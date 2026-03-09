using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimelineBattleView : MonoBehaviour
{
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private TMP_Text playerManaText;
    [SerializeField] private TMP_Text enemy1HealthText;
    [SerializeField] private TMP_Text enemy2HealthText;
    [SerializeField] private TMP_Text turnActorText;
    [SerializeField] private TMP_Text timelineText;
    [SerializeField] private TMP_Text enemy1StatusText;
    [SerializeField] private TMP_Text enemy2StatusText;
    [SerializeField] private TMP_Text playerStatusText;
    [SerializeField] private Button attackCardButton;
    [SerializeField] private Button heavyAttackCardButton;
    [SerializeField] private Button blockCardButton;
    [SerializeField] private Button counterCardButton;
    [SerializeField] private Button waitCardButton;
    [SerializeField] private Button enemy1TargetButton;
    [SerializeField] private Button enemy2TargetButton;
    [SerializeField] private Button playerTargetButton;
    [SerializeField] private Button nextTurnButton;

    public void BindButtons(
        UnityAction onSelectAttack,
        UnityAction onSelectHeavyAttack,
        UnityAction onSelectBlock,
        UnityAction onSelectCounter,
        UnityAction onSelectWait,
        UnityAction onTargetEnemy1,
        UnityAction onTargetEnemy2,
        UnityAction onTargetPlayer,
        UnityAction onNextTurn)
    {
        attackCardButton.onClick.AddListener(onSelectAttack);
        heavyAttackCardButton.onClick.AddListener(onSelectHeavyAttack);
        blockCardButton.onClick.AddListener(onSelectBlock);
        counterCardButton.onClick.AddListener(onSelectCounter);
        waitCardButton.onClick.AddListener(onSelectWait);
        enemy1TargetButton.onClick.AddListener(onTargetEnemy1);
        enemy2TargetButton.onClick.AddListener(onTargetEnemy2);
        playerTargetButton.onClick.AddListener(onTargetPlayer);
        nextTurnButton.onClick.AddListener(onNextTurn);
    }

    public void UnbindButtons(
        UnityAction onSelectAttack,
        UnityAction onSelectHeavyAttack,
        UnityAction onSelectBlock,
        UnityAction onSelectCounter,
        UnityAction onSelectWait,
        UnityAction onTargetEnemy1,
        UnityAction onTargetEnemy2,
        UnityAction onTargetPlayer,
        UnityAction onNextTurn)
    {
        attackCardButton.onClick.RemoveListener(onSelectAttack);
        heavyAttackCardButton.onClick.RemoveListener(onSelectHeavyAttack);
        blockCardButton.onClick.RemoveListener(onSelectBlock);
        counterCardButton.onClick.RemoveListener(onSelectCounter);
        waitCardButton.onClick.RemoveListener(onSelectWait);
        enemy1TargetButton.onClick.RemoveListener(onTargetEnemy1);
        enemy2TargetButton.onClick.RemoveListener(onTargetEnemy2);
        playerTargetButton.onClick.RemoveListener(onTargetPlayer);
        nextTurnButton.onClick.RemoveListener(onNextTurn);
    }

    public void SetCardButtonsEnabled(
        bool attackEnabled,
        bool heavyAttackEnabled,
        bool blockEnabled,
        bool counterEnabled,
        bool waitEnabled)
    {
        attackCardButton.interactable = attackEnabled;
        heavyAttackCardButton.interactable = heavyAttackEnabled;
        blockCardButton.interactable = blockEnabled;
        counterCardButton.interactable = counterEnabled;
        waitCardButton.interactable = waitEnabled;
    }

    public void SetTargetButtonsEnabled(bool enemy1Enabled, bool enemy2Enabled, bool playerEnabled)
    {
        enemy1TargetButton.interactable = enemy1Enabled;
        enemy2TargetButton.interactable = enemy2Enabled;
        playerTargetButton.interactable = playerEnabled;
    }

    public void SetNextTurnEnabled(bool isEnabled)
    {
        nextTurnButton.interactable = isEnabled;
    }

    public void SetHealth(BattleActor player, BattleActor enemy1, BattleActor enemy2)
    {
        playerHealthText.text = $"{player.CurrentHealth}/{player.MaxHealth}";
        enemy1HealthText.text = $"{enemy1.CurrentHealth}/{enemy1.MaxHealth}";
        enemy2HealthText.text = $"{enemy2.CurrentHealth}/{enemy2.MaxHealth}";
    }

    public void SetMana(int mana)
    {
        playerManaText.text = mana.ToString();
    }

    public void SetTurnText(string value)
    {
        turnActorText.text = value;
    }

    public void SetTimelineText(string value)
    {
        timelineText.text = value;
    }

    public void SetUnitStatusTexts(string enemy1Value, string enemy2Value, string playerValue)
    {
        enemy1StatusText.text = enemy1Value;
        enemy2StatusText.text = enemy2Value;
        playerStatusText.text = playerValue;
    }
}
