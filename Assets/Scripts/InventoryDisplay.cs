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

    [Tooltip("Centang: ikon muncul di semua kotak yang dipakai (galon 2 kotak, ban 3 kotak). Kosongkan: hanya di kotak pertama.")]
    public bool showIconOnAllSlots = true;

    public Color emptyColor = new Color(1f, 1f, 1f, 0.25f);
    public Color filledColor = new Color(0.3f, 0.9f, 0.4f, 1f);

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

        int index = 0;

        // Isi kotak sesuai sampah yang diambil
        foreach (var entry in inventory.items)
        {
            for (int k = 0; k < entry.size; k++)
            {
                if (index >= slotImages.Length) break;

                bool showIcon = showIconOnAllSlots || k == 0;
                SetFilled(slotImages[index], entry.icon, showIcon);
                index++;
            }
        }

        // Sisa kotak dikosongkan
        for (; index < slotImages.Length; index++)
        {
            SetEmpty(slotImages[index]);
        }

        if (slotText != null)
            slotText.text = "Slot: " + inventory.UsedSlots + "/" + inventory.maxSlots;
    }

    void SetFilled(Image img, Sprite icon, bool showIcon)
    {
        if (img == null) return;

        if (showIcon && icon != null)
        {
            img.sprite = icon;
            img.color = Color.white;      // warna asli gambar
            img.preserveAspect = true;    // gambar tidak gepeng
        }
        else
        {
            // Tidak ada ikon: tetap hijau seperti sebelumnya
            img.sprite = null;
            img.color = filledColor;
        }
    }

    void SetEmpty(Image img)
    {
        if (img == null) return;

        img.sprite = null;
        img.color = emptyColor;
    }
}