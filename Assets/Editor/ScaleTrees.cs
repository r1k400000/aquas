using UnityEngine;
using UnityEditor;

public class ScaleTrees : EditorWindow
{
    Terrain terrain;
    int protoIndex = 0;
    bool allTypes = false;
    float multiplier = 2f;
    bool keepWidthRatio = true;

    [MenuItem("Tools/Underwater/Scale Trees")]
    static void Open()
    {
        var w = GetWindow<ScaleTrees>("Scale Trees");
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

        multiplier = EditorGUILayout.FloatField("Pengali ukuran", multiplier);
        keepWidthRatio = EditorGUILayout.Toggle("Lebar ikut dikali", keepWidthRatio);

        EditorGUILayout.HelpBox(
            "Ukuran tiap tree yang sudah dilukis dikali angka ini (1 = tetap, 2 = dua kali, 0.5 = setengah). " +
            "Bisa diulang dan bisa di-Undo (Ctrl+Z).", MessageType.None);

        if (GUILayout.Button("Terapkan"))
            Apply();
    }

    void Apply()
    {
        var td = terrain.terrainData;
        var instances = td.treeInstances;
        int count = 0;

        for (int i = 0; i < instances.Length; i++)
        {
            if (!allTypes && instances[i].prototypeIndex != protoIndex) continue;
            instances[i].heightScale *= multiplier;
            if (keepWidthRatio) instances[i].widthScale *= multiplier;
            count++;
        }

        Undo.RegisterCompleteObjectUndo(td, "Scale Trees");
        td.SetTreeInstances(instances, true);
        EditorUtility.SetDirty(td);
        Debug.Log($"Scale Trees: {count} tree dikali {multiplier}.");
    }
}