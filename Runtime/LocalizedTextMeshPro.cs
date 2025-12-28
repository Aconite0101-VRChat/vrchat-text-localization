
using TMPro;
using UnityEngine;

namespace TextLocalization
{
    [RequireComponent(typeof(TextMeshPro))]
    public class LocalizedTextMeshPro : LocalizedText
    {
        private TextMeshPro tc;
        void Start()
        {
            tc = GetComponent<TextMeshPro>();
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);

            tc.text = base.GetTextToChanged(language) ?? tc.text;
        }
    }
}
