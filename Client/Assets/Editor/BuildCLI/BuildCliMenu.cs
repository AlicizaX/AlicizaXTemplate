using AlicizaX.Editor.Toolbar;
using UnityEditor;
using YooAsset;

public static class BuildCliMenu
{
    private const string OutputRoot = "../Build";
    private const string BundleRoot = "../Bundle";
    private const string FileName = "Test.exe";
    private static readonly string[] Scenes = { "Assets/Scenes/Main.unity" };

    [EditorToolFunction("Build/Offline-Mode/Build_Exe")]
    private static void BuildOfflineExe()
    {
        BuildCli.Execute(AppRequest(ResourceBuildMode.Offline));
    }

    [EditorToolFunction("Build/Offline-Mode/Build_AB")]
    private static void BuildOfflineRes()
    {
        BuildCli.BuildResources(ResourceRequest(ResourceBuildMode.Offline));
    }

    [EditorToolFunction("Build/Online-Mode/Build_Exe")]
    private static void BuildOnlineExe()
    {
        BuildCli.Execute(AppRequest(ResourceBuildMode.Online));
    }

    [EditorToolFunction("Build/Online-Mode/Build_AB")]
    private static void BuildOnlineRes()
    {
        BuildCli.BuildResources(ResourceRequest(ResourceBuildMode.Online));
    }

    private static BuildRequest AppRequest(ResourceBuildMode mode)
    {
        return new BuildRequest
        {
            BuildTarget = BuildTarget.StandaloneWindows64,
            OutputDirectory = OutputRoot,
            ResourceOutputDirectory = BundleRoot,
            FileName = FileName,
            Scenes = Scenes,
            Development = false,
            ResourceMode = mode,
            PlayMode = mode == ResourceBuildMode.Online
                ? (int)EPlayMode.HostPlayMode
                : (int)EPlayMode.OfflinePlayMode,
        };
    }

    private static BuildRequest ResourceRequest(ResourceBuildMode mode)
    {
        return new BuildRequest
        {
            BuildTarget = BuildTarget.StandaloneWindows64,
            OutputDirectory = BundleRoot,
            ResourceMode = mode,
            PlayMode = mode == ResourceBuildMode.Online
                ? (int)EPlayMode.HostPlayMode
                : (int)EPlayMode.OfflinePlayMode,
        };
    }
}
