using AlicizaX;
using AlicizaX.Audio.Runtime;
using UnityEngine;

namespace GameLogic
{
    internal static class GameAudioSettings
    {
        private const string MasterVolumeKey = "Game.Settings.MasterVolume";

        public static float MasterVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));

        public static void ApplyMasterVolume(float volume)
        {
            if (AppServices.TryGet(out IAudioService audio)) audio.Volume = volume;
            else AudioListener.volume = volume;
        }

        public static void SetMasterVolume(float volume)
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, volume);
            ApplyMasterVolume(volume);
        }
    }
}
