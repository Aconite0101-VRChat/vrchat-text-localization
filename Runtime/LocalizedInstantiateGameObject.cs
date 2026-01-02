
using UdonSharp;
using UnityEngine;

namespace VRCLocalization
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class LocalizedInstantiateGameObject : VRCLocalizationBehaviour
    {

        [SerializeField]
        private bool assignParent = true;
        [SerializeField]
        private bool setLocalTransform = false;

        [HideInInspector]
        public GameObject[] values;

        private GameObject spawnedObject;

        private void SpawnObjectByIndex(int index)
        {
            Destroy(spawnedObject);

            spawnedObject = Instantiate(values[index]);
            if (assignParent) spawnedObject.transform.SetParent(transform, setLocalTransform);
        }


        public override void OnLanguageChanged(string language)
        {
            base.OnLanguageChanged(language);
            int index = GetIndex(language);
            if (index >= 0) SpawnObjectByIndex(index);
        }
    }
}

