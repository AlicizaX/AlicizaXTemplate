using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

    // {{ AOT assemblies
    public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
    {
        "AlicizaX.Framework.Runtime.dll",
        "AlicizaX.Localization.Runtime.dll",
        "AlicizaX.UI.Extension.dll",
        "Luban.Runtime.dll",
        "System.Core.dll",
        "System.Runtime.CompilerServices.Unsafe.dll",
        "UniTask.Runtime.dll",
        "UnityEngine.CoreModule.dll",
        "ZString.dll",
        "mscorlib.dll",
    };
    // }}

    // {{ constraint implement type
    // }} 

    // {{ AOT generic types
    // AlicizaX.EventContainer.HandlerComparer<GameLogic.Player.PlayerDataChangedEvent,object>
    // AlicizaX.EventContainer<GameLogic.Player.PlayerDataChangedEvent,object>
    // AlicizaX.EventInitialSize<GameLogic.Player.PlayerDataChangedEvent>
    // AlicizaX.InEventHandler<GameLogic.Player.PlayerDataChangedEvent>
    // AlicizaX.Resource.Runtime.AssetLease<object>
    // AlicizaX.UI.Runtime.UIWidget<object>
    // AlicizaX.UI.Runtime.UIWindow<object>
    // AlicizaX.UI.UGListBase<object,object>
    // Cysharp.Text.Utf16ValueStringBuilder.TryFormat<int>
    // Cysharp.Text.Utf16ValueStringBuilder.TryFormat<object>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Game.Config.ConfigService.<Initialize>d__20>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Game.Config.Tables.Global.<LoadAsync>d__13>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Game.Config.Tables.Shop.<LoadAsync>d__12>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Game.Config.Tables.TbAudio.<LoadAsync>d__12>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<Game.Config.Tables.TbItem.<LoadAsync>d__12>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GameLogic.HotfixEntry.ConfigBytesLoader.<LoadCommonAsync>d__0,object>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask.<>c<GameLogic.UI.UIHomeWindow.<SwitchLanguageAsync>d__4>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Game.Config.ConfigService.<Initialize>d__20>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Game.Config.Tables.Global.<LoadAsync>d__13>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Game.Config.Tables.Shop.<LoadAsync>d__12>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Game.Config.Tables.TbAudio.<LoadAsync>d__12>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<Game.Config.Tables.TbItem.<LoadAsync>d__12>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GameLogic.HotfixEntry.ConfigBytesLoader.<LoadCommonAsync>d__0,object>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTask<GameLogic.UI.UIHomeWindow.<SwitchLanguageAsync>d__4>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid.<>c<GameLogic.HotfixEntry.<EntranceAsync>d__8>
    // Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoid<GameLogic.HotfixEntry.<EntranceAsync>d__8>
    // Cysharp.Threading.Tasks.CompilerServices.IStateMachineRunnerPromise<object>
    // Cysharp.Threading.Tasks.ITaskPoolNode<object>
    // Cysharp.Threading.Tasks.IUniTaskSource<AlicizaX.Resource.Runtime.AssetLease<object>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,byte>>
    // Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,object>>
    // Cysharp.Threading.Tasks.IUniTaskSource<byte>
    // Cysharp.Threading.Tasks.IUniTaskSource<object>
    // Cysharp.Threading.Tasks.Internal.StatePool<Cysharp.Threading.Tasks.UniTask.Awaiter<object>>
    // Cysharp.Threading.Tasks.Internal.StateTuple<Cysharp.Threading.Tasks.UniTask.Awaiter<object>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<AlicizaX.Resource.Runtime.AssetLease<object>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,byte>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,object>>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<byte>
    // Cysharp.Threading.Tasks.UniTask.Awaiter<object>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<AlicizaX.Resource.Runtime.AssetLease<object>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,byte>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,object>>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<byte>
    // Cysharp.Threading.Tasks.UniTask.IsCanceledSource<object>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<AlicizaX.Resource.Runtime.AssetLease<object>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,byte>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,object>>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<byte>
    // Cysharp.Threading.Tasks.UniTask.MemoizeSource<object>
    // Cysharp.Threading.Tasks.UniTask<AlicizaX.Resource.Runtime.AssetLease<object>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,byte>>
    // Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,object>>
    // Cysharp.Threading.Tasks.UniTask<byte>
    // Cysharp.Threading.Tasks.UniTask<object>
    // Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<Cysharp.Threading.Tasks.AsyncUnit>
    // Cysharp.Threading.Tasks.UniTaskCompletionSourceCore<object>
    // Cysharp.Threading.Tasks.UniTaskExtensions.<>c__19<object>
    // System.Action<Cysharp.Threading.Tasks.UniTask>
    // System.Action<GameLogic.Player.FakeBagItemData>
    // System.Action<UnityEngine.Color>
    // System.Action<int,int>
    // System.Action<int>
    // System.Action<object,object>
    // System.Action<object>
    // System.Buffers.ArrayPool<ushort>
    // System.Buffers.TlsOverPerCoreLockedStacksArrayPool.LockedStack<ushort>
    // System.Buffers.TlsOverPerCoreLockedStacksArrayPool.PerCoreLockedStacks<ushort>
    // System.Buffers.TlsOverPerCoreLockedStacksArrayPool<ushort>
    // System.ByReference<ushort>
    // System.Collections.Concurrent.ConcurrentQueue.<Enumerate>d__28<object>
    // System.Collections.Concurrent.ConcurrentQueue.Segment<object>
    // System.Collections.Concurrent.ConcurrentQueue<object>
    // System.Collections.Generic.ArraySortHelper<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.ArraySortHelper<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.ArraySortHelper<UnityEngine.Color>
    // System.Collections.Generic.ArraySortHelper<object>
    // System.Collections.Generic.Comparer<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Collections.Generic.Comparer<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.Comparer<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,byte>>
    // System.Collections.Generic.Comparer<System.ValueTuple<byte,object>>
    // System.Collections.Generic.Comparer<UnityEngine.Color>
    // System.Collections.Generic.Comparer<byte>
    // System.Collections.Generic.Comparer<object>
    // System.Collections.Generic.Dictionary.Enumerator<System.RuntimeTypeHandle,object>
    // System.Collections.Generic.Dictionary.Enumerator<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>
    // System.Collections.Generic.Dictionary.Enumerator<int,object>
    // System.Collections.Generic.Dictionary.Enumerator<object,int>
    // System.Collections.Generic.Dictionary.Enumerator<object,object>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<System.RuntimeTypeHandle,object>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
    // System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
    // System.Collections.Generic.Dictionary.KeyCollection<System.RuntimeTypeHandle,object>
    // System.Collections.Generic.Dictionary.KeyCollection<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>
    // System.Collections.Generic.Dictionary.KeyCollection<int,object>
    // System.Collections.Generic.Dictionary.KeyCollection<object,int>
    // System.Collections.Generic.Dictionary.KeyCollection<object,object>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<System.RuntimeTypeHandle,object>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
    // System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
    // System.Collections.Generic.Dictionary.ValueCollection<System.RuntimeTypeHandle,object>
    // System.Collections.Generic.Dictionary.ValueCollection<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>
    // System.Collections.Generic.Dictionary.ValueCollection<int,object>
    // System.Collections.Generic.Dictionary.ValueCollection<object,int>
    // System.Collections.Generic.Dictionary.ValueCollection<object,object>
    // System.Collections.Generic.Dictionary<System.RuntimeTypeHandle,object>
    // System.Collections.Generic.Dictionary<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>
    // System.Collections.Generic.Dictionary<int,object>
    // System.Collections.Generic.Dictionary<object,int>
    // System.Collections.Generic.Dictionary<object,object>
    // System.Collections.Generic.EqualityComparer<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Collections.Generic.EqualityComparer<AlicizaX.UI.GroupAdapter.TypeStats<object>>
    // System.Collections.Generic.EqualityComparer<System.RuntimeTypeHandle>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,byte>>
    // System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,object>>
    // System.Collections.Generic.EqualityComparer<byte>
    // System.Collections.Generic.EqualityComparer<int>
    // System.Collections.Generic.EqualityComparer<object>
    // System.Collections.Generic.ICollection<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.ICollection<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.RuntimeTypeHandle,object>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
    // System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.ICollection<UnityEngine.Color>
    // System.Collections.Generic.ICollection<object>
    // System.Collections.Generic.IComparer<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.IComparer<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.IComparer<UnityEngine.Color>
    // System.Collections.Generic.IComparer<object>
    // System.Collections.Generic.IEnumerable<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.IEnumerable<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.RuntimeTypeHandle,object>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
    // System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.IEnumerable<UnityEngine.Color>
    // System.Collections.Generic.IEnumerable<object>
    // System.Collections.Generic.IEnumerator<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.IEnumerator<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<System.RuntimeTypeHandle,object>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
    // System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
    // System.Collections.Generic.IEnumerator<UnityEngine.Color>
    // System.Collections.Generic.IEnumerator<object>
    // System.Collections.Generic.IEqualityComparer<System.RuntimeTypeHandle>
    // System.Collections.Generic.IEqualityComparer<int>
    // System.Collections.Generic.IEqualityComparer<object>
    // System.Collections.Generic.IList<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.IList<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.IList<UnityEngine.Color>
    // System.Collections.Generic.IList<object>
    // System.Collections.Generic.KeyValuePair<System.RuntimeTypeHandle,object>
    // System.Collections.Generic.KeyValuePair<int,AlicizaX.UI.GroupAdapter.TypeStats<object>>
    // System.Collections.Generic.KeyValuePair<int,object>
    // System.Collections.Generic.KeyValuePair<object,int>
    // System.Collections.Generic.KeyValuePair<object,object>
    // System.Collections.Generic.List.Enumerator<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.List.Enumerator<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.List.Enumerator<UnityEngine.Color>
    // System.Collections.Generic.List.Enumerator<object>
    // System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.List<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.List<UnityEngine.Color>
    // System.Collections.Generic.List<object>
    // System.Collections.Generic.ObjectComparer<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Collections.Generic.ObjectComparer<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.Generic.ObjectComparer<GameLogic.Player.FakeBagItemData>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,byte>>
    // System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,object>>
    // System.Collections.Generic.ObjectComparer<UnityEngine.Color>
    // System.Collections.Generic.ObjectComparer<byte>
    // System.Collections.Generic.ObjectComparer<object>
    // System.Collections.Generic.ObjectEqualityComparer<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Collections.Generic.ObjectEqualityComparer<AlicizaX.UI.GroupAdapter.TypeStats<object>>
    // System.Collections.Generic.ObjectEqualityComparer<System.RuntimeTypeHandle>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,byte>>
    // System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,object>>
    // System.Collections.Generic.ObjectEqualityComparer<byte>
    // System.Collections.Generic.ObjectEqualityComparer<int>
    // System.Collections.Generic.ObjectEqualityComparer<object>
    // System.Collections.ObjectModel.ReadOnlyCollection<Cysharp.Threading.Tasks.UniTask>
    // System.Collections.ObjectModel.ReadOnlyCollection<GameLogic.Player.FakeBagItemData>
    // System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Color>
    // System.Collections.ObjectModel.ReadOnlyCollection<object>
    // System.Comparison<Cysharp.Threading.Tasks.UniTask>
    // System.Comparison<GameLogic.Player.FakeBagItemData>
    // System.Comparison<UnityEngine.Color>
    // System.Comparison<object>
    // System.Func<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Func<Cysharp.Threading.Tasks.UniTask<object>>
    // System.Func<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Func<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Func<System.ValueTuple<byte,byte>>
    // System.Func<System.ValueTuple<byte,object>>
    // System.Func<byte>
    // System.Func<int>
    // System.Func<object,AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Func<object,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Func<object,System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Func<object,System.ValueTuple<byte,byte>>
    // System.Func<object,System.ValueTuple<byte,object>>
    // System.Func<object,byte>
    // System.Func<object,object>
    // System.Func<object>
    // System.Linq.Enumerable.<TakeIterator>d__25<object>
    // System.Predicate<Cysharp.Threading.Tasks.UniTask>
    // System.Predicate<GameLogic.Player.FakeBagItemData>
    // System.Predicate<UnityEngine.Color>
    // System.Predicate<object>
    // System.ReadOnlySpan<ushort>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,byte>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.ValueTuple<byte,object>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<byte>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,byte>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.ValueTuple<byte,object>>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<byte>
    // System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,byte>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<System.ValueTuple<byte,object>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<byte>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable.ConfiguredValueTaskAwaiter<object>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,byte>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<System.ValueTuple<byte,object>>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<byte>
    // System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable<object>
    // System.Runtime.CompilerServices.TaskAwaiter<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,byte>>
    // System.Runtime.CompilerServices.TaskAwaiter<System.ValueTuple<byte,object>>
    // System.Runtime.CompilerServices.TaskAwaiter<byte>
    // System.Runtime.CompilerServices.TaskAwaiter<object>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,byte>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<System.ValueTuple<byte,object>>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<byte>
    // System.Runtime.CompilerServices.ValueTaskAwaiter<object>
    // System.Span<ushort>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,byte>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<System.ValueTuple<byte,object>>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<byte>
    // System.Threading.Tasks.ContinuationTaskFromResultTask<object>
    // System.Threading.Tasks.Sources.IValueTaskSource<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,byte>>
    // System.Threading.Tasks.Sources.IValueTaskSource<System.ValueTuple<byte,object>>
    // System.Threading.Tasks.Sources.IValueTaskSource<byte>
    // System.Threading.Tasks.Sources.IValueTaskSource<object>
    // System.Threading.Tasks.Task<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,byte>>
    // System.Threading.Tasks.Task<System.ValueTuple<byte,object>>
    // System.Threading.Tasks.Task<byte>
    // System.Threading.Tasks.Task<object>
    // System.Threading.Tasks.TaskFactory<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,byte>>
    // System.Threading.Tasks.TaskFactory<System.ValueTuple<byte,object>>
    // System.Threading.Tasks.TaskFactory<byte>
    // System.Threading.Tasks.TaskFactory<object>
    // System.Threading.Tasks.ValueTask<AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,byte>>
    // System.Threading.Tasks.ValueTask<System.ValueTuple<byte,object>>
    // System.Threading.Tasks.ValueTask<byte>
    // System.Threading.Tasks.ValueTask<object>
    // System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>
    // System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,AlicizaX.Resource.Runtime.AssetLease<object>>>>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,byte>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>
    // System.ValueTuple<byte,System.ValueTuple<byte,byte>>
    // System.ValueTuple<byte,System.ValueTuple<byte,object>>
    // System.ValueTuple<byte,byte>
    // System.ValueTuple<byte,object>
    // UnityEngine.Events.InvokableCall<byte>
    // UnityEngine.Events.InvokableCall<int>
    // UnityEngine.Events.InvokableCall<object>
    // UnityEngine.Events.UnityAction<byte>
    // UnityEngine.Events.UnityAction<int>
    // UnityEngine.Events.UnityAction<object>
    // UnityEngine.Events.UnityEvent<byte>
    // UnityEngine.Events.UnityEvent<int>
    // UnityEngine.Events.UnityEvent<object>
    // }}

    public void RefMethods()
    {
        // System.Void AlicizaX.EventBus.Publish<GameLogic.Player.PlayerDataChangedEvent>(GameLogic.Player.PlayerDataChangedEvent&)
        // System.Void AlicizaX.EventBus.ReservePayloadCapacity<GameLogic.Player.PlayerDataChangedEvent>(int)
        // System.Void AlicizaX.EventListenerProxy.AddUIEvent<GameLogic.Player.PlayerDataChangedEvent>(AlicizaX.InEventHandler<GameLogic.Player.PlayerDataChangedEvent>)
        // string AlicizaX.Localization.Runtime.LocalizationKit.Format<int>(string,int)
        // string AlicizaX.Localization.Runtime.LocalizationKit.Format<object,int,int>(string,object,int,int)
        // string AlicizaX.Localization.Runtime.LocalizationKit.Format<object,int>(string,object,int)
        // string AlicizaX.Localization.Runtime.LocalizationKit.Format<object,object,object>(string,object,object,object)
        // string AlicizaX.Localization.Runtime.LocalizationKit.Format<object,object>(string,object,object)
        // string AlicizaX.Localization.Runtime.LocalizationKit.Format<object>(string,object)
        // Cysharp.Threading.Tasks.UniTask<AlicizaX.Resource.Runtime.AssetLease<object>> AlicizaX.Resource.Runtime.IResourceService.LoadLeaseAsync<object>(string,System.Threading.CancellationToken,string)
        // Cysharp.Threading.Tasks.UniTask<object> AlicizaX.UI.Runtime.IUIService.ShowUI<object>(object)
        // object AlicizaX.UI.Runtime.UIBase.CreateWidgetSync<object>(UnityEngine.Transform,bool)
        // System.Void AlicizaX.UI.Runtime.UIMetaRegistry.RegisterWidget<object,object>(System.Func<object>,bool)
        // System.Void AlicizaX.UI.Runtime.UIMetaRegistry.RegisterWindow<object,object>(System.Func<object>,AlicizaX.UI.Runtime.UILayer,int,bool)
        // System.Void Cysharp.Text.Utf16ValueStringBuilder.AppendFormat<int>(string,int)
        // System.Void Cysharp.Text.Utf16ValueStringBuilder.AppendFormat<object,int,int>(string,object,int,int)
        // System.Void Cysharp.Text.Utf16ValueStringBuilder.AppendFormat<object,int>(string,object,int)
        // System.Void Cysharp.Text.Utf16ValueStringBuilder.AppendFormat<object,object,object>(string,object,object,object)
        // System.Void Cysharp.Text.Utf16ValueStringBuilder.AppendFormat<object,object>(string,object,object)
        // System.Void Cysharp.Text.Utf16ValueStringBuilder.AppendFormat<object>(string,object)
        // System.Void Cysharp.Text.Utf16ValueStringBuilder.AppendFormatInternal<int>(int,int,System.ReadOnlySpan<System.Char>,string)
        // System.Void Cysharp.Text.Utf16ValueStringBuilder.AppendFormatInternal<object>(object,int,System.ReadOnlySpan<System.Char>,string)
        // string Cysharp.Text.ZString.Format<int>(string,int)
        // string Cysharp.Text.ZString.Format<object,int,int>(string,object,int,int)
        // string Cysharp.Text.ZString.Format<object,int>(string,object,int)
        // string Cysharp.Text.ZString.Format<object,object,object>(string,object,object,object)
        // string Cysharp.Text.ZString.Format<object,object>(string,object,object)
        // string Cysharp.Text.ZString.Format<object>(string,object)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,Game.Config.ConfigService.<Initialize>d__20>(Cysharp.Threading.Tasks.UniTask.Awaiter&,Game.Config.ConfigService.<Initialize>d__20&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,GameLogic.UI.UIHomeWindow.<SwitchLanguageAsync>d__4>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,GameLogic.UI.UIHomeWindow.<SwitchLanguageAsync>d__4&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,Game.Config.Tables.Global.<LoadAsync>d__13>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,Game.Config.Tables.Global.<LoadAsync>d__13&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,Game.Config.Tables.Shop.<LoadAsync>d__12>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,Game.Config.Tables.Shop.<LoadAsync>d__12&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,Game.Config.Tables.TbAudio.<LoadAsync>d__12>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,Game.Config.Tables.TbAudio.<LoadAsync>d__12&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,Game.Config.Tables.TbItem.<LoadAsync>d__12>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,Game.Config.Tables.TbItem.<LoadAsync>d__12&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<AlicizaX.Resource.Runtime.AssetLease<object>>,GameLogic.HotfixEntry.ConfigBytesLoader.<LoadCommonAsync>d__0>(Cysharp.Threading.Tasks.UniTask.Awaiter<AlicizaX.Resource.Runtime.AssetLease<object>>&,GameLogic.HotfixEntry.ConfigBytesLoader.<LoadCommonAsync>d__0&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Config.ConfigService.<Initialize>d__20>(Game.Config.ConfigService.<Initialize>d__20&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Config.Tables.Global.<LoadAsync>d__13>(Game.Config.Tables.Global.<LoadAsync>d__13&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Config.Tables.Shop.<LoadAsync>d__12>(Game.Config.Tables.Shop.<LoadAsync>d__12&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Config.Tables.TbAudio.<LoadAsync>d__12>(Game.Config.Tables.TbAudio.<LoadAsync>d__12&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<Game.Config.Tables.TbItem.<LoadAsync>d__12>(Game.Config.Tables.TbItem.<LoadAsync>d__12&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder.Start<GameLogic.UI.UIHomeWindow.<SwitchLanguageAsync>d__4>(GameLogic.UI.UIHomeWindow.<SwitchLanguageAsync>d__4&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskMethodBuilder<object>.Start<GameLogic.HotfixEntry.ConfigBytesLoader.<LoadCommonAsync>d__0>(GameLogic.HotfixEntry.ConfigBytesLoader.<LoadCommonAsync>d__0&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter,GameLogic.HotfixEntry.<EntranceAsync>d__8>(Cysharp.Threading.Tasks.UniTask.Awaiter&,GameLogic.HotfixEntry.<EntranceAsync>d__8&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<byte>,GameLogic.HotfixEntry.<EntranceAsync>d__8>(Cysharp.Threading.Tasks.UniTask.Awaiter<byte>&,GameLogic.HotfixEntry.<EntranceAsync>d__8&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,GameLogic.HotfixEntry.<EntranceAsync>d__8>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,GameLogic.HotfixEntry.<EntranceAsync>d__8&)
        // System.Void Cysharp.Threading.Tasks.CompilerServices.AsyncUniTaskVoidMethodBuilder.Start<GameLogic.HotfixEntry.<EntranceAsync>d__8>(GameLogic.HotfixEntry.<EntranceAsync>d__8&)
        // Cysharp.Threading.Tasks.Internal.StateTuple<Cysharp.Threading.Tasks.UniTask.Awaiter<object>> Cysharp.Threading.Tasks.Internal.StateTuple.Create<Cysharp.Threading.Tasks.UniTask.Awaiter<object>>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>)
        // System.Void Cysharp.Threading.Tasks.UniTaskExtensions.Forget<object>(Cysharp.Threading.Tasks.UniTask<object>)
        // string Luban.StringUtil.CollectionToString<UnityEngine.Color>(System.Collections.Generic.IEnumerable<UnityEngine.Color>)
        // string Luban.StringUtil.CollectionToString<object>(System.Collections.Generic.IEnumerable<object>)
        // object[] System.Array.Empty<object>()
        // System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Take<object>(System.Collections.Generic.IEnumerable<object>,int)
        // System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.TakeIterator<object>(System.Collections.Generic.IEnumerable<object>,int)
        // System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
        // System.Span<ushort> System.MemoryExtensions.AsSpan<ushort>(ushort[],int)
        // ushort& System.Runtime.CompilerServices.Unsafe.Add<ushort>(ushort&,int)
        // ushort& System.Runtime.CompilerServices.Unsafe.As<byte,ushort>(byte&)
        // int System.Runtime.CompilerServices.Unsafe.SizeOf<int>()
        // int System.Runtime.CompilerServices.Unsafe.SizeOf<object>()
        // object UnityEngine.Component.GetComponent<object>()
        // object UnityEngine.Component.GetComponentInChildren<object>(bool)
        // object UnityEngine.GameObject.AddComponent<object>()
        // object UnityEngine.Resources.Load<object>(string)
        // string string.Join<UnityEngine.Color>(string,System.Collections.Generic.IEnumerable<UnityEngine.Color>)
        // string string.Join<object>(string,System.Collections.Generic.IEnumerable<object>)
        // string string.JoinCore<UnityEngine.Color>(System.Char*,int,System.Collections.Generic.IEnumerable<UnityEngine.Color>)
        // string string.JoinCore<object>(System.Char*,int,System.Collections.Generic.IEnumerable<object>)
    }
}
