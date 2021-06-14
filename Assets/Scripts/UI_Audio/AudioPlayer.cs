﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI_Audio {
	public class AudioPlayer: MonoBehaviour {
		private const float AttenuationConst = 5f;
		private static readonly HashSet<Sound> OneTimeSounds = new HashSet<Sound>();
		private static GameObject _oneTimeSoundsHandler;

		[NonSerialized] public static AudioDB AudioManager;
		private static readonly HashSet<Sound> Musics = new HashSet<Sound>();
		private static GameObject _musicHandler;
		private static AudioSource _currentMusic;

		[SerializeField] private string[] soundsKeys;
		[SerializeField] private bool playOnAwake;
		private readonly List<Sound> _sounds = new List<Sound>();
		private bool _adjustingVolume;
		private AudioListener _listener;

		private static AudioSource PlaySoundOnObject(GameObject target, HashSet<Sound> sounds, string soundKey) {
			AudioSource source;
			
			foreach (Sound sound in sounds.Where(sound => sound.key == soundKey)) {
				source = sound.GetAudioSource();
				if (source) source.Play();
				return source;
			}
			
			if (!AudioManager || !AudioManager.TryGetSound(soundKey, out Sound newSound))
				return null;
			
			newSound.SetAudioSource(target.AddComponent<AudioSource>());
			newSound.InitSource();
			source = newSound.GetAudioSource();
			if (source) source.Play();
			sounds.Add(newSound);
			return source;
		}
		
		public static void PlayMusic(string musicKey) {
			if (!_musicHandler) {
				Musics.Clear();
				_musicHandler = new GameObject("OneTimeSoundsHandler");
				DontDestroyOnLoad(_musicHandler);
			}

			if (_currentMusic)
				_currentMusic.Stop();
			
			_currentMusic = PlaySoundOnObject(_musicHandler, Musics, musicKey);
		}
		
		public static void PlaySoundNoDistance(string soundKey) {
			if (!_oneTimeSoundsHandler) {
				OneTimeSounds.Clear();
				_oneTimeSoundsHandler = new GameObject("OneTimeSoundsHandler");
				DontDestroyOnLoad(_oneTimeSoundsHandler);
			}
			PlaySoundOnObject(_oneTimeSoundsHandler, OneTimeSounds, soundKey);
		}
		
		public void Start() {
			_listener = LocalGameManager.Instance.worldCamera.gameObject.GetComponent<AudioListener>();
			foreach (string soundKey in soundsKeys)
				AddSound(soundKey);
			if (playOnAwake)
				_sounds.ForEach(sound => PlaySound(sound.key));
		}

		private bool ContainsSound(string key, out Sound sound) {
			sound = GetSound(key);
			return sound != Sound.None;
		}

		private Sound GetSound(string key) {
			foreach (Sound sound in _sounds)
				if (sound.key == key)
					return sound;
			
			Debug.LogWarning($"The sound '{key}' is not attached to this gameObject!");
			return Sound.None;
		}

		public void ModifySoundSettings(string key, float maxVolume = float.NaN, float maxDistance = float.NaN) {
			if (!ContainsSound(key, out Sound sound))
				return;
			if (!float.IsNaN(maxDistance))
				sound.SetMaxDistance(maxDistance);
			if (!float.IsNaN(maxVolume))
				sound.SetMaxVolume(maxDistance);
			sound.InitSource();
		}

		public void SetLooping(string key, bool state) {
			if (!ContainsSound(key, out Sound sound)) return;
			sound.SetLoop(state);
			sound.InitSource();
		}
		
		public void AddSound(string key) {
			if (!AudioManager || ContainsSound(key, out Sound _) || !AudioManager.TryGetSound(key, out Sound sound)) return;
			AudioSource source = gameObject.AddComponent<AudioSource>();
			sound.SetAudioSource(source);
			sound.InitSource();
			_sounds.Add(sound);
		}

		public void RemoveSound(string key) {
			Sound sound = GetSound(key);
			if (sound.IsNone()) return;
			Destroy(sound.GetAudioSource());
			_sounds.Remove(sound);
		}

		public void PlaySound(string key) {
			Sound sound = GetSound(key);
			if (sound.IsNone()) return;
			sound.GetAudioSource().Play();
			ApplyVolumeReduction(sound);
			if (!_adjustingVolume)
				StartCoroutine(AdjustVolume());
		}

		private void ApplyVolumeReduction(Sound sound) {
			AudioSource source = sound.GetAudioSource();
			Vector2 listenerPos = _listener.gameObject.transform.position;
			Vector2 sourcePos = source.transform.position;
			float normalizedGap = (listenerPos - sourcePos).magnitude / sound.maxDistance;
			if (normalizedGap > 1) {
				source.volume = 0;
				return;
			}
			float attenuation = (1 - normalizedGap) / (1 + normalizedGap * AttenuationConst);
			source.volume = attenuation * sound.maxVolume;
		}

		private IEnumerator AdjustVolume() {
			_adjustingVolume = true;
			yield return null;
			// Take action the next frame
			bool isSoundPlaying;
			do {
				isSoundPlaying = false;
				foreach (Sound sound in _sounds.Where(sound => sound.IsPlaying())) {
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
