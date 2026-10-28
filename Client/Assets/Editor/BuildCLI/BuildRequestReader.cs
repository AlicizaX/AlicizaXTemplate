using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using YooAsset;

public static class BuildRequestReader
{
    [Serializable]
    private sealed class FileParams
    {
        public string buildTarget;
        public string outputPath;
        public string[] scenes;
        public string defines;
        public string versionName;
        public int versionCode;
        public bool development;
    }

    public static BuildRequest Read()
    {
        string path = GetArg("-uclParams");
        FileParams file = JsonUtility.FromJson<FileParams>(File.ReadAllText(path));

        var request = new BuildRequest
        {
            BuildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), file.buildTarget, true),
            OutputDirectory = Path.GetDirectoryName(file.outputPath),
            FileName = Path.GetFileName(file.outputPath),
            Scenes = file.scenes,
            Defines = file.defines,
            VersionName = file.versionName,
            VersionCode = file.versionCode,
            Development = file.development,
            ResourceMode = ParseEnum<ResourceBuildMode>(Required("RES_MODE")),
            PackageVersion = Environment.GetEnvironmentVariable("UCL_PARAM_PACKAGE_VERSION"),
            Language = Environment.GetEnvironmentVariable("UCL_PARAM_LANGUAGE"),
            Decryption = Environment.GetEnvironmentVariable("UCL_PARAM_DECRYPTION"),
            Encryption = Environment.GetEnvironmentVariable("UCL_PARAM_ENCRYPTION"),
        };
        request.PlayMode = PlayModeValue(request.ResourceMode);

        if (request.Scenes == null || request.Scenes.Length == 0)
        {
            throw new Exception("构建场景列表为空");
        }

        return request;
    }

    public static BuildRequest ReadResources()
    {
        string path = GetArg("-uclParams");
        FileParams file = JsonUtility.FromJson<FileParams>(File.ReadAllText(path));
        var request = new BuildRequest
        {
            BuildTarget = (BuildTarget)Enum.Parse(typeof(BuildTarget), file.buildTarget, true),
            OutputDirectory = Path.GetDirectoryName(file.outputPath),
            Defines = file.defines,
            ResourceMode = ResourceBuildMode.Online,
            PackageVersion = Environment.GetEnvironmentVariable("UCL_PARAM_PACKAGE_VERSION"),
            Encryption = Environment.GetEnvironmentVariable("UCL_PARAM_ENCRYPTION"),
        };
        if (string.IsNullOrEmpty(request.OutputDirectory))
        {
            throw new Exception("资源包输出目录为空");
        }

        return request;
    }

    private static int PlayModeValue(ResourceBuildMode mode)
    {
        return mode == ResourceBuildMode.Online
            ? (int)EPlayMode.HostPlayMode
            : (int)EPlayMode.OfflinePlayMode;
    }

    private static string Required(string key)
    {
        string value = Environment.GetEnvironmentVariable("UCL_PARAM_" + key);
        if (string.IsNullOrEmpty(value))
        {
            throw new Exception("缺少 UCL_PARAM_" + key);
        }

        return value;
    }

    private static T ParseEnum<T>(string value) where T : struct
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }

    private static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
            {
                return args[i + 1];
            }
        }

        throw new Exception("缺少参数 " + name);
    }
}
