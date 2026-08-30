using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public interface IPlayerDamageable
{
    void TakeDamage(float damage);
}

[System.Serializable]
public class BossPattern
{
    public int patternID;
    public float attackPower;
}

public class BossControl : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Chase,
        Attack,
        Die
    }

    [Header("Player")]
    public Transform Player;

    [Header("Boss")]
    public float moveSpeed;
    public float detectionRange;
    public float attackDurationMin;
    public float attackDurationMax;
    public float patternCooldownMin;
    public float patternCooldownMax;

    [Header("Health")]
    public float maxHealth = 10000f;
    public Image healthBar;

    [Header("Attack Patterns")]
    public List<BossPattern> attackPatterns =
        new List<BossPattern>();

    public Action<int, float> OnPatternSelected;
    public Action<int> OnPatternAttack;
    public Action OnDeath;

    private float currentHealth;
    private bool isDead;
    private bool canMove = true;

    private List<BossPattern> remainingPatterns =
        new List<BossPattern>();

    private Dictionary<BossState, IState<BossControl>> states =
        new Dictionary<BossState, IState<BossControl>>();

    private StateMachine<BossControl> stateMachine;

    void Start()
    {
        currentHealth = maxHealth;

        UpdateHealthBar();

        states.Add(BossState.Idle, new BossIdle());
        states.Add(BossState.Chase, new BossChase());
        states.Add(BossState.Attack, new BossAttack());
        states.Add(BossState.Die, new BossDie());

        ResetPatterns();

        stateMachine = new StateMachine<BossControl>(
            this,
            states[BossState.Idle]
        );
    }

    void Update()
    {
        if (!isDead &&
            Keyboard.current != null &&
            Keyboard.current.gKey.wasPressedThisFrame)
        {
            TakeDamage(1000f);
        }

        if (stateMachine != null)
            stateMachine.DoOperateUpdate();
    }

    public void ChangeState(BossState state)
    {
        if (stateMachine == null)
            return;

        if (isDead && state != BossState.Die)
            return;

        stateMachine.SetState(states[state]);
    }

    public bool IsPlayerInRange()
    {
        if (Player == null)
            return false;

        float distance = Vector2.Distance(
            transform.position,
            Player.position
        );

        return distance <= detectionRange;
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    public bool CanMove()
    {
        return canMove;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public void DamagePlayer(float damage)
    {
        if (Player == null)
            return;

        MonoBehaviour[] scripts =
            Player.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            IPlayerDamageable player =
                script as IPlayerDamageable;

            if (player != null)
            {
                player.TakeDamage(damage);
                return;
            }
        }
    }

    public void UseAttackPattern()
    {
        if (isDead)
            return;

        if (attackPatterns == null ||
            attackPatterns.Count == 0)
        {
            Debug.LogWarning(
                gameObject.name +
                "의 Attack Patterns가 비어 있습니다."
            );

            return;
        }

        if (remainingPatterns.Count == 0)
            ResetPatterns();

        if (remainingPatterns.Count == 0)
            return;

        int randomIndex = UnityEngine.Random.Range(
            0,
            remainingPatterns.Count
        );

        BossPattern pattern =
            remainingPatterns[randomIndex];

        remainingPatterns.RemoveAt(randomIndex);

        if (pattern == null)
            return;

        Debug.Log(
            "패턴 사용 : " +
            pattern.patternID +
            " / 공격력 : " +
            pattern.attackPower
        );

        OnPatternSelected?.Invoke(
            pattern.patternID,
            pattern.attackPower
        );
    }

    public void AddAttackPattern(
        int patternID,
        float attackPower)
    {
        foreach (BossPattern pattern in attackPatterns)
        {
            if (pattern == null)
                continue;

            if (pattern.patternID == patternID)
                return;
        }

        BossPattern newPattern = new BossPattern();

        newPattern.patternID = patternID;
        newPattern.attackPower = attackPower;

        attackPatterns.Add(newPattern);

        ResetPatterns();
    }

    public bool IsPlayerInAttackRange(
        Vector3 attackPosition,
        float attackRange)
    {
        if (Player == null)
            return false;

        float distance = Vector2.Distance(
            attackPosition,
            Player.position
        );

        return distance <= attackRange;
    }

    private void ResetPatterns()
    {
        remainingPatterns.Clear();

        if (attackPatterns == null)
            return;

        foreach (BossPattern pattern in attackPatterns)
        {
            if (pattern != null)
                remainingPatterns.Add(pattern);
        }
    }

    public float GetRandomAttackDuration()
    {
        return UnityEngine.Random.Range(
            attackDurationMin,
            attackDurationMax
        );
    }

    public float GetRandomPatternCooldown()
    {
        return UnityEngine.Random.Range(
            patternCooldownMin,
            patternCooldownMax
        );
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        UpdateHealthBar();

        Debug.Log(
            gameObject.name +
            " HP : " +
            currentHealth +
            " / " +
            maxHealth
        );

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
            canMove = false;

            OnDeath?.Invoke();

            StopBossSkills();

            ChangeState(BossState.Die);
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        if (maxHealth <= 0)
        {
            healthBar.fillAmount = 0;
            return;
        }

        healthBar.fillAmount =
            currentHealth / maxHealth;
    }

    void StopBossSkills()
    {
        MonoBehaviour[] scripts =
            GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour script in scripts)
        {
            if (script == this ||
                script is TraumaBossDie ||
                script is BossVisual)
                continue;

            script.StopAllCoroutines();
            script.enabled = false;
        }
    }

    public void PlayAttackMotion(int patternID)
    {
        if (isDead)
            return;

        OnPatternAttack?.Invoke(patternID);
    }
}