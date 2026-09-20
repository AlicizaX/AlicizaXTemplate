using AlicizaX.Resource.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 构建时把 resource.* 启动参数烤进场景副本里的 ResourceComponent。
/// 没有对应 key 则不改字段；不会 SaveScene / SaveAssets。
/// </summary>
public sealed class ResourceLaunchBake : IProcessSceneWithReport
{
    public int callbackOrder => 0;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (!BuildPipeline.isBuildingPlayer)
        {
            return;
        }

        bool hasPlayMode = BuildLaunchContext.TryGet(BuildLaunchContext.ResourcePlayMode, out string playMode);
        bool hasDecryption = BuildLaunchContext.TryGet(BuildLaunchContext.ResourceDecryption, out string decryption);
        if (!hasPlayMode && !hasDecryption)
        {
            return;
        }

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            ResourceComponent[] components = roots[i].GetComponentsInChildren<ResourceComponent>(true);
            for (int j = 0; j < components.Length; j++)
            {
                SerializedObject so = new SerializedObject(components[j]);
                if (hasPlayMode)
                {
                    SerializedProperty playModeProperty = so.FindProperty("_playMode");
                    if (playModeProperty != null && int.TryParse(playMode, out int mode))
                    {
                        playModeProperty.intValue = mode;
                    }
                }

                if (hasDecryption)
                {
                    SerializedProperty decryptionProperty = so.FindProperty("decryptionServices");
                    if (decryptionProperty != null)
                    {
                        decryptionProperty.stringValue = decryption ?? string.Empty;
                    }
                }

                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
