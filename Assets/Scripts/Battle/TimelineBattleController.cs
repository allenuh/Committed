using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum ActionType
{
    Attack,
    HeavyAttack,
    Block,
    Counter,
    Wait
}

public struct QueuedAction
{
    public int resolveTurn;
    public int sequence;
    public BattleActor caster;
    public BattleActor target;
    public ActionType type;
    public int amount;
}

public class TimelineBattleController : MonoBehaviour
{
    private const int AttackDelay = 2;
    private const int HeavyAttackDelay = 3;
    private const int BlockDelay = 1;
    private const int CounterDelay = 2;
    private const int HeavyAttackDamageMultiplier = 2;
    private const int PlayerStartingMana = 3;
    private const int PlayerManaRegenPerTurn = 2;
    private const int AttackManaCost = 2;
    private const int HeavyAttackManaCost = 3;
    private const int BlockManaCost = 1;
    private const int CounterManaCost = 2;

    [Header("Scene References")]
    [SerializeField] private BattleActor playerActor;
    [SerializeField] private BattleActor enemy1Actor;
    [SerializeField] private BattleActor enemy2Actor;
    [SerializeField] private TimelineBattleView view;

    private readonly List<QueuedAction> queuedActions = new List<QueuedAction>();
    private readonly List<string> timelineLogLines = new List<string>();

    private int currentTurn;
    private int nextSequence;
    private bool enemy1UsesAttack;
    private bool enemy2UsesAttack;
    private bool battleEnded;
    private bool waitingForPlayerAction;
    private bool playerTurnLockedByWait;
    private bool hasEnteredPlayerTurn;
    private ActionType? selectedPlayerAction;
    private int playerMana;

    private void Awake()
    {
        view.BindButtons(
            OnSelectAttackCardPressed,
            OnSelectHeavyAttackCardPressed,
            OnSelectBlockCardPressed,
            OnSelectCounterCardPressed,
            OnSelectWaitCardPressed,
            OnTargetEnemy1Pressed,
            OnTargetEnemy2Pressed,
            OnTargetPlayerPressed,
            OnNextTurnPressed);
    }

    private void Start()
    {
        StartBattle();
    }

    private void OnDestroy()
    {
        view.UnbindButtons(
            OnSelectAttackCardPressed,
            OnSelectHeavyAttackCardPressed,
            OnSelectBlockCardPressed,
            OnSelectCounterCardPressed,
            OnSelectWaitCardPressed,
            OnTargetEnemy1Pressed,
            OnTargetEnemy2Pressed,
            OnTargetPlayerPressed,
            OnNextTurnPressed);
    }

    public void OnSelectAttackCardPressed()
    {
        if (battleEnded || !waitingForPlayerAction || playerTurnLockedByWait)
        {
            return;
        }

        selectedPlayerAction = ActionType.Attack;
        UpdateCardAndTargetAvailability();
        UpdateView();
    }

    public void OnSelectHeavyAttackCardPressed()
    {
        if (battleEnded || !waitingForPlayerAction || playerTurnLockedByWait)
        {
            return;
        }

        selectedPlayerAction = ActionType.HeavyAttack;
        UpdateCardAndTargetAvailability();
        UpdateView();
    }

    public void OnSelectBlockCardPressed()
    {
        if (battleEnded || !waitingForPlayerAction || playerTurnLockedByWait)
        {
            return;
        }

        selectedPlayerAction = ActionType.Block;
        UpdateCardAndTargetAvailability();
        UpdateView();
    }

    public void OnSelectCounterCardPressed()
    {
        if (battleEnded || !waitingForPlayerAction || playerTurnLockedByWait)
        {
            return;
        }

        selectedPlayerAction = ActionType.Counter;
        UpdateCardAndTargetAvailability();
        UpdateView();
    }

    public void OnSelectWaitCardPressed()
    {
        if (battleEnded || !waitingForPlayerAction || playerTurnLockedByWait)
        {
            return;
        }

        playerTurnLockedByWait = true;
        selectedPlayerAction = null;
        LogTimeline($"Turn {currentTurn}: Player waited.");
        UpdateCardAndTargetAvailability();
        UpdateView();
    }

    public void OnTargetEnemy1Pressed()
    {
        TryQueueSelectedPlayerAction(enemy1Actor);
    }

    public void OnTargetEnemy2Pressed()
    {
        TryQueueSelectedPlayerAction(enemy2Actor);
    }

    public void OnTargetPlayerPressed()
    {
        TryQueueSelectedPlayerAction(playerActor);
    }

    public void OnNextTurnPressed()
    {
        if (battleEnded || !waitingForPlayerAction)
        {
            return;
        }

        waitingForPlayerAction = false;
        playerTurnLockedByWait = false;
        selectedPlayerAction = null;
        view.SetCardButtonsEnabled(false, false, false, false, false);
        view.SetTargetButtonsEnabled(false, false, false);
        view.SetNextTurnEnabled(false);

        currentTurn++;
        BeginTurn();
    }

    private void StartBattle()
    {
        queuedActions.Clear();
        timelineLogLines.Clear();

        nextSequence = 0;
        currentTurn = 1;
        enemy1UsesAttack = true;
        enemy2UsesAttack = true;
        battleEnded = false;
        waitingForPlayerAction = false;
        playerTurnLockedByWait = false;
        hasEnteredPlayerTurn = false;
        selectedPlayerAction = null;
        playerMana = PlayerStartingMana;

        playerActor.ResetState();
        enemy1Actor.ResetState();
        enemy2Actor.ResetState();

        view.SetNextTurnEnabled(false);
        view.SetCardButtonsEnabled(false, false, false, false, false);
        view.SetTargetButtonsEnabled(false, false, false);
        view.SetMana(playerMana);
        LogTimeline("Battle started.");
        BeginTurn();
    }

    private void BeginTurn()
    {
        ResolveDueActionsForCurrentTurn();
        if (EvaluateBattleState())
        {
            return;
        }

        QueueEnemyAction(enemy1Actor);
        QueueEnemyAction(enemy2Actor);

        if (!playerActor.IsAlive)
        {
            EndBattle("Defeat.");
            return;
        }

        if (hasEnteredPlayerTurn)
        {
            playerMana += PlayerManaRegenPerTurn;
        }
        else
        {
            hasEnteredPlayerTurn = true;
        }

        waitingForPlayerAction = true;
        playerTurnLockedByWait = false;
        selectedPlayerAction = null;
        view.SetNextTurnEnabled(true);
        UpdateCardAndTargetAvailability();
        UpdateView();
    }

    private void ResolveDueActionsForCurrentTurn()
    {
        List<QueuedAction> dueActions = new List<QueuedAction>();

        for (int i = 0; i < queuedActions.Count; i++)
        {
            if (queuedActions[i].resolveTurn <= currentTurn)
            {
                dueActions.Add(queuedActions[i]);
            }
        }

        dueActions.Sort((left, right) =>
        {
            int turnCompare = left.resolveTurn.CompareTo(right.resolveTurn);
            return turnCompare != 0 ? turnCompare : left.sequence.CompareTo(right.sequence);
        });

        for (int i = 0; i < dueActions.Count; i++)
        {
            RemoveQueuedAction(dueActions[i].sequence);
            ResolveSingleAction(dueActions[i]);
        }

        UpdateView();
    }

    private void ResolveSingleAction(QueuedAction action)
    {
        if (!action.caster.IsAlive)
        {
            LogTimeline($"Turn {currentTurn}: {GetActorName(action.caster)} action canceled (caster defeated).");
            return;
        }

        if (action.type == ActionType.Block)
        {
            action.caster.ApplyBlock();
            LogTimeline($"Turn {currentTurn}: {action.caster.UnitName} is guarding.");
            return;
        }

        if (action.type == ActionType.Counter)
        {
            action.caster.ApplyCounter();
            LogTimeline($"Turn {currentTurn}: {action.caster.UnitName} is countering.");
            return;
        }

        if (action.type == ActionType.Wait)
        {
            LogTimeline($"Turn {currentTurn}: {action.caster.UnitName} waited.");
            return;
        }

        if (!action.target.IsAlive)
        {
            LogTimeline($"Turn {currentTurn}: {action.caster.UnitName} attack canceled (target defeated).");
            return;
        }

        bool targetWasBlocking = action.target.HasPendingBlock;
        bool targetWasCountering = action.target.HasPendingCounter;
        DamageResolution damageResult = action.target.ApplyIncomingAttack(action.amount);
        string defenseSuffix = targetWasCountering ? " (countered)" : targetWasBlocking ? " (blocked)" : string.Empty;
        LogTimeline($"Turn {currentTurn}: {action.caster.UnitName} hit {action.target.UnitName} for {damageResult.damageTaken}.{defenseSuffix}");

        if (targetWasCountering && damageResult.reflectedDamage > 0 && action.caster.IsAlive)
        {
            int reflectedDamage = action.caster.ApplyDamage(damageResult.reflectedDamage);
            LogTimeline($"Turn {currentTurn}: {action.target.UnitName} reflected {reflectedDamage} to {action.caster.UnitName}.");
        }
    }

    private void QueueEnemyAction(BattleActor enemy)
    {
        if (!enemy.IsAlive)
        {
            LogTimeline($"Turn {currentTurn}: {enemy.UnitName} is defeated and skips.");
            return;
        }

        bool useAttack = ConsumeEnemyPattern(enemy);

        if (useAttack)
        {
            QueueAction(enemy, playerActor, ActionType.Attack, enemy.AttackDamage, AttackDelay);
            LogTimeline($"Turn {currentTurn}: {enemy.UnitName} queued Attack for turn {currentTurn + AttackDelay}.");
        }
        else
        {
            QueueAction(enemy, enemy, ActionType.Block, 0, BlockDelay);
            LogTimeline($"Turn {currentTurn}: {enemy.UnitName} queued Block for turn {currentTurn + BlockDelay}.");
        }

        UpdateView();
    }

    private bool ConsumeEnemyPattern(BattleActor enemy)
    {
        if (enemy == enemy1Actor)
        {
            bool choice = enemy1UsesAttack;
            enemy1UsesAttack = !enemy1UsesAttack;
            return choice;
        }

        if (enemy == enemy2Actor)
        {
            bool choice = enemy2UsesAttack;
            enemy2UsesAttack = !enemy2UsesAttack;
            return choice;
        }

        return true;
    }

    private void QueueAction(BattleActor caster, BattleActor target, ActionType type, int amount, int delay)
    {
        QueuedAction action = new QueuedAction
        {
            resolveTurn = currentTurn + delay,
            sequence = nextSequence++,
            caster = caster,
            target = target,
            type = type,
            amount = Mathf.Max(0, amount)
        };

        queuedActions.Add(action);
    }

    private void RemoveQueuedAction(int sequence)
    {
        for (int i = 0; i < queuedActions.Count; i++)
        {
            if (queuedActions[i].sequence == sequence)
            {
                queuedActions.RemoveAt(i);
                return;
            }
        }
    }

    private bool EvaluateBattleState()
    {
        bool playerDead = !playerActor.IsAlive;
        bool enemiesDead = !enemy1Actor.IsAlive && !enemy2Actor.IsAlive;

        if (!playerDead && !enemiesDead)
        {
            return false;
        }

        if (playerDead && enemiesDead)
        {
            EndBattle("Both sides defeated.");
            return true;
        }

        if (playerDead)
        {
            EndBattle("Defeat.");
            return true;
        }

        EndBattle("Victory.");
        return true;
    }

    private void EndBattle(string result)
    {
        if (battleEnded)
        {
            return;
        }

        battleEnded = true;
        waitingForPlayerAction = false;
        playerTurnLockedByWait = false;
        selectedPlayerAction = null;
        view.SetCardButtonsEnabled(false, false, false, false, false);
        view.SetTargetButtonsEnabled(false, false, false);
        view.SetNextTurnEnabled(false);
        LogTimeline(result);
        UpdateView();
    }

    private void LogTimeline(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        timelineLogLines.Add(message);
    }

    private void UpdateView()
    {
        view.SetHealth(playerActor, enemy1Actor, enemy2Actor);
        view.SetMana(playerMana);
        if (battleEnded)
        {
            view.SetTurnText("Battle complete.");
        }
        else if (waitingForPlayerAction)
        {
            if (playerTurnLockedByWait)
            {
                view.SetTurnText($"Turn {currentTurn}: Wait selected");
            }
            else if (selectedPlayerAction.HasValue)
            {
                view.SetTurnText($"Turn {currentTurn}: Target for {selectedPlayerAction.Value}");
            }
            else
            {
                view.SetTurnText($"Turn {currentTurn}: Select Card");
            }
        }
        else
        {
            view.SetTurnText($"Turn {currentTurn}: Ready (press Next Turn)");
        }

        view.SetTimelineText(BuildTimelineLogText());
        view.SetUnitStatusTexts(
            BuildQueuedActionsTextForUnit(enemy1Actor),
            BuildQueuedActionsTextForUnit(enemy2Actor),
            BuildQueuedActionsTextForUnit(playerActor));
    }

    private string BuildQueuedActionsTextForUnit(BattleActor actor)
    {
        List<QueuedAction> sorted = new List<QueuedAction>();

        for (int i = 0; i < queuedActions.Count; i++)
        {
            if (queuedActions[i].caster == actor)
            {
                sorted.Add(queuedActions[i]);
            }
        }

        sorted.Sort((left, right) =>
        {
            int turnCompare = left.resolveTurn.CompareTo(right.resolveTurn);
            return turnCompare != 0 ? turnCompare : left.sequence.CompareTo(right.sequence);
        });

        if (sorted.Count == 0)
        {
            return "(empty)";
        }

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < sorted.Count; i++)
        {
            QueuedAction action = sorted[i];
            string targetName = action.target.UnitName;
            int remainingTurns = Mathf.Max(0, action.resolveTurn - currentTurn);
            builder.Append(GetActorName(action.caster))
                .Append(" ")
                .Append(action.type)
                .Append(" -> ")
                .Append(targetName)
                .Append(" In ")
                .Append(remainingTurns)
                .Append(" Turns");

            if (i < sorted.Count - 1)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private string BuildTimelineLogText()
    {
        if (timelineLogLines.Count == 0)
        {
            return "Timeline:\n(no events yet)";
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Timeline:");

        for (int i = 0; i < timelineLogLines.Count; i++)
        {
            builder.Append(timelineLogLines[i]);

            if (i < timelineLogLines.Count - 1)
            {
                builder.AppendLine();
            }
        }

        return builder.ToString();
    }

    private string GetActorName(BattleActor actor)
    {
        return string.IsNullOrWhiteSpace(actor.UnitName) ? actor.name : actor.UnitName;
    }

    private void CompletePlayerActionAndWaitForNextTurn()
    {
        selectedPlayerAction = null;
        UpdateCardAndTargetAvailability();
        UpdateView();
    }

    private void TryQueueSelectedPlayerAction(BattleActor target)
    {
        if (battleEnded || !waitingForPlayerAction || !selectedPlayerAction.HasValue)
        {
            return;
        }

        ActionType action = selectedPlayerAction.Value;
        int manaCost = GetManaCost(action);

        if (playerMana < manaCost)
        {
            return;
        }

        if (action == ActionType.Attack || action == ActionType.HeavyAttack)
        {
            if (target != enemy1Actor && target != enemy2Actor)
            {
                return;
            }

            if (!target.IsAlive)
            {
                return;
            }

            if (action == ActionType.Attack)
            {
                QueueAction(playerActor, target, ActionType.Attack, playerActor.AttackDamage, AttackDelay);
                LogTimeline($"Turn {currentTurn}: Player queued Attack for turn {currentTurn + AttackDelay}.");
                playerMana -= AttackManaCost;
            }
            else
            {
                int heavyDamage = Mathf.Max(1, playerActor.AttackDamage * HeavyAttackDamageMultiplier);
                QueueAction(playerActor, target, ActionType.HeavyAttack, heavyDamage, HeavyAttackDelay);
                LogTimeline($"Turn {currentTurn}: Player queued Heavy Attack for turn {currentTurn + HeavyAttackDelay}.");
                playerMana -= HeavyAttackManaCost;
            }

            CompletePlayerActionAndWaitForNextTurn();
            return;
        }

        if (action == ActionType.Block)
        {
            if (target != playerActor || !playerActor.IsAlive)
            {
                return;
            }

            QueueAction(playerActor, playerActor, ActionType.Block, 0, BlockDelay);
            LogTimeline($"Turn {currentTurn}: Player queued Block for turn {currentTurn + BlockDelay}.");
            playerMana -= BlockManaCost;
            CompletePlayerActionAndWaitForNextTurn();
            return;
        }

        if (action == ActionType.Counter)
        {
            if (target != playerActor || !playerActor.IsAlive)
            {
                return;
            }

            QueueAction(playerActor, playerActor, ActionType.Counter, 0, CounterDelay);
            LogTimeline($"Turn {currentTurn}: Player queued Counter for turn {currentTurn + CounterDelay}.");
            playerMana -= CounterManaCost;
            CompletePlayerActionAndWaitForNextTurn();
        }
    }

    private void UpdateTargetButtons()
    {
        if (!waitingForPlayerAction || playerTurnLockedByWait || !selectedPlayerAction.HasValue)
        {
            view.SetTargetButtonsEnabled(false, false, false);
            return;
        }

        ActionType selectedAction = selectedPlayerAction.Value;
        int manaCost = GetManaCost(selectedAction);
        if (playerMana < manaCost)
        {
            view.SetTargetButtonsEnabled(false, false, false);
            return;
        }

        if (selectedAction == ActionType.Attack || selectedAction == ActionType.HeavyAttack)
        {
            view.SetTargetButtonsEnabled(enemy1Actor.IsAlive, enemy2Actor.IsAlive, false);
            return;
        }

        if (selectedAction == ActionType.Block || selectedAction == ActionType.Counter)
        {
            view.SetTargetButtonsEnabled(false, false, playerActor.IsAlive);
            return;
        }

        view.SetTargetButtonsEnabled(false, false, false);
    }

    private void UpdateCardAndTargetAvailability()
    {
        if (!waitingForPlayerAction || playerTurnLockedByWait || battleEnded)
        {
            view.SetCardButtonsEnabled(false, false, false, false, false);
            view.SetTargetButtonsEnabled(false, false, false);
            return;
        }

        view.SetCardButtonsEnabled(
            playerMana >= AttackManaCost,
            playerMana >= HeavyAttackManaCost,
            playerMana >= BlockManaCost,
            playerMana >= CounterManaCost,
            true);

        UpdateTargetButtons();
    }

    private int GetManaCost(ActionType action)
    {
        if (action == ActionType.Attack)
        {
            return AttackManaCost;
        }

        if (action == ActionType.HeavyAttack)
        {
            return HeavyAttackManaCost;
        }

        if (action == ActionType.Block)
        {
            return BlockManaCost;
        }

        if (action == ActionType.Counter)
        {
            return CounterManaCost;
        }

        return 0;
    }
}
