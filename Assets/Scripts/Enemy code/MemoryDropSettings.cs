using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MemoryDropSettings", menuName = "Memory/Drop Settings")]
public class MemoryDropSettings : ScriptableObject
{
    [SerializeField] private MemoryPickup pickupPrefab;
    [SerializeField] private List<MemoryData> memories = new List<MemoryData>();

    public MemoryPickup PickupPrefab => pickupPrefab;
    public IReadOnlyList<MemoryData> Memories => memories;
}
