using UdonSharp;

namespace VRCLocalization
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VRCSceneLocalizationDescriptor : UdonSharpBehaviour
    {
        [LocalizationSettingsAttribute] public UnityEngine.Object settings;
    }
}