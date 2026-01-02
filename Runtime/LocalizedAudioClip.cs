
using UdonSharp;
using UnityEngine;

namespace VRCLocalization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(AudioSource))]
    public class LocalizedAudioClip : VRCLocalizationBehaviour
    {
        [HideInInspector]
        public AudioClip[] values;

        private AudioSource ac;
        bool isInitialized = false;
        void Start()
        {
            ac = GetComponent<AudioSource>();
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);

            bool isPlaying = ac.isPlaying;
            int index = GetIndex(language);

            if (index >= 0)
            {
                ac.clip = values[index];
                if ((!isInitialized && ac.playOnAwake) || isPlaying)
                {
                    ac.Play();
                    isInitialized = true;
                }
            }
            
        }
    }
}
