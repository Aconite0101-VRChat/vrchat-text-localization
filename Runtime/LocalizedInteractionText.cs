

using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;

namespace VRCLocalization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalizedInteractionText : LocalizedText
    {

        private UdonSharpBehaviour[] ucs;
        void Start()
        {
            ucs = GetComponents<UdonSharpBehaviour>();
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);


            foreach (UdonSharpBehaviour uc in ucs)
            {
                uc.InteractionText = GetTextToChanged(language) ?? uc.InteractionText;
            }
        }
    }
}
