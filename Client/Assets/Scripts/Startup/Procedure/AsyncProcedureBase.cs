using System;
using System.Threading;
using AlicizaX;
using Cysharp.Threading.Tasks;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 异步流程基类：进入时运行 <see cref="RunAsync"/>，离开（切换或销毁流程）时取消。
    /// 子类在 RunAsync 结束时调用 SwitchProcedure 进入下一步；所有 await 都应传入取消令牌，
    /// 保证离开后的流程不会再切换状态。
    /// </summary>
    internal abstract class AsyncProcedureBase : ProcedureBase
    {
        private CancellationTokenSource _cts;

        protected sealed override void OnEnter()
        {
            _cts = new CancellationTokenSource();
            RunAsync(_cts.Token).Forget();
        }

        protected sealed override void OnLeave()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        protected abstract UniTask RunAsync(CancellationToken cancellationToken);

        /// <inheritdoc cref="RetryAsync{T}(Func{CancellationToken, UniTask{T}}, CancellationToken)"/>
        protected async UniTask RetryAsync(Func<CancellationToken, UniTask> attempt, CancellationToken cancellationToken)
        {
            await RetryAsync(async ct =>
            {
                await attempt(ct);
                return true;
            }, cancellationToken);
        }

        /// <summary>
        /// 执行 <paramref name="attempt"/>，失败后每隔 <see cref="StartupSetting.RetryIntervalSeconds"/> 秒重试，
        /// 最多 <see cref="StartupSetting.MaxRetryCount"/> 次；仍失败则输出警告，流程停在当前步骤。
        /// </summary>
        protected async UniTask<T> RetryAsync<T>(Func<CancellationToken, UniTask<T>> attempt, CancellationToken cancellationToken)
        {
            for (int retryCount = 0;; retryCount++)
            {
                try
                {
                    return await attempt(cancellationToken);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    if (retryCount >= StartupSetting.MaxRetryCount)
                    {
                        Log.Warning($"[{GetType().Name}] {e.Message} Still failing after {StartupSetting.MaxRetryCount} retries, launch stopped.");
                        // 停在这一步直到流程被销毁。需要手动重试时，把这里改成等待重试请求即可。
                        return await UniTask.Never<T>(cancellationToken);
                    }

                    Log.Info($"[{GetType().Name}] {e.Message} Retry {retryCount + 1}/{StartupSetting.MaxRetryCount} in {StartupSetting.RetryIntervalSeconds}s.");
                    await UniTask.Delay(TimeSpan.FromSeconds(StartupSetting.RetryIntervalSeconds), DelayType.Realtime,
                        cancellationToken: cancellationToken);
                }
            }
        }
    }
}
