using System.Collections.Generic;
using UnityEngine;

// 카메라가 방 밖을 비추지 않도록 가두는 영역. 방 프리팹 안에 하나 놓으면 된다.
// 방은 런타임에 Instantiate되므로 씬의 카메라를 미리 참조할 수 없다 —
// 반대로 자기 자신을 등록해 두고 카메라가 찾아 쓰게 한다.
[DisallowMultipleComponent]
public class CameraBounds : MonoBehaviour
{
    // 단일 필드로 두면 방을 갈아끼울 때 깨진다. Destroy는 프레임 끝으로 미뤄지므로
    // 새 방의 OnEnable이 먼저 돌고 옛 방의 OnDisable이 나중에 돌아 방금 등록한 값을 지워 버린다.
    // 목록으로 두고 마지막 것을 쓰면 순서와 무관하게 항상 살아 있는 방이 이긴다.
    private static readonly List<CameraBounds> active = new List<CameraBounds>();

    public static CameraBounds Active =>
        active.Count > 0 ? active[active.Count - 1] : null;


    [Header("영역")]
    // 이 오브젝트에 BoxCollider2D가 있으면 그쪽을 쓴다. 없을 때만 아래 값을 쓴다.
    [SerializeField] private Vector2 size = new Vector2(40f, 20f);

    [SerializeField] private Vector2 center;

    private BoxCollider2D box;


    // 방이 mirror 밑에 붙어 위치가 바뀌므로 월드 좌표는 그때그때 계산한다.
    public Bounds Area
    {
        get
        {
            if (box != null)
                return box.bounds;

            return new Bounds(transform.position + (Vector3)center, new Vector3(size.x, size.y, 1f));
        }
    }


    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();

        // 경계용 콜라이더가 플레이어를 막아 세우면 곤란하다.
        if (box != null)
            box.isTrigger = true;
    }


    private void OnEnable()
    {
        if (!active.Contains(this))
            active.Add(this);
    }


    private void OnDisable()
    {
        active.Remove(this);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 편집 중에는 Awake가 돌지 않아 box가 비어 있다 — 그때도 콜라이더를 그려 주려고 직접 찾는다.
        BoxCollider2D editorBox = box != null ? box : GetComponent<BoxCollider2D>();

        Bounds area = editorBox != null
            ? editorBox.bounds
            : new Bounds(transform.position + (Vector3)center, new Vector3(size.x, size.y, 1f));

        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.8f);
        Gizmos.DrawWireCube(area.center, area.size);
    }
#endif
}
