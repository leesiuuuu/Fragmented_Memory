using System;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField, Min(0)] private int clearStarDustReward = 50;

    [Header("Managers")]
    [SerializeField] private SpawnManager spawnManager;

    [Header("Spawn")]
    [SerializeField] private Transform playerSpawn;

    [Header("Doors")]
    // 전진 문은 RoomDoorTrigger.doorIndex로, 되돌아가는 문은 isBackDoor로 구분한다.
    // 배열로 두면 문을 늘려도 구독·개폐 코드가 늘어나지 않는다.
    [SerializeField] private RoomDoorTrigger[] doors;

    // 방 프리팹은 런타임에 Instantiate되므로 씬의 GameManager를 직렬화할 수 없다.
    // StartRoom에서 주입받는 이유가 이것이다.
    private GameManager gameManager;
    private MapNode node;

    private bool isStarted;
    private bool isCleared;

    private CurrencyWallet rewardWallet;

    public MapNode Node => node;

    public event Action<RoomManager, RoomDoorTrigger> ExitSelected;

    private void Awake()
    {
        if (doors == null)
            return;

        foreach (RoomDoorTrigger door in doors)
        {
            if (door != null)
                door.Entered += HandleDoorEntered;
        }
    }

    private void OnDestroy()
    {
        if (doors != null)
        {
            foreach (RoomDoorTrigger door in doors)
            {
                if (door != null)
                    door.Entered -= HandleDoorEntered;
            }
        }
    }

    public void StartRoom(GameManager game, MapNode mapNode, GameObject player)
    {
        if (isStarted || player == null || mapNode == null)
            return;

        isStarted = true;
        isCleared = false;

        gameManager = game;
        node = mapNode;

        rewardWallet = player.GetComponent<CurrencyWallet>();

        if (playerSpawn != null)
            player.transform.position = playerSpawn.position;

        // 방 시작 시 반드시 문을 닫는다.
        CloseDoors();


        // 보스방은 트리 밖이라 RoomData가 없다. 보스는 프리팹이 직접 들고 있으므로
        // 스폰도 자동 클리어도 하지 않고 문을 잠근 채로 둔다.
        if (mapNode.type == RoomType.Boss)
            return;


        // 되돌아온 방 — 적을 다시 스폰하지도, 보상을 다시 주지도 않는다.
        if (mapNode.cleared)
        {
            ClearWithoutReward();
            return;
        }


        // 전투 계열이 아니면 적이 없다 — 들어가자마자 클리어 처리해야 문이 열린다.
        if (!IsCombat(mapNode.type))
        {
            // 보물방은 보상 선택 자체가 목적이므로 보상 흐름을 그대로 탄다.
            // 상인·휴식은 별가루도 보상 패널도 주지 않고 문만 연다.
            if (mapNode.type == RoomType.Treasure)
                RoomClear();
            else
                ClearWithoutReward();

            return;
        }


        if (spawnManager != null)
        {
            spawnManager.SpawnEnemies(this, mapNode, game.Stage, game.EnemyPower, rewardWallet);
        }
    }

    public void RoomClear()
    {
        // 중복 처리 x
        if (isCleared)
            return;

        isCleared = true;

        MarkNodeCleared();

        // 기본 보상
        rewardWallet?.Add(clearStarDustReward);

        // 조각 선택은 모든 방을 끝낸 뒤 현실로 돌아가기 전에 한 번만 띄운다.
        // 방은 별가루만 지급하고 문을 열며, 스테이지 보상은 GameManager가 처리한다.
        OpenDoors();
    }

    // 상인·휴식처럼 전투도 보상도 없는 방, 그리고 이미 클리어한 방 —
    // 별가루와 보상 패널을 건너뛰고 문만 연다.
    private void ClearWithoutReward()
    {
        if (isCleared)
            return;

        isCleared = true;

        MarkNodeCleared();

        OpenDoors();
    }

    private void MarkNodeCleared()
    {
        if (gameManager != null)
            gameManager.OnRoomCleared();

        Debug.Log($"[RoomManager] 방 클리어 — {(node != null ? node.type.ToString() : "?")}");
    }

    private void CloseDoors()
    {
        if (doors == null)
            return;

        foreach (RoomDoorTrigger door in doors)
        {
            if (door != null)
                door.SetInteractable(false);
        }
    }

    private void OpenDoors()
    {
        if (!isCleared || doors == null)
            return;

        // 루트 방은 부모가 없다 — 돌아갈 곳이 없으므로 뒤 문은 닫아둔다.
        bool canGoBack = node != null && node.parent != null;

        foreach (RoomDoorTrigger door in doors)
        {
            if (door == null)
                continue;

            door.SetInteractable(!door.IsBackDoor || canGoBack);
        }
    }

    private void HandleDoorEntered(RoomDoorTrigger door)
    {
        if (!isCleared)
            return;

        ExitSelected?.Invoke(this, door);
    }

    private static bool IsCombat(RoomType type)
    {
        return type == RoomType.Battle || type == RoomType.Elite;
    }
}
