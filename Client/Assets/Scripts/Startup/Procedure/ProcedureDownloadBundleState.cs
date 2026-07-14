using AlicizaX.Resource.Runtime;
using AlicizaX;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace Unity.Startup.Procedure
{
    internal sealed class ProcedureDownloadBundleState : ProcedureBase
    {
        ResourceDownloaderOperation downloader;

        protected override void OnEnter()
        {
            CreateDownloader();
        }

        protected override void OnLeave()
        {
            downloader.CancelDownload();
            downloader = null;
        }

        private void CreateDownloader()
        {
            downloader = AppServices.Require<IResourceService>().CreateResourceDownloader();
            if (downloader.TotalDownloadCount == 0)
            {
                Log.Info("没有发现需要下载的资源");
                SwitchProcedure<ProcedurePatchDoneState>();
            }
            else
            {
                // 发现新更新文件后，挂起流程系统
                int totalDownloadCount = downloader.TotalDownloadCount;
                long totalDownloadBytes = downloader.TotalDownloadBytes;

                float sizeMb = totalDownloadBytes / 1048576f;
                sizeMb = Mathf.Clamp(sizeMb, 0.1f, float.MaxValue);
                string totalSizeMb = sizeMb.ToString("f1");

                Log.Info($"一共发现了{downloader.TotalDownloadCount}个资源需要更新下载,总共需要下载文件大小为:{totalSizeMb}!");

                DownloadFiles().Forget();
            }
        }

        private async UniTask DownloadFiles()
        {
            // 注册下载回调
            void DownloaderOnDownloadErrorCallback(DownloadErrorEventArgs data)
            {
                Log.Error($"PackagegName:{data.PackageName} FileName:{data.FileName} Error:{data.ErrorInfo}");
                CreateDownloader();
            }

            downloader.DownloadError += DownloaderOnDownloadErrorCallback;
            downloader.DownloadProgressChanged += OnDownloadProgressCallback;
            downloader.StartDownload();
            await downloader;

            // 检测下载结果
            if (downloader.Status != EOperationStatus.Succeeded)
            {
                Log.Error("资源更新失败!");
                return;
            }

            SwitchProcedure<ProcedurePatchDoneState>();
        }

        private void OnDownloadProgressCallback(DownloadProgressChangedEventArgs data)
        {
            EventBus.Publish(AssetDownloadProgressUpdateEventArgs.Create(data.PackageName, data.TotalDownloadCount, data.CurrentDownloadCount, data.TotalDownloadBytes, data.CurrentDownloadBytes));
        }
    }
}
