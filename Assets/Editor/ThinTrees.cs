using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ThinTrees : EditorWindow
{
    Terrain terrain;
    int protoIndex = 0;
    float minDistance = 3f;
    bool allTypes = false;
    int seed = 1;

    [MenuItem("Tools/Underwater/Thin Trees")]
    static void Open()
    {
        var w = GetWindow<ThinTrees>("Thin Trees");
        w.minSize = new Vector2(320, 190);
    }

    void OnGUI()
    {
        if (terrain == null && Selection.activeGameObject != null)
            terrain = Selection.activeGameObject.GetComponent<Terrain>();

        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
        if (terrain == null)
        {
            EditorGUILayout.HelpBox("Pilih objek Terrain di Hierarchy atau seret ke sini.", MessageType.Info);
            return;
        }

        var protos = terrain.terrainData.treePrototypes;
        if (protos.Length == 0)
        {
            EditorGUILayout.HelpBox("Terrain ini belum punya tree.", MessageType.Warning);
            return;
        }

        var names = new string[protos.Length];
        for (int i = 0; i < protos.Length; i++)
            names[i] = i + ": " + (protos[i].prefab != null ? protos[i].prefab.name : "(kosong)");

        allTypes = EditorGUILayout.Toggle("Semua jenis tree", allTypes);
        using (new EditorGUI.DisabledScope(allTypes))
            protoIndex = Mathf.Clamp(EditorGUILayout.Popup("Jenis tree", protoIndex, names), 0, protos.Length - 1);

        minDistance = EditorGUILayout.FloatField("Jarak minimal (unit)", minDistance);
        seed = EditorGUILayout.IntField("Seed acak", seed);

        EditorGUILayout.HelpBox(
            "Tree yang jaraknya kurang dari 'Jarak minimal' ke tree lain akan dihapus. " +
            "Bisa di-Undo (Ctrl+Z). Coba mulai dari 2-4 unit.", MessageType.None);

        if (GUILayout.Button("Jarangkan"))
            Thin(protos.Length);
    }

    void Thin(int protoCount)
    {
        var td = terrain.terrainData;
        var instances = td.treeInstances;
        Vector3 size = td.size;
        float cell = Mathf.Max(0.01f, minDistance);

        var rng = new System.Random(seed);
        var order = new List<int>();
        var keepOthers = new List<TreeInstance>();

        for (int i = 0; i < instances.Length; i++)
        {
            if (allTypes || instances[i].prototypeIndex == protoIndex) order.Add(i);
            else keepOthers.Add(instances[i]);
        }

        // acak urutan supaya yang dihapus tidak selalu di sisi yang sama
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            int t = order[i]; order[i] = order[j]; order[j] = t;
        }

        var grid = new Dictionary<long, List<Vector2>>();
        var kept = new List<TreeInstance>();

        foreach (int idx in order)
        {
            var ti = instances[idx];
            Vector2 p = new Vector2(ti.position.x * size.x, ti.position.z * size.z);
            int cx = Mathf.FloorToInt(p.x / cell), cz = Mathf.FloorToInt(p.y / cell);

            bool tooClose = false;
            for (int dx = -1; dx <= 1 && !tooClose; dx++)
            for (int dz = -1; dz <= 1 && !tooClose; dz++)
            {
                if (grid.TryGetValue(Key(cx + dx, cz + dz), out var list))
                    foreach (var q in list)
                        if ((q - p).sqrMagnitude < minDistance * minDistance) { tooClose = true; break; }
            }
            if (tooClose) continue;

            long k = Key(cx, cz);
            if (!grid.TryGetValue(k, out var l)) { l = new List<Vector2>(); grid[k] = l; }
            l.Add(p);
            kept.Add(ti);
        }

        // tree jenis lain ikut dihitung sebagai penghalang hanya jika "Semua jenis" aktif
        int before = instances.Length;
        var result = new List<TreeInstance>(keepOthers);
        result.AddRange(kept);

        Undo.RegisterCompleteObjectUndo(td, "Thin Trees");
        td.SetTreeInstances(result.ToArray(), true);
        EditorUtility.SetDirty(td);

        Debug.Log($"Thin Trees: {before} -> {result.Count} tree (dihapus {before - result.Count}).");
    }

    static long Key(int x, int z) { return ((long)x << 32) ^ (uint)z; }
}