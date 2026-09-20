using AlicizaX.Localization.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 构建时把 localization.language 烤进场景副本里的 LocalizationComponent。
/// 没有该 key 则不改字段；不会 SaveScene / SaveAssets。
/// </summary>
public sealed class LocalizationLaunchBake : IProcessSceneWithReport
{
    public int callbackOrder => 0;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (!BuildPipeline.isBuildingPlayer)
        {
            return;
        }

        if (!BuildLaunchContext.TryGet(BuildLaunchContext.LocalizationLanguage, out string language)
            || string.IsNullOrEmpty(language))
        {
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            LocalizationComponent[] components = roots[i].GetComponentsInChildren<LocalizationComponent>(true);
            for (int j = 0; j < components.Length; j++)
            {
                SerializedObject so = new SerializedObject(components[j]);
                SerializedProperty languageProperty = so.FindProperty("_language");
                if (languageProperty != null)
                {
                    languageProperty.stringValue = language;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
