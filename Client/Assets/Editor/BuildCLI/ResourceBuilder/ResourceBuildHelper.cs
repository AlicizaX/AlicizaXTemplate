using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        if (!Directory.Exists(buildParameter.OutputPath))
        {
            Directory.CreateDirectory(buildParameter.OutputPath);
        }

        try
        {
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
                BuildOutputRoot = buildParameter.OutputPath,
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
                IgnoreTypeTreeChanges = buildParameter.IgnoreTypeTreeChanges,
                TrackSpriteAtlasDependencies = buildParameter.TrackSpriteAtlasDependencies,
            };

            ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
            var report = pipeline.Run(parameters, true);

            if (report.Success)
            {
                // if (_copyAfterBuild && !string.IsNullOrEmpty(_copyDestination))
                // {
                //     CopyFiles(report.OutputPackageDirectory, _copyDestination);
                // }
                Debug.Log($"AB包构建完成！\n输出目录: {report.OutputPackageDirectory}");
            }
            else
            {
                Debug.Log($"错误信息: {report.ErrorInfo}");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
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

    private static void CopyFiles(string source, string destination)
    {
        try
        {
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            foreach (string file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            }

            Debug.Log($"文件拷贝完成: {source} -> {destination}");
        }
        catch (Exception e)
        {
            Debug.LogError($"文件拷贝失败: {e.Message}");
        }
    }

    private static IBundleEncryptor CreateEncryptionInstance(string encryptionService)
    {
        if (string.IsNullOrEmpty(encryptionService)) return null;

        var type = Type.GetType(encryptionService);
        if (type != null)
        {
            return (IBundleEncryptor)Activator.CreateInstance(type);
        }

        return null;
    }
}
