
using UdonSharp;
using VRC.Udon;

namespace VRCLocalization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalizedInteractionText : LocalizedText
    {

        private UdonBehaviour[] ucs;
        void Start()
        {
            ucs = GetComponents<UdonBehaviour>();
        }

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);


            foreach (UdonBehaviour uc in ucs)
            {
                uc.InteractionText = GetTextToChanged(language) ?? uc.InteractionText;
            }
        }
    }
}
