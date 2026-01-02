
using UdonSharp;
using UnityEngine;

namespace VRCLocalization
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalizedText : VRCLocalizationBehaviour
    {
        [HideInInspector]
        public string[] values;

        protected string GetTextToChanged(string language)
        {
            int index = GetIndex(language);
            if (index >= 0) return values[index];
            return null;
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);
        }
    }
}
