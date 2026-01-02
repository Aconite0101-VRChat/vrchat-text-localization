using UnityEngine;

namespace VRCLocalization
{
    public class LocalizationKeyPopupAttribute : PropertyAttribute
    {
        public string ValuesFieldName;
        public LocalizationKeyPopupAttribute(string valuesFieldName = "values")
        {
            ValuesFieldName = valuesFieldName;
        }
    }
}
