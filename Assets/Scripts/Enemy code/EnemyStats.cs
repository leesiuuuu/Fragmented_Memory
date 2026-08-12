using UnityEngine;

// 스탯이 프리팹에 박혀 있으면 스테이지별로 값을 곱할 방법이 없다.
// 기준값은 EnemyData(SO)가 들고, 실제 값은 스폰 시점에 Setup()이 정한다.
public class EnemyStats : MonoBehaviour
{
    // 씬에 직접 놓고 테스트할 때만 쓰는 기본값.
    // 정상 경로에서는 SpawnManager가 Setup()으로 주입한다.
    [SerializeField] private EnemyData fallbackData;


    public EnemyData Data { get; private set; }

    // 이름을 유지해야 EnemyHP·EnemyAttack의 호출부가 그대로 산다
    public int maxHP { get; private set; }
    public int attack { get; private set; }
    public int defense { get; private set; }

    public int currentHP { get; set; }


    private void Awake()
    {
        // Setup 없이 씬에 놓인 적도 죽은 채로 시작하지는 않게 한다
        if (Data == null && fallbackData != null)
            Setup(fallbackData, 1f);
    }


    // power는 RunManager.EnemyPower — 스테이지 진행도와 방 깊이에 따른 난이도 배율.
    // Instantiate 직후, Start가 돌기 전에 불려야 한다.
    public void Setup(EnemyData data, float power)
    {
        if (data == null)
        {
            Debug.LogError($"[EnemyStats] {name}: EnemyData가 null입니다.");
            return;
        }

        Data = data;

        // 정예는 같은 풀에서 뽑히지 않으므로 배율을 여기서 겹쳐 건다
        float multiplier = power * (data.isElite ? data.eliteMultiplier : 1f);

        maxHP = Mathf.Max(1, Mathf.RoundToInt(data.maxHP * multiplier));
        attack = Mathf.Max(0, Mathf.RoundToInt(data.attack * multiplier));
        defense = Mathf.Max(0, Mathf.RoundToInt(data.defense * multiplier));

        currentHP = maxHP;
    }
}
