using System;
using Cysharp.Threading.Tasks;
using AlicizaX;
using AlicizaX.Localization;
using UnityEngine;
using YooAsset;

namespace Unity.Startup.Procedure
{
    /// <summary>
    /// 启动游戏
    /// </summary>
    public class ProcedureEntryState : ProcedureBase
    {
        protected override void OnEnter()
        {
            UpdateProgressUtils.Start();
            Log.Info(GameApp.Resource.PlayMode);
            if (GameApp.Resource.PlayMode == EPlayMode.OfflinePlayMode || GameApp.Resource.PlayMode == EPlayMode.EditorSimulateMode)
            {
                SwitchProcedure<ProcedureInitPackageState>();
                return;
            }

            //检查设备是否能够访问互联网
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Log.Warning("The device is not connected to the network");
                return;
            }

            if (GameApp.Resource.PlayMode == EPlayMode.WebPlayMode)
            {
                SwitchProcedure<ProcedureInitPackageState>();
                return;
            }

            GetRemoteVersionInfo();
        }

        private async void GetRemoteVersionInfo()
        {
            try
            {
                await StartupSetting.GetRemoteVersion();
                SwitchProcedure<ProcedureGetAppVersionInfoState>();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                await UniTask.Delay(3000);
                GetRemoteVersionInfo();
            }
        }
    }
}
