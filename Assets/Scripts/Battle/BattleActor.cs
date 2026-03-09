using UnityEngine;

public struct DamageResolution
{
    public int damageTaken;
    public int reflectedDamage;
}

public enum UnitSide
{
    Player,
    Enemy
}

public class BattleActor : MonoBehaviour
{
    [SerializeField] private string unitName = "Unit";
    [SerializeField] private UnitSide side = UnitSide.Enemy;
    [SerializeField] private int maxHealth = 20;
    [SerializeField] private int attackDamage = 5;

    private int currentHealth;
    private bool hasPendingBlock;
    private bool hasPendingCounter;

    public string UnitName => unitName;
    public UnitSide Side => side;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int AttackDamage => attackDamage;
    public bool HasPendingBlock => hasPendingBlock;
    public bool HasPendingCounter => hasPendingCounter;
    public bool IsAlive => currentHealth > 0;

    private void Awake()
    {
        ResetState();
    }

    public void Configure(string actorName, UnitSide actorSide, int health, int damage)
    {
        unitName = actorName;
        side = actorSide;
        maxHealth = Mathf.Max(1, health);
        attackDamage = Mathf.Max(0, damage);
        ResetState();
    }

    public void ResetState()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        attackDamage = Mathf.Max(0, attackDamage);
        currentHealth = maxHealth;
        hasPendingBlock = false;
        hasPendingCounter = false;
    }

    public void ApplyBlock()
    {
        if (!IsAlive)
        {
            return;
        }

        hasPendingBlock = true;
        hasPendingCounter = false;
    }

    public void ApplyCounter()
    {
        if (!IsAlive)
        {
            return;
        }

        hasPendingCounter = true;
        hasPendingBlock = false;
    }

    public DamageResolution ApplyIncomingAttack(int damageAmount)
    {
        DamageResolution result = new DamageResolution();

        if (!IsAlive)
        {
            return result;
        }

        damageAmount = Mathf.Max(0, damageAmount);
        int resolvedDamage = damageAmount;

        if (hasPendingCounter)
        {
            resolvedDamage /= 2;
            result.reflectedDamage = damageAmount / 2;
            hasPendingCounter = false;
        }
        else if (hasPendingBlock)
        {
            resolvedDamage /= 2;
            hasPendingBlock = false;
        }

        result.damageTaken = Mathf.Min(resolvedDamage, currentHealth);
        currentHealth -= result.damageTaken;

        return result;
    }

    public int ApplyDamage(int damageAmount)
    {
        if (!IsAlive)
        {
            return 0;
        }

        damageAmount = Mathf.Max(0, damageAmount);
        int resolvedDamage = damageAmount;

        if (hasPendingCounter)
        {
            resolvedDamage /= 2;
            hasPendingCounter = false;
        }
        else if (hasPendingBlock)
        {
            resolvedDamage /= 2;
            hasPendingBlock = false;
        }

        resolvedDamage = Mathf.Min(resolvedDamage, currentHealth);
        currentHealth -= resolvedDamage;

        return resolvedDamage;
    }
}
