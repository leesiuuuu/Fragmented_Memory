using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public bool IsBusy { get; private set; }

    public void StartAction()
    {
        IsBusy = true;
    }

    public void EndAction()
    {
        IsBusy = false;
    }
}