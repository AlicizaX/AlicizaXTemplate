using System.IO;
using AlicizaX.Editor.Extension;
using AlicizaX;
using AlicizaX.Debugger.Runtime;
using UnityEditor;
using UnityEngine;
using YooAsset;
using YooAsset.Editor;

namespace BuildCLI
{
    public static class JenkinsBuildCLI
    {
        static string[] scenePath = new[] { "Assets/Scenes/Main.unity" };

        [EditorToolFunction("Build/Offline-Mode/Build_Exe")]
        public static void TestBuildExe()
        {
            StandaloneBuildParameters parameter = new StandaloneBuildParameters();
            parameter.DevelopBuild = false;
            parameter.ShowDebugWnd = DebuggerActiveWindowType.AlwaysOpen;
            parameter.OutPutPath = "../Build";
            parameter.FileName = "AlicizaX.exe";
            parameter.Scenes = scenePath;
            parameter.ResMode = (int)EPlayMode.OfflinePlayMode;
            parameter.FullScreenMode = FullScreenMode.Windowed;
            parameter.Language = "ChineseSimplified";
            AppBuildHelper.BuildStandalone(parameter);
        }

        [EditorToolFunction("Build/Offline-Mode/Build_AB")]
        public static void BuildOfflineRes()
        {
            ResourceBuildParameter buildParameter = new ResourceBuildParameter();
            buildParameter.ResourceBuildTarget = BuildTarget.StandaloneWindows;
            buildParameter.UseDefaultPackageVersion = true;
            buildParameter.OutputPath = "../Bundle";
            buildParameter.BuildMode = ResourceBuildMode.Offline;
            ResourceBuildHelper.BuildResourcePackage(buildParameter);
        }

        [EditorToolFunction("Build/Online-Mode/Build_Exe")]
        public static void TestBuildOnlineExe()
        {
            StandaloneBuildParameters parameter = new StandaloneBuildParameters();
            parameter.DevelopBuild = false;
            parameter.ShowDebugWnd = DebuggerActiveWindowType.AlwaysOpen;
            parameter.OutPutPath = "../Build";
            parameter.FileName = "SAOK.exe";
            parameter.Scenes = scenePath;
            parameter.ResMode = (int)EPlayMode.HostPlayMode;
            parameter.Language = "ChineseSimplified";
            AppBuildHelper.BuildStandalone(parameter);
        }

        [EditorToolFunction("Build/Online-Mode/Build_AB")]
        public static void TestBuildRes()
        {
            ResourceBuildParameter buildParameter = new ResourceBuildParameter();
            buildParameter.ResourceBuildTarget = BuildTarget.StandaloneWindows;
            buildParameter.UseDefaultPackageVersion = true;
            buildParameter.OutputPath = "../Bundle";
            buildParameter.BuildMode = ResourceBuildMode.Online;
            buildParameter.EncryptionServiceType = string.Empty;
            ResourceBuildHelper.BuildResourcePackage(buildParameter);
        }
    }
}
