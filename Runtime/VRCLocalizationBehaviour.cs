
using UdonSharp;
using UnityEngine;

namespace VRCLocalization
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRCLocalizationBehaviour : UdonSharpBehaviour
    {
        [LocalizationKeyPopup]
        public string key;

        [HideInInspector]
        public string[] languages;

        protected int GetIndex(string language)
        {
            if (languages == null) return -1;
            for (int i = 0; i < languages.Length; i++)
            {
                if (languages[i] == language) return i;
            }
            return 0;
        }
    }
}
