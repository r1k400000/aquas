using UnityEngine;

public class TrashItem : MonoBehaviour
{
    [Tooltip("Nama sampah yang tampil di teks 'Tekan E'")]
    public string itemName = "Sampah";

    [Tooltip("Berapa slot yang dipakai sampah ini (1 sampai 5)")]
    [Range(1, 5)]
    public int slotSize = 1;
}