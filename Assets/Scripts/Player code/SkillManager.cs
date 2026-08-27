using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// 기본 입력 키는 담당 컴포넌트의 Inspector에서 변경할 수 있다.
// 이동은 A와 D를 사용한다.
// 점프는 Space를 사용한다.
// 대쉬는 왼쪽 Shift를 사용한다.
// 일반 공격은 마우스 왼쪽 버튼을 사용한다.
// 패링은 마우스 오른쪽 버튼을 사용한다.
// 검기는 Q를 사용한다.
// 강한 참격은 R을 사용한다.
// 찌르기는 F를 사용한다. 패링 성공 후 다음 찌르기가 강화된다.
// 내려찍기는 T를 사용한다.
// 궁극기는 G를 사용한다.
// 상호작용은 E를 사용한다.
// 플레이어 상태 패널은 Tab으로 열고 닫는다.
// 소모품 인벤토리는 I로 열고 닫는다.
// 선택한 소모품은 E로 사용한다.
// 일시정지는 Esc를 사용한다.
public class SkillManager : MonoBehaviour
{
    private const float StrikeRiseSpeed = 35f;
    private const float StrikeFallSpeed = 30f;
    private const float SlashDamageMultiplier = 0.75f;
    private const float SlashCoolTime = 3f;
    private const float StrongMeleeDamageMultiplier = 1.5f;
    private const float StrongStrikeProjectileDamageMultiplier = 1f;
    private const float StrongStrikeCoolTime = 6f;
    private const float PokeDamageMultiplier = 1f;
    private const float ParryPokeDamageMultiplier = 1.25f;
    private const float PokeLifeStealIncrease = 20f;
    private const float PokeCoolTime = 5f;
    private const float StrikeDirectDamageMultiplier = 2f;
    private const float StrikeAreaDamageMultiplier = 0.5f;
    private const float StrikeStunDuration = 1.5f;
    private const float StrikeCoolTime = 8f;
    private const float UltimateDamageMultiplier = 1.5f;
    private const int UltimateAttackBonus = 100;
    private const float UltimateBuffDuration = 10f;
    private const float UltimateCoolTime = 12f;
    private const float CodeSkillActionDuration = 0.25f;

    [Header("입력")]
    [FormerlySerializedAs("waveKey")]
    [SerializeField] private KeyCode slashKey = KeyCode.Q;
    [FormerlySerializedAs("strongSlashKey")]
    [SerializeField] private KeyCode strongStrikeKey = KeyCode.R;
    [SerializeField] private KeyCode pokeKey = KeyCode.F;
    [SerializeField] private KeyCode strikeKey = KeyCode.C;
    [SerializeField] private KeyCode ultimateKey = KeyCode.V;

    [Header("검기")]
    [FormerlySerializedAs("wavePrefab")]
    [SerializeField] private PlayerSkillProjectile slashPrefab;
    [FormerlySerializedAs("waveSpawnOffset")]
    [SerializeField] private Vector2 slashSpawnOffset = new Vector2(1f, 0f);
    [FormerlySerializedAs("waveSize")]
    [SerializeField] private Vector2 slashSize = new Vector2(1.2f, 0.4f);
    [FormerlySerializedAs("waveSpeed")]
    [SerializeField] private float slashSpeed = 10f;
    [FormerlySerializedAs("waveMaxDistance")]
    [SerializeField] private float slashMaxDistance = 8f;

    [Header("강한 참격")]
    [SerializeField] private GameObject strongMeleeAttackBoxPrefab;
    [SerializeField] private Vector2 strongMeleeOffset = new Vector2(1.2f, 0f);
    [FormerlySerializedAs("strongWavePrefab")]
    [SerializeField] private PlayerSkillProjectile strongStrikeProjectilePrefab;
    [FormerlySerializedAs("strongWaveSpawnOffset")]
    [SerializeField] private Vector2 strongStrikeProjectileSpawnOffset = new Vector2(1f, 0f);
    [FormerlySerializedAs("strongWaveSize")]
    [SerializeField] private Vector2 strongStrikeProjectileSize = new Vector2(1.8f, 0.8f);
    [FormerlySerializedAs("strongWaveSpeed")]
    [SerializeField] private float strongStrikeProjectileSpeed = 8f;
    [FormerlySerializedAs("strongWaveMaxDistance")]
    [SerializeField] private float strongStrikeProjectileMaxDistance = 6f;

    [Header("찌르기")]
    [SerializeField] private GameObject pokeAttackBoxPrefab;
    [SerializeField] private Vector2 pokeOffset = new Vector2(1f, 0f);
    [SerializeField] private float pokeLifeStealDuration = 5f;

    [Header("내려찍기")]
    [SerializeField] private GameObject strikeDirectAttackBoxPrefab;
    [SerializeField] private GameObject strikeAreaAttackBoxPrefab;
    [SerializeField] private Vector2 strikeOffset = new Vector2(1f, 0f);

    [Header("궁극기")]
    [SerializeField] private GameObject ultimateAttackBoxPrefab;
    [SerializeField] private Vector2 ultimateOffset = new Vector2(2f, 0f);

    private Animator animator;
    private PlayerStats playerStats;
    private PlayerCombat combat;
    private SpriteRenderer spriteRenderer;
    private PlayerHP playerHP;
    private Rigidbody2D rigid;
    private PlayerMovement movement;
    private ParryManager parryManager;
    private bool canSlash = true;
    private bool canStrongStrike = true;
    private bool canPoke = true;
    private bool canStrike = true;
    private bool canUltimate = true;
    private float pendingPokeDamageMultiplier = PokeDamageMultiplier;
    private readonly HashSet<EnemyHP> hitEnemies = new HashSet<EnemyHP>();

    public bool IsLifeStealBoostActive { get; private set; }
    public bool IsUltimateBuffActive { get; private set; }

    private Coroutine pokeLifeStealRoutine;
    private Coroutine ultimateBuffRoutine;
    private PlayerSynergyManager synergyManager;
    private Coroutine skillCooldownRoutine;
    private float skillCooldownReduction;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>();
        combat = GetComponent<PlayerCombat>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHP = GetComponent<PlayerHP>();
        rigid = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        parryManager = GetComponent<ParryManager>();
        synergyManager = GetComponent<PlayerSynergyManager>();
    }

    private void Update()
    {
        if (playerHP.IsDead || GameplayInputLock.IsLocked)
            return;

        SkillInput();
    }

    private void SkillInput()
    {
        if (Input.GetKeyDown(slashKey) && !combat.IsBusy && canSlash)
            Slash();

        if (Input.GetKeyDown(strongStrikeKey) && !combat.IsBusy && canStrongStrike)
            StrongStrike();

        if (Input.GetKeyDown(pokeKey) && !combat.IsBusy && canPoke)
            Poke(parryManager != null && parryManager.ConsumePoke());

        if (Input.GetKeyDown(strikeKey) && !combat.IsBusy && canStrike)
            Strike();

        if (Input.GetKeyDown(ultimateKey) && !combat.IsBusy && canUltimate)
            Ultimate();
    }

    private void Slash()
    {
        if (slashPrefab == null)
        {
            Debug.LogError("[SkillManager] 검기 투사체 프리팹이 연결되지 않았습니다.", this);
            return;
        }

        combat.StartAction();
        canSlash = false;
        animator.SetTrigger("Slash");
        Invoke(nameof(ResetSlashCoolTime), GetSkillCooldown(SlashCoolTime));
    }

    private void StrongStrike()
    {
        combat.StartAction();
        canStrongStrike = false;
        animator.SetTrigger("StrongStrike");
        Invoke(nameof(ResetStrongStrikeCoolTime), GetSkillCooldown(StrongStrikeCoolTime));
    }

    public void SlashProjectile()
    {
        if (playerHP.IsDead)
            return;

        SpawnProjectile(slashPrefab, slashSpawnOffset, slashSize, SlashDamageMultiplier,
            slashSpeed, slashMaxDistance);
    }

    public void StrongStrikeMeleeDamage()
    {
        if (playerHP.IsDead)
            return;

        AttackDamage(StrongMeleeDamageMultiplier, strongMeleeOffset,
            strongMeleeAttackBoxPrefab);
    }

    public void StrongStrikeProjectile()
    {
        if (playerHP.IsDead)
            return;

        if (strongStrikeProjectilePrefab == null)
        {
            Debug.LogError("[SkillManager] 강한 참격 투사체 프리팹이 연결되지 않았습니다.", this);
            return;
        }

        SpawnProjectile(strongStrikeProjectilePrefab, strongStrikeProjectileSpawnOffset,
            strongStrikeProjectileSize, StrongStrikeProjectileDamageMultiplier,
            strongStrikeProjectileSpeed, strongStrikeProjectileMaxDistance);
    }

    private void Poke(bool parrySucceeded)
    {
        combat.StartAction();
        canPoke = false;
        pendingPokeDamageMultiplier = parrySucceeded
            ? ParryPokeDamageMultiplier
            : PokeDamageMultiplier;
        animator.SetTrigger("Poke");
        Invoke(nameof(ResetPokeCoolTime), GetSkillCooldown(PokeCoolTime));
    }

    private void Strike()
    {
        combat.StartAction();
        canStrike = false;
        animator.SetTrigger("Strike");
        Invoke(nameof(ResetStrikeCoolTime), GetSkillCooldown(StrikeCoolTime));
    }

    private void Ultimate()
    {
        combat.StartAction();
        canUltimate = false;
        animator.SetTrigger("Ultimate");
        Invoke(nameof(ResetUltimateCoolTime), GetSkillCooldown(UltimateCoolTime));
    }

    public void UltimateDamage()
    {
        if (playerHP.IsDead)
            return;

        AttackDamage(UltimateDamageMultiplier, ultimateOffset,
            ultimateAttackBoxPrefab, true);

        if (ultimateBuffRoutine != null)
            StopCoroutine(ultimateBuffRoutine);

        IsUltimateBuffActive = true;
        ultimateBuffRoutine = StartCoroutine(UltimateBuff());
    }

    public void PokeDamage()
    {
        if (!playerHP.IsDead && EffectManager.Instance != null)
            PlayEffect(EffectId.Poke);

        AttackDamage(pendingPokeDamageMultiplier, pokeOffset, pokeAttackBoxPrefab);

        if (pokeLifeStealRoutine != null)
            StopCoroutine(pokeLifeStealRoutine);

        IsLifeStealBoostActive = true;
        pokeLifeStealRoutine = StartCoroutine(PokeLifeStealBuff());
    }

    public void StrikeRise()
    {
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, StrikeRiseSpeed);
    }

    public void StrikeHover()
    {
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
    }

    public void StrikeFall()
    {
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, -StrikeFallSpeed);
    }

    public void StrikeDamage()
    {
        movement.ClearForcedHorizontalSpeed();

        if (!playerHP.IsDead && EffectManager.Instance != null)
            PlayEffect(EffectId.Strike);

        Vector2 center = GetAttackCenter(strikeOffset);
        BoxCollider2D directAttackBox = SpawnAttackBox(strikeDirectAttackBoxPrefab, center);
        BoxCollider2D areaAttackBox = SpawnAttackBox(strikeAreaAttackBoxPrefab, center);
        if (directAttackBox == null || areaAttackBox == null)
            return;

        Collider2D[] directHits = Physics2D.OverlapBoxAll(
            directAttackBox.bounds.center, directAttackBox.bounds.size, 0f);
        Collider2D[] areaHits = Physics2D.OverlapBoxAll(
            areaAttackBox.bounds.center, areaAttackBox.bounds.size, 0f);
        HashSet<EnemyHP> directEnemies = CollectEnemies(directHits);

        foreach (EnemyHP enemy in directEnemies)
            DealDamage(enemy, StrikeDirectDamageMultiplier, false);

        hitEnemies.Clear();
        foreach (Collider2D hit in areaHits)
        {
            EnemyHP enemy = hit.GetComponentInParent<EnemyHP>();
            if (enemy == null || directEnemies.Contains(enemy) || !hitEnemies.Add(enemy))
                continue;

            DealDamage(enemy, StrikeAreaDamageMultiplier, false);
            EnemyStun stun = enemy.GetComponent<EnemyStun>();
            if (stun == null)
                stun = enemy.gameObject.AddComponent<EnemyStun>();
            stun.Apply(StrikeStunDuration);
        }
    }

    private void AttackDamage(float multiplier, Vector2 offset, GameObject attackBoxPrefab,
        bool ignoreDefense = false)
    {
        BoxCollider2D attackBox = SpawnAttackBox(attackBoxPrefab, GetAttackCenter(offset));
        if (attackBox == null)
            return;

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            attackBox.bounds.center, attackBox.bounds.size, 0f);
        hitEnemies.Clear();

        foreach (Collider2D hit in enemies)
        {
            EnemyHP enemy = hit.GetComponentInParent<EnemyHP>();
            if (enemy != null && hitEnemies.Add(enemy))
                DealDamage(enemy, multiplier, ignoreDefense);
        }
    }

    private void DealDamage(EnemyHP enemy, float multiplier, bool ignoreDefense)
    {
        int damage = Mathf.RoundToInt(playerStats.GetAttackDamage() * multiplier);
        int dealtDamage = enemy.TakeDamage(damage, ignoreDefense);

        synergyManager?.OnDamageDealt(enemy, dealtDamage);

        if (dealtDamage > 0 && playerStats.CurrentLifeSteal > 0f)
            playerHP.Heal(Mathf.RoundToInt(dealtDamage * playerStats.CurrentLifeSteal / 100f));
    }

    private void SpawnProjectile(PlayerSkillProjectile prefab, Vector2 offset,
        Vector2 size, float multiplier, float speed, float distance)
    {
        float direction = spriteRenderer.flipX ? -1f : 1f;
        Vector3 position = transform.position
            + new Vector3(offset.x * direction, offset.y, 0f);
        PlayerSkillProjectile projectile;

        if (prefab == null)
            return;

        projectile = Instantiate(prefab, position, Quaternion.identity);
        projectile.transform.localScale = new Vector3(size.x, size.y, 1f);

        projectile.Initialize(playerStats, playerHP, new Vector2(direction, 0f),
            speed, distance, multiplier);
    }

    private BoxCollider2D SpawnAttackBox(GameObject prefab, Vector2 position)
    {
        if (prefab == null)
        {
            Debug.LogError("[SkillManager] 공격 판정 프리팹이 연결되지 않았습니다.", this);
            return null;
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        BoxCollider2D attackBox = instance.GetComponent<BoxCollider2D>();
        if (attackBox == null)
        {
            Debug.LogError("[SkillManager] 공격 판정 프리팹에 BoxCollider2D가 없습니다.", instance);
            Destroy(instance);
            return null;
        }

        SpriteRenderer visual = instance.GetComponent<SpriteRenderer>();
        if (visual != null)
        {
            visual.sortingLayerID = spriteRenderer.sortingLayerID;
            visual.sortingOrder = spriteRenderer.sortingOrder + 1;
        }

        Destroy(instance, CodeSkillActionDuration);
        return attackBox;
    }

    private Vector2 GetAttackCenter(Vector2 offset)
    {
        float direction = spriteRenderer.flipX ? -1f : 1f;
        return (Vector2)transform.position + new Vector2(offset.x * direction, offset.y);
    }

    private static HashSet<EnemyHP> CollectEnemies(Collider2D[] hits)
    {
        HashSet<EnemyHP> enemies = new HashSet<EnemyHP>();
        foreach (Collider2D hit in hits)
        {
            EnemyHP enemy = hit.GetComponentInParent<EnemyHP>();
            if (enemy != null)
                enemies.Add(enemy);
        }
        return enemies;
    }

    private void PlayEffect(EffectId id)
    {
        Vector3 position = transform.position
            + (spriteRenderer.flipX ? Vector3.left : Vector3.right);
        Quaternion rotation = spriteRenderer.flipX
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.identity;
        EffectManager.Instance.Play(id, position, rotation);
    }

    private IEnumerator PokeLifeStealBuff()
    {
        playerStats.SetTemporaryLifeSteal(PokeLifeStealIncrease);
        yield return new WaitForSeconds(pokeLifeStealDuration);
        IsLifeStealBoostActive = false;
        playerStats.SetTemporaryLifeSteal(0f);
        pokeLifeStealRoutine = null;
    }

    private IEnumerator UltimateBuff()
    {
        playerStats.SetTemporaryAttack(UltimateAttackBonus);
        yield return new WaitForSeconds(UltimateBuffDuration);
        IsUltimateBuffActive = false;
        playerStats.SetTemporaryAttack(0);
        ultimateBuffRoutine = null;
    }

    public void EndSkill()
    {
        combat.EndAction();
    }

    public void ApplySkillCooldownReduction(float amount, float duration)
    {
        if (skillCooldownRoutine != null)
            StopCoroutine(skillCooldownRoutine);

        skillCooldownRoutine = StartCoroutine(SkillCooldownEffect(amount, duration));
    }

    private IEnumerator SkillCooldownEffect(float amount, float duration)
    {
        skillCooldownReduction = Mathf.Max(0f, amount);

        yield return new WaitForSeconds(duration);

        skillCooldownReduction = 0f;
        skillCooldownRoutine = null;
    }

    private float GetSkillCooldown(float baseCooldown)
    {
        return Mathf.Max(0f, baseCooldown - skillCooldownReduction);
    }

    private void ResetSlashCoolTime() => canSlash = true;
    private void ResetStrongStrikeCoolTime() => canStrongStrike = true;
    private void ResetPokeCoolTime() => canPoke = true;
    private void ResetStrikeCoolTime() => canStrike = true;
    private void ResetUltimateCoolTime() => canUltimate = true;

    private void OnDisable()
    {
        CancelInvoke();
        movement?.ClearForcedHorizontalSpeed();
        combat?.EndAction();
        playerStats?.SetTemporaryAttack(0);
        playerStats?.SetTemporaryLifeSteal(0f);
        IsLifeStealBoostActive = false;
        IsUltimateBuffActive = false;
        skillCooldownReduction = 0f;
        skillCooldownRoutine = null;
        canSlash = true;
        canStrongStrike = true;
        canPoke = true;
        canStrike = true;
        canUltimate = true;
        pendingPokeDamageMultiplier = PokeDamageMultiplier;
    }

}
