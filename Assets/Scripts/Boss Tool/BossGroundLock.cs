using UnityEngine;

// 보스를 바닥 위에 붙여 둔다.
// BossControl은 Rigidbody2D를 쓰지 않고 트랜스폼을 직접 옮기기 때문에
// 한 번 공중으로 올라가면 스스로 내려오지 않는다.
// 점프 계열 패턴은 Suspend()로 잠시 풀었다가 Resume()으로 다시 붙인다.
[RequireComponent(typeof(BossControl))]
public class BossGroundLock : MonoBehaviour
{
    [SerializeField] private float rayHeight = 5f;
    [SerializeField] private float rayDistance = 50f;
    [SerializeField] private float groundOffset;

    private BossControl bossControl;
    private Collider2D body;
    private bool suspended;

    private void Awake()
    {
        bossControl = GetComponent<BossControl>();
        body = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (bossControl != null)
            bossControl.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (bossControl != null)
            bossControl.OnDeath -= HandleDeath;
    }

    private void Start()
    {
        SnapToGround();
    }

    // 패턴이 남긴 높이를 매 프레임 정리한다. 패턴 이동 자체는 LateUpdate 전에 끝난다.
    private void LateUpdate()
    {
        if (!suspended)
            SnapToGround();
    }

    public void Suspend()
    {
        suspended = true;
    }

    public void Resume()
    {
        suspended = false;
        SnapToGround();
    }

    // 사망 연출은 TraumaBossDie가 위치를 따로 잡으므로 고정을 놓아준다.
    private void HandleDeath()
    {
        enabled = false;
    }

    public void SnapToGround()
    {
        if (!TryGetGroundY(out float y))
            return;

        Vector3 position = transform.position;
        position.y = y;
        transform.position = position;
    }

    private bool TryGetGroundY(out float y)
    {
        y = 0f;

        Vector2 origin = transform.position;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin + Vector2.up * rayHeight,
            Vector2.down,
            rayDistance);

        float groundY = float.MinValue;
        bool found = false;

        // 보스 자신은 Ground 태그가 아니므로 자연히 걸러진다.
        foreach (RaycastHit2D hit in hits)
        {
            if (!hit.collider.CompareTag("Ground")
                || hit.point.y > origin.y
                || hit.point.y <= groundY)
                continue;

            groundY = hit.point.y;
            found = true;
        }

        if (!found)
            return false;

        // 콜라이더 중심이 아니라 발밑이 바닥에 닿아야 한다.
        float footToPivot = body != null
            ? transform.position.y - body.bounds.min.y
            : 0f;

        y = groundY + footToPivot + groundOffset;
        return true;
    }
}
