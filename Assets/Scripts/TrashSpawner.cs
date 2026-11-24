using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Tempelkan ke objek kosong "TrashSpawner" yang diletakkan di tengah area penyebaran.
// Sampah disebar di Editor (klik tombol di Inspector), jadi hasilnya tersimpan di scene
// dan bisa kamu geser atau hapus satu per satu kalau ada yang kurang pas.
public class TrashSpawner : MonoBehaviour
{
    [Header("Wajib diisi")]
    public Terrain terrain;

    [Tooltip("Isi dengan prefab sampah. Prefab yang dimasukkan 2x akan lebih sering muncul.")]
    public GameObject[] prefabs;

    [Header("Pengaturan sebaran")]
    public int count = 30;

    [Tooltip("Ukuran area (X dan Z) dengan pusat di posisi objek ini")]
    public Vector2 areaSize = new Vector2(100f, 100f);

    [Tooltip("Jarak minimal antar sampah")]
    public float minDistance = 3f;

    [Tooltip("Geser tinggi sampah dari permukaan tanah (negatif = lebih tenggelam)")]
    public float heightOffset = 0f;

    public bool randomRotationY = true;

    [Tooltip("Variasi ukuran acak (1 = tidak diubah)")]
    public Vector2 randomScale = new Vector2(1f, 1f);

    [Header("Hasil")]
    [Tooltip("Sampah hasil sebaran dimasukkan ke objek ini. Kosongkan untuk memakai objek ini sendiri.")]
    public Transform container;

    public Transform Container { get { return container != null ? container : transform; } }

#if UNITY_EDITOR
    public void Spawn()
    {
        if (terrain == null)
        {
            Debug.LogError("TrashSpawner: kolom Terrain belum diisi.");
            return;
        }
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogError("TrashSpawner: daftar Prefabs masih kosong.");
            return;
        }

        var placed = new List<Vector3>();
        Vector3 center = transform.position;
        int spawned = 0;

        for (int i = 0; i < count; i++)
        {
            bool found = false;
            Vector3 pos = Vector3.zero;

            for (int attempt = 0; attempt < 30 && !found; attempt++)
            {
                float x = center.x + Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
                float z = center.z + Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);

                Vector3 p = new Vector3(x, 0f, z);
                p.y = terrain.SampleHeight(p) + terrain.transform.position.y + heightOffset;

                bool tooClose = false;
                foreach (var q in placed)
                {
                    if ((new Vector2(q.x, q.z) - new Vector2(p.x, p.z)).sqrMagnitude < minDistance * minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (!tooClose) { pos = p; found = true; }
            }

            if (!found) continue;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            if (prefab == null) continue;

            GameObject go;
            if (PrefabUtility.IsPartOfPrefabAsset(prefab))
                go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, Container);
            else
                go = Instantiate(prefab, Container);

            go.transform.position = pos;
            if (randomRotationY)
                go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            float s = Random.Range(randomScale.x, randomScale.y);
            go.transform.localScale *= s;

            Undo.RegisterCreatedObjectUndo(go, "Spawn Sampah");
            placed.Add(pos);
            spawned++;
        }

        Debug.Log($"TrashSpawner: {spawned} dari {count} sampah berhasil disebar.");
    }

    public void ClearSpawned()
    {
        Transform c = Container;
        for (int i = c.childCount - 1; i >= 0; i--)
        {
            // Hanya hapus child yang punya TrashItem, supaya objek lain aman
            if (c.GetChild(i).GetComponent<TrashItem>() != null)
                Undo.DestroyObjectImmediate(c.GetChild(i).gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 1f);
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, 2f, areaSize.y));
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(TrashSpawner))]
public class TrashSpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        var spawner = (TrashSpawner)target;

        if (GUILayout.Button("Sebar Sampah", GUILayout.Height(30)))
            spawner.Spawn();

        if (GUILayout.Button("Hapus Sampah Hasil Sebar"))
            spawner.ClearSpawned();
    }
}
#endif