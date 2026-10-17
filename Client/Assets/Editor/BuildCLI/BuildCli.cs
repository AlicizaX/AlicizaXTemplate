using System;
using UnityEditor;
using UnityEngine;

public static class BuildCli
{
    public static void Run()
    {
        int exitCode = 1;
        try
        {
            Execute(BuildRequestReader.Read());
            exitCode = 0;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        EditorApplication.Exit(exitCode);
    }

    public static void RunResources()
    {
        int exitCode = 1;
        try
        {
            BuildResources(BuildRequestReader.ReadResources());
            exitCode = 0;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        EditorApplication.Exit(exitCode);
    }

    public static void Execute(BuildRequest request)
    {
        BuildTargetGroup group = Prepare(request);
#if HYBIRDCLR_SUPPORT
        if (HasPlayerDefine(group, "ENABLE_HYBRIDCLR"))
        {
            PrepareHybrid(request);
            BuildResource(request);
            BuildApp(request);
            HybridClrBuild.CopyAot(request.BuildTarget);
            BuildResource(request);
            return;
        }
#endif
        BuildResource(request);
        BuildApp(request);
    }

    public static void BuildResources(BuildRequest request)
    {
        BuildTargetGroup group = Prepare(request);
#if HYBIRDCLR_SUPPORT
        if (HasPlayerDefine(group, "ENABLE_HYBRIDCLR"))
        {
            PrepareHybrid(request);
        }
#endif
        BuildResource(request);
    }

    private static BuildTargetGroup Prepare(BuildRequest request)
    {
        BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(request.BuildTarget);
        if (EditorUserBuildSettings.activeBuildTarget != request.BuildTarget
            && !EditorUserBuildSettings.SwitchActiveBuildTarget(group, request.BuildTarget))
        {
            throw new Exception("切换构建平台失败: " + request.BuildTarget);
        }

        if (!string.IsNullOrEmpty(request.Defines))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, request.Defines);
        }

        return group;
    }

#if HYBIRDCLR_SUPPORT
    private static void PrepareHybrid(BuildRequest request)
    {
        HybridClrBuild.Install();
        HybridClrBuild.Generate();
        HybridClrBuild.CompileHotUpdate(request.BuildTarget);
    }
#endif

    private static bool HasPlayerDefine(BuildTargetGroup group, string define)
    {
        string[] defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';');
        for (int i = 0; i < defines.Length; i++)
        {
            if (defines[i].Trim() == define)
            {
                return true;
            }
        }

        return false;
    }

    private static void BuildResource(BuildRequest request)
    {
        var parameter = new ResourceBuildParameter
        {
            ResourceBuildTarget = request.BuildTarget,
            OutputPath = string.IsNullOrEmpty(request.ResourceOutputDirectory)
                ? request.OutputDirectory
                : request.ResourceOutputDirectory,
            UseDefaultPackageVersion = string.IsNullOrEmpty(request.PackageVersion),
            PackageVersion = request.PackageVersion,
            BuildMode = request.ResourceMode,
            EncryptionServiceType = request.Encryption,
        };
        ResourceBuildHelper.BuildResourcePackage(parameter);
    }

    private static void BuildApp(BuildRequest request)
    {
        IBuildParameters parameter = CreateAppParameters(request);
        parameter.OutPutPath = request.OutputDirectory;
        parameter.FileName = request.FileName;
        parameter.Scenes = request.Scenes;
        parameter.DevelopBuild = request.Development;
        parameter.Version = request.VersionName;
        parameter.ResMode = request.PlayMode;
        parameter.Language = request.Language;
        parameter.DecryptionServices = request.Decryption;

        if (parameter is AndroidBuildParameters android)
        {
            android.BundleVersionCode = request.VersionCode;
            AppBuildHelper.BuildAndroid(android);
            return;
        }

        AppBuildHelper.BuildStandalone((StandaloneBuildParameters)parameter);
    }

    private static IBuildParameters CreateAppParameters(BuildRequest request)
    {
        if (request.BuildTarget == BuildTarget.Android)
        {
            return new AndroidBuildParameters();
        }

        return new StandaloneBuildParameters(request.BuildTarget);
    }
}
