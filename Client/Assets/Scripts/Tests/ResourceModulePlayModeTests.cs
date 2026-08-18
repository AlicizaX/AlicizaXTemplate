using System;
using System.Collections;
using AlicizaX;
using AlicizaX.ObjectPool;
using AlicizaX.Resource.Runtime;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using YooAsset;
using Object = UnityEngine.Object;

namespace Game.Tests
{
    public sealed class ResourceModulePlayModeTests
    {
        private const string DefaultPackageName = "DefaultPackage";
        private const string AudioLocation = "UI_MouseClick_01_A";
        private const string PrefabLocation = "UIHomeWindow";

        private GameObject _root;
        private IResourceService _resource;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return BootstrapAsync().ToCoroutine();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _resource = null;
            if (_root != null)
            {
                Object.Destroy(_root);
                _root = null;
            }

            yield return null;
            if (AppServices.HasWorld)
            {
                AppServices.Shutdown();
            }

            YooAssets.Destroy();
        }

        [UnityTest]
        public IEnumerator InitPackageAsync_SharesInFlightTask()
        {
            yield return Run(async () =>
            {
                UniTask<bool> first = _resource.InitPackageAsync();
                UniTask<bool> second = _resource.InitPackageAsync();
                (bool firstSucceeded, bool secondSucceeded) = await UniTask.WhenAll(first, second);

                Assert.IsTrue(firstSucceeded, "First InitPackageAsync should succeed.");
                Assert.IsTrue(secondSucceeded, "Second concurrent InitPackageAsync should share the in-flight task and succeed.");
                Assert.AreEqual(EOperationStatus.Succeeded, YooAssets.GetPackage(DefaultPackageName).InitializeStatus);
            });
        }

        [UnityTest]
        public IEnumerator LoadLease_SyncJoinsAsync_KeepsSingleRecord()
        {
            yield return Run(async () =>
            {
                await EnsurePackageReady();
                AssertLocation(AudioLocation);

                UniTask<ResourceAssetLease<AudioClip>> asyncLoad = _resource.LoadLeaseAsync<AudioClip>(AudioLocation);
                ResourceAssetLease<AudioClip> syncLease = _resource.LoadLease<AudioClip>(AudioLocation);
                ResourceAssetLease<AudioClip> asyncLease = await asyncLoad;

                try
                {
                    Assert.IsTrue(syncLease.IsValid, "Sync LoadLease should join the in-flight async load.");
                    Assert.IsTrue(asyncLease.IsValid, "Async LoadLease should complete with a valid lease.");
                    Assert.AreSame(syncLease.Asset, asyncLease.Asset);

                    ResourceAssetInfo info = FindRequiredRecord(AudioLocation, typeof(AudioClip).Name);
                    Assert.IsTrue(info.HandleValid, "Merged load should keep one valid Yoo handle.");
                    Assert.AreEqual(1, CountValidRecords(AudioLocation, typeof(AudioClip).Name));
                    Assert.GreaterOrEqual(info.DirectRefCount, 2, "Both leases should retain the same asset record.");
                }
                finally
                {
                    syncLease.Dispose();
                    asyncLease.Dispose();
                }
            });
        }

        [UnityTest]
        public IEnumerator AudioClipLease_UnloadAfterRelease_DropsHandle()
        {
            yield return Run(async () =>
            {
                await EnsurePackageReady();
                AssertLocation(AudioLocation);

                ResourceAssetLease<AudioClip> lease = _resource.LoadLease<AudioClip>(AudioLocation);
                Assert.IsTrue(lease.IsValid);
                Assert.IsNotNull(lease.Asset);
                lease.Dispose();

                ResourceAssetInfo idleInfo = FindRequiredRecord(AudioLocation, typeof(AudioClip).Name);
                Assert.AreEqual(ResourceAssetState.Idle, idleInfo.State, "Released audio clip should enter Idle.");
                Assert.IsTrue(idleInfo.HandleValid, "Idle record should keep the handle until unused unload.");

                _resource.UnloadUnusedAssets(true);
                Assert.AreEqual(0, CountValidRecords(AudioLocation, typeof(AudioClip).Name),
                    "Force unused unload should release the idle audio handle.");
            });
        }

        [UnityTest]
        public IEnumerator ForceUnloadAllAssets_AllowsOwnerReregister()
        {
            yield return Run(async () =>
            {
                await EnsurePackageReady();
                AssertLocation(PrefabLocation);

                GameObject ownerObject = new GameObject("ResourceOwnerTest");
                ResourceOwner owner = ownerObject.AddComponent<ResourceOwner>();
                IResourceBindingService binding = _resource.BindingService;
                try
                {
                    Assert.AreEqual(ResourceBindStatus.Success, binding.RegisterOwner(owner));
                    Assert.IsTrue(owner.IsRegistered);

                    ResourceAssetLease<GameObject> lease = _resource.LoadLease<GameObject>(PrefabLocation);
                    Assert.IsTrue(lease.IsValid);
                    lease.Dispose();

                    _resource.ForceUnloadAllAssets();

                    Assert.IsFalse(owner.IsRegistered, "ForceUnloadAllAssets should clear existing ResourceOwner registration.");
                    Assert.AreEqual(ResourceBindStatus.Success, binding.RegisterOwner(owner),
                        "The same BindingService should accept a new owner registration.");
                    Assert.IsTrue(owner.IsRegistered);
                    Assert.Greater(owner.OwnerId, 0);
                    Assert.AreNotEqual(0u, owner.Generation);
                }
                finally
                {
                    Object.Destroy(ownerObject);
                }
            });
        }

        private async UniTask BootstrapAsync()
        {
            if (AppServices.HasWorld)
            {
                AppServices.Shutdown();
            }

            _root = new GameObject("ResourceModuleTestRoot");
            Object.DontDestroyOnLoad(_root);
            _root.AddComponent<RootModule>();
            ResourceComponent resourceComponent = _root.AddComponent<ResourceComponent>();
            resourceComponent.PackageName = DefaultPackageName;
            _root.AddComponent<ObjectPoolComponent>();

            _resource = AppServices.App.Require<IResourceService>();
            _resource.DefaultPackageName = DefaultPackageName;
            _resource.PlayMode = EPlayMode.EditorSimulateMode;
            _resource.IdleAssetExpireTime = 60f;
            await UniTask.Yield();
        }

        private async UniTask EnsurePackageReady()
        {
            bool initialized = await _resource.InitPackageAsync();
            Assert.IsTrue(initialized, "InitPackageAsync should succeed in EditorSimulateMode.");

            RequestPackageVersionOperation versionOperation = _resource.RequestPackageVersionAsync();
            await versionOperation.ToUniTask();
            Assert.AreEqual(EOperationStatus.Succeeded, versionOperation.Status, versionOperation.Error);

            string packageVersion = _resource.PlayMode == EPlayMode.EditorSimulateMode
                ? "Simulate"
                : versionOperation.PackageVersion;
            LoadPackageManifestOperation manifestOperation = _resource.LoadPackageManifestAsync(packageVersion);
            await manifestOperation.ToUniTask();
            Assert.AreEqual(EOperationStatus.Succeeded, manifestOperation.Status, manifestOperation.Error);
        }

        private void AssertLocation(string location)
        {
            Assert.IsTrue(_resource.IsLocationValid(location), "Missing Yoo location: " + location);
        }

        private int CountValidRecords(string location, string typeName)
        {
            ResourceAssetInfo[] buffer = new ResourceAssetInfo[64];
            int total = _resource.GetAssetInfos(buffer, 0, buffer.Length);
            int count = 0;
            int start = 0;
            while (start < total)
            {
                int written = Math.Min(buffer.Length, total - start);
                if (start > 0)
                {
                    _resource.GetAssetInfos(buffer, start, written);
                }

                for (int i = 0; i < written; i++)
                {
                    ResourceAssetInfo info = buffer[i];
                    if (info.HandleValid && info.Location == location && info.TypeName == typeName)
                    {
                        count++;
                    }
                }

                start += written;
            }

            return count;
        }

        private ResourceAssetInfo FindRequiredRecord(string location, string typeName)
        {
            ResourceAssetInfo[] buffer = new ResourceAssetInfo[64];
            int total = _resource.GetAssetInfos(buffer, 0, buffer.Length);
            int start = 0;
            while (start < total)
            {
                int written = Math.Min(buffer.Length, total - start);
                if (start > 0)
                {
                    _resource.GetAssetInfos(buffer, start, written);
                }

                for (int i = 0; i < written; i++)
                {
                    ResourceAssetInfo info = buffer[i];
                    if (info.HandleValid && info.Location == location && info.TypeName == typeName)
                    {
                        return info;
                    }
                }

                start += written;
            }

            Assert.Fail("Expected a valid resource record for " + location);
            return default;
        }

        private static IEnumerator Run(Func<UniTask> test)
        {
            return test().ToCoroutine();
        }
    }
}
