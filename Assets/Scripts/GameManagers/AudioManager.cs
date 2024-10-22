using System;
using System.Collections.Generic;
using DataStorage;
using UnityEngine;
using UnityEngine.Audio;

namespace GameManagers
{
    using IDType = Int64;

    public struct AudioID
    {
        private readonly IDType _value;

        public AudioID(IDType value)
        {
            _value = value;
        }

        public static implicit operator IDType(AudioID id) => id._value;
        public static implicit operator AudioID(IDType value) => new(value);
    }

    public delegate TableID OnAudioEndedDelegate(TableID endedSound);

    public class AudioManager : MonoBehaviour
    {
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
        [SerializeField] private IDataContainer<SoundTableRow> _soundTable;

        private Dictionary<AudioID, ActiveSoundData> _activeSounds = new Dictionary<AudioID, ActiveSoundData>();
        private Queue<AudioSource> _audioSourcePool = new Queue<AudioSource>();
        private AudioID _currentAudioId = 0;

        private int _maxUsedAudioSources = 0;
        private float _timeTillAudioSourcesDestroying = AudioSourceDestroyingInterval;

        // Public Methods
        public AudioID PlaySound(TableID soundTableID, OnAudioEndedDelegate onAudioEnded)
        {
            AudioSource source = GetAvailableSource(false);
            return LaunchSoundOnSource(source, soundTableID, null, onAudioEnded);
        }

        public AudioID PlaySound(TableID soundTableID, Transform toFollow, OnAudioEndedDelegate onAudioEnded)
        {
            AudioSource source = GetAvailableSource(true);
            source.transform.position = toFollow.position;
            return LaunchSoundOnSource(source, soundTableID, toFollow, onAudioEnded);
        }

        public AudioID PlaySound(TableID soundTableID, Vector3 position, OnAudioEndedDelegate onAudioEnded)
        {
            AudioSource source = GetAvailableSource(true);
            source.transform.position = position;
            return LaunchSoundOnSource(source, soundTableID, null, onAudioEnded);
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

        private AudioID LaunchSoundOnSource(AudioSource source, TableID soundTableID, Transform toFollow, OnAudioEndedDelegate onAudioEnded)
        {
            AudioID id = _currentAudioId;
            _currentAudioId++;

            _activeSounds.Add(id, new ActiveSoundData()
            {
                source = source,
                transformToFollow = toFollow
            });

            Coroutine coroutine = StartCoroutine(SoundPlayingCoroutine(source, id, soundTableID, onAudioEnded));
            if (!_activeSounds.TryGetValue(id, out ActiveSoundData data))
            {
                // The sound was not found or another error happened
                return -1;
            }

            UpdateMaxUsedSources();

            data.coroutine = coroutine;
            return id;
        }

        private System.Collections.IEnumerator SoundPlayingCoroutine(AudioSource source, AudioID audioID, TableID soundTableID, OnAudioEndedDelegate onAudioEnded)
        {
            while (source != null && soundTableID != TableID.NONE)
            {

                if (!_soundTable.Get(soundTableID, out SoundTableRow sound))
                {
                    break;
                }

                source.clip = sound.clip;
                source.outputAudioMixerGroup = sound.mixerGroup;
                source.loop = false;
                source.Play();

                yield return new WaitForSeconds(source.clip.length);

                soundTableID = onAudioEnded.Invoke(soundTableID);
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