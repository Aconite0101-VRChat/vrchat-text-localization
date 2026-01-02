
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace VRCLocalization
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(RawImage))]
    public class LocalizedTexture : VRCLocalizationBehaviour
    {
        [HideInInspector]
        public Texture[] values;

        private RawImage rc;

        void Start()
        {
            rc = GetComponent<RawImage>();
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);
            int index = GetIndex(language);
            if (index >= 0) rc.texture = values[index];
        }
    }
}
