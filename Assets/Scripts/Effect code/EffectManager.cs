using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [System.Serializable]
    private class EffectPool
    {
        public EffectId id;
        public GameObject prefab;
        [Min(0)] public int initialSize = 5;
        public bool canExpand = true;

        // 호출부에서 전달한 rotation을 기준으로 적용되는 로컬 위치 보정값
        public Vector3 positionOffset;

        // 호출부에서 전달한 rotation에 추가로 적용되는 오일러 회전 보정값
        public Vector3 rotationOffset;
    }

    private class EffectInstance
    {
        public GameObject gameObject;
        public ParticleSystem[] particles;
        public Animator[] animators;
    }

    // 씬에 EffectManager를 놓지 않아도 되도록, 처음 필요해질 때 이 프리팹으로 스스로 만들어진다.
    // 경로는 Resources 폴더 기준이다.
    private const string BootstrapPrefabPath = "EffectManager";

    private static EffectManager instance;
    private static bool bootstrapFailed;

    // HitStop과 같은 이유로 자동 생성한다 — 보스 씬·테스트 씬까지 전부 배선하는 것은
    // 잊기 쉽고, 잊으면 연출이 조용히 사라져 버그인지 연출이 없는 것인지 구분할 수 없다.
    public static EffectManager Instance
    {
        get
        {
            if (instance != null)
                return instance;

            return Bootstrap();
        }
    }

    [SerializeField] private List<EffectPool> effectPools = new List<EffectPool>();

    private bool initialized;

    private readonly Dictionary<EffectId, EffectPool> poolSettings =
        new Dictionary<EffectId, EffectPool>();
    private readonly Dictionary<EffectId, Queue<EffectInstance>> pools =
        new Dictionary<EffectId, Queue<EffectInstance>>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("EffectManager가 씬에 두 개 이상 있습니다.");
            enabled = false;
            return;
        }

        instance = this;
        EnsureInitialized();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    // 도메인 리로드를 끈 환경에서 파괴된 인스턴스의 참조가 다음 판으로 넘어가지 않게 비운다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        instance = null;
        bootstrapFailed = false;
    }

    private static EffectManager Bootstrap()
    {
        // 프리팹이 없는 프로젝트에서 매 호출마다 씬을 뒤지고 경고를 찍지 않도록 먼저 걸러낸다.
        // 씬에 놓인 쪽이 있었다면 그쪽 Awake가 이미 instance를 채웠을 것이다.
        if (bootstrapFailed)
            return null;

        // Awake가 아직 돌지 않았을 뿐 씬에 놓인 쪽이 있을 수 있다.
        // 그걸 못 보고 새로 만들면 둘이 되어 한쪽이 스스로를 꺼 버린다.
        EffectManager placed =
            FindFirstObjectByType<EffectManager>(FindObjectsInactive.Exclude);

        if (placed != null)
        {
            instance = placed;
            placed.EnsureInitialized();
            return placed;
        }

        GameObject prefab = Resources.Load<GameObject>(BootstrapPrefabPath);

        if (prefab == null)
        {
            bootstrapFailed = true;
            Debug.LogWarning(
                $"Resources/{BootstrapPrefabPath} 프리팹이 없어 EffectManager를 자동 생성하지 못했습니다.");
            return null;
        }

        GameObject host = Instantiate(prefab);
        host.name = prefab.name;

        // Instantiate가 Awake를 즉시 돌리므로 여기서는 instance가 채워져 있다.
        return instance;
    }

    private void EnsureInitialized()
    {
        if (initialized)
            return;

        initialized = true;
        InitializePools();
    }

    // 아직 프리팹을 꽂지 않은 이펙트를 호출부가 조용히 건너뛸 수 있게 한다.
    // Play는 미등록이면 매번 경고를 찍는데, 선택적인 연출까지 경고로 덮이면 로그를 못 읽는다.
    public bool IsRegistered(EffectId id)
    {
        EnsureInitialized();

        return poolSettings.ContainsKey(id);
    }


    public void Play(EffectId id, Vector3 position, Quaternion rotation)
    {
        EnsureInitialized();

        EffectInstance effect = GetEffect(id);

        if (effect == null)
            return;

        EffectPool effectPool = poolSettings[id];
        Vector3 finalPosition = position + rotation * effectPool.positionOffset;
        Quaternion finalRotation = rotation * Quaternion.Euler(effectPool.rotationOffset);

        effect.gameObject.transform.SetPositionAndRotation(finalPosition, finalRotation);
        effect.gameObject.SetActive(true);

        foreach (ParticleSystem particle in effect.particles)
        {
            particle.Clear(true);
            particle.Play(true);
        }

        foreach (Animator animator in effect.animators)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        StartCoroutine(ReturnAfterPlayback(id, effect));
    }

    private void InitializePools()
    {
        foreach (EffectPool effectPool in effectPools)
        {
            if (effectPool == null || effectPool.prefab == null)
            {
                Debug.LogWarning("EffectManager에 프리팹이 비어 있는 항목이 있습니다.");
                continue;
            }

            if (poolSettings.ContainsKey(effectPool.id))
            {
                Debug.LogWarning($"{effectPool.id} 이펙트가 중복 등록되어 있습니다.");
                continue;
            }

            poolSettings.Add(effectPool.id, effectPool);
            pools.Add(effectPool.id, new Queue<EffectInstance>());

            for (int i = 0; i < effectPool.initialSize; i++)
                pools[effectPool.id].Enqueue(CreateEffect(effectPool));
        }
    }

    private EffectInstance GetEffect(EffectId id)
    {
        if (!poolSettings.TryGetValue(id, out EffectPool effectPool))
        {
            Debug.LogWarning($"{id} 이펙트가 EffectManager에 등록되어 있지 않습니다.");
            return null;
        }

        Queue<EffectInstance> pool = pools[id];

        if (pool.Count > 0)
            return pool.Dequeue();

        if (effectPool.canExpand)
            return CreateEffect(effectPool);

        return null;
    }

    private EffectInstance CreateEffect(EffectPool effectPool)
    {
        GameObject instance = Instantiate(effectPool.prefab, transform);
        instance.SetActive(false);

        return new EffectInstance
        {
            gameObject = instance,
            particles = instance.GetComponentsInChildren<ParticleSystem>(true),
            animators = instance.GetComponentsInChildren<Animator>(true)
        };
    }

    private IEnumerator ReturnAfterPlayback(EffectId id, EffectInstance effect)
    {
        yield return null;

        while (effect.gameObject != null && IsPlaying(effect.particles, effect.animators))
            yield return null;

        if (effect.gameObject == null)
            yield break;

        effect.gameObject.SetActive(false);
        effect.gameObject.transform.SetParent(transform);
        pools[id].Enqueue(effect);
    }

    private bool IsPlaying(ParticleSystem[] particles, Animator[] animators)
    {
        foreach (ParticleSystem particle in particles)
        {
            if (particle != null && particle.IsAlive(true))
                return true;
        }

        foreach (Animator animator in animators)
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                continue;

            if (animator.IsInTransition(0))
                return true;

            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                return true;
        }

        return false;
    }
}
