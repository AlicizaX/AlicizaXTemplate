using AlicizaX;

[Prewarm(4)]
public readonly struct AssetDownloadProgressUpdateEventArgs : IPayloadEventArgs
{
    /// <summary>
    /// 包名称
    /// </summary>
    public readonly string PackageName;

    /// <summary>
    /// 总下载数量
    /// </summary>
    public readonly int TotalDownloadCount;

    /// <summary>
    /// 当前下载数量
    /// </summary>
    public readonly int CurrentDownloadCount;

    /// <summary>
    /// 总下载大小
    /// </summary>
    public readonly long TotalDownloadSizeBytes;

    /// <summary>
    /// 当前下载大小
    /// </summary>
    public readonly long CurrentDownloadSizeBytes;


    public AssetDownloadProgressUpdateEventArgs(string packageName, int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes)
    {
        PackageName = packageName;
        TotalDownloadCount = totalDownloadCount;
        CurrentDownloadCount = currentDownloadCount;
        TotalDownloadSizeBytes = totalDownloadSizeBytes;
        CurrentDownloadSizeBytes = currentDownloadSizeBytes;
    }

    /// <summary>
    /// 创建下载进度更新
    /// </summary>
    /// <param name="packageName">包名称</param>
    /// <param name="totalDownloadCount">总下载数量</param>
    /// <param name="currentDownloadCount">当前下载数量</param>
    /// <param name="totalDownloadSizeBytes">总下载大小</param>
    /// <param name="currentDownloadSizeBytes">当前下载大小</param>
    /// <returns></returns>
    public static AssetDownloadProgressUpdateEventArgs Create(string packageName, int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes)
    {
        return new AssetDownloadProgressUpdateEventArgs(packageName, totalDownloadCount, currentDownloadCount, totalDownloadSizeBytes, currentDownloadSizeBytes);
    }
}
