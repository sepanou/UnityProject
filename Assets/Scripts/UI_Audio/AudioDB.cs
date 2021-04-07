using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UI_Audio
{
    [CreateAssetMenu(fileName = "AudioDB", menuName = "DataBanks/AudioDB", order = 4)]
    public class AudioDB : ScriptableObject
    {
        public static AudioDB Instance;

        [SerializeField] private Sound[] sounds;

        public void Awake()
        {
            if (!Instance)
                Instance = this;
            else
                Destroy(this);
        }

        public void PlayUISound(string soundKey) => AudioPlayer.PlaySoundNoDistance(soundKey);
        
        public void PlayMusic(string musicKey) => AudioPlayer.PlayMusic(musicKey);

        public bool TryGetSound(string keyName, out Sound sound)
        {
            foreach (Sound sample in sounds)
            {
                if (sample.key != keyName) continue;
                sound = sample;
                return true;
            }

            Debug.LogWarning($"There is no sound with the name {keyName}!");
            sound = Sound.None;
            return false;
        }
    }

    [Serializable]
#pragma warning disable 660,661
    public struct Sound
#pragma warning restore 660,661
    {
        public static Sound None = new Sound {key = "None"};
        
        public AudioMixerGroup mixerGroup;
        public AudioClip clip;
        [Range(0,1)] public float maxVolume;
        public float maxDistance;
        public bool loop;
        public string key;
        private AudioSource _source;

        public void SetLoop(bool state) => loop = state;

        public void SetMaxVolume(float value)
        {
            if (value >= 0 && value <= 1)
                maxVolume = value;
        }

        public void SetMaxDistance(float value)
        {
            if (value < 0) return;
            maxDistance = value;
        }

        public void SetAudioSource(AudioSource source) => _source = source;
        
        public AudioSource GetAudioSource() => _source;

        public void InitSource()
        {
            if (!_source)
            {
                Debug.LogWarning("AudioSource has not been set! NullPointerException");
                return;
            }

            _source.clip = clip;
            _source.loop = loop;
            _source.volume = maxVolume;
            _source.outputAudioMixerGroup = mixerGroup;
        }

        public bool IsPlaying() => _source.isPlaying;

        public bool IsNone() => key == "None";

        public static bool operator ==(Sound s1, Sound s2) => s1.key == s2.key;

        public static bool operator !=(Sound s1, Sound s2) => !(s1 == s2);
    }
}
