using System;
using System.Collections.Generic;
using System.IO;
using AlicizaX;
using AlicizaX.Editor;
using UnityEditor;
using UnityEngine;

public static class AppBuildHelper
{
    /// <summary>
    /// 构建 Android 应用
    /// </summary>
    public static void BuildAndroid(AndroidBuildParameters androidParams, bool showExplorer = false)
    {
        if (!PreBuildCheck(androidParams))
        {
            Debug.LogError("Android 构建前检查失败！");
            return;
        }

        // 设置 Android 特定参数
        ApplyAndroidSpecificSettings(androidParams);

        // 执行通用构建流程
        BuildApplication(androidParams, showExplorer);
    }

    /// <summary>
    /// 构建 Standalone 应用 (Windows/Mac/Linux)
    /// </summary>
    public static void BuildStandalone(StandaloneBuildParameters standaloneParams, bool showExplorer = false)
    {
        if (!PreBuildCheck(standaloneParams))
        {
            Debug.LogError("Standalone 构建前检查失败！");
            return;
        }

        // 设置 Standalone 特定参数
        ApplyStandaloneSpecificSettings(standaloneParams);

        // 执行通用构建流程
        BuildApplication(standaloneParams, showExplorer);
    }

    #region 私有方法

    private static void BuildApplication(IBuildParameters buildParams, bool showExplorer = false)
    {
        if (!Directory.Exists(buildParams.OutPutPath))
        {
            Directory.CreateDirectory(buildParams.OutPutPath);
        }

        PlayerSettings.bundleVersion = buildParams.Version;
        Debug.Log($"开始构建应用 - 目标平台: {buildParams.BuildTarget}");

        // 生成应用设置
        GeneratAppBuilderSetting(buildParams);
        Debug.Log("生成 AppBuilderSetting.bytes 完成");

        try
        {
            var options = new BuildPlayerOptions
            {
                scenes = buildParams.Scenes,
                locationPathName = Path.Combine(buildParams.OutPutPath, buildParams.FileName),
                target = buildParams.BuildTarget,
                options = buildParams.DevelopBuild ? BuildOptions.Development : BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            HandleBuildReport(report);

            if (showExplorer && report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                DirectoryInfo directoryInfo = Directory.GetParent(report.summary.outputPath);
                if (directoryInfo != null)
                    OpenFolder.Execute(directoryInfo.FullName);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"构建过程中发生异常: {e.Message}\n{e.StackTrace}");
        }
    }

    private static void ApplyAndroidSpecificSettings(AndroidBuildParameters androidParams)
    {
        // 设置 Android 平台特定的 PlayerSettings
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = androidParams.MinSdkVersion;
        PlayerSettings.Android.targetSdkVersion = androidParams.TargetSdkVersion;

        if (!string.IsNullOrEmpty(androidParams.BundleIdentifier))
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, androidParams.BundleIdentifier);
        }

        PlayerSettings.Android.bundleVersionCode = androidParams.BundleVersionCode;

        // 设置纹理压缩格式
        EditorUserBuildSettings.androidBuildSubtarget = androidParams.TextureCompression;

        Debug.Log($"Android 特定设置应用完成 - MinSDK: {androidParams.MinSdkVersion}, Bundle: {androidParams.BundleIdentifier}");
    }

    private static void ApplyStandaloneSpecificSettings(StandaloneBuildParameters standaloneParams)
    {
        // 设置 Standalone 平台特定的 PlayerSettings
        PlayerSettings.fullScreenMode = standaloneParams.FullScreenMode;

        if (standaloneParams.FullScreenMode == FullScreenMode.Windowed)
        {
            PlayerSettings.defaultScreenWidth = standaloneParams.DefaultScreenWidth;
            PlayerSettings.defaultScreenHeight = standaloneParams.DefaultScreenHeight;
        }

        PlayerSettings.runInBackground = standaloneParams.RunInBackground;

        Debug.Log($"Standalone 特定设置应用完成 - 分辨率: {standaloneParams.DefaultScreenWidth}x{standaloneParams.DefaultScreenHeight}, 全屏: {standaloneParams.FullScreenMode}");
    }

    private static bool PreBuildCheck(IBuildParameters buildParams)
    {
        // 检查场景
        if (buildParams.Scenes == null || buildParams.Scenes.Length == 0)
        {
            Debug.LogError("构建场景列表为空！");
            return false;
        }

        foreach (string scenePath in buildParams.Scenes)
        {
            if (!File.Exists(scenePath))
            {
                Debug.LogError($"场景文件不存在: {scenePath}");
                return false;
            }
        }

        // 检查输出路径
        if (string.IsNullOrEmpty(buildParams.OutPutPath))
        {
            Debug.LogError("输出路径为空！");
            return false;
        }

        // 检查文件名
        if (string.IsNullOrEmpty(buildParams.FileName))
        {
            Debug.LogError("文件名为空！");
            return false;
        }

        return true;
    }


    public static void GeneratAppBuilderSetting(IBuildParameters buildParameters)
    {
        const string AppBuilderSettingPath = "Assets/Resources/ServiceDynamicBindInfo.bytes";
        ServiceDynamicBindInfo appBuilderSetting = new ServiceDynamicBindInfo();
        appBuilderSetting.Language = buildParameters.Language;
        appBuilderSetting.DebuggerActiveWindowType = buildParameters.ShowDebugWnd;
        appBuilderSetting.ResMode = buildParameters.ResMode;
        appBuilderSetting.DecryptionServices = buildParameters.DecryptionServices;
        File.WriteAllText(AppBuilderSettingPath, Utility.Json.ToJson(appBuilderSetting));
        AssetDatabase.ImportAsset(AppBuilderSettingPath, ImportAssetOptions.ForceUpdate);
    }

    private static void HandleBuildReport(UnityEditor.Build.Reporting.BuildReport report)
    {
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"构建完成: {report.summary.outputPath}");
        }
        else
        {
#if UNITY_2023_1_OR_NEWER
            Debug.LogError(report.SummarizeErrors());
#else
            var errors = new List<string>();
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                    {
                        errors.Add($"[Step: {step.name}] {msg.content}");
                    }
                }
            }

            Debug.LogError($"构建失败，错误信息:\n{string.Join("\n", errors)}");
#endif
        }
    }

    #endregion
}
