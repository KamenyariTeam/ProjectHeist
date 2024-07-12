using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GameControllers
{
    public class AudioManager : MonoBehaviour
    {
        public enum SoundType
        {
            MusicIdle,
            MusicAction,
            PistolShot,
            Steps,
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
        [SerializeField] private List<Sound> _sounds;

        private Dictionary<int, ActiveSoundData> _activeSounds = new Dictionary<int, ActiveSoundData>();

        public int PlaySound(SoundType type)
        {
            return PlaySound(type, gameObject, (soundType) => SoundType.COUNT);
        }

        public int PlaySound(SoundType type, OnAudioEndedDelegate onAudioEnded)
        {
            return PlaySound(type, gameObject, onAudioEnded);
        }
        public int PlaySound(SoundType type, GameObject parentObject)
        {
            return PlaySound(type, parentObject, (soundType) => SoundType.COUNT);
        }

        public int PlaySound(SoundType type, GameObject parentObject, OnAudioEndedDelegate onAudioEnded)
        {
            var source = parentObject.AddComponent<AudioSource>();
            int id = source.GetInstanceID();

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

        public bool StopSound(int id)
        {
            if (_activeSounds.TryGetValue(id, out ActiveSoundData data))
            {
                Destroy(data.source);
                StopCoroutine(data.coroutine);
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

        protected void OnDestroy()
        {
            var activeSoundIds = new List<int>(_activeSounds.Keys);
            foreach (int id in activeSoundIds)
            {
                StopSound(id);
            }
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

            Destroy(source);    
            _activeSounds.Remove(source.GetInstanceID());
        }

    }

}