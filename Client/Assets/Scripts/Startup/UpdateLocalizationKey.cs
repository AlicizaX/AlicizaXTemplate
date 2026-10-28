using AlicizaX.Localization.Runtime;

public static class UpdateLocalizationKey
{
    public static class Bootstrap
    {
        public const string UPDATE_CHECKING_Raw = "Bootstrap.Update.Checking";
        public static string UPDATE_CHECKING => LocalizationKit.Get(UPDATE_CHECKING_Raw);
        public const string UPDATE_DOWNLOADFAILED_Raw = "Bootstrap.Update.DownloadFailed";
        public static string UPDATE_DOWNLOADFAILED => LocalizationKit.Get(UPDATE_DOWNLOADFAILED_Raw);
        public const string UPDATE_DOWNLOADING_Raw = "Bootstrap.Update.Downloading";
        public static string UPDATE_DOWNLOADING<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2) => LocalizationKit.Format(UPDATE_DOWNLOADING_Raw, arg0, arg1, arg2);
        public const string UPDATE_NETWORKERROR_Raw = "Bootstrap.Update.NetworkError";
        public static string UPDATE_NETWORKERROR => LocalizationKit.Get(UPDATE_NETWORKERROR_Raw);
        public const string UPDATE_NOASSETS_Raw = "Bootstrap.Update.NoAssets";
        public static string UPDATE_NOASSETS => LocalizationKit.Get(UPDATE_NOASSETS_Raw);
        public const string UPDATE_READY_Raw = "Bootstrap.Update.Ready";
        public static string UPDATE_READY => LocalizationKit.Get(UPDATE_READY_Raw);
        public const string UPDATE_RETRY_Raw = "Bootstrap.Update.Retry";
        public static string UPDATE_RETRY => LocalizationKit.Get(UPDATE_RETRY_Raw);
        public const string UPDATE_SIZE_Raw = "Bootstrap.Update.Size";
        public static string UPDATE_SIZE<T0, T1>(T0 arg0, T1 arg1) => LocalizationKit.Format(UPDATE_SIZE_Raw, arg0, arg1);
        public const string UPDATE_SPEED_Raw = "Bootstrap.Update.Speed";
        public static string UPDATE_SPEED<T0, T1>(T0 arg0, T1 arg1) => LocalizationKit.Format(UPDATE_SPEED_Raw, arg0, arg1);
    }
}
