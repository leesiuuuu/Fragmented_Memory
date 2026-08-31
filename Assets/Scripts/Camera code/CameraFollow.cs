using UnityEngine;

// 카메라를 플레이어의 자식으로 두면 대상 좌표가 그대로 화면에 박힌다 —
// 점프·대시의 미세한 떨림까지 화면 전체가 따라 흔들리고, 플레이어는 정중앙에 못 박혀
// 움직이는 느낌이 나지 않는다. 부모에서 떼어내 따로 좇게 하는 것이 이 스크립트의 목적이다.
//
// CameraAspectFitter와 같은 오브젝트에 붙어도 안전하다 —
// 저쪽은 orthographicSize와 rect만, 이쪽은 position만 건드린다.
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("대상")]
    // 비워 두면 부모를, 부모도 없으면 씬의 PlayerMovement를 찾는다.
    [SerializeField] private Transform target;

    // 부모에 매달린 채로는 아무리 보간해도 부모 좌표가 먼저 반영된다.
    // 시작할 때 스스로 떨어져 나오므로 씬마다 카메라를 손으로 옮길 필요가 없다.
    [SerializeField] private bool detachFromParentOnStart = true;

    // 0이면 떨어져 나오기 직전의 로컬 좌표를 그대로 쓴다. z는 반드시 음수여야 한다.
    [SerializeField] private Vector3 offset;


    [Header("데드존")]
    // 이 사각형(중심 기준 반너비/반높이) 안에서 대상이 움직이는 동안은 카메라가 가만히 있는다.
    // 0으로 두면 대상에 딱 붙어 배경만 미끄러지는, 지금과 같은 느낌이 된다.
    // 세로를 넉넉히 잡는 이유 — 점프할 때마다 화면이 출렁이는 것을 막는다.
    [SerializeField] private Vector2 deadZone = new Vector2(1.6f, 2.2f);


    [Header("보간")]
    // SmoothDamp의 도달 시간. 작을수록 딱딱하고 클수록 늘어진다.
    // 세로를 느리게 두는 것도 점프 출렁임을 죽이기 위한 것이다.
    [SerializeField, Min(0f)] private float smoothTimeX = 0.16f;
    [SerializeField, Min(0f)] private float smoothTimeY = 0.28f;


    [Header("선행 시야")]
    // 진행 방향으로 화면을 미리 밀어 준다. 액션 게임에서 앞이 보이는 것과 안 보이는 것은
    // 반응 속도 체감이 완전히 다르다. 0이면 끈다.
    [SerializeField, Min(0f)] private float lookAheadDistance = 2.2f;

    // 이 속도(초당 유닛) 미만이면 멈춘 것으로 보고 선행 시야를 되돌린다.
    [SerializeField, Min(0f)] private float lookAheadSpeedThreshold = 1.5f;

    [SerializeField, Min(0f)] private float lookAheadSmoothTime = 0.35f;


    [Header("순간이동")]
    // 방을 갈아끼울 때 RoomManager가 플레이어를 스폰 지점으로 순간이동시킨다.
    // 그대로 보간하면 카메라가 맵을 가로질러 날아가므로, 한 프레임에 이만큼 넘게
    // 움직이면 이동이 아니라 순간이동으로 보고 즉시 붙는다.
    [SerializeField, Min(0f)] private float teleportDistance = 8f;


    private Camera cam;

    // 카메라가 좇는 실제 지점. 대상 좌표가 아니라 데드존을 통과한 뒤의 값이다.
    private Vector2 focus;

    private Vector3 lastTargetPosition;
    private Vector2 velocity;
    private float lookAhead;
    private float lookAheadVelocity;

    private Vector3 shakeOffset;
    private float shakeStrength;
    private float shakeDecay;

    private bool initialized;


    public Transform Target => target;


    private void Awake()
    {
        cam = GetComponent<Camera>();

        Transform parent = transform.parent;

        // 오프셋을 부모에서 떨어지기 전에 챙겨 둔다 — 떨어진 뒤엔 로컬 좌표가 곧 월드 좌표다.
        if (offset == Vector3.zero)
        {
            offset = parent != null
                ? transform.localPosition
                : new Vector3(0f, 0f, transform.position.z);
        }

        if (target == null)
            target = parent;

        if (detachFromParentOnStart && parent != null)
            transform.SetParent(null, true);
    }


    private void Start()
    {
        if (target == null)
        {
            PlayerMovement player = FindFirstObjectByType<PlayerMovement>(FindObjectsInactive.Include);

            if (player != null)
                target = player.transform;
        }

        if (target == null)
        {
            Debug.LogWarning("[CameraFollow] 좇을 대상이 없습니다. 카메라가 제자리에 멈춥니다.");
            return;
        }

        SnapToTarget();
    }


    private void LateUpdate()
    {
        UpdateShake();

        // 대상이 없어도 흔들림만은 계속 반영해야 한다. 초점을 건드리면 화면이 튄다.
        if (target == null)
        {
            ApplyPosition(transform.position);
            return;
        }

        float deltaTime = Time.deltaTime;
        Vector3 targetPosition = target.position;

        // 순간이동은 보간하지 않는다. 방 전환에서 카메라가 맵을 가로지르는 것을 막는다.
        if ((targetPosition - lastTargetPosition).sqrMagnitude > teleportDistance * teleportDistance)
        {
            lastTargetPosition = targetPosition;
            SnapToTarget();
            return;
        }

        UpdateLookAhead(targetPosition, deltaTime);
        UpdateFocus(targetPosition);

        lastTargetPosition = targetPosition;

        Vector2 desired = focus + new Vector2(offset.x + lookAhead, offset.y);

        // x·y를 따로 감쇠해야 가로는 민첩하고 세로는 차분한, 플랫포머다운 움직임이 나온다.
        Vector2 next = new Vector2(
            Mathf.SmoothDamp(transform.position.x, desired.x, ref velocity.x, smoothTimeX, Mathf.Infinity, deltaTime),
            Mathf.SmoothDamp(transform.position.y, desired.y, ref velocity.y, smoothTimeY, Mathf.Infinity, deltaTime));

        ApplyPosition(next);
    }


    // 방 전환·부활처럼 보간 없이 즉시 맞춰야 할 때.
    public void SnapToTarget()
    {
        if (target == null)
            return;

        focus = target.position;
        lastTargetPosition = target.position;
        velocity = Vector2.zero;
        lookAhead = 0f;
        lookAheadVelocity = 0f;
        initialized = true;

        ApplyPosition(focus + new Vector2(offset.x, offset.y));
    }


    // 타격 연출용. strength는 최대 흔들림 폭(유닛), duration은 0으로 잦아드는 데 걸리는 시간.
    public void Shake(float strength, float duration)
    {
        if (strength <= 0f || duration <= 0f)
            return;

        // 약한 흔들림이 강한 흔들림을 덮어쓰지 않게 한다.
        if (strength < shakeStrength)
            return;

        shakeStrength = strength;
        shakeDecay = strength / duration;
    }


    private void UpdateFocus(Vector3 targetPosition)
    {
        if (!initialized)
        {
            focus = targetPosition;
            initialized = true;
            return;
        }

        // 대상이 데드존을 벗어난 만큼만 초점을 민다 — 안쪽에서 움직이는 동안은 화면이 멈춰 있다.
        float dx = targetPosition.x - focus.x;
        float dy = targetPosition.y - focus.y;

        if (dx > deadZone.x)
            focus.x += dx - deadZone.x;
        else if (dx < -deadZone.x)
            focus.x += dx + deadZone.x;

        if (dy > deadZone.y)
            focus.y += dy - deadZone.y;
        else if (dy < -deadZone.y)
            focus.y += dy + deadZone.y;
    }


    private void UpdateLookAhead(Vector3 targetPosition, float deltaTime)
    {
        if (lookAheadDistance <= 0f || deltaTime <= 0f)
            return;

        // Rigidbody2D를 읽지 않고 좌표 변화로 속도를 낸다 —
        // 대상이 플레이어가 아니어도(보스 연출 등) 그대로 동작하게 하기 위해서다.
        float speedX = (targetPosition.x - lastTargetPosition.x) / deltaTime;

        float desired = Mathf.Abs(speedX) >= lookAheadSpeedThreshold
            ? Mathf.Sign(speedX) * lookAheadDistance
            : 0f;

        lookAhead = Mathf.SmoothDamp(
            lookAhead, desired, ref lookAheadVelocity, lookAheadSmoothTime, Mathf.Infinity, deltaTime);
    }


    private void UpdateShake()
    {
        if (shakeStrength <= 0f)
        {
            shakeOffset = Vector3.zero;
            return;
        }

        // 히트스톱(timeScale 0) 중에도 흔들림은 보여야 한다 — 그게 타격의 무게를 만든다.
        shakeStrength = Mathf.Max(0f, shakeStrength - shakeDecay * Time.unscaledDeltaTime);

        shakeOffset = shakeStrength > 0f
            ? (Vector3)(Random.insideUnitCircle * shakeStrength)
            : Vector3.zero;
    }


    private void ApplyPosition(Vector2 position)
    {
        Vector2 clamped = ClampToBounds(position);

        // 흔들림은 경계 보정 뒤에 더한다. 먼저 더하면 벽에 붙었을 때 흔들림이 잘려 나간다.
        transform.position = new Vector3(
            clamped.x + shakeOffset.x,
            clamped.y + shakeOffset.y,
            offset.z);
    }


    private Vector2 ClampToBounds(Vector2 position)
    {
        CameraBounds bounds = CameraBounds.Active;

        if (bounds == null || cam == null || !cam.orthographic)
            return position;

        Bounds area = bounds.Area;

        // 화면 반높이는 orthographicSize 그대로, 반너비는 종횡비를 곱한 값이다.
        // CameraAspectFitter가 창 크기에 따라 size를 바꾸므로 매 프레임 다시 읽는다.
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // 방이 화면보다 좁으면 가둘 수가 없다 — 그럴 땐 방 중앙에 맞춘다.
        position.x = area.size.x <= halfWidth * 2f
            ? area.center.x
            : Mathf.Clamp(position.x, area.min.x + halfWidth, area.max.x - halfWidth);

        position.y = area.size.y <= halfHeight * 2f
            ? area.center.y
            : Mathf.Clamp(position.y, area.min.y + halfHeight, area.max.y - halfHeight);

        return position;
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (deadZone == Vector2.zero)
            return;

        Vector3 center = Application.isPlaying
            ? new Vector3(focus.x, focus.y, 0f)
            : (target != null ? target.position : transform.position);

        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
        Gizmos.DrawWireCube(center, new Vector3(deadZone.x * 2f, deadZone.y * 2f, 0f));
    }
#endif
}
