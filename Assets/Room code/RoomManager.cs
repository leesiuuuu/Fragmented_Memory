using UnityEngine;

public class RoomManager : MonoBehaviour
{
    private bool isStarted = false;

    public void StartRoom()
    {
        if (isStarted)
            return;

        isStarted = true;

        Debug.Log("방 입장");

    }
}