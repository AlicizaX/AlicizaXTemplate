using UnityEditor;
using UnityEngine;

public interface IBuildParameters
{
    BuildTarget BuildTarget { get; }
    string OutPutPath { get; set; }
    bool DevelopBuild { get; set; }
    int ResMode { get; set; }
    string FileName { get; set; }
    string Language { get; set; }
    string[] Scenes { get; set; }
    string Version { get; set; }

    string DecryptionServices { get; set; }
}

[System.Serializable]
public class AndroidBuildParameters : IBuildParameters
{
    /// <summary>
    /// 默认最低 API 等级：Unity 6000.5 起不再支持 API 22（Android 5.1），
    /// <see cref="AndroidSdkVersions.AndroidApiLevel22"/> 已废弃为编译错误，最低只能到 API 26（Android 8.0）。
    /// </summary>
#if UNITY_6000_5_OR_NEWER
    private const AndroidSdkVersions DefaultMinSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
#else
    private const AndroidSdkVersions DefaultMinSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
#endif

    // 通用参数
    public BuildTarget BuildTarget => BuildTarget.Android;
    public string OutPutPath { get; set; }
    public bool DevelopBuild { get; set; }
    public int ResMode { get; set; }
    public string FileName { get; set; }
    public string Language { get; set; }
    public string[] Scenes { get; set; }
    public string Version { get; set; }
    public string DecryptionServices { get; set; }

    /// <summary>
    /// 纹理压缩格式
    /// </summary>
    public MobileTextureSubtarget TextureCompression { get; set; } = MobileTextureSubtarget.ASTC;

    /// <summary>
    /// 最低支持的 Android API 等级
    /// </summary>
    public AndroidSdkVersions MinSdkVersion { get; set; } = DefaultMinSdkVersion;

    /// <summary>
    /// 目标 API 等级
    /// </summary>
    public AndroidSdkVersions TargetSdkVersion { get; set; } = AndroidSdkVersions.AndroidApiLevelAuto;

    /// <summary>
    /// 应用标识符
    /// </summary>
    public string BundleIdentifier { get; set; }

    /// <summary>
    /// 版本代码
    /// </summary>
    public int BundleVersionCode { get; set; } = 1;
}


[System.Serializable]
public class StandaloneBuildParameters : IBuildParameters
{
    public StandaloneBuildParameters(BuildTarget buildTarget)
    {
        BuildTarget = buildTarget;
    }

    // 通用参数
    public BuildTarget BuildTarget { get; }
    public string OutPutPath { get; set; }
    public bool DevelopBuild { get; set; }
    public int ResMode { get; set; }
    public string FileName { get; set; }
    public string Language { get; set; }
    public string[] Scenes { get; set; }
    public string Version { get; set; }
    public string DecryptionServices { get; set; }


    /// <summary>
    /// 是否全屏
    /// </summary>
    public FullScreenMode FullScreenMode { get; set; } = FullScreenMode.FullScreenWindow;

    /// <summary>
    /// 默认屏幕宽度 (窗口模式有效)
    /// </summary>
    public int DefaultScreenWidth { get; set; } = 1280;

    /// <summary>
    /// 默认屏幕高度 (窗口模式有效)
    /// </summary>
    public int DefaultScreenHeight { get; set; } = 720;

    /// <summary>
    /// 是否允许后台运行
    /// </summary>
    public bool RunInBackground { get; set; } = true;
}
