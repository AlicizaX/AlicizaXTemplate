using AlicizaX;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 更新完成：通知界面就绪，等玩家确认后销毁启动流程并调用热更入口。
    /// </summary>
    internal sealed class ProcedureEnterGameState : ProcedureBase
    {
        private readonly LaunchContext _context;
        private EventRuntimeHandle _startRequested;

        public ProcedureEnterGameState(LaunchContext context) => _context = context;

        protected override void OnEnter()
        {
            _startRequested = EventBus.Subscribe<LaunchStartRequestedEvent>(EnterGame);
            EventBus.Publish(new LaunchStateChangedEvent(LaunchState.Ready));
        }

        protected override void OnLeave() => _startRequested.Dispose();

        private void EnterGame()
        {
            ProcedureBuilder.DestroyProcedure();
            // 热更入口约定：static void Entrance(object[] args)，args[0] 为 List<Assembly>。
            _context.EntryMethod?.Invoke(null, new object[] { new object[] { _context.HotfixAssemblies } });
        }
    }
}
