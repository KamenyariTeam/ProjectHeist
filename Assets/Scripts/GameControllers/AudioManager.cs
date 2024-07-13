using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GameControllers
{
    using AudioID = System.Int32;

    public class AudioManager : MonoBehaviour
    {
        public enum SoundType
        {
            GunShot,
            EmptyGunShot,
            GunReload,
            Step,
            COUNT
        }

        [System.Serializable]
        public class Sound
        {
            public SoundType type;
            public AudioClip clip;
            public AudioMixerGroup mixerGroup;
        }

        private class ActiveSoundData
        {
            public AudioSource source;
            public Coroutine coroutine;
        }

        public delegate SoundType OnAudioEndedDelegate(SoundType endedSound);

        private static readonly string VolumeParameterTemplate = "{0}_Volume";

        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private GameObject _audioSourcePrefab;
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private List<Sound> _sounds;

        private Dictionary<AudioID, ActiveSoundData> _activeSounds = new Dictionary<AudioID, ActiveSoundData>();
        private Queue<AudioSource> _audioSourcePool = new Queue<AudioSource>();

        public AudioID PlaySound(SoundType type, OnAudioEndedDelegate onAudioEnded)
        {
            AudioSource source = GetAvailableSource(false);
            return LaunchSoundOnSource(source, type, onAudioEnded);
        }

        public AudioID PlaySound(SoundType type, Transform parentTransform, OnAudioEndedDelegate onAudioEnded)
        {
            AudioSource source = GetAvailableSource(true);
            source.transform.SetParent(parentTransform);
            source.transform.localPosition = Vector3.zero;
            return LaunchSoundOnSource(source, type, onAudioEnded);
        }

        public AudioID PlaySound(SoundType type, Vector3 position, OnAudioEndedDelegate onAudioEnded)
        {
            AudioSource source = GetAvailableSource(true);
            source.transform.position = position;
            return LaunchSoundOnSource(source, type, onAudioEnded);
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

        protected void Awake()
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewAudioSource();
            }
        }

        protected void OnDestroy()
        {
            var activeSoundIds = new List<int>(_activeSounds.Keys);
            foreach (int id in activeSoundIds)
            {
                StopSound(id);
            }
        }

        private void CreateNewAudioSource()
        {
            GameObject obj = Instantiate(_audioSourcePrefab);
            var source = obj.GetComponent<AudioSource>();
            _audioSourcePool.Enqueue(source);
        }

        private AudioID LaunchSoundOnSource(AudioSource source, SoundType type, OnAudioEndedDelegate onAudioEnded)
        {
            AudioID id = source.GetInstanceID();

            _activeSounds.Add(id, new ActiveSoundData()
            {
                source = source
            });

            Coroutine coroutine = StartCoroutine(SoundPlayingCoroutine(source, type, onAudioEnded));
            if (_activeSounds.TryGetValue(id, out ActiveSoundData data))
            {
                data.coroutine = coroutine;
            }

            return id;
        }

        private AudioSource GetAvailableSource(bool isVolumetric)
        {
            if (_audioSourcePool.Count == 0)
            {
                CreateNewAudioSource();
            }

            AudioSource source = _audioSourcePool.Dequeue();
            source.spatialBlend = isVolumetric ? 1.0f : 0.0f;
            source.transform.SetParent(null);
            return source;
        }

        private System.Collections.IEnumerator SoundPlayingCoroutine(AudioSource source, SoundType soundType, OnAudioEndedDelegate onAudioEnded)
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

            if (source != null)
            {
                source.Stop();
                _audioSourcePool.Enqueue(source);
                _activeSounds.Remove(source.GetInstanceID());
            }
        }
    }
}