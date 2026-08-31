using UnityEngine;

// 보스 아레나의 좌우 끝을 이어 붙인다.
// 플레이어가 한쪽 끝을 넘어가면 반대쪽 끝으로 보내 사실상 무한한 바닥이 된다.
// 카메라가 고정이라 화면을 벗어나는 지점이 곧 경계다.
// 이 컴포넌트가 붙은 보스에게만 적용되므로 3차 보스는 기존처럼 막힌 아레나를 쓴다.
public class BossArenaWrap : MonoBehaviour
{
    // 중심에서 좌우 끝까지의 거리. 보스방 바닥 폭의 절반으로 맞춘다.
    [SerializeField, Min(1f)] private float halfWidth = 22f;

    // 넘어간 즉시 다시 튕겨 나가지 않도록 반대편에서 살짝 안쪽에 놓는다.
    [SerializeField, Min(0f)] private float margin = 1f;

    // 비워 두면 보스가 스폰된 위치를 아레나 중심으로 본다.
    [SerializeField] private Transform center;

    private Transform player;
    private Rigidbody2D playerBody;

    private void Start()
    {
        // RoomManager가 BossControl.Player를 꽂아 준 뒤에 잡아야 한다.
        BossControl bossControl = GetComponentInChildren<BossControl>(true);

        if (bossControl == null || bossControl.Player == null)
        {
            Debug.LogWarning("[BossArenaWrap] 플레이어를 찾지 못해 좌우 순환을 끕니다.", this);
            enabled = false;
            return;
        }

        player = bossControl.Player;
        playerBody = player.GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        float origin = center != null ? center.position.x : transform.position.x;
        float offset = player.position.x - origin;

        if (offset > halfWidth)
            MovePlayerTo(origin - halfWidth + margin);
        else if (offset < -halfWidth)
            MovePlayerTo(origin + halfWidth - margin);
    }

    private void MovePlayerTo(float x)
    {
        Vector3 position = player.position;
        position.x = x;

        player.position = position;

        // 보간이 켜진 Rigidbody2D는 트랜스폼만 옮기면 한 프레임 늘어져 보인다.
        if (playerBody != null)
            playerBody.position = position;
    }
}
