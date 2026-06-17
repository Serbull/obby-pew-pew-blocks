#if BridgePlatform_yg && Storage_yg
using UnityEngine;
using Playgama;

namespace YG
{
    public partial class PlatformYG2 : IPlatformsYG2
    {
        private static float _defaultAudioVolume;
        public static void InitAudio()
        {
            _defaultAudioVolume = AudioListener.volume;
            ApplyAudioState(Bridge.platform.isAudioEnabled);
            Bridge.platform.audioStateChanged += OnAudioStateChanged;
        }

        private static void OnAudioStateChanged(bool isEnabled)
        {
            ApplyAudioState(isEnabled);
        }

        private static void ApplyAudioState(bool isEnabled)
        {
            if (isEnabled)
            {
                AudioListener.volume = _defaultAudioVolume;
            }
            else
            {
                _defaultAudioVolume = AudioListener.volume;
                AudioListener.volume = 0;
            }
        }
    }
}
#endif