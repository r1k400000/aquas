using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Letakkan di folder Assets/Editor
// Cara pakai: pilih prefab sampah di Project window (boleh banyak sekaligus),
// lalu jalankan menu Tools > Underwater > Setup Trash Items.
// Script ini mengisi nama, jumlah slot, dan nilai koin berdasarkan NAMA prefab,
// dan menambahkan komponen TrashItem kalau belum ada.
public static class SetupTrashItems
{
    struct Info
    {
        public string displayName;
        public int slot;
        public int value;
        public Info(string n, int s, int v) { displayName = n; slot = s; value = v; }
    }

    // Kunci = nama prefab, huruf kecil, tanpa spasi
    static readonly Dictionary<string, Info> table = new Dictionary<string, Info>
    {
        { "kresek",       new Info("Kantong Plastik", 1, 1) },
        { "botolplastik", new Info("Botol Plastik",   1, 2) },
        { "kaleng",       new Info("Kaleng",          1, 3) },
        { "galon",        new Info("Galon",           2, 4) },
        { "ban",          new Info("Ban Bekas",       3, 6) },
        { "botolkaca",    new Info("Botol Kaca",      1, 2) },
        { "jaring",       new Info("Jaring",          1, 2) },
        { "jerigen",      new Info("Jerigen",         1, 2) },
        { "sterofom",     new Info("Sterofom",        1, 2) },
        { "wadah",        new Info("Wadah",           1, 2) },
    };

    [MenuItem("Tools/Underwater/Setup Trash Items")]
    static void Run()
    {
        var selected = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        if (selected.Length == 0)
        {
            Debug.LogError("Pilih prefab sampah di Project window terlebih dahulu.");
            return;
        }

        int done = 0;
        foreach (var obj in selected)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!path.EndsWith(".prefab")) continue;

            string key = obj.name.Replace(" ", "").ToLower();
            if (!table.TryGetValue(key, out Info info))
            {
                Debug.LogWarning("Nama prefab tidak dikenali, dilewati: " + obj.name);
                continue;
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            var item = root.GetComponent<TrashItem>();
            if (item == null) item = root.AddComponent<TrashItem>();

            item.itemName = info.displayName;
            item.slotSize = info.slot;
            item.value = info.value;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);

            Debug.Log($"{obj.name}: {info.displayName}, {info.slot} slot, {info.value} koin");
            done++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Selesai. {done} prefab diatur.");
    }
}