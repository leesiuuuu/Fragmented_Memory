using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject reality;
    [SerializeField] private GameObject mirror;

    [SerializeField] private GameObject roomPrefab;

    private GameObject currentRoom;

    private void Start()
    {
        reality.SetActive(true);
        mirror.SetActive(false);
    }

    public void EnterMirror()
    {
        reality.SetActive(false);
        mirror.SetActive(true);

        CreateRoom();
    }

    public void MoveNextRoom()
    {
        CreateRoom();
    }

    private void CreateRoom()
    {
        if (currentRoom != null)
        {
            Destroy(currentRoom);
        }

        currentRoom = Instantiate(roomPrefab, mirror.transform);
    }

    public void ReturnReality()
    {
        if (currentRoom != null)
        {
            Destroy(currentRoom);
        }

        mirror.SetActive(false);
        reality.SetActive(true);
    }
}