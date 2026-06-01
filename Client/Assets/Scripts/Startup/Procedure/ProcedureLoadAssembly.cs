using System;
using System.Collections.Generic;
using System.Reflection;
using AlicizaX;
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

            if (GameApp.Resource.PlayMode == EPlayMode.EditorSimulateMode)
            {
                LoadAssembliesFromDomain();
            }
            else
            {
                LoadHotUpdateAssembliesFromResource();
            }
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
                GameApp.Resource.LoadAsset<TextAsset>(hotUpdateDllName, LoadAssemblyAssetSuccess);
            }

            if (m_LoadAssemblyAssetCount == 0)
            {
                m_LoadAssemblyComplete = true;
            }
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

                if (textAsset != null)
                {
                    GameApp.Resource.UnloadAsset(textAsset);
                }
            }
        }

        // Load AOT metadata only when HybridCLR is enabled.
        private void LoadMetadataForAOTAssembly()
        {
            if (GameApp.Resource.PlayMode == EPlayMode.EditorSimulateMode || AOTGenericReferences.PatchedAOTAssemblyList.Count == 0)
            {
                m_LoadMetadataAssemblyComplete = true;
                return;
            }

            m_LoadMetadataAssemblyComplete = false;
            foreach (string aotDllName in AOTGenericReferences.PatchedAOTAssemblyList)
            {
                m_LoadMetadataAssetCount++;
                GameApp.Resource.LoadAsset<TextAsset>(aotDllName, LoadMetadataAssetSuccess);
            }

            if (m_LoadMetadataAssetCount == 0)
            {
                m_LoadMetadataAssemblyComplete = true;
            }
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

                if (textAsset != null)
                {
                    GameApp.Resource.UnloadAsset(textAsset);
                }
            }
        }
#endif
    }
}
