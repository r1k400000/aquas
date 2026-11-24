using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class FixGroupPivot
{
    const string OutDir = "Assets/TerrainDetails/PivotFixed";

    // Dasar ke Y = 0, X/Z dipusatkan
    [MenuItem("Tools/Fix Group Prefab Pivot/Bottom Center")]
    static void BottomCenter() { Run(true, 0f); }

    // Dasar dibenamkan 10% / 20% dari tinggi, supaya tidak melayang di lereng
    [MenuItem("Tools/Fix Group Prefab Pivot/Bottom Center + Sink 10%")]
    static void BottomCenterSink10() { Run(true, 0.10f); }

    [MenuItem("Tools/Fix Group Prefab Pivot/Bottom Center + Sink 20%")]
    static void BottomCenterSink20() { Run(true, 0.20f); }

    // Hanya dasar ke Y = 0, X/Z tidak diubah
    [MenuItem("Tools/Fix Group Prefab Pivot/Bottom Only (Y)")]
    static void BottomOnly() { Run(false, 0f); }

    static void Run(bool centerXZ, float sink)
    {
        if (!AssetDatabase.IsValidFolder("Assets/TerrainDetails"))
            AssetDatabase.CreateFolder("Assets", "TerrainDetails");
        if (!AssetDatabase.IsValidFolder(OutDir))
            AssetDatabase.CreateFolder("Assets/TerrainDetails", "PivotFixed");

        var selected = Selection.GetFiltered<GameObject>(SelectionMode.Assets);
        if (selected.Length == 0)
        {
            Debug.LogError("Pilih prefab _Group di Project terlebih dahulu!");
            return;
        }

        int done = 0;
        foreach (var obj in selected)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!path.EndsWith(".prefab")) continue;

            var root = PrefabUtility.LoadPrefabContents(path);
            // Kelompokkan bagian mesh berdasarkan material yang sama
            var groups = new Dictionary<Material, List<CombineInstance>>();
            var order = new List<Material>();
            Matrix4x4 toRoot = root.transform.worldToLocalMatrix;

            foreach (var mf in root.GetComponentsInChildren<MeshFilter>())
            {
                var mr = mf.GetComponent<MeshRenderer>();
                if (mf.sharedMesh == null || mr == null) continue;

                var sharedMats = mr.sharedMaterials;
                for (int i = 0; i < mf.sharedMesh.subMeshCount; i++)
                {
                    Material m = i < sharedMats.Length ? sharedMats[i] : null;
                    if (m == null) continue;
                    if (!groups.ContainsKey(m))
                    {
                        groups[m] = new List<CombineInstance>();
                        order.Add(m);
                    }
                    groups[m].Add(new CombineInstance
                    {
                        mesh = mf.sharedMesh,
                        subMeshIndex = i,
                        transform = toRoot * mf.transform.localToWorldMatrix
                    });
                }
            }
            PrefabUtility.UnloadPrefabContents(root);

            if (order.Count == 0)
            {
                Debug.LogWarning("Tidak ada mesh di: " + path);
                continue;
            }

            if (order.Count > 1)
                Debug.LogWarning($"{path} memakai {order.Count} material berbeda. " +
                    "Detail Mesh Terrain hanya boleh 1 material, jadi prefab ini tidak bisa dipakai di Paint Details. " +
                    "Pakai Paint Trees untuk prefab ini.");

            // Gabungkan tiap kelompok material menjadi 1 submesh
            var finalCombines = new List<CombineInstance>();
            foreach (var m in order)
            {
                var part = new Mesh();
                part.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                part.CombineMeshes(groups[m].ToArray(), true, true);
                finalCombines.Add(new CombineInstance { mesh = part, subMeshIndex = 0, transform = Matrix4x4.identity });
            }

            var mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.CombineMeshes(finalCombines.ToArray(), false, false);

            Bounds b = mesh.bounds;
            Vector3 offset = new Vector3(centerXZ ? b.center.x : 0f, b.min.y + b.size.y * sink, centerXZ ? b.center.z : 0f);
            var verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++) verts[i] -= offset;
            mesh.vertices = verts;
            mesh.RecalculateBounds();

            string name = Path.GetFileNameWithoutExtension(path);
            if (sink > 0f) name += "_Sink" + Mathf.RoundToInt(sink * 100);
            mesh.name = name + "_PivotFixed";

            string meshPath = AssetDatabase.GenerateUniqueAssetPath($"{OutDir}/{name}_PivotFixed.asset");
            AssetDatabase.CreateAsset(mesh, meshPath);

            var go = new GameObject(name + "_PivotFixed");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterials = order.ToArray();

            string prefabPath = AssetDatabase.GenerateUniqueAssetPath($"{OutDir}/{name}_PivotFixed.prefab");
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            Debug.Log($"Pivot diperbaiki ({order.Count} material): {prefabPath}");
            done++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Selesai. {done} prefab diproses. Hasil ada di {OutDir}");
    }
}