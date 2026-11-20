

using System;
using AlicizaX;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 热更完成后等待玩家确认进入游戏。
    /// </summary>
    public sealed class ProcedureUpdateFinishState : ProcedureBase
    {
        private readonly Action _enterGame;
        private EventRuntimeHandle _startRequest;

        internal ProcedureUpdateFinishState(Action enterGame) => _enterGame = enterGame;

        protected override void OnEnter()
        {
            UpdateProgressUtils.Dispose();
            _startRequest = EventBus.Subscribe<LaunchStartRequestedEvent>(StartGame);
            EventBus.Publish(new LaunchStateChangedEvent(LaunchState.Ready));
        }

        protected override void OnLeave() => _startRequest.Dispose();

        private void StartGame()
        {
            ProcedureBuilder.DestroyProcedure();
            _enterGame();
        }
    }
}
