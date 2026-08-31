using UnityEngine;

// 스테이지 트리(StageMap)를 들고 현재 노드를 따라 방을 갈아끼운다.
// 방 프리팹을 직접 들고 있지 않은 이유 — 어떤 방을 띄울지는 노드가 정한다.
public class GameManager : MonoBehaviour
{
    [Header("스테이지")]
    [SerializeField] private StageData stageData;


    [Header("씬 참조")]
    [SerializeField] private Transform mirror;

    // 비워 두면 씬에서 찾는다. 런이 끝날 때 다시 열어 줘야 한다.
    [SerializeField] private MirrorDoorTrigger mirrorDoor;

    [SerializeField] private GameObject player;

    [SerializeField] private RewardManager rewardManager;

    [SerializeField] private ShopManager shopManager;

    [SerializeField] private ShopInteract realityShop;

    [SerializeField] private Transform realitySpawnPoint;


    [Header("시드")]
    // 0이면 거울에 들어갈 때마다 새 판. 값을 박으면 그 판이 그대로 재현되므로 디버깅용으로 쓴다.
    [SerializeField] private int fixedStageSeed;


    [Header("난이도")]
    // 방을 하나 내려갈 때마다 적 능력치에 더해지는 배율.
    // depth가 곧 진행도라 별도 카운터가 필요 없다.
    [SerializeField] private float powerPerDepth = 0.15f;


    private StageMap map;
    private MapNode currentNode;
    private GameObject currentRoom;
    private bool waitingForStageReward;


    public StageData Stage => stageData;

    public MapNode CurrentNode => currentNode;

    // EnemyStats.Setup(data, power)로 넘어가는 값
    public float EnemyPower => 1f + (currentNode != null ? currentNode.depth : 0) * powerPerDepth;


    private void Awake()
    {
        if (realityShop == null)
            realityShop = FindFirstObjectByType<ShopInteract>(FindObjectsInactive.Include);

        EnsureMirrorDoorReference();

        if (rewardManager != null && player != null)
            rewardManager.Initialize(player.GetComponent<Inventory>());

        if (shopManager != null && player != null)
        {
            shopManager.Initialize(player);
            RefreshRealityShop();
        }
    }


    public void EnterMirror()
    {
        if (stageData == null)
        {
            Debug.LogError("[GameManager] stageData가 비어 있습니다.");
            return;
        }

        EnsureRealityShopReference();
        realityShop?.SetAvailable(false);

        int seed = fixedStageSeed != 0
            ? fixedStageSeed
            : Random.Range(int.MinValue, int.MaxValue);

        map = StageGenerator.Generate(stageData, seed);

        Debug.Log($"[GameManager] 스테이지 생성 — seed {seed} · 깊이 {map.depth} · 노드 {map.NodeCount}개");

        LoadRoom(map.root);
    }


    public void EnterReality()
    {
        EnsureRealityShopReference();
        realityShop?.SetAvailable(true);
        RefreshRealityShop();

        // 거울 문을 되돌리지 않으면 런을 한 번밖에 돌 수 없다.
        EnsureMirrorDoorReference();
        mirrorDoor?.SetAvailable(true);
    }


    public void EndRun()
    {
        if (rewardManager != null)
            rewardManager.RewardSelected -= HandleStageRewardSelected;

        waitingForStageReward = false;

        if (currentRoom != null)
            Destroy(currentRoom);

        currentRoom = null;
        currentNode = null;
        map = null;

        if (player != null)
        {
            if (realitySpawnPoint != null)
            {
                player.transform.SetPositionAndRotation(
                    realitySpawnPoint.position,
                    realitySpawnPoint.rotation);

                Rigidbody2D rigid = player.GetComponent<Rigidbody2D>();

                if (rigid != null)
                    rigid.linearVelocity = Vector2.zero;
            }

            // 런을 넘어 유지되는 것 — 기억 조각·아이템·별가루.
            // 초기화되는 것 — 체력과 사망 상태. 없으면 쓰러진 채로 현실에 서 있게 된다.
            player.GetComponent<PlayerHP>()?.RestoreAfterRun();
        }

        EnterReality();
    }


    public void RefreshRealityShop()
    {
        shopManager?.PrepareShop();
    }


    private void EnsureRealityShopReference()
    {
        if (realityShop == null)
            realityShop = FindFirstObjectByType<ShopInteract>(FindObjectsInactive.Include);
    }


    private void EnsureMirrorDoorReference()
    {
        if (mirrorDoor == null)
            mirrorDoor = FindFirstObjectByType<MirrorDoorTrigger>(FindObjectsInactive.Include);
    }


    // doorIndex는 MapNode.next의 인덱스와 같다. 0 = 왼쪽 문, 1 = 오른쪽 문.
    public void MoveNextRoom(int doorIndex)
    {
        if (currentNode == null)
        {
            Debug.LogError("[GameManager] 스테이지가 시작되지 않았는데 문을 통과했습니다.");
            return;
        }

        // 보스방에서 또 문을 타면 next가 둘 다 null이라 보스가 무한히 다시 로드된다
        if (currentNode.type == RoomType.Boss)
        {
            ShowStageReward();
            return;
        }

        if (doorIndex < 0 || doorIndex >= currentNode.next.Length)
        {
            Debug.LogError($"[GameManager] 잘못된 doorIndex {doorIndex}");
            return;
        }

        MapNode next = currentNode.next[doorIndex];

        // 마지막 깊이의 문은 보스로 이어진다 — 보스는 트리 밖에 있다
        if (next == null)
        {
            LoadBoss();
            return;
        }

        LoadRoom(next);
    }


    // 되돌아가기. 같은 노드는 seed가 같으므로 같은 방·같은 배치가 다시 나온다.
    public void MovePrevRoom()
    {
        if (currentNode == null || currentNode.parent == null)
            return;

        LoadRoom(currentNode.parent);
    }


    public void OnRoomCleared()
    {
        if (currentNode != null)
            currentNode.cleared = true;
    }


    private void ShowStageReward()
    {
        if (waitingForStageReward)
            return;

        if (rewardManager != null)
        {
            waitingForStageReward = true;
            rewardManager.RewardSelected -= HandleStageRewardSelected;
            rewardManager.RewardSelected += HandleStageRewardSelected;

            if (rewardManager.GenerateRewards())
                return;

            rewardManager.RewardSelected -= HandleStageRewardSelected;
            waitingForStageReward = false;
        }

        EndRun();
    }


    private void HandleStageRewardSelected()
    {
        if (rewardManager != null)
            rewardManager.RewardSelected -= HandleStageRewardSelected;

        waitingForStageReward = false;
        EndRun();
    }


    private void LoadRoom(MapNode node)
    {
        if (node.room == null)
        {
            Debug.LogError($"[GameManager] d{node.depth} {node.type}: RoomData가 없습니다. StageData.roomPool을 확인하세요.");
            return;
        }

        GameObject prefab = node.room.PickVariant(node.seed);

        if (prefab == null)
            return;

        currentNode = node;

        SwapRoom(prefab, node);

        Debug.Log($"[GameManager] 입장 — d{node.depth} {node.type} · seed {node.seed} · power {EnemyPower:0.00}");
    }


    private void LoadBoss()
    {
        // 보스가 없다고 그냥 return하면 방이 바뀌지 않는데 ExitSelected 구독은 이미 끊긴 뒤라
        // 문 앞에서 영구히 멈춘다. 보스를 건너뛰고 스테이지를 마무리해 런이 끝나게 한다.
        if (stageData.bossPrefab == null)
        {
            Debug.LogError($"[GameManager] {stageData.name}: bossPrefab이 비어 있습니다. 보스를 건너뛰고 스테이지를 종료합니다.");
            ShowStageReward();
            return;
        }

        // 보스방은 트리 노드가 아니므로 방 개수에 포함되지 않는다.
        // 노드를 하나 만들어 넘기는 것은 RoomManager가 노드 단위로만 동작하기 때문이다.
        MapNode bossNode = new MapNode
        {
            depth = map.depth,
            type = RoomType.Boss,
            parent = currentNode,
            seed = currentNode.seed
        };

        currentNode = bossNode;

        SwapRoom(stageData.bossPrefab, bossNode);

        Debug.Log("[GameManager] 보스방 입장");
    }


    private void SwapRoom(GameObject prefab, MapNode node)
    {
        if (currentRoom != null)
            Destroy(currentRoom);

        currentRoom = Instantiate(prefab, mirror);

        RoomManager roomManager = currentRoom.GetComponent<RoomManager>();

        if (roomManager == null)
        {
            Debug.LogError($"[GameManager] {prefab.name}에 RoomManager가 없습니다.");
            return;
        }

        roomManager.ExitSelected += HandleRoomExit;
        roomManager.StartRoom(this, node, player);
    }


    // 어느 문으로 나갔는지가 트리 분기를 정한다.
    // 뒤 문은 부모로 올라가므로 doorIndex(= next 인덱스)를 쓰지 않는다.
    private void HandleRoomExit(RoomManager room, RoomDoorTrigger door)
    {
        room.ExitSelected -= HandleRoomExit;

        if (door.IsBackDoor)
            MovePrevRoom();
        else
            MoveNextRoom(door.DoorIndex);
    }
}
