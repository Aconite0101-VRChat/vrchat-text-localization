

using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

namespace VRCLocalization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [RequireComponent(typeof(VRCPickup))]
    public class LocalizedPickup : UdonSharpBehaviour
    {

        [LocalizationKeyPopup("useTextValues")]
        public string useTextKey;

        [LocalizationKeyPopup("interactionTextValues")]
        public string interactionTextKey;

        [HideInInspector]
        public string[] languages;

        [HideInInspector]
        public string[] useTextValues;

        [HideInInspector]
        public string[] interactionTextValues;


        private VRCPickup[] pcs;
        void Start()
        {
            pcs = GetComponents<VRCPickup>();
        }

        private string GetValue(string[] targetValues, string language)
        {
            if (languages == null) return null;
            for (int i= 0; i < languages.Length; i++)
            {
                if (languages[i] == language) return targetValues[i];
            }
            return null;
        }


        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);

            string useText = GetValue(useTextValues, language);
            string interactionText = GetValue(interactionTextValues, language);

            foreach (VRCPickup pc in pcs)
            {
                pc.UseText = useText ?? pc.UseText;
                pc.InteractionText = interactionText ?? pc.InteractionText;
            }
        }
    }
}
