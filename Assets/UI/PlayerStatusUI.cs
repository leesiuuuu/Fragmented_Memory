using TMPro;
using UnityEngine;

// 플레이어의 실제 스탯과 체력 변경을 받아 상태 패널에 표시한다.
// Tab으로 패널을 열고 닫는다. 열린 동안에는 플레이 입력을 막는다.
public class PlayerStatusUI : MonoBehaviour
{
    private const string InputLockId = "PlayerStatusUI";

    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private PlayerStatusPanel panelPrefab;

    private PlayerStats stats;
    private PlayerHP playerHP;
    private SkillManager skillManager;
    private PlayerStatusPanel panelInstance;
    private GameObject panelRoot;
    private TMP_Text statusText;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        playerHP = GetComponent<PlayerHP>();
        skillManager = GetComponent<SkillManager>();

        if (panelPrefab != null)
        {
            panelInstance = Instantiate(panelPrefab);
            panelRoot = panelInstance.PanelRoot;
            statusText = panelInstance.StatusText;
        }

        SetVisible(false);
    }

    private void OnEnable()
    {
        if (stats != null)
            stats.StatsChanged += Refresh;
        if (playerHP != null)
            playerHP.HealthChanged += HandleHealthChanged;
    }

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        if (panelRoot == null || !Input.GetKeyDown(toggleKey))
            return;

        if (panelRoot.activeSelf || !GameplayInputLock.IsLocked)
            SetVisible(!panelRoot.activeSelf);
    }

    private void OnDisable()
    {
        GameplayInputLock.SetLocked(InputLockId, false);

        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (stats != null)
            stats.StatsChanged -= Refresh;
        if (playerHP != null)
            playerHP.HealthChanged -= HandleHealthChanged;
    }

    private void OnDestroy()
    {
        GameplayInputLock.SetLocked(InputLockId, false);

        if (panelInstance != null)
            Destroy(panelInstance.gameObject);
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (statusText == null || stats == null)
            return;

        string activeEffects = string.Empty;

        if (skillManager != null && skillManager.IsUltimateBuffActive)
            activeEffects += "\n궁극기 공격력 증가";
        if (skillManager != null && skillManager.IsLifeStealBoostActive)
            activeEffects += "\n찌르기 피흡 증가";

        statusText.text =
            $"플레이어 상태\n\n"+
            $"체력  {stats.currentHealth} / {stats.maxHealth}\n"+
            $"공격력  {stats.CurrentAttack}\n"+
            $"방어력  {stats.CurrentDefense}\n"+
            $"치명타 확률  {stats.CurrentCriticalChance:0.##}%\n"+
            $"치명타 데미지  {stats.CurrentCriticalDamage:0.##}%\n"+
            $"피흡  {stats.CurrentLifeSteal:0.##}%\n"+
            $"매력  {stats.CurrentCharm}"+
            (string.IsNullOrEmpty(activeEffects)
                ? string.Empty
                : $"\n\n적용 중인 효과{activeEffects}")+
            "\n\n<size=70%><color=#FFFFFF>Tab 닫기</color></size>";
    }

    private void SetVisible(bool visible)
    {
        if (panelRoot == null)
        {
            if (visible)
                Debug.LogError("[PlayerStatusUI] 상태 패널 프리팹이 연결되지 않았습니다.", this);
            return;
        }

        panelRoot.SetActive(visible);
        GameplayInputLock.SetLocked(InputLockId, visible);

        if (visible)
            Refresh();
    }

}
