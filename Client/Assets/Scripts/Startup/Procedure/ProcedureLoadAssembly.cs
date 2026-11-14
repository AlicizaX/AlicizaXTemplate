using System;
using System.Collections.Generic;
using System.Reflection;
using AlicizaX;
using AlicizaX.Resource.Runtime;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;
#if ENABLE_HYBRIDCLR
using HybridCLR;
#endif

namespace Unity.Startup.Procedure
{
    public sealed class ProcedureLoadAssembly : ProcedureBase
    {
        private readonly List<Assembly> m_HotfixAssemblies = new List<Assembly>();

#if ENABLE_HYBRIDCLR
        private int m_LoadAssemblyAssetCount;
        private int m_LoadMetadataAssetCount;
#endif
        private bool m_LoadAssemblyComplete;
        private bool m_LoadMetadataAssemblyComplete;
        private bool m_EntryInvoked;
        private Assembly m_MainLogicAssembly;

        protected override void OnEnter()
        {
            Log.Info("ProcedureLoadAssembly OnEnter");

            m_LoadAssemblyComplete = false;
            m_LoadMetadataAssemblyComplete = true;
            m_EntryInvoked = false;
            m_MainLogicAssembly = null;
            m_HotfixAssemblies.Clear();

#if ENABLE_HYBRIDCLR
            m_LoadAssemblyAssetCount = 0;
            m_LoadMetadataAssetCount = 0;
            LoadMetadataForAOTAssembly();

#if UNITY_EDITOR
            // 编辑器下始终使用工程编译出的程序集（Library/ScriptAssemblies）。
            // 原因：在编辑器里对热更 DLL 执行 Assembly.Load(bytes) 会再加载一份同名程序集，
            // 而 AssetBundle 中 Prefab 的脚本引用解析到的是编辑器程序集，
            // 于是 GetComponent(热更类型) 会因类型身份不同而失败（表现为 UI holder 丢失）。
            // 要验证真正下载的热更 DLL，请出包测试。
            LoadAssembliesFromDomain();
#else
            if (GameApp.ResourcePackage.PlayMode == EPlayMode.EditorSimulateMode)
            {
                LoadAssembliesFromDomain();
            }
            else
            {
                LoadHotUpdateAssembliesFromResource();
            }
#endif
#else
            LoadAssembliesFromDomain();
#endif
        }

        protected override void OnUpdate()
        {
            if (m_EntryInvoked || !m_LoadAssemblyComplete || !m_LoadMetadataAssemblyComplete)
            {
                return;
            }

            m_EntryInvoked = true;
            InvokeMainLogicEntry();
        }

        private void InvokeMainLogicEntry()
        {
            SwitchProcedure<ProcedureUpdateFinishState>();

            if (m_MainLogicAssembly == null)
            {
                Log.Warning($"Main logic assembly '{StartupSetting.EntranceDll}' missing.");
                return;
            }

            Type appType = m_MainLogicAssembly.GetType(StartupSetting.EntranceClass);
            if (appType == null)
            {
                Log.Warning($"Main logic type '{StartupSetting.EntranceClass}' missing.");
                return;
            }

            MethodInfo entryMethod = appType.GetMethod(
                StartupSetting.EntranceMethod,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (entryMethod == null)
            {
                Log.Warning($"Main logic entry method '{StartupSetting.EntranceMethod}' missing.");
                return;
            }

            entryMethod.Invoke(null, new object[] { new object[] { m_HotfixAssemblies } });
        }

        private void LoadAssembliesFromDomain()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
#if UNITY_EDITOR
                // 关闭域重载时，上一次播放中 Assembly.Load(bytes) 加载的热更副本会残留在域里，
                // 这类程序集 Location 为空，不能作为编辑器下的代码来源。
                // 注意：动态程序集（Roslyn / HybridCLR Emit 等）访问 Location 会抛 NotSupportedException。
                if (assembly.IsDynamic)
                    continue;
                string location;
                try
                {
                    location = assembly.Location;
                }
                catch (NotSupportedException)
                {
                    continue;
                }
                if (string.IsNullOrEmpty(location))
                    continue;
#endif
                string assemblyName = ToDllName(assembly);
                if (IsEntranceAssembly(assemblyName))
                {
                    m_MainLogicAssembly = assembly;
                }

                if (IsHotUpdateAssembly(assemblyName) && !m_HotfixAssemblies.Contains(assembly))
                {
                    m_HotfixAssemblies.Add(assembly);
                }
            }

            m_LoadAssemblyComplete = true;
        }

        private static string ToDllName(Assembly assembly)
        {
            return $"{assembly.GetName().Name}.dll";
        }

        private static bool IsEntranceAssembly(string assemblyName)
        {
            return string.Equals(StartupSetting.EntranceDll, assemblyName, StringComparison.Ordinal);
        }

        private static bool IsHotUpdateAssembly(string assemblyName)
        {
            foreach (string hotUpdateDllName in StartupSetting.HotUpdateAssemblies)
            {
                if (string.Equals(hotUpdateDllName, assemblyName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

#if ENABLE_HYBRIDCLR
        private void LoadHotUpdateAssembliesFromResource()
        {
            foreach (string hotUpdateDllName in StartupSetting.HotUpdateAssemblies)
            {
                m_LoadAssemblyAssetCount++;
                LoadAssemblyAssetAsync(hotUpdateDllName).Forget();
            }

            if (m_LoadAssemblyAssetCount == 0)
            {
                m_LoadAssemblyComplete = true;
            }
        }

        private async UniTaskVoid LoadAssemblyAssetAsync(string location)
        {
            using AssetLease<TextAsset> lease = await GameApp.Resource.LoadLeaseAsync<TextAsset>(location);
            LoadAssemblyAssetSuccess(lease.Asset);
        }

        private void LoadAssemblyAssetSuccess(TextAsset textAsset)
        {
            try
            {
                if (textAsset == null)
                {
                    Log.Warning("Load assembly asset failed.");
                    return;
                }

                string assetName = textAsset.name;
                Log.Info($"Load assembly asset success, assetName: [ {assetName} ]");

                Assembly assembly = Assembly.Load(textAsset.bytes);
                if (IsEntranceAssembly(assetName))
                {
                    m_MainLogicAssembly = assembly;
                }

                if (!m_HotfixAssemblies.Contains(assembly))
                {
                    m_HotfixAssemblies.Add(assembly);
                }

                Log.Info($"Assembly [ {assembly.GetName().Name} ] loaded");
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
            finally
            {
                m_LoadAssemblyAssetCount--;
                m_LoadAssemblyComplete = m_LoadAssemblyAssetCount == 0;
            }
        }

        // Load AOT metadata only when HybridCLR is enabled.
        private void LoadMetadataForAOTAssembly()
        {
            if (GameApp.ResourcePackage.PlayMode == EPlayMode.EditorSimulateMode || AOTGenericReferences.PatchedAOTAssemblyList.Count == 0)
            {
                m_LoadMetadataAssemblyComplete = true;
                return;
            }

            m_LoadMetadataAssemblyComplete = false;
            foreach (string aotDllName in AOTGenericReferences.PatchedAOTAssemblyList)
            {
                m_LoadMetadataAssetCount++;
                LoadMetadataAssetAsync(aotDllName).Forget();
            }

            if (m_LoadMetadataAssetCount == 0)
            {
                m_LoadMetadataAssemblyComplete = true;
            }
        }

        private async UniTaskVoid LoadMetadataAssetAsync(string location)
        {
            using AssetLease<TextAsset> lease = await GameApp.Resource.LoadLeaseAsync<TextAsset>(location);
            LoadMetadataAssetSuccess(lease.Asset);
        }

        private void LoadMetadataAssetSuccess(TextAsset textAsset)
        {
            try
            {
                if (textAsset == null)
                {
                    Log.Warning("Load metadata asset failed.");
                    return;
                }

                string assetName = textAsset.name;
                HomologousImageMode mode = HomologousImageMode.SuperSet;
                LoadImageErrorCode err = (LoadImageErrorCode)RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, mode);
                Log.Info($"LoadMetadataForAOTAssembly:{assetName}. mode:{mode} ret:{err}");
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                throw;
            }
            finally
            {
                m_LoadMetadataAssetCount--;
                m_LoadMetadataAssemblyComplete = m_LoadMetadataAssetCount == 0;
            }
        }
#endif
    }
}
