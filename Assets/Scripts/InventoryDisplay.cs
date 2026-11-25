using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tempelkan script ini ke objek "InventoryPanel" di Canvas.
public class InventoryDisplay : MonoBehaviour
{
    public Inventory inventory;

    [Tooltip("Teks yang menampilkan 'Slot: 2/5'")]
    public TMP_Text slotText;

    [Tooltip("Isi dengan 5 Image kotak slot, urut dari kiri ke kanan")]
    public Image[] slotImages;

    public Color emptyColor = new Color(1f, 1f, 1f, 0.25f);

    [Tooltip("Warna kotak kalau sampahnya tidak punya gambar (icon)")]
    public Color filledColor = new Color(0.3f, 0.9f, 0.4f, 1f);

    [Tooltip("Sampah 2-3 slot: tampilkan gambar di semua slot yang dipakai (kalau mati, hanya di slot pertama)")]
    public bool showIconInAllSlots = true;

    Sprite[] defaultSprites;

    void Awake()
    {
        // Simpan gambar bawaan tiap kotak supaya bisa dikembalikan saat slot kosong
        defaultSprites = new Sprite[slotImages.Length];
        for (int i = 0; i < slotImages.Length; i++)
            if (slotImages[i] != null) defaultSprites[i] = slotImages[i].sprite;
    }

    void OnEnable()
    {
        if (inventory != null) inventory.OnChanged += Refresh;
    }

    void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    void Start()
    {
        Refresh();
    }

    void Refresh()
    {
        if (inventory == null) return;

        int slot = 0;

        // Isi slot sesuai urutan sampah yang diambil
        foreach (var entry in inventory.items)
        {
            for (int k = 0; k < entry.size && slot < slotImages.Length; k++, slot++)
            {
                bool useIcon = entry.icon != null && (k == 0 || showIconInAllSlots);
                ApplyFilled(slot, useIcon ? entry.icon : null);
            }
        }

        // Sisanya kosong
        for (; slot < slotImages.Length; slot++)
            ApplyEmpty(slot);

        if (slotText != null)
            slotText.text = "Slot: " + inventory.UsedSlots + "/" + inventory.maxSlots;
    }

    void ApplyFilled(int index, Sprite icon)
    {
        Image img = slotImages[index];
        if (img == null) return;

        if (icon != null)
        {
            img.sprite = icon;
            img.color = Color.white;
            img.preserveAspect = true;
        }
        else
        {
            img.sprite = defaultSprites[index];
            img.color = filledColor;
            img.preserveAspect = false;
        }
    }

    void ApplyEmpty(int index)
    {
        Image img = slotImages[index];
        if (img == null) return;

        img.sprite = defaultSprites[index];
        img.color = emptyColor;
        img.preserveAspect = false;
    }
}