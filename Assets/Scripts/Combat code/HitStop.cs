using System.Collections;
using UnityEngine;

// 타격 순간 시간을 아주 짧게 멈춰 무게를 만든다.
// 씬마다 오브젝트를 놓지 않아도 되도록 처음 호출될 때 스스로 만들어진다 —
// 보스 씬·테스트 씬까지 전부 배선하는 것은 잊기 쉽고, 잊으면 조용히 죽는다.
public static class HitStop
{
    // 일반 공격 0.05 · 스킬 0.08 · 궁극기와 패링 0.12 정도가 무난한 출발점이다.
    public const float Light = 0.05f;
    public const float Medium = 0.08f;
    public const float Heavy = 0.12f;

    private static Runner runner;


    public static void Play(float duration)
    {
        if (duration <= 0f)
            return;

        // 일시정지·사망 연출이 이미 시간을 멈춰 두었다.
        // 여기서 끼어들면 히트스톱이 끝나면서 timeScale을 1로 되돌려 정지를 풀어 버린다.
        if (GameplayInputLock.IsLocked || Time.timeScale <= 0f)
            return;

        EnsureRunner();
        runner.Freeze(duration);
    }


    private static void EnsureRunner()
    {
        if (runner != null)
            return;

        GameObject host = new GameObject("[HitStop]")
        {
            hideFlags = HideFlags.HideAndDontSave
        };

        Object.DontDestroyOnLoad(host);
        runner = host.AddComponent<Runner>();
    }


    // 씬을 다시 로드하면 파괴된 러너의 참조가 남는다.
    // 도메인 리로드를 끈 환경에서도 정적 상태가 새 판으로 넘어가지 않게 비운다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        runner = null;
    }


    private class Runner : MonoBehaviour
    {
        private Coroutine running;
        private float restoreScale = 1f;


        public void Freeze(float duration)
        {
            // 이미 멈춰 있는 중이라면 원래 배속을 덮어쓰지 않는다 — 0을 복원값으로 삼으면 영영 멈춘다.
            if (running != null)
                StopCoroutine(running);
            else
                restoreScale = Time.timeScale;

            running = StartCoroutine(FreezeRoutine(duration));
        }


        private IEnumerator FreezeRoutine(float duration)
        {
            Time.timeScale = 0f;

            // timeScale이 0이므로 실시간으로 세지 않으면 영원히 끝나지 않는다.
            yield return new WaitForSecondsRealtime(duration);

            // 멈춘 사이에 일시정지나 사망이 끼어들었다면 시간의 주인은 그쪽이다.
            // 여기서 되돌리면 정지가 풀려 버리므로 손대지 않고 물러난다.
            if (!GameplayInputLock.IsLocked)
                Time.timeScale = restoreScale;

            running = null;
        }


        private void OnDisable()
        {
            if (running == null)
                return;

            StopCoroutine(running);
            running = null;

            if (!GameplayInputLock.IsLocked)
                Time.timeScale = restoreScale;
        }
    }
}
