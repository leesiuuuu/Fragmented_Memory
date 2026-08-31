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

    public static EffectManager Instance { get; private set; }

    [SerializeField] private List<EffectPool> effectPools = new List<EffectPool>();

    private readonly Dictionary<EffectId, EffectPool> poolSettings =
        new Dictionary<EffectId, EffectPool>();
    private readonly Dictionary<EffectId, Queue<EffectInstance>> pools =
        new Dictionary<EffectId, Queue<EffectInstance>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("EffectManager가 씬에 두 개 이상 있습니다.");
            enabled = false;
            return;
        }

        Instance = this;
        InitializePools();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // 아직 프리팹을 꽂지 않은 이펙트를 호출부가 조용히 건너뛸 수 있게 한다.
    // Play는 미등록이면 매번 경고를 찍는데, 선택적인 연출까지 경고로 덮이면 로그를 못 읽는다.
    public bool IsRegistered(EffectId id)
    {
        return poolSettings.ContainsKey(id);
    }


    public void Play(EffectId id, Vector3 position, Quaternion rotation)
    {
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
