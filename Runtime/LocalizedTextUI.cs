
using UnityEngine;
using UnityEngine.UI;

namespace VRCLocalization
{
    [RequireComponent(typeof(Text))]
    public class LocalizedTextUI : LocalizedText
    {
        private Text tc;
        void Start()
        {
            tc = GetComponent<Text>();
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);

            tc.text = base.GetTextToChanged(language) ?? tc.text;
        }
    }
}
