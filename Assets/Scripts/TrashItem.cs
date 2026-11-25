using UnityEngine;

public class TrashItem : MonoBehaviour
{
    [Tooltip("Nama sampah yang tampil di teks 'Tekan E'")]
    public string itemName = "Sampah";

    [Tooltip("Berapa slot yang dipakai sampah ini (1 sampai 5)")]
    [Range(1, 5)]
    public int slotSize = 1;

    [Tooltip("Berapa koin sampah ini kalau dijual ke Bank Sampah")]
    public int value = 1;

    [Tooltip("Gambar ikon yang tampil di kotak slot")]
    public Sprite icon;
}