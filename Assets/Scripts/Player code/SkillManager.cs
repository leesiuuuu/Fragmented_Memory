using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 기본 입력 키 (각 입력은 담당 컴포넌트의 Inspector에서 변경 가능)
// Z: 검기
// X: 패링
// C: 강한 참격
// R: 찌르기 (패링 성공 후 다음 찌르기 강화)
// F: 내려찍기
// V: 궁극기
// Tab: 플레이어 상태 패널 열기/닫기
public class SkillManager : MonoBehaviour
{
    private const float StrikeRiseSpeed = 35f;
    private const float StrikeFallSpeed = 30f;
    private const float WaveDamageMultiplier = 0.75f;
    private const float WaveCoolTime = 3f;
    private const float StrongMeleeDamageMultiplier = 1.5f;
    private const float StrongWaveDamageMultiplier = 1f;
    private const float StrongSlashCoolTime = 6f;
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

    [Header("입력")]
    [SerializeField] private KeyCode waveKey = KeyCode.Z;
    [SerializeField] private KeyCode strongSlashKey = KeyCode.C;
    [SerializeField] private KeyCode pokeKey = KeyCode.R;
    [SerializeField] private KeyCode strikeKey = KeyCode.F;
    [SerializeField] private KeyCode ultimateKey = KeyCode.V;

    [Header("검기")]
    [SerializeField] private PlayerSkillProjectile wavePrefab;
    [SerializeField] private Vector2 waveSpawnOffset = new Vector2(1f, 0f);
    [SerializeField] private Vector2 waveSize = new Vector2(1.2f, 0.4f);
    [SerializeField] private float waveSpeed = 10f;
    [SerializeField] private float waveMaxDistance = 8f;

    [Header("강한 참격")]
    [SerializeField] private Vector2 strongMeleeSize = new Vector2(2.5f, 1.8f);
    [SerializeField] private Vector2 strongMeleeOffset = new Vector2(1.2f, 0f);
    [SerializeField] private PlayerSkillProjectile strongWavePrefab;
    [SerializeField] private Vector2 strongWaveSpawnOffset = new Vector2(1f, 0f);
    [SerializeField] private Vector2 strongWaveSize = new Vector2(1.8f, 0.8f);
    [SerializeField] private float strongWaveDelay = 0.15f;
    [SerializeField] private float strongWaveSpeed = 8f;
    [SerializeField] private float strongWaveMaxDistance = 6f;

    [Header("찌르기")]
    [SerializeField] private Vector2 pokeSize = new Vector2(1.5f, 1f);
    [SerializeField] private Vector2 pokeOffset = new Vector2(1f, 0f);
    [SerializeField] private float pokeLifeStealDuration = 5f;

    [Header("내려찍기")]
    [SerializeField] private Vector2 strikeDirectSize = new Vector2(1.5f, 1f);
    [SerializeField] private Vector2 strikeAreaSize = new Vector2(4f, 2f);
    [SerializeField] private Vector2 strikeOffset = new Vector2(1f, 0f);

    [Header("궁극기")]
    [SerializeField] private Vector2 ultimateSize = new Vector2(4f, 1.5f);
    [SerializeField] private Vector2 ultimateOffset = new Vector2(2f, 0f);
    [Header("임시 연출")]
    [SerializeField] private SkillAreaVisual areaVisualPrefab;
    [SerializeField] private float codeSkillActionDuration = 0.25f;

    private Animator animator;
    private PlayerStats playerStats;
    private PlayerCombat combat;
    private SpriteRenderer spriteRenderer;
    private PlayerHP playerHP;
    private Rigidbody2D rigid;
    private PlayerMovement movement;
    private ParryManager parryManager;
    private bool canWave = true;
    private bool canStrongSlash = true;
    private bool canPoke = true;
    private bool canStrike = true;
    private bool canUltimate = true;
    private float pendingPokeDamageMultiplier = PokeDamageMultiplier;
    private readonly HashSet<EnemyHP> hitEnemies = new HashSet<EnemyHP>();

    public bool IsLifeStealBoostActive { get; private set; }
    public bool IsUltimateBuffActive { get; private set; }

    private Coroutine pokeLifeStealRoutine;
    private Coroutine ultimateBuffRoutine;

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
    }

    private void Update()
    {
        if (playerHP.IsDead || GameplayInputLock.IsLocked)
            return;

        SkillInput();
    }

    private void SkillInput()
    {
        if (Input.GetKeyDown(waveKey) && !combat.IsBusy && canWave)
            Wave();

        if (Input.GetKeyDown(strongSlashKey) && !combat.IsBusy && canStrongSlash)
            StrongSlash();

        if (Input.GetKeyDown(pokeKey) && !combat.IsBusy && canPoke)
            Poke(parryManager != null && parryManager.ConsumePoke());

        if (Input.GetKeyDown(strikeKey) && !combat.IsBusy && canStrike)
            Strike();

        if (Input.GetKeyDown(ultimateKey) && !combat.IsBusy && canUltimate)
            Ultimate();
    }

    private void Wave()
    {
        if (wavePrefab == null)
        {
            Debug.LogError("[SkillManager] 검기 투사체 프리팹이 연결되지 않았습니다.", this);
            return;
        }

        combat.StartAction();
        canWave = false;
        SpawnProjectile(wavePrefab, waveSpawnOffset, waveSize, WaveDamageMultiplier,
            waveSpeed, waveMaxDistance);
        Invoke(nameof(EndSkill), codeSkillActionDuration);
        Invoke(nameof(ResetWaveCoolTime), WaveCoolTime);
    }

    private void StrongSlash()
    {
        combat.StartAction();
        canStrongSlash = false;
        ShowTemporaryVisual(GetAttackCenter(strongMeleeOffset), strongMeleeSize,
            new Color(1f, 1f, 1f, 0.5f));
        AttackDamage(StrongMeleeDamageMultiplier, strongMeleeOffset, strongMeleeSize);
        Invoke(nameof(SpawnStrongWave), strongWaveDelay);
        Invoke(nameof(EndSkill), codeSkillActionDuration);
        Invoke(nameof(ResetStrongSlashCoolTime), StrongSlashCoolTime);
    }

    private void SpawnStrongWave()
    {
        if (strongWavePrefab == null)
        {
            Debug.LogError("[SkillManager] 강한 참격 투사체 프리팹이 연결되지 않았습니다.", this);
            return;
        }

        SpawnProjectile(strongWavePrefab, strongWaveSpawnOffset, strongWaveSize,
            StrongWaveDamageMultiplier, strongWaveSpeed, strongWaveMaxDistance);
    }

    private void Poke(bool parrySucceeded)
    {
        combat.StartAction();
        canPoke = false;
        pendingPokeDamageMultiplier = parrySucceeded
            ? ParryPokeDamageMultiplier
            : PokeDamageMultiplier;
        animator.SetTrigger("Poke");
        Invoke(nameof(ResetPokeCoolTime), PokeCoolTime);
    }

    private void Strike()
    {
        combat.StartAction();
        canStrike = false;
        animator.SetTrigger("Strike");
        Invoke(nameof(ResetStrikeCoolTime), StrikeCoolTime);
    }

    private void Ultimate()
    {
        combat.StartAction();
        canUltimate = false;
        animator.SetTrigger("Ultimate");
        Invoke(nameof(ResetUltimateCoolTime), UltimateCoolTime);
    }

    public void UltimateDamage()
    {
        if (playerHP.IsDead)
            return;

        AttackDamage(UltimateDamageMultiplier, ultimateOffset, ultimateSize, true);

        if (ultimateBuffRoutine != null)
            StopCoroutine(ultimateBuffRoutine);

        IsUltimateBuffActive = true;
        ultimateBuffRoutine = StartCoroutine(UltimateBuff());
    }

    public void PokeDamage()
    {
        if (!playerHP.IsDead && EffectManager.Instance != null)
            PlayEffect(EffectId.Poke);

        AttackDamage(pendingPokeDamageMultiplier, pokeOffset, pokeSize);

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
        ShowTemporaryVisual(center, strikeAreaSize, new Color(1f, 1f, 1f, 0.45f));
        Collider2D[] directHits = Physics2D.OverlapBoxAll(center, strikeDirectSize, 0f);
        Collider2D[] areaHits = Physics2D.OverlapBoxAll(center, strikeAreaSize, 0f);
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

    private void AttackDamage(float multiplier, Vector2 offset, Vector2 size,
        bool ignoreDefense = false)
    {
        Collider2D[] enemies = Physics2D.OverlapBoxAll(GetAttackCenter(offset), size, 0f);
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

    private void ShowTemporaryVisual(Vector2 position, Vector2 size, Color color)
    {
        if (areaVisualPrefab == null)
            return;

        SkillAreaVisual visual = Instantiate(areaVisualPrefab, position, Quaternion.identity);
        visual.Show(size, color, spriteRenderer.sortingOrder + 1, codeSkillActionDuration);
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

    private void ResetWaveCoolTime() => canWave = true;
    private void ResetStrongSlashCoolTime() => canStrongSlash = true;
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
        canWave = true;
        canStrongSlash = true;
        canPoke = true;
        canStrike = true;
        canUltimate = true;
        pendingPokeDamageMultiplier = PokeDamageMultiplier;
    }

    private void OnDrawGizmosSelected()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        float direction = renderer != null && renderer.flipX ? -1f : 1f;
        Vector2 position = transform.position;
        Gizmos.DrawWireCube(position + new Vector2(strongMeleeOffset.x * direction,
            strongMeleeOffset.y), strongMeleeSize);
        Gizmos.DrawWireCube(position + new Vector2(strikeOffset.x * direction,
            strikeOffset.y), strikeAreaSize);
        Gizmos.DrawWireCube(position + new Vector2(ultimateOffset.x * direction,
            ultimateOffset.y), ultimateSize);
    }
}
