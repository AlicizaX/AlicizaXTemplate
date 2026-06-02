using UnityEditor;
using YooAsset;
using YooAsset.Editor;

public class ResourceBuildParameter
{
    public BuildTarget ResourceBuildTarget;
    public string PackageVersion;
    public bool UseDefaultPackageVersion;
    public string OutputPath;
    public ResourceBuildMode BuildMode;

    public ECompressOption CompressOption = ECompressOption.LZ4;
    public EFileNameStyle FileNameStyle = EFileNameStyle.BundleName_HashName;

    /// <summary>
    /// 从文件头里剥离Unity版本信息
    /// </summary>
    public bool StripUnityVersion = false;

    /// <summary>
    /// 禁止写入类型树结构（可以降低包体和内存并提高加载效率）
    /// </summary>
    public bool DisableWriteTypeTree = false;

    /// <summary>
    /// 忽略类型树变化（无效参数）
    /// </summary>
    public bool IgnoreTypeTreeChanges = true;

    /// <summary>
    /// 使用可寻址地址代替资源路径
    /// 说明：开启此项可以节省运行时清单占用的内存！
    /// </summary>
    public bool ReplaceAssetPathWithAddress = false;

    /// <summary>
    /// 自动建立资源对象对图集的依赖关系
    /// </summary>
    public bool TrackSpriteAtlasDependencies = false;

    public string EncryptionServiceType;
}

public enum ResourceBuildMode
{
    Offline,
    Online,
}
