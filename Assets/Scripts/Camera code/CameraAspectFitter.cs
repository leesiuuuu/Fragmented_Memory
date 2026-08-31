using UnityEngine;

// 직교 카메라는 orthographic size가 곧 세로 반높이라 화면이 좁아지면 가로가 잘린다.
// 방 프리팹은 폭이 고정이므로, 기준 종횡비보다 좁은 화면에서는 size를 키워
// 최소한 기준 가로 폭만큼은 항상 보이게 맞춘다.
[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class CameraAspectFitter : MonoBehaviour
{
    [Header("기준")]
    // UI의 Reference Resolution과 같은 값을 쓴다 — 화면 구성이 UI와 어긋나지 않게.
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    // 기준 종횡비에서의 orthographic size. 씬에 설정된 값을 그대로 쓰려면 0으로 둔다.
    [SerializeField, Min(0f)] private float referenceOrthographicSize;

    [Header("넓은 화면")]
    // 기준보다 넓은 화면에서 방 바깥이 보이는 것을 막는다.
    // 켜면 좌우에 검은 띠가 생기고, 끄면 그만큼 더 넓게 보인다.
    [SerializeField] private bool pillarboxWideScreens;

    private Camera targetCamera;
    private int lastWidth;
    private int lastHeight;

    private float ReferenceAspect => referenceResolution.y > 0f
        ? referenceResolution.x / referenceResolution.y
        : 16f / 9f;

    private void OnEnable()
    {
        targetCamera = GetComponent<Camera>();

        if (referenceOrthographicSize <= 0f && targetCamera != null)
            referenceOrthographicSize = targetCamera.orthographicSize;

        Apply();
    }

    private void LateUpdate()
    {
        // 창 크기가 바뀔 때만 다시 계산한다 — 매 프레임 건드리면 다른 카메라 연출과 싸운다.
        if (Screen.width == lastWidth && Screen.height == lastHeight)
            return;

        Apply();
    }

    private void Apply()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (targetCamera == null || !targetCamera.orthographic || referenceOrthographicSize <= 0f)
            return;

        lastWidth = Screen.width;
        lastHeight = Screen.height;

        float referenceAspect = ReferenceAspect;
        float currentAspect = targetCamera.aspect;

        if (currentAspect < referenceAspect)
        {
            // 좁은 화면 — 세로를 늘려 기준 가로 폭을 지킨다.
            targetCamera.orthographicSize = referenceOrthographicSize * (referenceAspect / currentAspect);
            targetCamera.rect = new Rect(0f, 0f, 1f, 1f);
            return;
        }

        targetCamera.orthographicSize = referenceOrthographicSize;

        if (!pillarboxWideScreens || currentAspect <= referenceAspect)
        {
            targetCamera.rect = new Rect(0f, 0f, 1f, 1f);
            return;
        }

        // 넓은 화면 — 뷰포트를 기준 비율로 좁히고 남는 좌우를 띠로 둔다.
        float width = referenceAspect / currentAspect;
        targetCamera.rect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
    }
}
