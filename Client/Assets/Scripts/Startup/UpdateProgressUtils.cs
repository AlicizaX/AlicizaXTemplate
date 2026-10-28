using System;
using System.Collections;
using AlicizaX.Resource.Runtime;
using AlicizaX;
using AlicizaX.Localization.Runtime;
using UnityEngine;


namespace Unity.Startup.Procedure
{
    public static class UpdateProgressUtils
    {
        static EventRuntimeHandle eventRuntimeHandle;

        public static void Start()
        {
            eventRuntimeHandle = EventBus.Subscribe<AssetDownloadProgressUpdateEventArgs>(SetProgressUpdate);
        }

        public static void Dispose()
        {
            eventRuntimeHandle.Dispose();
        }

        private static float _lastUpdateDownloadedSize;
        private static float _totalSpeed;
        private static int _speedSampleCount;
        private static long _currentDownloadBytes;

        private static float CurrentSpeed
        {
            get
            {
                float interval = Math.Max(Time.deltaTime, 0.01f); // 防止deltaTime过小
                var sizeDiff = _currentDownloadBytes - _lastUpdateDownloadedSize;
                _lastUpdateDownloadedSize = _currentDownloadBytes;
                var speed = sizeDiff / interval;

                // 使用滑动窗口计算平均速度
                _totalSpeed += speed;
                _speedSampleCount++;
                return _totalSpeed / _speedSampleCount;
            }
        }

        public static IEnumerator StartProgressCoroutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                Progress(progress);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Progress(1f);
        }

        private static void Progress(float v)
        {
            //自己可以在这更新热更进度等...
        }


        private static void SetProgressUpdate(in AssetDownloadProgressUpdateEventArgs gameEventArgs)
        {
            _currentDownloadBytes = gameEventArgs.CurrentDownloadSizeBytes;
            float progress = gameEventArgs.CurrentDownloadSizeBytes / (gameEventArgs.TotalDownloadSizeBytes * 1f);
            string currentSizeMb = Utility.File.GetBytesSize(gameEventArgs.CurrentDownloadSizeBytes);
            string totalSizeMb = Utility.File.GetBytesSize(gameEventArgs.TotalDownloadSizeBytes);
            float bytesPerSecond = CurrentSpeed;
            string speed = Utility.File.GetLengthString((int)bytesPerSecond);


            string line1 = UpdateLocalizationKey.Bootstrap.UPDATE_DOWNLOADING(gameEventArgs.CurrentDownloadCount, gameEventArgs.TotalDownloadCount, progress * 100f);
            string line2 = UpdateLocalizationKey.Bootstrap.UPDATE_SIZE(currentSizeMb, totalSizeMb);
            string line3 = UpdateLocalizationKey.Bootstrap.UPDATE_SPEED(speed,
                GetRemainingTime(gameEventArgs.TotalDownloadSizeBytes, gameEventArgs.CurrentDownloadSizeBytes, bytesPerSecond));


            Log.Info($"{line1} \n {line2}\n {line3}");
        }

        private static string GetRemainingTime(long totalBytes, long currentBytes, float speed)
        {
            int needTime = 0;
            if (speed > 0)
            {
                needTime = (int)((totalBytes - currentBytes) / speed);
            }

            TimeSpan ts = new TimeSpan(0, 0, needTime);
            return ts.ToString(@"mm\:ss");
        }
    }
}
