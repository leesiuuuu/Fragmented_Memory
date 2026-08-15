using System;
using System.Collections;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField, Min(0)] private int clearStarDustReward = 50;
    [SerializeField, Min(0f)] private float rewardDisplayDelay = 1.2f;

    [Header("Managers")]
    [SerializeField] private SpawnManager spawnManager;

    [Header("Spawn")]
    [SerializeField] private Transform playerSpawn;

    [Header("Doors")]
    [SerializeField] private RoomDoorTrigger leftDoor;
    [SerializeField] private RoomDoorTrigger rightDoor;

    private bool isStarted;
    private bool isCleared;

    private CurrencyWallet rewardWallet;
    private RewardManager rewardManager;

    public event Action<RoomManager, RoomDoorTrigger> ExitSelected;

    private void Awake()
    {
        if (leftDoor != null)
            leftDoor.Entered += HandleDoorEntered;

        if (rightDoor != null)
            rightDoor.Entered += HandleDoorEntered;
    }

    private void OnDestroy()
    {
        if (leftDoor != null)
            leftDoor.Entered -= HandleDoorEntered;

        if (rightDoor != null)
            rightDoor.Entered -= HandleDoorEntered;

        if (rewardManager != null)
            rewardManager.RewardSelected -= HandleRewardSelected;
    }

    public void SetRewardManager(RewardManager manager)
    {
        if (rewardManager != null)
            rewardManager.RewardSelected -= HandleRewardSelected;

        rewardManager = manager;

        if (rewardManager != null && isCleared)
            rewardManager.RewardSelected += HandleRewardSelected;
    }

    public void StartRoom(GameObject player)
    {
        if (isStarted || player == null)
            return;

        isStarted = true;
        isCleared = false;

        rewardWallet = player.GetComponent<CurrencyWallet>();

        if (playerSpawn != null)
            player.transform.position = playerSpawn.position;

        // 방 시작 시 반드시 문을 닫는다.
        CloseDoors();

        if (spawnManager != null)
        {
            spawnManager.SpawnEnemies(this, rewardWallet);
        }
    }

    public void RoomClear()
    {
        // 중복 처리 x
        if (isCleared)
            return;

        isCleared = true;

        // 기본 보상
        rewardWallet?.Add(clearStarDustReward);

        // 보상 선택을 완료한 뒤 문을 연다.
        if (rewardManager != null)
        {
            rewardManager.RewardSelected -= HandleRewardSelected;
            rewardManager.RewardSelected += HandleRewardSelected;
            StartCoroutine(ShowRewardAfterDelay());
            return;
        }

        OpenDoors();
    }

    private IEnumerator ShowRewardAfterDelay()
    {
        yield return new WaitForSeconds(rewardDisplayDelay);

        if (rewardManager != null && rewardManager.GenerateRewards())
            yield break;

        if (rewardManager != null)
            rewardManager.RewardSelected -= HandleRewardSelected;

        OpenDoors();
    }

    private void CloseDoors()
    {
        if (leftDoor != null)
            leftDoor.SetInteractable(false);

        if (rightDoor != null)
            rightDoor.SetInteractable(false);
    }

    private void OpenDoors()
    {
        if (!isCleared)
            return;

        if (leftDoor != null)
            leftDoor.SetInteractable(true);

        if (rightDoor != null)
            rightDoor.SetInteractable(true);
    }

    private void HandleRewardSelected()
    {
        if (rewardManager != null)
            rewardManager.RewardSelected -= HandleRewardSelected;

        OpenDoors();
    }

    private void HandleDoorEntered(RoomDoorTrigger door)
    {
        if (!isCleared)
            return;

        ExitSelected?.Invoke(this, door);
    }
}

