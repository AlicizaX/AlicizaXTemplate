using AlicizaX;
using UnityEngine;

namespace Unity.Startup.Procedure
{
    // 启动流程与启动界面之间的全部事件，二者互不引用：
    //   流程 → 界面：LaunchStateChangedEvent、LaunchDownloadProgressEvent
    //   界面 → 流程：LaunchStartRequestedEvent

    internal enum LaunchState
    {
        /// <summary>正在检查 / 更新。</summary>
        Checking,

        /// <summary>更新完成。</summary>
        Ready
    }

    internal readonly struct LaunchStateChangedEvent : IPayloadEventArgs
    {
        public readonly LaunchState State;

        public LaunchStateChangedEvent(LaunchState state) => State = state;
    }

    internal readonly struct LaunchDownloadProgressEvent : IPayloadEventArgs
    {
        public readonly int CurrentCount;
        public readonly int TotalCount;
        public readonly long CurrentBytes;
        public readonly long TotalBytes;

        public LaunchDownloadProgressEvent(int currentCount, int totalCount, long currentBytes, long totalBytes)
        {
            CurrentCount = currentCount;
            TotalCount = totalCount;
            CurrentBytes = currentBytes;
            TotalBytes = totalBytes;
        }

        /// <summary>0~1，有字节数时按字节计算，否则按文件数。</summary>
        public float Progress => Mathf.Clamp01(TotalBytes > 0
            ? (float)((double)CurrentBytes / TotalBytes)
            : CurrentCount / (float)Mathf.Max(1, TotalCount));
    }

    /// <summary>玩家确认进入游戏。</summary>
    internal readonly struct LaunchStartRequestedEvent : IEmptyEventArgs
    {
    }
}
