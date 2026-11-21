using System;
using System.Threading;
using AlicizaX;
using Cysharp.Threading.Tasks;
using YooAsset;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 下载补丁资源，进度通过 <see cref="LaunchDownloadProgressEvent"/> 通知界面；失败时按 RetryAsync 的规则重试。
    /// </summary>
    internal sealed class ProcedureDownloadBundleState : AsyncProcedureBase
    {
        protected override async UniTask RunAsync(CancellationToken cancellationToken)
        {
            await RetryAsync(DownloadAsync, cancellationToken);
            SwitchProcedure<ProcedureClearCacheState>();
        }

        private static async UniTask DownloadAsync(CancellationToken cancellationToken)
        {
            ResourceDownloaderOperation downloader = GameApp.ResourcePackage.CreateResourceDownloader();
            if (downloader.TotalDownloadCount == 0)
            {
                Log.Info("No assets need to be downloaded.");
                return;
            }

            Log.Info($"Downloading {downloader.TotalDownloadCount} files, {Utility.File.GetBytesSize(downloader.TotalDownloadBytes)}.");
            downloader.DownloadProgressChanged += OnDownloadProgress;
            downloader.DownloadError += OnDownloadError;

            using (cancellationToken.Register(downloader.CancelDownload))
            {
                downloader.StartDownload();
                await downloader.ToUniTask();
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (downloader.Status != EOperationStatus.Succeeded)
                throw new InvalidOperationException($"Download failed: {downloader.Error}");
        }

        private static void OnDownloadProgress(DownloadProgressChangedEventArgs e)
        {
            EventBus.Publish(new LaunchDownloadProgressEvent(
                e.CurrentDownloadCount, e.TotalDownloadCount, e.CurrentDownloadBytes, e.TotalDownloadBytes));
        }

        private static void OnDownloadError(DownloadErrorEventArgs e)
        {
            Log.Error($"Download failed. Package: {e.PackageName}, File: {e.FileName}, Error: {e.ErrorInfo}");
        }
    }
}
