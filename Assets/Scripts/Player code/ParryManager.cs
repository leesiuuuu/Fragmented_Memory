using System;
using UnityEngine;

// 패링 입력 시간을 열어 두고 PlayerHP가 피격될 때 성공 여부를 판정한다.
// 성공 상태는 다음 찌르기가 소비한다. 성공 표시는 그때까지 유지된다.
public class ParryManager : MonoBehaviour
{
    [SerializeField] private KeyCode parryKey = KeyCode.S;
    [SerializeField] private float parryDuration = 0.2f;
    [SerializeField] private Vector2 parrySize = new Vector2(2f, 1.5f);
    [SerializeField] private Vector2 parryOffset = new Vector2(1f, 0f);
    [SerializeField] private LayerMask parryLayers = ~0;

    [Header("판정 창 표시")]
    // 애니메이션은 0.58초인데 실제로 막히는 구간은 parryDuration뿐이다.
    // 그 차이를 눈으로 알 수 없으면 패링은 감으로 누르는 기술이 된다.
    [SerializeField] private bool tintDuringWindow = true;
    [SerializeField] private Color windowColor = new Color(0.55f, 0.9f, 1f);

    [Header("성공 연출")]
    [SerializeField, Min(0f)] private float successHitStop = HitStop.Heavy;
    [SerializeField, Min(0f)] private float successShakeStrength = 0.22f;
    [SerializeField, Min(0f)] private float successShakeDuration = 0.2f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerHP playerHP;
    private PlayerCombat combat;
    // 0으로 두면 게임이 시작된 첫 프레임에 Time.time == 0이라 판정 창이 열린 것으로 읽힌다.
    private float parryEndTime = -1f;

    private Color baseColor = Color.white;
    private bool windowShown;

    // 애니메이션 길이가 아니라 이 값이 실제 판정 구간이다.
    public bool IsWindowOpen => Time.time <= parryEndTime;

    public bool CanPoke { get; private set; }
    public event Action ParrySucceeded;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHP = GetComponent<PlayerHP>();
        combat = GetComponent<PlayerCombat>();

        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    private void Update()
    {
        UpdateWindowTint();

        if (playerHP.IsDead || GameplayInputLock.IsLocked)
            return;

        if (Input.GetKeyDown(parryKey) && !combat.IsBusy)
        {
            combat.StartAction();
            parryEndTime = Time.time + parryDuration;
            animator.SetTrigger("Parry");
        }
    }

    public bool TryParry()
    {
        if (Time.time > parryEndTime)
            return false;

        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Vector2 center = (Vector2)transform.position
            + new Vector2(parryOffset.x * direction.x, parryOffset.y);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, parrySize, 0f, parryLayers);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponentInParent<EnemyAttack>() == null
                && hits[i].GetComponentInParent<EnemyProjectile>() == null)
                continue;

            parryEndTime = 0f;
            CanPoke = true;

            PlaySuccessFeedback(center);

            ParrySucceeded?.Invoke();
            return true;
        }

        return false;
    }

    public bool ConsumePoke()
    {
        if (!CanPoke)
            return false;

        CanPoke = false;
        return true;
    }

    public void EndParry()
    {
        combat.EndAction();
    }


    // 판정 창이 열려 있는 동안만 색을 바꾼다. 성공하면 parryEndTime이 0이 되어 즉시 풀린다 —
    // 막힌 순간과 회복 동작이 색으로 구분된다.
    private void UpdateWindowTint()
    {
        if (!tintDuringWindow || spriteRenderer == null)
            return;

        bool open = IsWindowOpen && !playerHP.IsDead;

        if (open == windowShown)
            return;

        windowShown = open;
        spriteRenderer.color = open ? windowColor : baseColor;
    }


    // 패링은 성공해도 아무 신호가 없었다. 성공 여부를 알 수 없으면 배울 수가 없다.
    private void PlaySuccessFeedback(Vector2 center)
    {
        HitStop.Play(successHitStop);

        if (CameraFollow.Active != null)
            CameraFollow.Active.Shake(successShakeStrength, successShakeDuration);

        // 전용 프리팹을 아직 꽂지 않았다면 조용히 건너뛴다.
        if (EffectManager.Instance != null &&
            EffectManager.Instance.IsRegistered(EffectId.Parry))
        {
            EffectManager.Instance.Play(
                EffectId.Parry, center, Quaternion.identity);
        }
    }


    private void OnDisable()
    {
        parryEndTime = -1f;

        if (windowShown && spriteRenderer != null)
            spriteRenderer.color = baseColor;

        windowShown = false;
    }

    private void OnDrawGizmosSelected()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        float direction = renderer != null && renderer.flipX ? -1f : 1f;
        Vector2 center = (Vector2)transform.position
            + new Vector2(parryOffset.x * direction, parryOffset.y);
        Gizmos.DrawWireCube(center, parrySize);
    }
}
