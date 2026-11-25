using System;
using System.Collections.Generic;
using UnityEngine;

// Tempelkan script ini ke objek Player.
public class Inventory : MonoBehaviour
{
    [Tooltip("Total slot inventaris")]
    public int maxSlots = 5;

    [Tooltip("Jumlah koin yang dimiliki pemain")]
    public int coins = 0;

    [Serializable]
    public class Entry
    {
        public string name;
        public int size;
        public int value;
        public Sprite icon;
    }

    // Daftar sampah yang sedang dibawa (bisa dilihat di Inspector saat Play)
    public List<Entry> items = new List<Entry>();

    // Dipanggil setiap kali isi inventaris atau koin berubah (dipakai oleh UI)
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

    // Total koin yang akan didapat kalau semua sampah yang dibawa dijual
    public int TotalValue
    {
        get
        {
            int total = 0;
            foreach (var e in items) total += e.value;
            return total;
        }
    }

    public bool CanAdd(int size)
    {
        return UsedSlots + size <= maxSlots;
    }

    public bool TryAdd(TrashItem item)
    {
        if (!CanAdd(item.slotSize)) return false;

        items.Add(new Entry
        {
            name = item.itemName,
            size = item.slotSize,
            value = item.value,
            icon = item.icon
        });
        if (OnChanged != null) OnChanged();
        return true;
    }

    // Jual semua sampah: koin bertambah, slot kembali kosong. Mengembalikan koin yang didapat.
    public int SellAll()
    {
        int earned = TotalValue;
        coins += earned;
        items.Clear();
        if (OnChanged != null) OnChanged();
        return earned;
    }

    // Kosongkan inventaris tanpa menambah koin
    public void Clear()
    {
        items.Clear();
        if (OnChanged != null) OnChanged();
    }
}