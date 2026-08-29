using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemInventory : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public ItemData item;
        public int count;
    }

    private readonly List<Entry> items = new List<Entry>();

    public IReadOnlyList<Entry> Items => items;
    public event Action Changed;

    public void Add(ItemData item)
    {
        if (item == null)
            return;

        Entry entry = items.Find(current => current.item == item);

        if (entry == null)
        {
            entry = new Entry { item = item };
            items.Add(entry);
        }

        entry.count++;
        Changed?.Invoke();
    }

    public bool Consume(ItemData item)
    {
        Entry entry = items.Find(current => current.item == item);

        if (entry == null || entry.count <= 0)
            return false;

        entry.count--;

        if (entry.count == 0)
            items.Remove(entry);

        Changed?.Invoke();
        return true;
    }

    public bool Contains(ItemData item)
    {
        Entry entry = items.Find(current => current.item == item);
        return entry != null && entry.count > 0;
    }
}
