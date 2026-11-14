using System;
using System.Collections.Generic;
using System.IO;
using AlicizaX.Editor.Toolbar;
using UnityEditor;
using UnityEngine;
using YooAsset;

namespace AlicizaX.Editor.Tools
{
    /// <summary>
    /// 清理 YooAsset 在编辑器下产生的热更（Host 模式）资源缓存。
    /// </summary>
    /// <remarks>
    /// YooAsset 3.x 在编辑器下的缓存根目录为 &lt;Project&gt;/Library/&lt;YooFolderName&gt;，
    /// 每个资源包在根目录下有一个同名子目录（BundleFiles / ManifestFiles / TempFiles / UnpackManifestFiles）。
    /// 该目录不参与版本管理，清理后下次以 Host 模式运行时会重新从 CDN 下载资源。
    /// </remarks>
    internal static class ClearHotUpdateCacheTool
    {
        private const string ToolMenuPath = "YooAsset/Clear Hot-Update Cache";

        private struct PackageCacheInfo
        {
            public string Name;
            public string Directory;
            public long Size;
            public int FileCount;
        }

        /// <summary>
        /// 工具栏 Tools 菜单入口：清理热更缓存。
        /// </summary>
        [EditorToolFunction(ToolMenuPath)]
        private static void ClearHotUpdateCache()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("清理热更缓存", "请先退出播放模式（Play Mode）后再清理热更缓存。", "确定");
                return;
            }

            string cacheRoot = GetEditorCacheRoot();
            if (string.IsNullOrEmpty(cacheRoot))
                return;

            if (!Directory.Exists(cacheRoot))
            {
                Debug.Log($"[热更缓存] 未找到编辑器缓存目录，无需清理：{cacheRoot}");
                return;
            }

            List<PackageCacheInfo> packages = CollectPackages(cacheRoot);
            if (packages.Count == 0)
            {
                Debug.Log($"[热更缓存] 缓存目录为空，无需清理：{cacheRoot}");
                return;
            }

            int totalFileCount = 0;
            long totalSize = 0;
            string[] packageNames = new string[packages.Count];
            for (int i = 0; i < packages.Count; i++)
            {
                totalFileCount += packages[i].FileCount;
                totalSize += packages[i].Size;
                packageNames[i] = packages[i].Name;
            }

            string message =
                $"即将删除 {packages.Count} 个资源包的缓存（{totalFileCount} 个文件，共 {FormatSize(totalSize)}）：\n" +
                $"{string.Join("、", packageNames)}\n\n" +
                $"目录：{cacheRoot}\n\n" +
                "删除后，下次进入 Host 模式时会重新从 CDN 下载资源。是否继续？";
            if (!EditorUtility.DisplayDialog("清理热更缓存", message, "删除", "取消"))
                return;

            // 如果上一次播放模式关闭了域重载，YooAssets 可能仍持有缓存文件的句柄，先行释放。
            if (YooAssets.IsInitialized)
            {
                Debug.Log("[热更缓存] 检测到 YooAssets 仍处于初始化状态，先释放资源缓存句柄。");
                YooAssets.Destroy();
            }

            int deletedPackageCount = 0;
            long deletedSize = 0;
            List<string> failures = null;
            for (int i = 0; i < packages.Count; i++)
            {
                PackageCacheInfo package = packages[i];
                EditorUtility.DisplayProgressBar("清理热更缓存", $"正在删除 {package.Name} ...", (float)i / packages.Count);
                if (TryDeleteDirectory(package.Directory, out string error))
                {
                    deletedPackageCount++;
                    deletedSize += package.Size;
                }
                else
                {
                    if (failures == null)
                        failures = new List<string>();
                    failures.Add($"{package.Name}: {error}");
                }
            }

            EditorUtility.ClearProgressBar();
            DeleteEmptyRoot(cacheRoot);

            string summary = $"[热更缓存] 已清理 {deletedPackageCount} 个资源包，释放 {FormatSize(deletedSize)}：{cacheRoot}";
            if (failures == null || failures.Count == 0)
            {
                Debug.Log(summary);
                return;
            }

            Debug.LogError($"{summary}\n以下资源包删除失败（文件可能被占用），请关闭 Unity 后手动删除缓存目录：\n{string.Join("\n", failures)}");
            EditorUtility.DisplayDialog("清理热更缓存", "部分缓存删除失败，详情见 Console。可关闭 Unity 后手动删除缓存目录。", "确定");
        }

        /// <summary>
        /// 获取编辑器下的缓存根目录：&lt;Project&gt;/Library/&lt;YooFolderName&gt;。
        /// </summary>
        /// <returns>缓存根目录；无法安全定位时返回 null。</returns>
        private static string GetEditorCacheRoot()
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot))
            {
                Debug.LogError("[热更缓存] 无法从 Application.dataPath 解析项目根目录。");
                return null;
            }

            string folderName = YooAssetConfiguration.GetYooFolderName();
            if (string.IsNullOrEmpty(folderName))
            {
                Debug.LogError("[热更缓存] YooAssetSettings.YooFolderName 为空时，缓存目录会与 Library 根目录重合，已取消操作以免误删。");
                return null;
            }

            string libraryRoot;
            string cacheRoot;
            try
            {
                libraryRoot = Path.GetFullPath(Path.Combine(projectRoot, "Library"));
                cacheRoot = Path.GetFullPath(Path.Combine(libraryRoot, folderName));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[热更缓存] 缓存目录解析失败：{exception.Message}");
                return null;
            }

            if (!cacheRoot.StartsWith(libraryRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogError($"[热更缓存] 计算出的缓存目录不在 Library 下，已取消操作：{cacheRoot}");
                return null;
            }

            return cacheRoot;
        }

        /// <summary>
        /// 收集缓存根目录下的所有资源包目录，并统计其大小。
        /// </summary>
        private static List<PackageCacheInfo> CollectPackages(string cacheRoot)
        {
            var packages = new List<PackageCacheInfo>();
            string[] directories;
            try
            {
                directories = Directory.GetDirectories(cacheRoot);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[热更缓存] 读取缓存目录失败：{cacheRoot}\n{exception}");
                return packages;
            }

            try
            {
                for (int i = 0; i < directories.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("清理热更缓存", "正在统计缓存大小...", (float)i / directories.Length);
                    int fileCount = 0;
                    long size = CalculateDirectorySize(directories[i], ref fileCount);
                    packages.Add(new PackageCacheInfo
                    {
                        Name = Path.GetFileName(directories[i]),
                        Directory = directories[i],
                        Size = size,
                        FileCount = fileCount,
                    });
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return packages;
        }

        private static long CalculateDirectorySize(string directory, ref int fileCount)
        {
            long size = 0;
            try
            {
                foreach (string filePath in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        size += new FileInfo(filePath).Length;
                        fileCount++;
                    }
                    catch (IOException)
                    {
                        // 统计阶段忽略瞬时不可访问的文件。
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return size;
        }

        /// <summary>
        /// 删除目录；首次失败时清除只读属性后重试一次。
        /// </summary>
        private static bool TryDeleteDirectory(string directory, out string error)
        {
            error = null;
            if (!Directory.Exists(directory))
                return true;

            try
            {
                Directory.Delete(directory, true);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }

            try
            {
                if (!Directory.Exists(directory))
                    return true;

                foreach (string filePath in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                    File.SetAttributes(filePath, FileAttributes.Normal);

                Directory.Delete(directory, true);
                error = null;
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private static void DeleteEmptyRoot(string cacheRoot)
        {
            try
            {
                if (Directory.Exists(cacheRoot) && Directory.GetFileSystemEntries(cacheRoot).Length == 0)
                    Directory.Delete(cacheRoot);
            }
            catch (Exception)
            {
                // 根目录清理失败不影响整体结果。
            }
        }

        private static string FormatSize(long bytes)
        {
            if (bytes >= 1024L * 1024L * 1024L)
                return $"{bytes / (1024f * 1024f * 1024f):F2} GB";
            if (bytes >= 1024L * 1024L)
                return $"{bytes / (1024f * 1024f):F2} MB";
            if (bytes >= 1024L)
                return $"{bytes / 1024f:F2} KB";
            return $"{bytes} B";
        }
    }
}
