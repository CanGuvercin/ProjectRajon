// Dosya konumu: Assets/Editor/TilemapLayerSwitcher.cs
using UnityEditor;
using UnityEngine;

public static class TilemapLayerSwitcher
{
    // Hierarchy'deki Tilemap GameObject isimlerini buraya yaz
    // Bu isimler ne ise ALT+tuş o isimli objeyi seçer
    private static readonly string[] LayerNames =
    {
        "Background",    // ALT + 1
        "Facade",        // ALT + 2
        "Ground",        // ALT + 3
        "GroundDetail",  // ALT + 4
        "Shadow",        // ALT + 5
        "BackProps",     // ALT + 6
        "FrontProps",    // ALT + 7
        "Collision",     // ALT + 8
    };

    [MenuItem("Rajon/Layers/Background      ALT+1  &1")]
    static void Select1() => SelectLayer(0);

    [MenuItem("Rajon/Layers/Facade          ALT+2  &2")]
    static void Select2() => SelectLayer(1);

    [MenuItem("Rajon/Layers/Ground          ALT+3  &3")]
    static void Select3() => SelectLayer(2);

    [MenuItem("Rajon/Layers/GroundDetail    ALT+4  &4")]
    static void Select4() => SelectLayer(3);

    [MenuItem("Rajon/Layers/Shadow          ALT+5  &5")]
    static void Select5() => SelectLayer(4);

    [MenuItem("Rajon/Layers/BackProps       ALT+6  &6")]
    static void Select6() => SelectLayer(5);

    [MenuItem("Rajon/Layers/FrontProps      ALT+7  &7")]
    static void Select7() => SelectLayer(6);

    [MenuItem("Rajon/Layers/Collision       ALT+8  &8")]
    static void Select8() => SelectLayer(7);

    // ---------------------------------------------------

    private static void SelectLayer(int index)
    {
        if (index < 0 || index >= LayerNames.Length) return;

        string targetName = LayerNames[index];

        // Tüm sahnede bu isimde bir obje ara (deaktif olanlar dahil)
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        GameObject target = null;

        foreach (var go in allObjects)
        {
            if (go.scene.isLoaded && go.name == targetName)
            {
                target = go;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning($"[RajonLayers] '{targetName}' bulunamadı. " +
                             $"Hierarchy'de bu isimde bir Tilemap objesi olmalı.");
            return;
        }

        Selection.activeGameObject = target;
        EditorGUIUtility.PingObject(target);

        // Scene view'e focus at ki çizmeye hemen başlayabilesin
        if (SceneView.lastActiveSceneView != null)
            SceneView.lastActiveSceneView.Focus();

        Debug.Log($"[RajonLayers] Aktif katman → <b>{targetName}</b>");
    }
}