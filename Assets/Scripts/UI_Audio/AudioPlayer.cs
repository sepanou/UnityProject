using System;
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

        private List<Sound> _sounds;
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
        
        public void Initialize()
        {
            _sounds = new List<Sound>();
            _adjustingVolume = false;
            _listener = MenuSettingsManager.CurrentCamera.gameObject.GetComponent<AudioListener>();
        }

        private bool ContainsSound(string key) => GetSound(key) != Sound.None;

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

        public Sound? AddSound(string key, float maxVolume = float.NaN, float maxDistance = float.NaN)
        {
            if (!AudioDB.Instance || ContainsSound(key) || !AudioDB.Instance.TryGetSound(key, out Sound sound))
                return null;

            AudioSource source = gameObject.AddComponent<AudioSource>();
            sound.SetAudioSource(source);
            if (!float.IsNaN(maxDistance))
                sound.SetMaxDistance(maxDistance);
            if (!float.IsNaN(maxVolume))
                sound.SetMaxVolume(maxDistance);
            
            sound.InitSource();
            _sounds.Add(sound);
            return sound;
        }
        
        public Sound? AddSound(string key, bool loop, float maxVolume = float.NaN, float maxDistance = float.NaN)
        {
            Sound? sound = AddSound(key, maxVolume, maxDistance);
            sound?.SetLoop(loop);
            return sound;
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
        
        public void PlaySoundDelay(string key, float delay)
        {
            Sound sound = GetSound(key);
            if (sound.IsNone()) return;

            sound.GetAudioSource().PlayDelayed(delay);
            ApplyVolumeReduction(sound);
            if (!_adjustingVolume)
                StartCoroutine(AdjustVolume());
        }
        
        public void PlaySoundScheduled(string key, double time)
        {
            Sound sound = GetSound(key);
            if (sound.IsNone()) return;
            
            sound.GetAudioSource().PlayScheduled(time);
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
