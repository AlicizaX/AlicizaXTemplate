using System;
using System.Collections.Generic;
using System.IO;
using AlicizaX.Editor;
using AlicizaX.Resource.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using YooAsset;

public static class AppBuildHelper
{
    /// <summary>
    /// 构建 Android 应用
    /// </summary>
    public static void BuildAndroid(AndroidBuildParameters androidParams, bool showExplorer = false)
    {
        PreBuildCheck(androidParams);

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
        PreBuildCheck(standaloneParams);

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

        try
        {
            var options = new BuildPlayerOptions
            {
                scenes = buildParams.Scenes,
                locationPathName = Path.Combine(buildParams.OutPutPath, buildParams.FileName),
                target = buildParams.BuildTarget,
                options = buildParams.DevelopBuild ? BuildOptions.Development : BuildOptions.None
            };

            var report = BuildPlayerWithResourceConfig(options, buildParams);

            Debug.Log($"构建完成: {report.summary.outputPath}");
            if (showExplorer)
            {
                DirectoryInfo directoryInfo = Directory.GetParent(report.summary.outputPath);
                if (directoryInfo != null)
                    OpenFolder.Execute(directoryInfo.FullName);
            }
        }
        finally
        {
            ResourcePlayerBuildConfig.Clear();
        }
    }

    /// <summary>
    /// 资源模式由 ResourceComponentBuildProcessor 在构建时写进场景里的 ResourceComponent。
    /// Set 会清掉 Player 场景数据缓存；万一仍命中缓存（处理器没执行），用 CleanBuildCache 重打一次，还不行就失败，绝不出沿用旧值的包。
    /// </summary>
    private static UnityEditor.Build.Reporting.BuildReport BuildPlayerWithResourceConfig(BuildPlayerOptions options, IBuildParameters buildParams)
    {
        var playMode = (EPlayMode)buildParams.ResMode;

        ResourcePlayerBuildConfig.Set(playMode, buildParams.DecryptionServices);
        var report = BuildPipeline.BuildPlayer(options);
        ThrowIfFailed(report);

        if (ResourcePlayerBuildConfig.InjectedCount > 0)
            return report;

        Debug.LogWarning("构建没有处理到 ResourceComponent（可能命中了增量构建缓存），使用 CleanBuildCache 重新构建");
        ResourcePlayerBuildConfig.Set(playMode, buildParams.DecryptionServices);
        options.options |= BuildOptions.CleanBuildCache;
        report = BuildPipeline.BuildPlayer(options);
        ThrowIfFailed(report);

        if (ResourcePlayerBuildConfig.InjectedCount == 0)
            throw new Exception("构建场景中没有找到 ResourceComponent，资源模式没有写进包体");

        return report;
    }

    private static void ThrowIfFailed(UnityEditor.Build.Reporting.BuildReport report)
    {
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception(CollectBuildErrors(report));
    }

    private static void ApplyAndroidSpecificSettings(AndroidBuildParameters androidParams)
    {
        // 设置 Android 平台特定的 PlayerSettings
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = androidParams.MinSdkVersion;
        PlayerSettings.Android.targetSdkVersion = androidParams.TargetSdkVersion;

        if (!string.IsNullOrEmpty(androidParams.BundleIdentifier))
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, androidParams.BundleIdentifier);
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

    private static void PreBuildCheck(IBuildParameters buildParams)
    {
        if (buildParams.Scenes == null || buildParams.Scenes.Length == 0)
        {
            throw new Exception("构建场景列表为空");
        }

        foreach (string scenePath in buildParams.Scenes)
        {
            if (!File.Exists(scenePath))
            {
                throw new Exception($"场景文件不存在: {scenePath}");
            }
        }

        if (string.IsNullOrEmpty(buildParams.OutPutPath))
        {
            throw new Exception("输出路径为空");
        }

        if (string.IsNullOrEmpty(buildParams.FileName))
        {
            throw new Exception("文件名为空");
        }
    }

    private static string CollectBuildErrors(UnityEditor.Build.Reporting.BuildReport report)
    {
#if UNITY_2023_1_OR_NEWER
        return report.SummarizeErrors();
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

        return $"构建失败:\n{string.Join("\n", errors)}";
#endif
    }

    #endregion
}
