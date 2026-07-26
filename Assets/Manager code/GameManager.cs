using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject roomPrefab;

    [SerializeField] private Transform mirror;

    [SerializeField] private GameObject player;

    private GameObject currentRoom;

    public void EnterMirror()
    {
        if (currentRoom != null)
            Destroy(currentRoom);

        currentRoom = Instantiate(roomPrefab, mirror);

        RoomManager roomManager = currentRoom.GetComponent<RoomManager>();

        roomManager.StartRoom(player);
    }

    public void MoveNextRoom()
    {
        Destroy(currentRoom);

        currentRoom = Instantiate(roomPrefab, mirror);

        RoomManager roomManager = currentRoom.GetComponent<RoomManager>();

        roomManager.StartRoom(player);
    }
}