using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI_Audio
{
    public class AudioPlayer : MonoBehaviour
    {
        private const float AttenuationConst = 5f;
        private static readonly HashSet<Sound> OneTimeSounds = new HashSet<Sound>();
        private static readonly GameObject OneTimeSoundsHandler = new GameObject("OneTimeSoundsHandler");

        private static readonly HashSet<Sound> Musics = new HashSet<Sound>();
        private static readonly GameObject MusicHandler = new GameObject("MusicPlayer");
        private static AudioSource _currentMusic;

        [SerializeField] private string[] soundsKeys;
        private readonly List<Sound> _sounds = new List<Sound>();
        private bool _adjustingVolume;
        private AudioListener _listener;

        private static AudioSource PlaySoundOnObject(GameObject target, HashSet<Sound> sounds, string soundKey)
        {
            AudioSource source;
            
            foreach (Sound sound in sounds)
            {
                if (sound.key != soundKey) continue;
                source = sound.GetAudioSource();
                if (source) source.Play();
                return source;
            }
            
            if (!AudioDB.Instance || !AudioDB.Instance.TryGetSound(soundKey, out Sound newSound))
                return null;
            
            newSound.SetAudioSource(target.AddComponent<AudioSource>());
            newSound.InitSource();
            source = newSound.GetAudioSource();
            if (source) source.Play();
            sounds.Add(newSound);
            return source;
        }
        
        public static void PlayMusic(string musicKey)
        {
            DontDestroyOnLoad(MusicHandler);
            
            if (_currentMusic)
                _currentMusic.Stop();
            
            _currentMusic = PlaySoundOnObject(MusicHandler, Musics, musicKey);
        }
        
        public static void PlaySoundNoDistance(string soundKey)
        {
            DontDestroyOnLoad(OneTimeSoundsHandler);
            PlaySoundOnObject(OneTimeSoundsHandler, OneTimeSounds, soundKey);
        }
        
        public void Start()
        {
            _adjustingVolume = false;
            _listener = MenuSettingsManager.Instance.worldCamera.gameObject.GetComponent<AudioListener>();
            foreach (string soundKey in soundsKeys)
                AddSound(soundKey);
        }

        private bool ContainsSound(string key, out Sound sound)
        {
            sound = GetSound(key);
            return sound != Sound.None;
        }

        private Sound GetSound(string key)
        {
            foreach (Sound sound in _sounds)
            {
                if (sound.key == key)
                    return sound;
            }
            
            Debug.LogWarning($"The sound '{key}' is not attached to this gameObject!");
            return Sound.None;
        }

        public void ModifySoundSettings(string key, float maxVolume = float.NaN, float maxDistance = float.NaN)
        {
            if (!ContainsSound(key, out Sound sound))
                return;
            
            if (!float.IsNaN(maxDistance))
                sound.SetMaxDistance(maxDistance);
            if (!float.IsNaN(maxVolume))
                sound.SetMaxVolume(maxDistance);
            
            sound.InitSource();
        }

        public void SetLooping(string key, bool state)
        {
            if (!ContainsSound(key, out Sound sound))
                return;
            sound.SetLoop(state);
            sound.InitSource();
        }
        
        public void AddSound(string key)
        {
            if (!AudioDB.Instance || !ContainsSound(key, out Sound s) || !AudioDB.Instance.TryGetSound(key, out Sound sound))
                return;

            AudioSource source = gameObject.AddComponent<AudioSource>();
            sound.SetAudioSource(source);
            sound.InitSource();
            _sounds.Add(sound);
        }

        public void RemoveSound(string key)
        {
            Sound sound = GetSound(key);
            if (sound.IsNone()) return;
            
            Destroy(sound.GetAudioSource());
            _sounds.Remove(sound);
        }

        public void PlaySound(string key)
        {
            Sound sound = GetSound(key);
            if (sound.IsNone()) return;

            sound.GetAudioSource().Play();
            ApplyVolumeReduction(sound);
            if (!_adjustingVolume)
                StartCoroutine(AdjustVolume());
        }

        private void ApplyVolumeReduction(Sound sound)
        {
            AudioSource source = sound.GetAudioSource();
            Vector2 listenerPos = _listener.gameObject.transform.position;
            Vector2 sourcePos = source.transform.position;
            
            float normalizedGap = (listenerPos - sourcePos).magnitude / sound.maxDistance;
            
            if (normalizedGap > 1)
            {
                source.volume = 0;
                return;
            }

            normalizedGap = 1 - normalizedGap;
            float attenuation = normalizedGap / (1 + normalizedGap * AttenuationConst);
            source.volume = attenuation * sound.maxVolume;
        }

        private IEnumerator AdjustVolume()
        {
            _adjustingVolume = true;
            yield return null;
            // Take action the next frame
            bool isSoundPlaying;
            do
            {
                isSoundPlaying = false;
                
                foreach (Sound sound in _sounds)
                {
                    if (!sound.IsPlaying()) continue;
                    isSoundPlaying = true;
                    ApplyVolumeReduction(sound);
                }
                
                if (isSoundPlaying)
                    yield return new WaitForSeconds(0.1f);
            } while (isSoundPlaying);

            _adjustingVolume = false;
        }
    }
}
