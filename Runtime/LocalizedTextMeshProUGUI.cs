
using TMPro;
using UnityEngine;

namespace TextLocalization
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedTextMeshProUGUI : LocalizedText
    {
        private TextMeshProUGUI tc;
        void Start()
        {
            tc = GetComponent<TextMeshProUGUI>();
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);

            tc.text = base.GetTextToChanged(language) ?? tc.text;
        }
    }
}
