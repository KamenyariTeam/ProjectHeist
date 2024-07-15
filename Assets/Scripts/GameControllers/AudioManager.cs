using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GameControllers.Audio
{
    using IDType = System.Int64;

    public struct AudioID
    {
        private readonly IDType _value;

        public AudioID(IDType value)
        {
            _value = value;
        }

        public static implicit operator IDType(AudioID id) => id._value;
        public static implicit operator AudioID(IDType value) => new AudioID(value);
    }

    public enum SoundType
    {
        GunShot,
        EmptyGunShot,
        GunReload,
        Step,
        MusicIdle,
        COUNT
    }

    public delegate SoundType OnAudioEndedDelegate(SoundType endedSound);

    public class AudioManager : MonoBehaviour
    {
        [System.Serializable]
        private class Sound
        {
            public SoundType type;
            public AudioClip clip;
            public AudioMixerGroup mixerGroup;
        }

        private class ActiveSoundData
        {
            public AudioSource source;
            public Coroutine coroutine;
            public Transform transformToFollow;
        }

        private static readonly string VolumeParameterTemplate = "{0}_Volume";
        private static readonly float AudioSourceDestroyingInterval = 30.0f;

        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private GameObject _audioSourcePrefab;
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private List<Sound> _sounds;

        private Dictionary<AudioID, ActiveSoundData> _activeSounds = new Dictionary<AudioID, ActiveSoundData>();
        private Queue<AudioSource> _audioSourcePool = new Queue<AudioSource>();
        private AudioID _currentAudioId = 0;

        private int _maxUsedAudioSources = 0;
        private float _timeTillAudioSourcesDestroying = AudioSourceDestroyingInterval;

        // Public Methods
        public AudioID PlaySound(SoundType type, OnAudioEndedDelegate onAudioEnded)
        {
            AudioSource source = GetAvailableSource(false);
            return LaunchSoundOnSource(source, type, null, onAudioEnded);
        }

        public AudioID PlaySound(SoundType type, Transform toFollow, OnAudioEndedDelegate onAudioEnded)
        {
            AudioSource source = GetAvailableSource(true);
            source.transform.position = toFollow.position;
            return LaunchSoundOnSource(source, type, toFollow, onAudioEnded);
        }

        public AudioID PlaySound(SoundType type, Vector3 position, OnAudioEndedDelegate onAudioEnded)
        {
            AudioSource source = GetAvailableSource(true);
            source.transform.position = position;
            return LaunchSoundOnSource(source, type, null, onAudioEnded);
        }

        public bool StopSound(AudioID id)
        {
            if (_activeSounds.TryGetValue(id, out ActiveSoundData data))
            {
                StopCoroutine(data.coroutine);
                data.source.Stop();
                _audioSourcePool.Enqueue(data.source);
                return _activeSounds.Remove(id);
            }
            return false;
        }

        public bool IsPlaying(AudioID id)
        {
            return _activeSounds.ContainsKey(id);
        }

        public void SetGroupVolume(string groupName, float volume)
        {
            string parameterName = string.Format(VolumeParameterTemplate, groupName);
            _audioMixer.SetFloat(parameterName, volume);
        }

        public float GetGroupVolume(string groupName)
        {
            string parameterName = string.Format(VolumeParameterTemplate, groupName);
            if (!_audioMixer.GetFloat(parameterName, out float volume))
            {
                return 0;
            }
            return volume;
        }


        // Unity methods
        protected void Awake()
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewAudioSource();
            }
        }

        protected void FixedUpdate()
        {
            UpdateSourcesPositions();

            if (_timeTillAudioSourcesDestroying >= 0)
            {
                _timeTillAudioSourcesDestroying -= Time.fixedDeltaTime;
            }
            else
            {
                DestroyUnusedAudioSources();
                _timeTillAudioSourcesDestroying = AudioSourceDestroyingInterval;
            }

        }


        // Private methods;
        private void CreateNewAudioSource()
        {
            GameObject obj = Instantiate(_audioSourcePrefab);
            var source = obj.GetComponent<AudioSource>();
            _audioSourcePool.Enqueue(source);
        }

        private AudioSource GetAvailableSource(bool isVolumetric)
        {
            if (_audioSourcePool.Count == 0)
            {
                CreateNewAudioSource();
            }

            AudioSource source = _audioSourcePool.Dequeue();
            source.spatialBlend = isVolumetric ? 1.0f : 0.0f;
            return source;
        }

        private void UpdateMaxUsedSources()
        {
            _maxUsedAudioSources = Math.Max(_maxUsedAudioSources, _activeSounds.Count);
        }

        private AudioID LaunchSoundOnSource(AudioSource source, SoundType type, Transform toFollow, OnAudioEndedDelegate onAudioEnded)
        {
            AudioID id = _currentAudioId;
            _currentAudioId++;

            _activeSounds.Add(id, new ActiveSoundData()
            {
                source = source,
                transformToFollow = toFollow
            });

            Coroutine coroutine = StartCoroutine(SoundPlayingCoroutine(source, id, type, onAudioEnded));
            if (!_activeSounds.TryGetValue(id, out ActiveSoundData data))
            {
                // The sound was not found or another error happened
                return -1;
            }

            UpdateMaxUsedSources();

            data.coroutine = coroutine;
            return id;
        }

        private System.Collections.IEnumerator SoundPlayingCoroutine(AudioSource source, AudioID audioID, SoundType soundType, OnAudioEndedDelegate onAudioEnded)
        {
            while (source != null && soundType != SoundType.COUNT)
            {
                Sound sound = _sounds.Find(s => s.type == soundType);
                if (sound == null)
                {
                    break;
                }

                source.clip = sound.clip;
                source.outputAudioMixerGroup = sound.mixerGroup;
                source.loop = false;
                source.Play();

                yield return new WaitForSeconds(source.clip.length);

                soundType = onAudioEnded.Invoke(soundType);
            }

            source.Stop();
            _audioSourcePool.Enqueue(source);
            _activeSounds.Remove(audioID);
        }

        private void DestroyUnusedAudioSources()
        {
            _maxUsedAudioSources = Math.Max(_maxUsedAudioSources, _initialPoolSize);
            int unusedSourcesCount = _activeSounds.Count + _audioSourcePool.Count - _maxUsedAudioSources;
            for (int i = 0; i < unusedSourcesCount; i++)
            {
                AudioSource source = _audioSourcePool.Dequeue();
                Destroy(source.gameObject);
            }
            _maxUsedAudioSources = 0;
        }

        private void UpdateSourcesPositions()
        {
            foreach (ActiveSoundData data in _activeSounds.Values)
            {
                if (data.transformToFollow != null)
                {
                    data.source.transform.position = data.transformToFollow.position;
                }
            }
        }
    }
}