using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private Transform mirror;
    [SerializeField] private GameObject player;
    [SerializeField] private RewardManager rewardManager;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private ShopInteract realityShop;

    private GameObject currentRoom;

    private void Awake()
    {
        if (realityShop == null)
            realityShop = FindFirstObjectByType<ShopInteract>(FindObjectsInactive.Include);

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
        EnsureRealityShopReference();
        realityShop?.SetAvailable(false);
        CreateRoom();
    }

    public void EnterReality()
    {
        EnsureRealityShopReference();
        realityShop?.SetAvailable(true);
        RefreshRealityShop();
    }

    private void EnsureRealityShopReference()
    {
        if (realityShop == null)
            realityShop = FindFirstObjectByType<ShopInteract>(FindObjectsInactive.Include);
    }

    public void MoveNextRoom()
    {
        CreateRoom();
    }

    public void RefreshRealityShop()
    {
        shopManager?.PrepareShop();
    }

    private void CreateRoom()
    {
        if (currentRoom != null)
            Destroy(currentRoom);

        currentRoom = Instantiate(roomPrefab, mirror);
        RoomManager room = currentRoom.GetComponent<RoomManager>();

        if (room == null)
        {
            Debug.LogError("Room prefab에 RoomManager가 없습니다.");
            return;
        }

        room.SetRewardManager(rewardManager);
        room.ExitSelected += HandleRoomExit;
        room.StartRoom(player);
    }

    private void HandleRoomExit(RoomManager room, RoomDoorTrigger door)
    {
        room.ExitSelected -= HandleRoomExit;
        MoveNextRoom();
    }
}
