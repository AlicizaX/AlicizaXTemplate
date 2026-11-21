using System.Collections.Generic;
using System.Reflection;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 启动流程的共享数据。由 <see cref="ProcedureEntry"/> 创建，通过构造函数注入给需要读写它的流程。
    /// </summary>
    internal sealed class LaunchContext
    {
        /// <summary>资源服务器地址。CheckVersion 写入，InitPackage 读取；离线 / 编辑器模拟模式下为空。</summary>
        public string CdnUrl { get; set; } = string.Empty;

        /// <summary>热更程序集，按 <see cref="StartupSetting.HotUpdateAssemblies"/> 顺序。LoadAssembly 写入，EnterGame 传给热更入口。</summary>
        public List<Assembly> HotfixAssemblies { get; set; }

        /// <summary>热更入口方法。LoadAssembly 解析（找不到时为 null），EnterGame 调用。</summary>
        public MethodInfo EntryMethod { get; set; }
    }
}
