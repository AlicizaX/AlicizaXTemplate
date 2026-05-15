using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class TmpFontAssetDynamicCacheCleaner
{
    private const string ChunbaiFontAssetPath = "Assets/Art/Font/CHUNBAI SDF.asset";
    private const string TmpFontAssetTypeName = "TMPro.TMP_FontAsset";

    [MenuItem("Tools/Font/TMP/Set CHUNBAI Dynamic And Clear Cache", priority = 2100)]
    private static void SetChunbaiDynamicAndClearCache()
    {
        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ChunbaiFontAssetPath);
        if (asset == null)
        {
            Debug.LogError($"TMP font asset not found: {ChunbaiFontAssetPath}");
            return;
        }

        ProcessFontAssets(new[] { asset });
    }

    [MenuItem("Tools/Font/TMP/Set Selected Dynamic And Clear Cache", priority = 2101)]
    private static void SetSelectedDynamicAndClearCache()
    {
        ProcessFontAssets(Selection.objects);
    }

    [MenuItem("Tools/Font/TMP/Set Selected Dynamic And Clear Cache", true)]
    private static bool ValidateSetSelectedDynamicAndClearCache()
    {
        return CollectFontAssets(Selection.objects).Count > 0;
    }

    [MenuItem("Assets/TMP Font Asset/Set Dynamic And Clear Cache", priority = 2100)]
    private static void SetSelectedAssetsDynamicAndClearCache()
    {
        ProcessFontAssets(Selection.objects);
    }

    [MenuItem("Assets/TMP Font Asset/Set Dynamic And Clear Cache", true)]
    private static bool ValidateSetSelectedAssetsDynamicAndClearCache()
    {
        return CollectFontAssets(Selection.objects).Count > 0;
    }

    private static void ProcessFontAssets(UnityEngine.Object[] selectedObjects)
    {
        var fontAssets = CollectFontAssets(selectedObjects);
        if (fontAssets.Count == 0)
        {
            Debug.LogWarning("No TMP font assets selected.");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Set TMP Font Dynamic",
                $"Set {fontAssets.Count} TMP font asset(s) to Dynamic and clear stored glyph/atlas cache?\n\nThis will modify the asset files on disk.",
                "Process",
                "Cancel"))
        {
            return;
        }

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var fontAsset in fontAssets)
            {
                ProcessFontAsset(fontAsset);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        var paths = new List<string>(fontAssets.Count);
        foreach (var fontAsset in fontAssets)
        {
            paths.Add(AssetDatabase.GetAssetPath(fontAsset));
        }

        AssetDatabase.ForceReserializeAssets(paths);
        Debug.Log($"Processed {fontAssets.Count} TMP font asset(s).");
    }

    private static void ProcessFontAsset(UnityEngine.Object fontAsset)
    {
        var path = AssetDatabase.GetAssetPath(fontAsset);
        var beforeSize = GetAssetFileSize(path);

        Undo.RegisterCompleteObjectUndo(fontAsset, "Set TMP Font Dynamic And Clear Cache");

        SetAtlasPopulationModeToDynamic(fontAsset);
        EnsureRuntimeSourceFontReference(fontAsset);
        ClearFontAssetData(fontAsset);
        ClearSerializedDynamicData(fontAsset);
        RemoveUnusedAtlasTextureSubAssets(path, fontAsset);

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssetIfDirty(fontAsset);

        var afterSize = GetAssetFileSize(path);
        Debug.Log(
            $"TMP font asset processed: {path}\n" +
            $"Mode: Dynamic, static glyph data cleared.\n" +
            $"File size before save: {FormatBytes(beforeSize)}, after save: {FormatBytes(afterSize)}",
            fontAsset);
    }

    private static List<UnityEngine.Object> CollectFontAssets(UnityEngine.Object[] selectedObjects)
    {
        var result = new List<UnityEngine.Object>();
        var seenPaths = new HashSet<string>();

        if (selectedObjects == null)
        {
            return result;
        }

        foreach (var selectedObject in selectedObjects)
        {
            if (selectedObject == null)
            {
                continue;
            }

            var path = AssetDatabase.GetAssetPath(selectedObject);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            if (Directory.Exists(path))
            {
                foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { path }))
                {
                    AddFontAssetByPath(AssetDatabase.GUIDToAssetPath(guid), result, seenPaths);
                }
            }
            else
            {
                AddFontAssetByPath(path, result, seenPaths);
            }
        }

        return result;
    }

    private static void AddFontAssetByPath(string path, List<UnityEngine.Object> result, HashSet<string> seenPaths)
    {
        if (string.IsNullOrEmpty(path) || !seenPaths.Add(path))
        {
            return;
        }

        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
        if (IsTmpFontAsset(asset))
        {
            result.Add(asset);
        }
    }

    private static bool IsTmpFontAsset(UnityEngine.Object asset)
    {
        return asset != null && asset.GetType().FullName == TmpFontAssetTypeName;
    }

    private static void SetAtlasPopulationModeToDynamic(UnityEngine.Object fontAsset)
    {
        var property = fontAsset.GetType().GetProperty("atlasPopulationMode", BindingFlags.Instance | BindingFlags.Public);
        if (property != null && property.CanWrite)
        {
            var dynamicValue = Enum.ToObject(property.PropertyType, 1);
            property.SetValue(fontAsset, dynamicValue);
            return;
        }

        var serializedObject = new SerializedObject(fontAsset);
        var atlasPopulationMode = serializedObject.FindProperty("m_AtlasPopulationMode");
        if (atlasPopulationMode != null)
        {
            atlasPopulationMode.enumValueIndex = 1;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void EnsureRuntimeSourceFontReference(UnityEngine.Object fontAsset)
    {
        var serializedObject = new SerializedObject(fontAsset);
        var editorRef = serializedObject.FindProperty("m_SourceFontFile_EditorRef");
        var runtimeRef = serializedObject.FindProperty("m_SourceFontFile");

        if (editorRef == null || runtimeRef == null)
        {
            Debug.LogWarning($"Unable to find source font fields on {AssetDatabase.GetAssetPath(fontAsset)}.", fontAsset);
            return;
        }

        if (editorRef.objectReferenceValue == null)
        {
            Debug.LogWarning(
                $"TMP font asset has no source font reference. Dynamic glyph generation may fail: {AssetDatabase.GetAssetPath(fontAsset)}",
                fontAsset);
            return;
        }

        runtimeRef.objectReferenceValue = editorRef.objectReferenceValue;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ClearFontAssetData(UnityEngine.Object fontAsset)
    {
        var method = fontAsset.GetType().GetMethod(
            "ClearFontAssetData",
            BindingFlags.Instance | BindingFlags.Public,
            null,
            new[] { typeof(bool) },
            null);

        if (method == null)
        {
            throw new MissingMethodException(fontAsset.GetType().FullName, "ClearFontAssetData(bool)");
        }

        method.Invoke(fontAsset, new object[] { true });
    }

    private static void ClearSerializedDynamicData(UnityEngine.Object fontAsset)
    {
        var serializedObject = new SerializedObject(fontAsset);

        SetIntOrEnum(serializedObject, "m_AtlasPopulationMode", 1);
        SetBool(serializedObject, "m_IsMultiAtlasTexturesEnabled", true);
        SetBool(serializedObject, "m_ClearDynamicDataOnBuild", true);
        SetInt(serializedObject, "m_AtlasTextureIndex", 0);

        ClearArray(serializedObject, "m_GlyphTable");
        ClearArray(serializedObject, "m_CharacterTable");
        ClearArray(serializedObject, "m_UsedGlyphRects");
        ClearArray(serializedObject, "m_FontFeatureTable.m_GlyphPairAdjustmentRecords");

        EnsureRuntimeSourceFontReference(fontAsset);
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void RemoveUnusedAtlasTextureSubAssets(string path, UnityEngine.Object fontAsset)
    {
        var referencedTextures = GetReferencedAtlasTextures(fontAsset);
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);

        foreach (var subAsset in subAssets)
        {
            if (subAsset is Texture2D texture && !referencedTextures.Contains(texture))
            {
                Undo.DestroyObjectImmediate(texture);
            }
        }
    }

    private static HashSet<Texture2D> GetReferencedAtlasTextures(UnityEngine.Object fontAsset)
    {
        var referencedTextures = new HashSet<Texture2D>();
        var serializedObject = new SerializedObject(fontAsset);
        var atlasTextures = serializedObject.FindProperty("m_AtlasTextures");

        if (atlasTextures == null || !atlasTextures.isArray)
        {
            return referencedTextures;
        }

        for (var i = 0; i < atlasTextures.arraySize; i++)
        {
            var texture = atlasTextures.GetArrayElementAtIndex(i).objectReferenceValue as Texture2D;
            if (texture != null)
            {
                referencedTextures.Add(texture);
            }
        }

        return referencedTextures;
    }

    private static void SetIntOrEnum(SerializedObject serializedObject, string propertyPath, int value)
    {
        var property = serializedObject.FindProperty(propertyPath);
        if (property == null)
        {
            return;
        }

        if (property.propertyType == SerializedPropertyType.Enum)
        {
            property.enumValueIndex = value;
        }
        else if (property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = value;
        }
    }

    private static void SetInt(SerializedObject serializedObject, string propertyPath, int value)
    {
        var property = serializedObject.FindProperty(propertyPath);
        if (property != null && property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = value;
        }
    }

    private static void SetBool(SerializedObject serializedObject, string propertyPath, bool value)
    {
        var property = serializedObject.FindProperty(propertyPath);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void ClearArray(SerializedObject serializedObject, string propertyPath)
    {
        var property = serializedObject.FindProperty(propertyPath);
        if (property != null && property.isArray)
        {
            property.ClearArray();
        }
    }

    private static long GetAssetFileSize(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0L;
    }

    private static string FormatBytes(long bytes)
    {
        const double kb = 1024d;
        const double mb = kb * 1024d;

        if (bytes >= mb)
        {
            return $"{bytes / mb:0.##} MB";
        }

        if (bytes >= kb)
        {
            return $"{bytes / kb:0.##} KB";
        }

        return $"{bytes} B";
    }
}
