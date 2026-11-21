using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using AlicizaX;
using Cysharp.Threading.Tasks;
#if ENABLE_HYBRIDCLR
using AlicizaX.Resource.Runtime;
using HybridCLR;
using UnityEngine;
#endif

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 加载热更程序集并解析热更入口，结果写入 <see cref="LaunchContext"/>。
    /// </summary>
    internal sealed class ProcedureLoadAssemblyState : AsyncProcedureBase
    {
        private const BindingFlags EntryMethodFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        private readonly LaunchContext _context;

        public ProcedureLoadAssemblyState(LaunchContext context) => _context = context;

        protected override async UniTask RunAsync(CancellationToken cancellationToken)
        {
            List<Assembly> assemblies = await LoadHotfixAssembliesAsync(cancellationToken);
            _context.HotfixAssemblies = assemblies;
            _context.EntryMethod = ResolveEntryMethod(assemblies);
            SwitchProcedure<ProcedureEnterGameState>();
        }

        private static UniTask<List<Assembly>> LoadHotfixAssembliesAsync(CancellationToken cancellationToken)
        {
#if ENABLE_HYBRIDCLR && !UNITY_EDITOR
            return LoadFromBundlesAsync(cancellationToken);
#else
            // 编辑器下始终使用工程编译出的程序集（Library/ScriptAssemblies）。
            // 原因：在编辑器里对热更 DLL 执行 Assembly.Load(bytes) 会再加载一份同名程序集，
            // 而 AssetBundle 中 Prefab 的脚本引用解析到的是编辑器程序集，
            // 于是 GetComponent(热更类型) 会因类型身份不同而失败（表现为 UI holder 丢失）。
            // 要验证真正下载的热更 DLL，请出包测试。
            return UniTask.FromResult(FindLoadedHotfixAssemblies());
#endif
        }

        private static List<Assembly> FindLoadedHotfixAssemblies()
        {
            Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();
            var result = new List<Assembly>(StartupSetting.HotUpdateAssemblies.Length);
            foreach (string dllName in StartupSetting.HotUpdateAssemblies)
            {
                Assembly assembly = Array.Find(loaded, a => ToDllName(a) == dllName && IsCompiledAssembly(a));
                if (assembly != null) result.Add(assembly);
            }

            return result;
        }

        /// <summary>
        /// 关闭域重载时，之前 Assembly.Load(bytes) 的副本会残留在域里（Location 为空），不能作为代码来源；
        /// 动态程序集访问 Location 会抛 NotSupportedException。
        /// </summary>
        private static bool IsCompiledAssembly(Assembly assembly)
        {
#if UNITY_EDITOR
            if (assembly.IsDynamic) return false;
            try
            {
                return !string.IsNullOrEmpty(assembly.Location);
            }
            catch (NotSupportedException)
            {
                return false;
            }
#else
            return true;
#endif
        }

        private static MethodInfo ResolveEntryMethod(List<Assembly> assemblies)
        {
            Assembly entryAssembly = assemblies.Find(a => ToDllName(a) == StartupSetting.EntranceDll);
            MethodInfo entryMethod = entryAssembly?.GetType(StartupSetting.EntranceClass)?.GetMethod(StartupSetting.EntranceMethod, EntryMethodFlags);
            if (entryMethod == null)
                Log.Warning($"Hotfix entry '{StartupSetting.EntranceClass}.{StartupSetting.EntranceMethod}' not found in '{StartupSetting.EntranceDll}'.");
            return entryMethod;
        }

        private static string ToDllName(Assembly assembly) => assembly.GetName().Name + ".dll";

#if ENABLE_HYBRIDCLR
        private static async UniTask<List<Assembly>> LoadFromBundlesAsync(CancellationToken cancellationToken)
        {
            // AOT 补充元数据要在热更代码运行前加载。
            IReadOnlyList<string> aotDllNames = AOTGenericReferences.PatchedAOTAssemblyList;
            byte[][] aotDlls = await LoadAllBytesAsync(aotDllNames, cancellationToken);
            for (int i = 0; i < aotDlls.Length; i++)
            {
                LoadImageErrorCode error = RuntimeApi.LoadMetadataForAOTAssembly(aotDlls[i], HomologousImageMode.SuperSet);
                Log.Info($"LoadMetadataForAOTAssembly: {aotDllNames[i]}, result: {error}");
            }

            byte[][] hotfixDlls = await LoadAllBytesAsync(StartupSetting.HotUpdateAssemblies, cancellationToken);
            var assemblies = new List<Assembly>(hotfixDlls.Length);
            for (int i = 0; i < hotfixDlls.Length; i++)
            {
                assemblies.Add(Assembly.Load(hotfixDlls[i]));
                Log.Info($"Hotfix assembly loaded: {StartupSetting.HotUpdateAssemblies[i]}");
            }

            return assemblies;
        }

        private static UniTask<byte[][]> LoadAllBytesAsync(IReadOnlyList<string> locations, CancellationToken cancellationToken)
        {
            var tasks = new UniTask<byte[]>[locations.Count];
            for (int i = 0; i < tasks.Length; i++)
                tasks[i] = LoadBytesAsync(locations[i], cancellationToken);
            return UniTask.WhenAll(tasks);
        }

        private static async UniTask<byte[]> LoadBytesAsync(string location, CancellationToken cancellationToken)
        {
            using AssetLease<TextAsset> lease = await GameApp.Resource.LoadLeaseAsync<TextAsset>(location, cancellationToken);
            if (!lease.IsValid) throw new InvalidOperationException($"Missing assembly asset: {location}");
            return lease.Asset.bytes;
        }
#endif
    }
}
