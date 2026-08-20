using System;
using UnityEngine;

public class RoomDoorTrigger : MonoBehaviour, InteractRule
{
    // MapNode.next의 인덱스와 같다. 0 = 왼쪽 문, 1 = 오른쪽 문.
    // 뒤집히면 검증 통과한 트리와 실제 주행이 어긋난다.
    [SerializeField] private int doorIndex;

    // 이전 방으로 돌아가는 문. 트리에서 부모로 올라가므로 doorIndex를 쓰지 않는다.
    [SerializeField] private bool isBackDoor;

    public int DoorIndex => doorIndex;

    public bool IsBackDoor => isBackDoor;

    public event Action<RoomDoorTrigger> Entered;

    private bool canUse;
    private bool used;

    private void Awake()
    {
        canUse = false;
        used = false;
    }

    public void SetInteractable(bool value)
    {
        canUse = value;
        used = false;

        gameObject.SetActive(value);
    }

    public void Interact()
    {
        if (!canUse)
            return;

        if (used)
            return;

        used = true;
        Entered?.Invoke(this);
    }
}
