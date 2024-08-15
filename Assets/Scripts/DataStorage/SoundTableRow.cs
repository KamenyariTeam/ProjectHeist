using UnityEngine;
using System;
using UnityEngine.Audio;

namespace DataStorage
{
    [Serializable]
    public class SoundTableRow : TableRowBase
    {
        public AudioClip clip;
        public AudioMixerGroup mixerGroup;
    }
}
