using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

public static class ResourceBuildHelper
{
    private const bool EnableSharePack = true;
    private const string BuildPackageName = "DefaultPackage";

    private static string GeneratePackageVersion()
    {
        int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
        return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
    }

    public static void BuildResourcePackage(ResourceBuildParameter buildParameter)
    {
        string output = Path.GetFullPath(buildParameter.OutputPath);
        string staging = output + ".building";
        if (Directory.Exists(staging))
        {
            Directory.Delete(staging, true);
        }

        Directory.CreateDirectory(staging);

        string copyParams = string.Empty;
        EBundledCopyOption copyOption = EBundledCopyOption.None;
        if (buildParameter.BuildMode == ResourceBuildMode.Online)
        {
            copyOption = EBundledCopyOption.ClearAndCopyByTags;
            copyParams = "Launch";
        }
        else
        {
            copyOption = EBundledCopyOption.ClearAndCopyAll;
        }

        var parameters = new ScriptableBuildParameters
        {
            BuildOutputRoot = staging,
            BuildTarget = buildParameter.ResourceBuildTarget,
            PackageName = BuildPackageName,
            BuildBundleType = (int)EBundleType.AssetBundle,
            BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString(),
            BundledFileRoot = BundleBuilderHelper.GetStreamingAssetsRoot(),
            PackageVersion = buildParameter.UseDefaultPackageVersion ? GeneratePackageVersion() : buildParameter.PackageVersion,
            CompressOption = buildParameter.CompressOption,
            BuiltinShadersBundleName = GetBuiltinShaderBundleName(),
            VerifyBuildingResult = true,
            ClearBuildCacheFiles = false,
            BundledCopyOption = copyOption,
            BundledCopyParams = copyParams,
            EnableSharePackRule = EnableSharePack,
            FileNameStyle = buildParameter.FileNameStyle,
            BundleEncryptor = CreateEncryptionInstance(buildParameter.EncryptionServiceType),
            ReplaceAssetPathWithAddress = buildParameter.ReplaceAssetPathWithAddress,
            StripUnityVersion = buildParameter.StripUnityVersion,
            DisableWriteTypeTree = buildParameter.DisableWriteTypeTree,
            TrackSpriteAtlasDependencies = buildParameter.TrackSpriteAtlasDependencies,
        };

        ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
        var report = pipeline.Run(parameters, true);
        if (!report.Success)
        {
            throw new Exception(report.ErrorInfo);
        }

        string replacement = output + ".next";
        if (Directory.Exists(replacement))
        {
            Directory.Delete(replacement, true);
        }

        CopyDirectory(report.OutputPackageDirectory, replacement);
        Directory.Delete(staging, true);
        if (Directory.Exists(output))
        {
            Directory.Delete(output, true);
        }

        Directory.Move(replacement, output);
        Debug.Log($"AB包构建完成！\n输出目录: {output}");
    }


    /// <summary>
    /// 内置着色器资源包名称
    /// 注意：和自动收集的着色器资源包名保持一致！
    /// </summary>
    private static string GetBuiltinShaderBundleName()
    {
        var uniqueBundleName = BundleCollectorSettingData.Setting.UniqueBundleName;
        var packRuleResult = DefaultBundlePackRule.CreateShadersPackRuleResult();
        return packRuleResult.GetBundleName(BuildPackageName, uniqueBundleName);
    }

    private static void CopyDirectory(string source, string destination)
    {
        if (!Directory.Exists(source))
        {
            throw new Exception($"资源包输出目录不存在: {source}");
        }

        Directory.CreateDirectory(destination);

        foreach (string file in Directory.GetFiles(source))
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
        }

        foreach (string directory in Directory.GetDirectories(source))
        {
            string target = Path.Combine(destination, Path.GetFileName(directory));
            Directory.CreateDirectory(target);
            CopyDirectory(directory, target);
        }
    }

    private static IBundleEncryptor CreateEncryptionInstance(string encryptionService)
    {
        if (string.IsNullOrEmpty(encryptionService))
        {
            return null;
        }

        var type = Type.GetType(encryptionService);
        if (type == null)
        {
            throw new Exception($"找不到资源加密服务: {encryptionService}");
        }

        return (IBundleEncryptor)Activator.CreateInstance(type);
    }
}
