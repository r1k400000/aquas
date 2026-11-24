using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Tooltip("Total slot inventaris")]
    public int maxSlots = 5;

    [Serializable]
    public class Entry
    {
        public string name;
        public int size;
        public Sprite icon;
    }

    // Daftar sampah yang sudah diambil (bisa dilihat di Inspector saat Play)
    public List<Entry> items = new List<Entry>();

    // Dipanggil setiap kali isi inventaris berubah (dipakai oleh UI)
    public event Action OnChanged;

    public int UsedSlots
    {
        get
        {
            int total = 0;
            foreach (var e in items) total += e.size;
            return total;
        }
    }

    public int FreeSlots { get { return maxSlots - UsedSlots; } }

    public bool CanAdd(int size)
    {
        return UsedSlots + size <= maxSlots;
    }

    public bool TryAdd(TrashItem item)
    {
        if (!CanAdd(item.slotSize)) return false;

        items.Add(new Entry { name = item.itemName, size = item.slotSize, icon = item.icon });
        if (OnChanged != null) OnChanged();
        return true;
    }

    // Kosongkan inventaris (nanti bisa dipanggil saat membuang sampah ke tempat sampah)
    public void Clear()
    {
        items.Clear();
        if (OnChanged != null) OnChanged();
    }
}