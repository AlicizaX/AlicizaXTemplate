using System;
using Cysharp.Threading.Tasks;
using AlicizaX;
using UnityEngine;
using YooAsset;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 获取版本信息
    /// </summary>
    public sealed class ProcedureGetAppVersionInfoState : ProcedureBase
    {
        private const int MaxTryCount = 3;
        private int currentTryCount;

        protected override void OnEnter()
        {
            base.OnEnter();
            GetAppVersionInfo();
        }


        private async void GetAppVersionInfo()
        {
            try
            {
                currentTryCount++;
                if (StartupSetting.Version != Application.version)
                {
                    Log.Warning($"Version inconsistency : {Application.version}->{StartupSetting.Version} ");
                    Utility.Platform.Quit();
#if !UNITY_EDITOR
                    Utility.Platform.OpenURL(StartupSetting.AppDownloadUrl);
#endif
                    return;
                }

                SwitchProcedure<ProcedureInitPackageState>();
            }
            catch (Exception e)
            {
                Log.Exception(e);

                currentTryCount++;

                if (currentTryCount <= MaxTryCount)
                {
                    await UniTask.Delay(3000);
                    GetAppVersionInfo();
                }
            }
        }
    }
}
