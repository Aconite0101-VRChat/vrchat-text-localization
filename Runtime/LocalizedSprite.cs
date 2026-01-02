
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace VRCLocalization
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(Image))]
    public class LocalizedSprite : VRCLocalizationBehaviour
    {
        [HideInInspector]
        public Sprite[] values;

        private Image ic;

        void Start()
        {
            ic = GetComponent<Image>();
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);
            int index = GetIndex(language);
            if (index >= 0) ic.sprite = values[index];
        }
    }
}
