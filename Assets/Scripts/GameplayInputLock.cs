using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameplayInputLock
{
    private static readonly HashSet<string> Sources = new HashSet<string>();

    public static bool IsLocked => Sources.Count > 0;
    public static event Action<bool> Changed;

    public static void SetLocked(string sourceId, bool locked)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return;

        bool wasLocked = IsLocked;

        if (locked)
            Sources.Add(sourceId);
        else
            Sources.Remove(sourceId);

        if (wasLocked != IsLocked)
            Changed?.Invoke(IsLocked);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        Sources.Clear();
        Changed = null;
    }
}
