using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Tempelkan script ini ke objek Player (bersama script Inventory).
public class PlayerPickup : MonoBehaviour
{
    [Header("Referensi")]
    public Inventory inventory;
    public TMP_Text promptText;

    [Header("Pengaturan")]
    [Tooltip("Jarak maksimal player bisa mengambil sampah")]
    public float pickupRadius = 2f;

    [Tooltip("Titik tengah deteksi, digeser ke atas dari kaki player")]
    public Vector3 centerOffset = new Vector3(0f, 1f, 0f);

    TrashItem current;

    void Awake()
    {
        if (inventory == null) inventory = GetComponent<Inventory>();
    }

    void Update()
    {
        current = FindNearestTrash();
        UpdatePrompt();

        if (current != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickup(current);
        }
    }

    TrashItem FindNearestTrash()
    {
        Vector3 center = transform.position + centerOffset;
        Collider[] hits = Physics.OverlapSphere(center, pickupRadius);

        TrashItem best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            TrashItem t = hit.GetComponentInParent<TrashItem>();
            if (t == null || !t.gameObject.activeInHierarchy) continue;

            float d = (t.transform.position - center).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }
        return best;
    }

    void UpdatePrompt()
    {
        if (promptText == null) return;

        if (current == null)
        {
            promptText.text = "";
            return;
        }

        if (inventory.CanAdd(current.slotSize))
            promptText.text = "Tekan E untuk ambil " + current.itemName + " (" + current.slotSize + " slot)";
        else
            promptText.text = "Slot tidak cukup! " + current.itemName + " butuh " + current.slotSize + " slot";
    }

    void TryPickup(TrashItem item)
    {
        if (inventory.TryAdd(item))
        {
            item.gameObject.SetActive(false);
        }
    }

    // Menampilkan lingkaran jangkauan di Scene View saat Player dipilih
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + centerOffset, pickupRadius);
    }
}