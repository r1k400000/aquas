using TMPro;
using UnityEngine;

// Tempelkan script ini ke objek "CoinPanel" di Canvas (pojok kiri atas).
public class CoinDisplay : MonoBehaviour
{
    public Inventory inventory;

    [Tooltip("Teks yang menampilkan jumlah koin")]
    public TMP_Text coinText;

    [Tooltip("Tulisan sebelum angka. Kosongkan kalau sudah ada gambar koin di sebelahnya.")]
    public string prefix = "";

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
        if (inventory == null || coinText == null) return;
        coinText.text = prefix + inventory.coins;
    }
}