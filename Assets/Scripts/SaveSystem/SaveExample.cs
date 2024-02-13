using UnityEditor;
using UnityEngine;

namespace SaveSystem
{
    public class SaveExample : MonoBehaviour, ISavableComponent
    {
        private void Reset()
        {
        }

        public ComponentData Serialize()
        {
            var data = new ExtendedComponentData();

            data.SetTransform("transform", transform);

            return data;
        }

        public void Deserialize(ComponentData data)
        {
            var unpacked = (ExtendedComponentData)data;

            unpacked.GetTransform("transform", transform);
        }
    }
}