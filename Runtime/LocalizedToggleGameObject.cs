
using UdonSharp;
using UnityEngine;

namespace VRCLocalization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalizedToggleGameObject : VRCLocalizationBehaviour
    {
        [SerializeField]
        private GameObject[] gameObjects;

        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);
            int index = GetIndex(language);
            if (index >= 0 && index < gameObjects.Length)
            {
                foreach (var gameObject in gameObjects)
                {
                    if (gameObject) gameObject.SetActive(false);
                }
                if (gameObjects[index]) gameObjects[index].SetActive(true);
            }
        }
    }
}


