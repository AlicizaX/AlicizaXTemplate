/// <summary>
/// 启动配置：热更入口、远端版本服务与重试策略。只放常量，运行期数据见 <see cref="Unity.Startup.Procedure.LaunchContext"/>。
/// </summary>
public static class StartupSetting
{
    public const string EntranceDll = "GameLogic.dll";
    public const string EntranceClass = "GameLogic.HotfixEntry";
    public const string EntranceMethod = "Entrance";

    /// <summary>热更程序集，按依赖顺序排列，也按此顺序加载。</summary>
    public static readonly string[] HotUpdateAssemblies = { "GameLib.dll", "GameProto.dll", "GameBase.dll", "GameLogic.dll" };

    /// <summary>远端版本服务，返回 JSON：{ version, cdnUrl, appDownloadUrl }。</summary>
    public const string VersionApi = "http://localhost:5000/api/Version?channel=Standlone";

    /// <summary>启动流程中单个操作失败后的重试次数；用完后输出警告，流程停在当前步骤。</summary>
    public const int MaxRetryCount = 3;

    /// <summary>重试间隔（秒）。</summary>
    public const float RetryIntervalSeconds = 3f;
}
