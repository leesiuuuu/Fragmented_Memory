using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private SpawnManager spawnManager;

    [Header("Spawn")]
    [SerializeField] private Transform playerSpawn;

    [Header("Doors")]
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;


    private bool isStarted;
    private bool isCleared;


    public void StartRoom(GameObject player)
    {
        if (isStarted)
            return;

        isStarted = true;

        // 플레이어 위치 이동
        player.transform.position = playerSpawn.position;


        // 방 시작 시 문 잠금
        CloseDoors();


        // 적 생성
        spawnManager.SpawnEnemies(this);


        Debug.Log("방 시작");
    }


    private void CloseDoors()
    {
        leftDoor.SetActive(false);
        rightDoor.SetActive(false);
    }


    public void RoomClear()
    {
        if (isCleared)
            return;

        isCleared = true;


        // 문 열기
        OpenDoors();


        Debug.Log("방 클리어");
    }


    private void OpenDoors()
    {
        leftDoor.SetActive(true);
        rightDoor.SetActive(true);
    }
}