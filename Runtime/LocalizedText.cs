
using UdonSharp;
using UnityEngine;

namespace TextLocalization
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalizedText : UdonSharpBehaviour
    {
        [LocalizationKeyPopup]
        public string key;

        [HideInInspector]
        public string[] languages;
        [HideInInspector]
        public string[] values;

        protected string GetTextToChanged(string language)
        {
            if (languages == null || values == null) return null;

            int langIndex = -1;
            for (int i = 0; i < languages.Length; i++)
            {
                if (languages[i] == language)
                {
                    langIndex = i;
                    break;
                }
            }

            if (langIndex == -1 && languages.Length > 0) langIndex = 0;
            if (langIndex == -1) return null;

            if (langIndex < values.Length)
            {
                return values[langIndex];
            }
            return null;
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);
        }
    }
}
