using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Tempelkan script ini ke objek Player (bersama script Inventory).
// Satu tombol E dipakai untuk: mengambil sampah, dan menjual sampah di Bank Sampah.
public class TrashPickup : MonoBehaviour
{
    [Header("Referensi")]
    public Inventory inventory;
    public TMP_Text promptText;

    [Header("Pengaturan")]
    [Tooltip("Jarak maksimal player bisa berinteraksi")]
    public float pickupRadius = 2f;

    [Tooltip("Titik tengah deteksi, digeser ke atas dari kaki player")]
    public Vector3 centerOffset = new Vector3(0f, 1f, 0f);

    [Tooltip("Lama pesan hasil (misalnya 'Terjual!') tampil, dalam detik")]
    public float messageDuration = 2f;

    TrashItem currentTrash;
    TrashBank currentBank;
    string message = "";
    float messageTimer;

    void Awake()
    {
        if (inventory == null) inventory = GetComponent<Inventory>();
    }

    void Update()
    {
        FindNearest();

        if (messageTimer > 0f) messageTimer -= Time.deltaTime;
        UpdatePrompt();

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentBank != null) Sell();
            else if (currentTrash != null) TryPickup(currentTrash);
        }
    }

    // Mencari sampah atau bank yang paling dekat
    void FindNearest()
    {
        currentTrash = null;
        currentBank = null;

        Vector3 center = transform.position + centerOffset;
        Collider[] hits = Physics.OverlapSphere(center, pickupRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);

        float best = float.MaxValue;
        foreach (var hit in hits)
        {
            // Jarak dihitung ke tepi collider, supaya objek besar tetap terdeteksi
            float d = (hit.bounds.ClosestPoint(center) - center).sqrMagnitude;
            if (d >= best) continue;

            TrashBank bank = hit.GetComponentInParent<TrashBank>();
            if (bank != null)
            {
                best = d;
                currentBank = bank;
                currentTrash = null;
                continue;
            }

            TrashItem t = hit.GetComponentInParent<TrashItem>();
            if (t != null && t.gameObject.activeInHierarchy)
            {
                best = d;
                currentTrash = t;
                currentBank = null;
            }
        }
    }

    void UpdatePrompt()
    {
        if (promptText == null) return;

        if (messageTimer > 0f)
        {
            promptText.text = message;
            return;
        }

        if (currentBank != null)
        {
            if (inventory.items.Count == 0)
                promptText.text = currentBank.bankName + ": belum ada sampah untuk dijual";
            else
                promptText.text = "Tekan E untuk jual sampah di " + currentBank.bankName + " (+" + inventory.TotalValue + " koin)";
        }
        else if (currentTrash != null)
        {
            string info = currentTrash.itemName + " (" + currentTrash.slotSize + " slot, " + currentTrash.value + " koin)";
            if (inventory.CanAdd(currentTrash.slotSize))
                promptText.text = "Tekan E untuk ambil " + info;
            else
                promptText.text = "Slot tidak cukup! " + currentTrash.itemName + " butuh " + currentTrash.slotSize + " slot";
        }
        else
        {
            promptText.text = "";
        }
    }

    void TryPickup(TrashItem item)
    {
        if (inventory.TryAdd(item))
        {
            item.gameObject.SetActive(false);
        }
    }

    void Sell()
    {
        if (inventory.items.Count == 0)
        {
            ShowMessage("Tidak ada sampah untuk dijual");
            return;
        }

        int earned = inventory.SellAll();
        ShowMessage("Terjual! +" + earned + " koin");
    }

    void ShowMessage(string text)
    {
        message = text;
        messageTimer = messageDuration;
    }

    // Menampilkan lingkaran jangkauan di Scene View saat Player dipilih
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + centerOffset, pickupRadius);
    }
}